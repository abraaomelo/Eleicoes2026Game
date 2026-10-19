using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum EnemyType
    {
        Yellow,
        Green,
        Purple,
        Blue
    }

    [SerializeField] private EnemyType type;

    private GameController gameController;

    public EnemyType Type => type;

    public void Initialize(GameController controller)
    {
        gameController = controller;
    }

    private void Update()
    {
        if (gameController == null)
            return;

        transform.position += Vector3.down
            * gameController.ScrollSpeed
            * Time.deltaTime;

        if (transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }
}


