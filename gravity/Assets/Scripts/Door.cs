using System;
using UnityEngine;

public class Door : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        GameManager.Instance.NextLevel();
    }
}
