using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Movimento do cenário")]
    [SerializeField, Min(0f)] private float scrollSpeed = 3f;
    public float ScrollSpeed => scrollSpeed;
}


