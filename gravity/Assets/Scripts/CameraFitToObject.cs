using UnityEngine;

[ExecuteAlways]
public class ZoomToObjectEditorAware : MonoBehaviour
{
    private Camera _cam;
    public GameObject target;

    [SerializeField]
    private float originalOrthographicSize = -1f;

    private int lastScreenWidth;
    private int lastScreenHeight;

    void Awake()
    {
        if (_cam == null)
            _cam = Camera.main;

        if (originalOrthographicSize < 0)
            originalOrthographicSize = _cam.orthographicSize;

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        UpdateZoom();
    }

    void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            UpdateZoom();
        }
    }

    // También se llama en Editor sin Play mode con OnValidate o Update
    void OnValidate()
    {
        if (_cam == null)
            _cam = Camera.main;

        if (originalOrthographicSize < 0 && _cam != null)
            originalOrthographicSize = _cam.orthographicSize;

        UpdateZoom();
    }

    private void UpdateZoom()
    {
        if (_cam == null || target == null) return;

        float currentRatio = (float)Screen.width / Screen.height;
        float ratio169 = 16f / 9f;

        _cam.orthographicSize = originalOrthographicSize;

        if (currentRatio < ratio169+0.01f)
        {
            IncreaseSizeUntilHalfVisible();
        }
    }

    private void IncreaseSizeUntilHalfVisible()
    {
        Bounds bounds = target.GetComponent<Renderer>().bounds;

        while (!IsHalfVisible(bounds))
        {
            _cam.orthographicSize += 0.1f;

            bounds = target.GetComponent<Renderer>().bounds;
        }
    }

    private bool IsHalfVisible(Bounds bounds)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        Vector3[] corners = new Vector3[8]
        {
            center + new Vector3( extents.x,  extents.y,  extents.z),
            center + new Vector3( extents.x,  extents.y, -extents.z),
            center + new Vector3( extents.x, -extents.y,  extents.z),
            center + new Vector3( extents.x, -extents.y, -extents.z),
            center + new Vector3(-extents.x,  extents.y,  extents.z),
            center + new Vector3(-extents.x,  extents.y, -extents.z),
            center + new Vector3(-extents.x, -extents.y,  extents.z),
            center + new Vector3(-extents.x, -extents.y, -extents.z)
        };

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        bool anyInFront = false;

        foreach (var corner in corners)
        {
            Vector3 vp = _cam.WorldToViewportPoint(corner);
            if (vp.z > 0)
                anyInFront = true;

            if (vp.x < minX) minX = vp.x;
            if (vp.x > maxX) maxX = vp.x;
            if (vp.y < minY) minY = vp.y;
            if (vp.y > maxY) maxY = vp.y;
        }

        if (!anyInFront) return false;

        float objectArea = Mathf.Max(0, maxX - minX) * Mathf.Max(0, maxY - minY);

        float interMinX = Mathf.Clamp01(minX);
        float interMaxX = Mathf.Clamp01(maxX);
        float interMinY = Mathf.Clamp01(minY);
        float interMaxY = Mathf.Clamp01(maxY);

        float intersectionArea = Mathf.Max(0, interMaxX - interMinX) * Mathf.Max(0, interMaxY - interMinY);

        if (objectArea <= 0) return false;

        float visibleRatio = intersectionArea / objectArea;

        return visibleRatio >= 0.5f;
    }
}
