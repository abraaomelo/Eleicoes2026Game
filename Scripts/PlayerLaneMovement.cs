using UnityEngine;

public class PlayerLaneMovement : MonoBehaviour
{
    [SerializeField] private float laneSpacing = 1.5f;
    [SerializeField] private float smoothTime = 0.1f;

    // -1 = esquerda, 0 = centro, 1 = direita.
    private int currentLane = 0;
    private float horizontalVelocity;
    private float centerX;

    private void Awake()
    {
        centerX = transform.position.x;
    }

    private void Update()
    {
        Vector3 position = transform.position;
        float targetX = centerX + currentLane * laneSpacing;

        position.x = Mathf.SmoothDamp(
            position.x,
            targetX,
            ref horizontalVelocity,
            smoothTime
        );

        transform.position = position;
    }

    public void MoveLeft()
    {
        if (!isActiveAndEnabled || currentLane <= -1)
            return;

        currentLane--;

        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayLaneChange();
    }

    public void MoveRight()
    {
        if (!isActiveAndEnabled || currentLane >= 1)
            return;

        currentLane++;

        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayLaneChange();
    }
}

