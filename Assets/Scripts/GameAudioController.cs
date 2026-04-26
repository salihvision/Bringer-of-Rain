using System.Collections.Generic;
using UnityEngine;

public enum AudioCue
{
    PlayerStep,
    PlayerJump,
    PlayerLand,
    PlayerBurst,
    PlayerHurt,
    PlayerRespawn,
    BurstHit,
    EnemyDash,
    EnemyHit,
    EnemyDefeat,
    ValveActivated,
    GateUnlocked,
    Checkpoint,
    ExitBlocked,
    ChapterTransition,
    Victory,
    BossWindup,
    BossSlam,
    BossShockwave,
    BossHit,
    BossPhaseTwo,
    BossDefeated
}

public class GameAudioController : MonoBehaviour
{
    public const float MasterVolume = 0.85f;
    public const float SfxVolume = 0.92f;
    public const float MusicVolume = 0.65f;

    private const int PoolSize = 18;
    private const string SfxRoot = "Audio/Sfx/";

    private static GameAudioController instance;

    private readonly Dictionary<AudioCue, CueDefinition> cueDefinitions = new();
    private readonly Dictionary<AudioCue, float> nextAllowedPlayTime = new();
    private readonly HashSet<AudioCue> missingWarnings = new();
    private readonly List<AudioSource> sourcePool = new();

    private int sourceIndex;

    public static void EnsureInScene()
    {
        if (instance != null)
        {
            return;
        }

        GameObject audioObject = new("GameAudio");
        audioObject.AddComponent<GameAudioController>();
    }

    public static void Play(AudioCue cue)
    {
        EnsureInScene();
        instance?.PlayInternal(cue);
    }

    public static void PlayAt(AudioCue cue, Vector3 position)
    {
        EnsureInScene();
        instance?.PlayInternal(cue);
    }

