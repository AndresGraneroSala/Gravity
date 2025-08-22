using System;
using UnityEngine;

public class LoopActivation : MonoBehaviour
{
    [SerializeField] private GameObject obj; 
    [SerializeField] private float delay;
    private float timer;

    private void Awake()
    {
        timer = delay;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            obj.SetActive(!obj.activeSelf);

            timer = delay;
        }
    }
}
