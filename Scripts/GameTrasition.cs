using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameTransition : MonoBehaviour
{
    public enum Style
    {
        Fade,
        Shutter
    }

    public static GameTransition Instance { get; private set; }
    public static bool IsActive { get; private set; }

    public static event Action SceneReady;

    [Header("Duração de cada metade da transição")]
    [SerializeField, Min(0.01f)]
    private float fadeDuration = 0.35f;

    [SerializeField, Min(0.01f)]
    private float shutterDuration = 0.12f;

    [Header("Aparência")]
    [SerializeField] private Color transitionColor = Color.black;

    private GameObject overlay;
    private Image fadeImage;
    private Image leftCurtain;
    private Image rightCurtain;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
        IsActive = false;
        SceneReady = null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreateOverlay();
    }

    private void CreateOverlay()
    {
        overlay = new GameObject(
            "TransitionOverlay",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(GraphicRaycaster)
        );

        overlay.transform.SetParent(transform, false);

        Canvas canvas = overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32760;

        Image blocker = CreateImage(
            "InputBlocker",
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f)
        );

        blocker.color = Color.clear;
        blocker.raycastTarget = true;

        fadeImage = CreateImage(
            "Fade",
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f)
        );

        leftCurtain = CreateImage(
            "LeftCurtain",
            Vector2.zero,
            new Vector2(0.5f, 1f),
            new Vector2(0f, 0.5f)
        );

        rightCurtain = CreateImage(
            "RightCurtain",
            new Vector2(0.5f, 0f),
            Vector2.one,
            new Vector2(1f, 0.5f)
        );

        overlay.SetActive(false);
    }

    private Image CreateImage(
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image)
        );

        obj.transform.SetParent(overlay.transform, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.pivot = pivot;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = obj.GetComponent<Image>();
        image.color = transitionColor;
        image.raycastTarget = false;

        return image;
    }

    public bool LoadScene(
        string sceneName,
        Style style,
        float minimumBeforeLoad = 0f)
    {
        if (IsActive)
            return false;

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"Cena '{sceneName}' não está disponível na build."
            );

            return false;
        }

        IsActive = true;

        StartCoroutine(
            TransitionRoutine(sceneName, style, minimumBeforeLoad)
        );

        return true;
    }

    private IEnumerator TransitionRoutine(
        string sceneName,
        Style style,
        float minimumBeforeLoad)
    {
        float startedAt = Time.realtimeSinceStartup;

        fadeImage.gameObject.SetActive(style == Style.Fade);
        leftCurtain.gameObject.SetActive(style == Style.Shutter);
        rightCurtain.gameObject.SetActive(style == Style.Shutter);

        ApplyProgress(style, 0f);
        overlay.SetActive(true);

        float duration = style == Style.Fade
            ? fadeDuration
            : shutterDuration;

        yield return Animate(style, true, duration);

        // Na entrada, mantém a tela coberta se o som da urna
        // ainda não teve tempo de terminar.
        float remaining = minimumBeforeLoad
            - (Time.realtimeSinceStartup - startedAt);

        if (remaining > 0f)
            yield return new WaitForSecondsRealtime(remaining);

        yield return SceneManager.LoadSceneAsync(sceneName);

        yield return Animate(style, false, duration);

        overlay.SetActive(false);
        IsActive = false;

        SceneReady?.Invoke();
    }

    private IEnumerator Animate(
        Style style,
        bool closing,
        float duration)
    {
        float elapsed = 0f;
        duration = Mathf.Max(0.01f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(elapsed / duration)
            );

            ApplyProgress(
                style,
                closing ? progress : 1f - progress
            );

            yield return null;
        }

        ApplyProgress(style, closing ? 1f : 0f);
    }

    private void ApplyProgress(Style style, float progress)
    {
        if (style == Style.Fade)
        {
            Color color = transitionColor;
            color.a = progress;
            fadeImage.color = color;
        }
        else
        {
            leftCurtain.rectTransform.localScale =
                new Vector3(progress, 1f, 1f);

            rightCurtain.rectTransform.localScale =
                new Vector3(progress, 1f, 1f);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}

