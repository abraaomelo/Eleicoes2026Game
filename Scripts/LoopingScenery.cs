using UnityEngine;

public class LoopingScenery : MonoBehaviour
{
    public enum SceneryCategory
    {
        Roadside,
        Road
    }

    [Header("Referências")]
    [SerializeField] private GameController gameController;

    [Header("Categoria de movimento")]
    [SerializeField] private SceneryCategory category;

    [Header("Limites de reciclagem")]
    [SerializeField] private float bottomY = -6f;
    [SerializeField] private float topY = 6f;

    private void Update()
    {
        if (gameController == null || topY <= bottomY)
            return;

        float speed = category == SceneryCategory.Road
            ? gameController.RoadSpeed
            : gameController.RoadsideSpeed;

        Vector3 position = transform.position;

        position.y -= speed * Time.deltaTime;

        if (position.y < bottomY)
        {
            float loopHeight = topY - bottomY;

            position.y = bottomY
                + Mathf.Repeat(position.y - bottomY, loopHeight);
        }

        transform.position = position;
    }
}


