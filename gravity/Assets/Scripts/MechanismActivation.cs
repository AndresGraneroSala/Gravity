using System;
using UnityEngine;

public class MechanismActivation : MonoBehaviour
{
    [SerializeField] private float timeActive;
    [SerializeField] private GameObject mechanism;
    [SerializeField] private bool setActive=false;

    [SerializeField] private GameObject loadBar;
    
    private float _timer=0;
    private bool _isInside;
    private bool _isFinished;
    
    // Update is called once per frame
    void Update()
    {
        if (_isFinished)
        {
            return;
        }
        
        if (_isInside)
        {
            _timer += Time.deltaTime;
            

            if (_timer >= timeActive)
            {
                mechanism.SetActive(setActive);
                _isFinished = true;
            }
            loadBar.transform.localScale = new Vector3 (GetScaleForLoadBar(),1,1);

        }
        else
        {
            _timer = 0;
            loadBar.transform.localScale = new Vector3 (0,1,1);

        }
    }

    private float GetScaleForLoadBar()
    {
        return 1 /(timeActive / _timer);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isInside = false;
        }
    }
}
