using UnityEngine;

/// <summary>
/// Generates procedural water splash / whip audio clips at runtime
/// and provides a static Play method for fire-and-forget use.
/// </summary>
public static class WaterSplashAudio
{
    private static AudioClip whipClip;
    private static AudioClip splashClip;
    private static AudioClip iceSpearThrowClip;
    private static AudioClip iceShatterClip;

    private const int SampleRate = 44100;
    private const float WhipDuration = 0.18f;
    private const float SplashDuration = 0.22f;
    private const float IceSpearThrowDuration = 0.2f;
    private const float IceShatterDuration = 0.24f;
    private const float MasterVolume = 0.35f;

    /// <summary>
    /// Play the water whip sound (horizontal attack).
    /// </summary>
    public static void PlayWhip(Vector3 position)
    {
        if (whipClip == null)
        {
            whipClip = GenerateWhipClip();
        }

        AudioSource.PlayClipAtPoint(whipClip, position, MasterVolume);
    }

    /// <summary>
    /// Play the water splash sound (downward stomp attack).
    /// </summary>
    public static void PlaySplash(Vector3 position)
    {
        if (splashClip == null)
        {
            splashClip = GenerateSplashClip();
        }

        AudioSource.PlayClipAtPoint(splashClip, position, MasterVolume);
    }

    public static void PlayIceSpearThrow(Vector3 position)
    {
        if (iceSpearThrowClip == null)
        {
            iceSpearThrowClip = GenerateIceSpearThrowClip();
        }

        AudioSource.PlayClipAtPoint(iceSpearThrowClip, position, MasterVolume * 0.9f);
    }

    public static void PlayIceShatter(Vector3 position)
    {
        if (iceShatterClip == null)
        {
            iceShatterClip = GenerateIceShatterClip();
        }

        AudioSource.PlayClipAtPoint(iceShatterClip, position, MasterVolume * 0.72f);
    }

    private static AudioClip GenerateWhipClip()
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * WhipDuration);
        float[] samples = new float[sampleCount];

        // Water whip: descending frequency sweep with noise, mimicking a fast lash through water
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount; // 0..1 progress
            float envelope = (1f - t) * (1f - t); // quadratic decay

            // Frequency sweeps downward from ~2200 Hz to ~400 Hz
            float freq = Mathf.Lerp(2200f, 400f, t);
            float phase = 2f * Mathf.PI * freq * i / SampleRate;

            // Mix a sine tone with filtered noise for a watery texture
            float tone = Mathf.Sin(phase) * 0.45f;
            float noise = (Random.value * 2f - 1f) * 0.55f;

            // Simple low-pass effect: blend with previous sample
            float raw = (tone + noise) * envelope;
            if (i > 0)
            {
                raw = samples[i - 1] * 0.3f + raw * 0.7f;
            }

            samples[i] = Mathf.Clamp(raw, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("WaterWhip", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip GenerateSplashClip()
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * SplashDuration);
        float[] samples = new float[sampleCount];

        // Water splash: burst of noise with resonant low thump, like hitting water from above
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;

            // Sharp attack, moderate decay
            float envelope = Mathf.Pow(1f - t, 1.5f);
            // Extra initial punch
            float attackBoost = t < 0.08f ? Mathf.Lerp(1.6f, 1f, t / 0.08f) : 1f;

            // Low resonant thump (~180 Hz decaying)
            float thumpFreq = Mathf.Lerp(180f, 90f, t);
            float thump = Mathf.Sin(2f * Mathf.PI * thumpFreq * i / SampleRate) * 0.4f;

            // Mid splash texture (~600-1400 Hz band noise)
            float noise = (Random.value * 2f - 1f);
            float midFreq = Mathf.Lerp(1400f, 600f, t);
            float midTone = Mathf.Sin(2f * Mathf.PI * midFreq * i / SampleRate);
            float splash = noise * 0.35f + midTone * noise * 0.25f;

            // High sparkle (bubbly overtones)
            float highFreq = Mathf.Lerp(3200f, 1800f, t);
            float sparkle = Mathf.Sin(2f * Mathf.PI * highFreq * i / SampleRate) * (1f - t) * 0.15f;

            float raw = (thump + splash + sparkle) * envelope * attackBoost;

            // Low-pass smoothing
            if (i > 0)
            {
                raw = samples[i - 1] * 0.25f + raw * 0.75f;
            }

            samples[i] = Mathf.Clamp(raw, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("WaterSplash", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip GenerateIceSpearThrowClip()
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * IceSpearThrowDuration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float envelope = Mathf.Pow(1f - t, 1.35f);
            float sweepFrequency = Mathf.Lerp(620f, 2600f, t);
            float glassFrequency = Mathf.Lerp(1800f, 3200f, t);
            float sweep = Mathf.Sin(2f * Mathf.PI * sweepFrequency * i / SampleRate) * 0.36f;
            float glass = Mathf.Sin(2f * Mathf.PI * glassFrequency * i / SampleRate) * 0.18f;
            float air = (Random.value * 2f - 1f) * 0.22f * (1f - t);
            float raw = (sweep + glass + air) * envelope;

            if (i > 0)
            {
                raw = samples[i - 1] * 0.18f + raw * 0.82f;
            }

            samples[i] = Mathf.Clamp(raw, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("IceSpearThrow", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip GenerateIceShatterClip()
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * IceShatterDuration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float envelope = Mathf.Pow(1f - t, 2.2f);
            float crack = (Random.value * 2f - 1f) * envelope * 0.55f;
            float bellA = Mathf.Sin(2f * Mathf.PI * 2300f * i / SampleRate) * Mathf.Exp(-t * 10f) * 0.18f;
            float bellB = Mathf.Sin(2f * Mathf.PI * 3450f * i / SampleRate) * Mathf.Exp(-t * 15f) * 0.14f;
            float tick = t < 0.04f ? Mathf.Sin(2f * Mathf.PI * 5200f * i / SampleRate) * (1f - t / 0.04f) * 0.24f : 0f;
            float raw = crack + bellA + bellB + tick;

            if (i > 0)
            {
                raw = samples[i - 1] * 0.1f + raw * 0.9f;
            }

            samples[i] = Mathf.Clamp(raw, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("IceSpearShatter", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
