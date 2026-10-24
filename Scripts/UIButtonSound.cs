using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour, IPointerDownHandler
{
    private Button targetButton;

    private void Awake()
    {
        targetButton = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left ||
            !targetButton.IsInteractable())
        {
            return;
        }

        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayButtonClick();
    }
}


