using UnityEngine;

public class PlayerPower : MonoBehaviour
{
    [SerializeField] private SpriteRenderer visual;

    private Color originalColor;
    private int currentPower = 0;

    private void Awake()
    {
        originalColor = visual.color;
    }

    public void SwitchPower()
    {
        currentPower = (currentPower + 1) % 4;

        switch (currentPower)
        {
            case 0:
                visual.color = originalColor;
                break;

            case 1:
                visual.color = Color.green;
                break;

            case 2:
                visual.color = new Color(0.6f, 0.2f, 0.8f);
                break;

        }
    }
}

