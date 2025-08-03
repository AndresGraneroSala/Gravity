using System;
using UnityEngine;
using UnityEngine.UI;

public class EnableDisable : MonoBehaviour
{
    [SerializeField] private GameObject objectToEnable;

    private void Awake()
    {
        objectToEnable.SetActive(false);
        GetComponent<Button>().onClick.AddListener(SwitchState);
    }

    private void SwitchState()
    {
        objectToEnable.SetActive(!objectToEnable.activeSelf);
    }
}
