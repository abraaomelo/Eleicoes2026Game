using UnityEngine;

public class LoopingScenery : MonoBehaviour
{
    [SerializeField] private GameController gameController;

    [Header("Limites de reciclagem")]
    [SerializeField] private float bottomY = -6f;
    [SerializeField] private float topY = 6f;

    private void Update()
    {
        if (gameController == null || topY <= bottomY)
            return;

        Vector3 position = transform.position;

        position.y -= gameController.ScrollSpeed * Time.deltaTime;

        // Saiu por baixo: reaparece acima, mantendo o espaçamento.
        if (position.y < bottomY)
        {
            float loopHeight = topY - bottomY;

            position.y = bottomY
                + Mathf.Repeat(position.y - bottomY, loopHeight);
        }

        transform.position = position;
    }
}


