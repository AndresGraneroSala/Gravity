using UnityEngine;

public class BouncyObstacle : MonoBehaviour
{
    [Tooltip("Multiplicador de energía tras rebote, 1=no pierde energía, 0.8=pierde 20%")]
    public float bounceDamping = 0.8f;
}