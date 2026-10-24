using System;
using UnityEngine;

public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    [Serializable]
    public class Sound
    {
        public AudioClip clip;

        [Range(0f, 1f)]
        public float volume = 0.5f;
    }

    [Header("Volume geral dos efeitos")]
    [SerializeField, Range(0f, 1f)]
    private float effectsVolume = 0.8f;

    [Header("Corrida")]
    [SerializeField] private Sound normalRunning = new Sound();
    [SerializeField] private Sound fastRunning = new Sound();

    [Header("Troca de lane e interface")]
    [SerializeField] private Sound laneChange = new Sound();
    [SerializeField] private Sound buttonClick = new Sound();

    [Header("Coleta")]
    [SerializeField] private Sound leftProposal = new Sound();
    [SerializeField] private Sound rightProposal = new Sound();

    [Header("Sequências especiais")]
    [SerializeField] private Sound electronicUrn = new Sound();
    [SerializeField] private Sound collision = new Sound();

    [Header("Mistura dos sons de corrida")]
    [Tooltip("Velocidade da rua em que o som rápido começa a entrar.")]
    [SerializeField, Min(0f)]
    private float fastStartSpeed = 3.5f;

    [Tooltip("Velocidade em que fica somente o som rápido.")]
    [SerializeField, Min(0f)]
    private float fastFullSpeed = 4.5f;

    private AudioSource effectsSource;
    private AudioSource normalRunningSource;
    private AudioSource fastRunningSource;
    private AudioSource sequenceSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        effectsSource = CreateSource(false);
        normalRunningSource = CreateSource(true);
        fastRunningSource = CreateSource(true);
        sequenceSource = CreateSource(false);
    }

    private AudioSource CreateSource(bool loop)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;

        return source;
    }

    public void SetRunningSpeed(float speed)
    {
        if (speed <= 0f)
        {
            StopRunning();
            return;
        }

        float blend = Mathf.InverseLerp(
            fastStartSpeed,
            Mathf.Max(fastStartSpeed + 0.01f, fastFullSpeed),
            speed
        );

        // Se algum arquivo estiver vazio, usa o que estiver disponível.
        if (fastRunning.clip == null)
            blend = 0f;
        else if (normalRunning.clip == null)
            blend = 1f;

        UpdateLoop(
            normalRunningSource,
            normalRunning,
            1f - blend
        );

        UpdateLoop(
            fastRunningSource,
            fastRunning,
            blend
        );
    }

    private void UpdateLoop(
        AudioSource source,
        Sound sound,
        float blend)
    {
        float volume = effectsVolume * sound.volume * blend;

        if (sound.clip == null || volume <= 0f)
        {
            source.Stop();
            return;
        }

        if (source.clip != sound.clip)
        {
            source.Stop();
            source.clip = sound.clip;
        }

        source.volume = volume;

        if (!source.isPlaying)
            source.Play();
    }

    public void StopRunning()
    {
        if (normalRunningSource != null)
            normalRunningSource.Stop();

        if (fastRunningSource != null)
            fastRunningSource.Stop();
    }

    private void PlayEffect(Sound sound)
    {
        if (sound.clip == null)
            return;

        effectsSource.volume = effectsVolume;
        effectsSource.PlayOneShot(sound.clip, sound.volume);
    }

    public void PlayLaneChange()
    {
        PlayEffect(laneChange);
    }

    public void PlayButtonClick()
    {
        PlayEffect(buttonClick);
    }

    public void PlayProposal(
        RoadItem.ProposalSide side,
        AudioClip exclusiveClip = null,
        float exclusiveVolume = 0.5f)
    {
        if (exclusiveClip != null)
        {
            effectsSource.volume = effectsVolume;

            effectsSource.PlayOneShot(
                exclusiveClip,
                Mathf.Clamp01(exclusiveVolume)
            );

            return;
        }

        PlayEffect(
            side == RoadItem.ProposalSide.Left
                ? leftProposal
                : rightProposal
        );
    }

    public float PlayUrn()
    {
        return PlaySequence(electronicUrn);
    }

    public float PlayCollision()
    {
        return PlaySequence(collision);
    }

    private float PlaySequence(Sound sound)
    {
        sequenceSource.Stop();

        if (sound.clip == null)
            return 0f;

        sequenceSource.clip = sound.clip;
        sequenceSource.volume = effectsVolume * sound.volume;
        sequenceSource.Play();

        return sound.clip.length;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}

