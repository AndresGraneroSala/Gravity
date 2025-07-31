using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(LineRenderer))]
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float force = 10f;
    [SerializeField, Range(0.5f, 200f)] private float lineLength = 3f;
    [SerializeField, Range(0.01f, 10f)] private float lineWidth = 0.05f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float minReboundSpeed = 1.5f;
    [SerializeField] private int maxJumps = 2;
    [SerializeField, Range(1, 100)] private int maxReflections = 3;

    private int _currentJumps = -1;
    [SerializeField] private Text jumpsText;
    
    [SerializeField] private GameObject impactMarkerPrefab;
    private List<GameObject> _markersToDisable= new List<GameObject>();

    private Queue<GameObject> _markersQueue = new Queue<GameObject>();
    
    private GameObject _parentMarkers;
    
    private Camera _cam;
    private Rigidbody2D _rb;
    private LineRenderer _lineRenderer;
    private Vector2 _currentVelocity = Vector2.zero;
    private Vector2 _aimDirection = Vector2.zero;
    private PlayerController _input;
    private Vector2 _colliderExtent;

    private BoxCollider2D _box;
    
    private void Awake()
    {
        UpdateJumpText();
    }

    private void Start()
    {
        
        InitPoolMarkers();
        
        _box = GetComponent<BoxCollider2D>();
        if (_box != null)
            _colliderExtent = _box.size * 0.5f;
        else
            Debug.LogError("PlayerMove requiere un BoxCollider2D para el trazado preciso.");

        _cam = Camera.main;
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.simulated = true;
        _rb.gravityScale = 0;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.freezeRotation = true;

        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.startWidth = lineWidth;
        _lineRenderer.endWidth = lineWidth;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.positionCount = 0;
        _lineRenderer.enabled = false;

        _input = GetComponent<PlayerController>();
        if (_input == null) Debug.LogError("Falta PlayerController en el GameObject");
    }

    private void InitPoolMarkers()
    {
        _parentMarkers=new GameObject
        {
            name = "Markers"
        };

        for (int i = 0; i < maxReflections; i++)
        {
            GameObject marker = Instantiate(impactMarkerPrefab, _parentMarkers.transform);
            marker.SetActive(false);
            _markersQueue.Enqueue(marker);
        }
    }

    private GameObject GetMarker()
    {
        if (_markersQueue.Count == 0)
        {
            Debug.LogError("No hay markers disponibles en la cola!");
            return null;
        }
        
        GameObject tmpMarker = _markersQueue.Dequeue();;
        tmpMarker.SetActive(true);
        
        _markersQueue.Enqueue(tmpMarker);
        
        return tmpMarker;
    }

    private void UpdateJumpText()
    {
        _currentJumps++;
        if (_currentJumps > maxJumps)
        {
            GameManager.Instance.RestartGame();
        }
        jumpsText.text = (maxJumps - _currentJumps).ToString();
    }

    private void Update()
    {
        if (_input.IsPausePressed())
        {
            GameManager.Instance.TogglePause();
        }

        if (_input == null || GameManager.Instance.isPaused()) return;
        HandleInput();
        MovePlayer();
    }

    private void HandleInput()
    {
        bool isAiming = _input.IsAiming();
        bool isCounter = _input.IsCountering();

        if (isAiming)
        {
            _lineRenderer.enabled = true;
            if (isCounter && _currentVelocity.magnitude > 0.1f)
                _aimDirection = -_currentVelocity.normalized;
            else if (!isCounter)
                _aimDirection = _input.GetAimDirection(_cam, transform.position);

            if (_aimDirection != Vector2.zero)
                DrawPredictionLine(_aimDirection);
        }
        else
        {
            _lineRenderer.enabled = false;
            ClearImpactMarkers();
        }

        if (_aimDirection != Vector2.zero && _input.IsLaunchPressed() && isAiming)
        {
            UpdateJumpText();

            Vector2 impulse = _aimDirection.normalized * force;
            float dot = Vector2.Dot(_currentVelocity, impulse);

            if (dot >= 0)
                _currentVelocity += impulse;
            else
            {
                float speedAlong = Vector2.Dot(_currentVelocity, _aimDirection.normalized);
                float netForce = impulse.magnitude - Mathf.Abs(speedAlong);
                _currentVelocity = (netForce > 0)
                    ? _aimDirection.normalized * netForce
                    : _currentVelocity + impulse;
            }

            _lineRenderer.enabled = false;
        }
    }

    private void MovePlayer()
    {
        transform.position += (Vector3)(_currentVelocity * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & obstacleLayer) == 0) return;
        if (_currentVelocity.sqrMagnitude < 0.01f) return;

        ContactPoint2D contact = collision.GetContact(0);
        Vector2 reflectDir = Vector2.Reflect(_currentVelocity.normalized, contact.normal);
        var bounce = collision.gameObject.GetComponent<BouncyObstacle>();
        float damping = (bounce != null) ? bounce.bounceDamping : 1f;

        Vector2 newVel = reflectDir * _currentVelocity.magnitude * damping;
        if (newVel.magnitude < minReboundSpeed)
            newVel = reflectDir * minReboundSpeed;

        _currentVelocity = newVel;
        transform.position += (Vector3)(contact.normal * 0.05f);
    }
    [SerializeField, Range(0.005f, 0.5f)] private float predictionStep = 0.08f; // distancia del sub-paso
    [SerializeField, Range(0.0001f, 0.01f)] private float predictionSkin = 0.001f; // “piel” para no tocar