    public static void PlayRandom(AudioCue cue)
    {
        Play(cue);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        BuildPool();
        BuildCueDefinitions();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void BuildPool()
    {
        for (int i = 0; i < PoolSize; i++)
        {
            GameObject sourceObject = new($"SfxSource_{i:00}");
            sourceObject.transform.SetParent(transform, false);

            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            sourcePool.Add(source);
        }
    }

    private void BuildCueDefinitions()
    {
        Register(AudioCue.PlayerStep, 0.26f, 0.92f, 1.08f, 0.06f,
            "Impact/footstep_concrete_000",
            "Impact/footstep_concrete_001",
            "Impact/footstep_concrete_002",
            "Impact/footstep_concrete_003",
            "Impact/footstep_concrete_004");
        Register(AudioCue.PlayerJump, 0.42f, 0.95f, 1.08f, 0.08f,
            "Digital/phaserUp1",
            "Digital/phaserUp2",
            "Digital/highUp");
        Register(AudioCue.PlayerLand, 0.4f, 0.88f, 1.04f, 0.12f,
            "Impact/impactSoft_medium_000",
            "Impact/impactSoft_medium_001",
            "Impact/impactPunch_medium_000");
        RegisterGenerated(AudioCue.PlayerBurst, CreateWaterWhipSplashClip(), 0.5f, 1f, 1f, 0.08f);
        Register(AudioCue.PlayerHurt, 0.48f, 0.84f, 1f, 0.12f,
            "Impact/impactPunch_medium_001",
            "Impact/impactPunch_medium_002",
            "Impact/impactPunch_heavy_000");
        Register(AudioCue.PlayerRespawn, 0.58f, 0.94f, 1.08f, 0.25f,
            "Digital/powerUp1",
            "Digital/powerUp2",
            "Digital/powerUp3");
        Register(AudioCue.BurstHit, 0.5f, 0.9f, 1.08f, 0.04f,
            "Impact/impactMetal_medium_000",
            "Impact/impactMetal_medium_001",
            "Impact/impactPunch_heavy_001");
        Register(AudioCue.EnemyDash, 0.38f, 0.94f, 1.1f, 0.16f,
            "Rpg/knifeSlice",
            "Rpg/knifeSlice2",
            "Digital/phaserDown1");
        Register(AudioCue.EnemyHit, 0.42f, 0.88f, 1.06f, 0.05f,
            "Impact/impactPunch_medium_003",
            "Impact/impactPunch_medium_004",
            "Impact/impactSoft_medium_002");
        Register(AudioCue.EnemyDefeat, 0.56f, 0.82f, 0.98f, 0.08f,
            "Impact/impactMetal_heavy_000",
            "Impact/impactMetal_heavy_001",
            "Impact/impactMining_000");
        Register(AudioCue.ValveActivated, 0.58f, 0.95f, 1.08f, 0.25f,
            "Interface/switch_001",
            "Interface/switch_002",
            "Rpg/metalLatch",
            "Digital/powerUp4");
        Register(AudioCue.GateUnlocked, 0.7f, 0.9f, 1.02f, 0.4f,
            "Rpg/doorOpen_1",
            "Rpg/doorOpen_2",
            "Interface/open_001");
        Register(AudioCue.Checkpoint, 0.56f, 0.96f, 1.08f, 0.25f,
            "Interface/confirmation_001",
            "Interface/confirmation_002",
            "Digital/powerUp2");
        Register(AudioCue.ExitBlocked, 0.44f, 0.88f, 1f, 0.35f,
            "Interface/error_001",
            "Interface/error_002",
            "Digital/lowDown");
        Register(AudioCue.ChapterTransition, 0.62f, 0.92f, 1.04f, 0.35f,
            "Interface/open_002",
            "Interface/open_003",
            "Digital/phaserUp1");
        Register(AudioCue.Victory, 0.72f, 0.96f, 1.05f, 0.5f,
            "Interface/confirmation_003",
            "Interface/confirmation_004",
            "Digital/powerUp4");
        Register(AudioCue.BossWindup, 0.48f, 0.78f, 0.94f, 0.35f,
            "Digital/phaserDown2",
            "Digital/lowDown",
            "Rpg/metalClick");
        Register(AudioCue.BossSlam, 0.8f, 0.78f, 0.92f, 0.2f,
            "Impact/impactMetal_heavy_002",
            "Impact/impactMetal_heavy_003",
            "Impact/impactMining_001");
        Register(AudioCue.BossShockwave, 0.36f, 0.86f, 1.02f, 0.08f,
            "Digital/zapTwoTone",
            "Digital/phaserDown1",
            "Digital/phaserDown2");
        Register(AudioCue.BossHit, 0.58f, 0.82f, 0.98f, 0.05f,
            "Impact/impactMetal_medium_002",
            "Impact/impactMetal_medium_003",
            "Impact/impactPunch_heavy_002");
        Register(AudioCue.BossPhaseTwo, 0.76f, 0.82f, 0.98f, 0.4f,
            "Impact/impactMetal_heavy_004",
            "Digital/powerUp3",
            "Digital/phaserUp2");
        Register(AudioCue.BossDefeated, 0.82f, 0.72f, 0.9f, 0.5f,
            "Impact/impactMining_002",
            "Impact/impactMining_003",
            "Rpg/chop");
    }

    private void Register(AudioCue cue, float volume, float pitchMin, float pitchMax, float cooldown, params string[] clipPaths)
    {
        List<AudioClip> clips = new();
        foreach (string clipPath in clipPaths)
        {
            AudioClip clip = Resources.Load<AudioClip>(SfxRoot + clipPath);
            if (clip != null)
            {
                clips.Add(clip);
            }
        }

        cueDefinitions[cue] = new CueDefinition(clips.ToArray(), volume, pitchMin, pitchMax, cooldown);
    }

    private void RegisterGenerated(AudioCue cue, AudioClip clip, float volume, float pitchMin, float pitchMax, float cooldown)
    {
        cueDefinitions[cue] = new CueDefinition(new[] { clip }, volume, pitchMin, pitchMax, cooldown);
    }

    private static AudioClip CreateWaterWhipSplashClip()
    {
        const int sampleRate = 44100;
        const float duration = 0.24f;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];
        System.Random random = new(1337);

        float previousNoise = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float progress = t / duration;
            float snapEnvelope = Mathf.Exp(-progress * 18f);
            float sprayEnvelope = Mathf.Clamp01(1f - progress);
            sprayEnvelope *= sprayEnvelope * Mathf.SmoothStep(0f, 1f, progress * 9f);

            float rawNoise = (float)(random.NextDouble() * 2.0 - 1.0);
            previousNoise = Mathf.Lerp(previousNoise, rawNoise, 0.38f);
            float spray = previousNoise * sprayEnvelope * 0.62f;

            float lowSplash = Mathf.Sin(2f * Mathf.PI * 145f * t) * Mathf.Exp(-progress * 8f) * 0.26f;
            float snap = Mathf.Sin(2f * Mathf.PI * 520f * t) * snapEnvelope * 0.2f;
            samples[i] = Mathf.Clamp((spray + lowSplash + snap) * 0.8f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("Generated_WaterWhipSplash", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void PlayInternal(AudioCue cue)
    {
        if (!cueDefinitions.TryGetValue(cue, out CueDefinition definition) || definition.Clips.Length == 0)
        {
            WarnMissingCue(cue);
            return;
        }

        if (nextAllowedPlayTime.TryGetValue(cue, out float nextTime) && Time.unscaledTime < nextTime)
        {
            return;
        }

        nextAllowedPlayTime[cue] = Time.unscaledTime + definition.Cooldown;

        AudioClip clip = definition.Clips[Random.Range(0, definition.Clips.Length)];
        AudioSource source = NextSource();
        source.clip = clip;
        source.volume = definition.Volume * MasterVolume * SfxVolume;
        source.pitch = Random.Range(definition.PitchMin, definition.PitchMax);
        source.Play();
    }

    private AudioSource NextSource()
    {
        AudioSource source = sourcePool[sourceIndex];
        sourceIndex = (sourceIndex + 1) % sourcePool.Count;
        return source;
    }

    private void WarnMissingCue(AudioCue cue)
    {
        if (missingWarnings.Add(cue))
        {
            Debug.LogWarning($"Audio cue {cue} has no loaded clips.");
        }
    }

    private readonly struct CueDefinition
    {
        public CueDefinition(AudioClip[] clips, float volume, float pitchMin, float pitchMax, float cooldown)
        {
            Clips = clips;
            Volume = volume;
            PitchMin = pitchMin;
            PitchMax = pitchMax;
            Cooldown = cooldown;
        }

        public AudioClip[] Clips { get; }
        public float Volume { get; }
        public float PitchMin { get; }
        public float PitchMax { get; }
        public float Cooldown { get; }
    }
}
