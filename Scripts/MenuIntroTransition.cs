using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuIntroTransition : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Button menuButton;
    [SerializeField] private Button frontButton;
    [SerializeField] private CanvasGroup instructionsGroup;
    [SerializeField] private RectTransform buttonTarget;

    [Header("Fade")]
    [Tooltip("Tempo para as instruções aparecerem, em segundos.")]
    [SerializeField, Min(0f)]
    private float fadeDuration = 0.6f;

    [Header("Movimento")]
    [Tooltip("Velocidade em unidades do Canvas por segundo.")]
    [SerializeField, Min(1f)]
    private float buttonMoveSpeed = 600f;

    private bool started;

    private void Awake()
    {
        if (menuButton != null)
        {
            menuButton.gameObject.SetActive(true);
            menuButton.interactable = true;
        }

        if (frontButton != null)
        {
            frontButton.interactable = false;
            frontButton.gameObject.SetActive(false);
        }

        if (instructionsGroup != null)
        {
            instructionsGroup.alpha = 0f;
            instructionsGroup.interactable = false;
            instructionsGroup.blocksRaycasts = false;
            instructionsGroup.gameObject.SetActive(false);
        }
    }

    public void ShowInstructions()
    {
        if (started)
            return;

        if (menuButton == null ||
            frontButton == null ||
            instructionsGroup == null ||
            buttonTarget == null)
        {
            Debug.LogError(
                "Preencha todas as referências do MenuIntroTransition."
            );

            return;
        }

        started = true;
        StartCoroutine(ShowInstructionsRoutine());
    }

    private IEnumerator ShowInstructionsRoutine()
    {
        RectTransform menuRect =
            menuButton.GetComponent<RectTransform>();

        RectTransform frontRect =
            frontButton.GetComponent<RectTransform>();

        // Coloca a cópia exatamente onde estava o botão original.
        frontRect.position = menuRect.position;

        instructionsGroup.alpha = 0f;
        instructionsGroup.interactable = false;
        instructionsGroup.blocksRaycasts = true;
        instructionsGroup.gameObject.SetActive(true);

        // Mantém o painel e o botão na frente dos demais elementos.
        instructionsGroup.transform.SetAsLastSibling();
        frontButton.transform.SetAsLastSibling();

        frontButton.interactable = false;
        frontButton.gameObject.SetActive(true);
        menuButton.gameObject.SetActive(false);

        // Primeiro: fade das instruções.
        float elapsed = 0f;
        float duration = Mathf.Max(0f, fadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            instructionsGroup.alpha =
                Mathf.SmoothStep(0f, 1f, progress);

            yield return null;
        }

        instructionsGroup.alpha = 1f;

        // Depois: deslocamento até o marcador.
        while (true)
        {
            Vector3 targetPosition =
                frontRect.parent.InverseTransformPoint(
                    buttonTarget.position
                );

            frontRect.localPosition = Vector3.MoveTowards(
                frontRect.localPosition,
                targetPosition,
                Mathf.Max(1f, buttonMoveSpeed)
                    * Time.unscaledDeltaTime
            );

            if (Vector3.Distance(
                    frontRect.localPosition,
                    targetPosition) < 0.1f)
            {
                frontRect.localPosition = targetPosition;
                break;
            }

            yield return null;
        }

        instructionsGroup.interactable = true;
        frontButton.interactable = true;
    }
}