private void DrawPredictionLine(Vector2 dir)
{
    if (_box == null || dir == Vector2.zero) return;

    // Limpiar todos los marcadores anteriores
    ClearImpactMarkers();

    Vector3 s = transform.lossyScale;
    Vector2 size = new Vector2(_box.size.x * Mathf.Abs(s.x), _box.size.y * Mathf.Abs(s.y));
    Vector2 origin = _box.bounds.center;
    Vector2 direction = dir.normalized;
    float remaining = lineLength;

    List<Vector3> points = new List<Vector3>();
    points.Add(origin);

    int reflections = 0;
    int safety = 0;
    const int safetyMax = 2000;

    while (remaining > 0f && safety++ < safetyMax)
    {
        float step = Mathf.Min(predictionStep, remaining);
        float castDist = step + predictionSkin;
        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, direction, castDist, obstacleLayer);

        if (hit.collider != null)
        {
            float stopDist = Mathf.Max(0f, hit.distance - predictionSkin);
            Vector2 stopPoint = origin + direction * stopDist;

            if ((points[points.Count - 1] - (Vector3)stopPoint).sqrMagnitude > 1e-8f)
                points.Add(stopPoint);

            // Instanciar marcador de impacto con rotación correcta
            if (impactMarkerPrefab != null)
            {
                GameObject marker = GetMarker();
                marker.transform.position = stopPoint;
                
                _markersToDisable.Add(marker);
            }

            if (reflections >= maxReflections) break;

            Vector2 newDir = Vector2.Reflect(direction, hit.normal).normalized;
            var bounce = hit.collider.GetComponent<BouncyObstacle>();
            if (bounce != null && bounce.bounceDamping < 1f)
            {
                // Lógica de damping (opcional)
            }

            origin = stopPoint + newDir * predictionSkin;
            reflections++;
            direction = newDir;
            remaining -= stopDist;

            if (stopDist < 1e-5f)
            {
                origin += direction * predictionSkin * 2f;
            }

            continue;
        }
        else
        {
            origin += direction * step;
            points.Add(origin);
            remaining -= step;
        }
    }

    _lineRenderer.positionCount = points.Count;
    for (int i = 0; i < points.Count; i++)
        _lineRenderer.SetPosition(i, points[i]);
}

// Método para limpiar todos los marcadores de impacto
private void ClearImpactMarkers()
{
    foreach (GameObject marker in _markersToDisable)
    {
        marker.SetActive(false);
    }
}


    private void OnDrawGizmos()
    {
        if (_lineRenderer == null || _lineRenderer.positionCount == 0) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < _lineRenderer.positionCount; i++)
        {
            Gizmos.DrawWireCube(_lineRenderer.GetPosition(i), GetScaledColliderSize());
        }
    }

    private Vector3 GetScaledColliderSize()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null) return Vector3.one;

        Vector3 lossyScale = transform.lossyScale;
        return new Vector3(box.size.x * lossyScale.x, box.size.y * lossyScale.y, 1f);
    }



}
