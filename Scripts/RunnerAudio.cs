using UnityEngine;

public class RunnerAudio : MonoBehaviour
{
    [SerializeField] private GameController gameController;

    private void Update()
    {
        if (GameAudio.Instance == null)
            return;

        float speed =
            gameController != null && gameController.IsRunning
                ? gameController.RoadSpeed
                : 0f;

        GameAudio.Instance.SetRunningSpeed(speed);
    }

    private void OnDisable()
    {
        if (GameAudio.Instance != null)
            GameAudio.Instance.StopRunning();
    }
}


