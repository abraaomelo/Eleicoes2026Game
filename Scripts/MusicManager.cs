using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Músicas")]
    [SerializeField] private AudioClip menuSong;
    [SerializeField] private AudioClip gameplaySong;

    [Header("Configurações")]
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f;
    [SerializeField] private string gameplaySceneName = "InfiniteRunner";

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.volume = volume;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        UpdateMusic(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateMusic(scene.name);
    }

    private void UpdateMusic(string sceneName)
    {
        AudioClip nextSong = sceneName == gameplaySceneName
            ? gameplaySong
            : menuSong;

        if (nextSong == null)
        {
            audioSource.Stop();
            audioSource.clip = null;
            return;
        }

        // A mesma música continua tocando entre cenas.
        if (audioSource.clip == nextSong && audioSource.isPlaying)
            return;

        audioSource.Stop();
        audioSource.clip = nextSong;
        audioSource.Play();
    }

    private void OnValidate()
    {
        if (audioSource != null)
            audioSource.volume = volume;
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }
}

