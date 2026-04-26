using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameStateController : MonoBehaviour
{
    private enum StoryPhase
    {
        ChapterOne,
        Transitioning,
        ChapterTwo,
        TransitioningChapterThree,
        ChapterThree,
        Finished
    }

    private readonly List<GameObject> restoreObjects = new();
    private static readonly Color UiTextColor = new(0.09f, 0.11f, 0.13f, 1f);
    private static readonly Color StoryTextColor = new(0.95f, 0.89f, 0.68f, 1f);
    private static readonly Color StoryShadowColor = new(0.02f, 0.02f, 0.03f, 0.95f);
    private static readonly Color RainColor = new(0.42f, 0.88f, 1f, 0.62f);
    private const string DefaultHintText = "Movement: WASD\nWater Whip: LMB1 / Enter / F\nBurst Below: S + LMB1 / Enter / F (while in air)\nIce Shard: LMB2";
    private const float TypewriterCharactersPerSecond = 46f;

    private PlayerController player;
    private string healthText = "HP 6/6";
    private string manaText = "MP 3/3";
    private string objectiveText = string.Empty;
    private string centerMessage = string.Empty;
    private string hintText = DefaultHintText;
    private string interactionPrompt = string.Empty;
    private string readableStoryTitle = string.Empty;
    private string readableStoryBody = string.Empty;
    private string trophyTitle = string.Empty;
    private string trophySubtitle = string.Empty;

    private GUIStyle hudStyle;
    private GUIStyle hintStyle;
    private GUIStyle centerStyle;
    private GUIStyle panelStyle;
    private GUIStyle fullScreenOverlayStyle;
    private GUIStyle storyTitleStyle;
    private GUIStyle storyBodyStyle;
    private GUIStyle storyFooterStyle;
    private GUIStyle promptStyle;
    private GUIStyle trophyTitleStyle;
    private GUIStyle trophyBodyStyle;
    private Texture2D rainTexture;
    private Font storyFont;

    private Texture2D healthBarTexture;
    private Texture2D manaBarTexture;
    private int currentHealth = 6;
    private int maxHealth = 6;
    private int currentMana = 3;
    private int maxMana = 3;

    private Vector3 checkpointPosition;
    private Vector3 chapterTwoSpawnPoint;
    private Vector3 chapterThreeSpawnPoint;
    private Vector2 chapterThreeCameraLimits;
    private GameObject gateBarrier;
    private GameObject chapterTwoRoot;
    private GameObject chapterThreeRoot;
    private StoryTrigger activeReadableTrigger;
    private int totalValves;
    private int activatedValves;
    private int readableVisibleCharacterCount;
    private float centerMessageExpiresAt;
    private float transitionTeleportAt;
    private float readableStoryStartedAt;
    private float rainStartedAt;
    private float trophyExpiresAt;

    private bool introVisible;
    private bool showCenterMessage;
    private bool readableStoryVisible;
    private bool readableStoryFullyRevealed;
    private bool readableStoryLocksInput;
    private bool bossDefeated;
    private bool rainActive;
    private bool trophyVisible;
    private bool trophyShown;
    private StoryPhase storyPhase;

    public bool CanExit { get; private set; }

    public void Initialize(int valveCount, Vector3 initialCheckpoint)
    {
        totalValves = valveCount;
        checkpointPosition = initialCheckpoint;
        storyPhase = StoryPhase.ChapterOne;
        BuildUi();
        UpdateObjective();
    }

    public void SetPlayer(PlayerController controlledPlayer)
    {
        player = controlledPlayer;
        UpdateHealth(player.CurrentHealth, player.MaxHealth);
        UpdateMana(player.CurrentManaFragments, player.MaxManaFragments);

        if (readableStoryVisible && readableStoryLocksInput)
        {
            player.SetInputLocked(true);
        }
    }

    public void SetReadableTarget(StoryTrigger trigger, string prompt)
    {
        if (trigger == null || ReadableStoriesBlocked)
        {
            return;
        }

        activeReadableTrigger = trigger;
        interactionPrompt = string.IsNullOrWhiteSpace(prompt) ? "Press W to read" : prompt;
    }

    public void ClearReadableTarget(StoryTrigger trigger)
    {
        if (activeReadableTrigger != trigger)
        {
            return;
        }

        activeReadableTrigger = null;
        interactionPrompt = string.Empty;
    }

    public void ShowReadableStory(string title, string body, bool lockPlayerInput)
    {
        HideCenterText();

        readableStoryTitle = title ?? string.Empty;
        readableStoryBody = body ?? string.Empty;
        readableStoryVisible = true;
        readableStoryFullyRevealed = readableStoryBody.Length == 0;
        readableVisibleCharacterCount = readableStoryFullyRevealed ? readableStoryBody.Length : 0;
        readableStoryStartedAt = Time.unscaledTime;
        readableStoryLocksInput = lockPlayerInput;

        if (lockPlayerInput)
        {
            player?.SetInputLocked(true);
        }
    }

    public void ConfigureProgression(GameObject gateToDisable, IEnumerable<GameObject> objectsToEnable)
    {
        gateBarrier = gateToDisable;
        restoreObjects.Clear();
        restoreObjects.AddRange(objectsToEnable);
    }

    public void ConfigureChapterTwo(GameObject chapterRootObject, Vector3 spawnPoint)
    {
        chapterTwoRoot = chapterRootObject;
        chapterTwoSpawnPoint = spawnPoint;
    }

    public void ConfigureChapterThree(GameObject chapterRootObject, Vector3 spawnPoint, Vector2 cameraLimits)
    {
        chapterThreeRoot = chapterRootObject;
        chapterThreeSpawnPoint = spawnPoint;
        chapterThreeCameraLimits = cameraLimits;
    }

    public void UpdateHealth(int currentHealthValue, int maxHealthValue)
    {
        currentHealth = currentHealthValue;
        maxHealth = maxHealthValue;
        healthText = $"HP {currentHealth}/{maxHealth}";
    }

    public void UpdateMana(int currentManaFragments, int maxManaFragments)
    {
        currentMana = currentManaFragments / 3;
        maxMana = maxManaFragments / 3;
        manaText = $"MP {currentMana}/{maxMana}";
    }

    public void RegisterValveActivated(string valveLabel)
    {
        activatedValves = Mathf.Clamp(activatedValves + 1, 0, totalValves);
        UpdateObjective();
        ShowTransientMessage($"{valveLabel} restored.", 2.25f);

        if (activatedValves >= totalValves)
        {
            UnlockAqueduct();
        }
    }

    public void SetCheckpoint(Vector3 newCheckpoint)
    {
        if (Vector3.Distance(checkpointPosition, newCheckpoint) < 0.05f)
        {
            return;
        }

        checkpointPosition = newCheckpoint;
        ShowTransientMessage("Checkpoint renewed.", 2.1f);
    }

    public void RespawnPlayer(PlayerController targetPlayer)
    {
        if (targetPlayer == null)
        {
            return;
        }

        targetPlayer.SetSpawnPoint(checkpointPosition);
        targetPlayer.TeleportToSpawn();
        targetPlayer.RestoreFullHealth();
        ShowTransientMessage("The tide-bearer rises again.", 2f);
    }

    public void NotifyPlayerActivity()
    {
        if (readableStoryVisible)
        {
            return;
        }

        if (introVisible || (showCenterMessage && storyPhase != StoryPhase.Finished))
        {
            HideCenterText();
        }
    }

    public void ShowStoryMessage(string message, float duration)
    {
        if (storyPhase == StoryPhase.Finished)
        {
            return;
        }

        centerMessage = message;
        showCenterMessage = true;
        introVisible = false;
        centerMessageExpiresAt = Time.unscaledTime + duration;
    }

    public void ShowTransientMessage(string message, float duration)
    {
        ShowStoryMessage(message, duration);
    }

    public void ReachReservoirGate()
    {
        if (!CanExit)
        {
            GameAudioController.Play(AudioCue.ExitBlocked);
            ShowTransientMessage("Restore the twin valves to awaken the reservoir gate.", 2.6f);
            return;
        }

        if (storyPhase != StoryPhase.ChapterOne)
        {
            return;
        }

        BeginChapterTwoTransition();
    }

    public void ReachTidalSeal()
    {
        if (storyPhase != StoryPhase.ChapterTwo)
        {
            return;
        }

        BeginChapterThreeTransition();
    }

    public void NotifyBossDefeated()
    {
        if (storyPhase != StoryPhase.ChapterThree)
        {
            return;
        }

        bossDefeated = true;
        StartRain();
        ShowTrophy("BRINGER OF RAIN", "Rain returned.");
        FinishRainArc();
    }

    public void CompleteGame()
    {
        if (storyPhase == StoryPhase.Finished)
        {
            return;
        }

        bossDefeated = true;
        StartRain();
        ShowTrophy("BRINGER OF RAIN", "Rain returned.");
        FinishRainArc();
    }

    private void FinishRainArc()
    {
        storyPhase = StoryPhase.Finished;
        GameAudioController.Play(AudioCue.Victory);
        objectiveText = "Objective: Rain Restored";
        hintText = "Rain has returned. Press Play again to restart.";
        player?.SetInputLocked(false);
        player?.SetVisualHidden(false);
        ShowReadableStory(
            "BRINGER OF RAIN",
            "The demon falls.\n\n" +
            "Clouds gather over the aqueduct. The sealed wells open, and the first rain reaches the village.",
            true);
    }

    private void Update()
    {
        if (storyPhase == StoryPhase.Transitioning && Time.unscaledTime >= transitionTeleportAt)
        {
            EnterChapterTwo();
        }
        else if (storyPhase == StoryPhase.TransitioningChapterThree && Time.unscaledTime >= transitionTeleportAt)
        {
            EnterChapterThree();
        }

        if (readableStoryVisible)
        {
            TickReadableStory();
        }
        else if (!ReadableStoriesBlocked && activeReadableTrigger != null && WasReadPressedThisFrame())
        {
            activeReadableTrigger.Open();
        }

        if (trophyVisible && Time.unscaledTime >= trophyExpiresAt)
        {
            trophyVisible = false;
        }

        if (storyPhase != StoryPhase.Finished &&
            storyPhase != StoryPhase.Transitioning &&
            storyPhase != StoryPhase.TransitioningChapterThree &&
            showCenterMessage &&
            !introVisible &&
            Time.unscaledTime >= centerMessageExpiresAt)
        {
            HideCenterText();
        }
    }

    private void OnGUI()
    {
        EnsureStyles();

        if (rainActive)
        {
            DrawRainOverlay();
        }

        if (healthBarTexture == null) healthBarTexture = Resources.Load<Texture2D>("UI/health_bar");
        if (manaBarTexture == null) manaBarTexture = Resources.Load<Texture2D>("UI/manabar");

        // UI Panel for objective
        GUI.Box(new Rect(12f, 12f, 600f, 60f), GUIContent.none, panelStyle);
        GUI.Label(new Rect(24f, 20f, 580f, 44f), objectiveText, hudStyle);

        const float barX = 24f;
        const float hpY = 20f;
        float hpHeight = 0f;
        float barGap = 0f;

        // Draw Health Bar
        if (healthBarTexture != null)
        {
            float hpWidth = 480f;
            hpHeight = hpWidth * (healthBarTexture.height / (float)healthBarTexture.width);

            int hpIndex = Mathf.Clamp(currentHealth, 0, 6);
            float[] hpClipWidths = { 320f, 420f, 520f, 620f, 720f, 820f, 1000f };
            float currentHpPercent = hpClipWidths[hpIndex] / 1000f;

            GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            GUI.DrawTexture(new Rect(barX, hpY, hpWidth, hpHeight), healthBarTexture);

            GUI.color = Color.white;
            GUI.BeginGroup(new Rect(barX, hpY, hpWidth * currentHpPercent, hpHeight));
            GUI.DrawTexture(new Rect(0f, 0f, hpWidth, hpHeight), healthBarTexture);
            GUI.EndGroup();

            // Pull MP rect up into HP's transparent padding so the visible
            // star art lands just under the visible chevron art.
            barGap = -hpHeight * 0.30f;
        }

        // Draw Mana Bar (stacked directly under HP, same left edge)
        if (manaBarTexture != null)
        {
            float mpWidth = 240f;
            float mpHeight = mpWidth * (manaBarTexture.height / (float)manaBarTexture.width);
            float mpX = barX + 40f;
            float mpY = hpY + hpHeight + barGap;

            int mpIndex = Mathf.Clamp(currentMana, 0, 3);
            float[] mpClipWidths = { 0f, 185f, 300f, 454f };
            float currentMpPercent = mpClipWidths[mpIndex] / 454f;

            GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            GUI.DrawTexture(new Rect(mpX, mpY, mpWidth, mpHeight), manaBarTexture);

            GUI.color = Color.white;
            GUI.BeginGroup(new Rect(mpX, mpY, mpWidth * currentMpPercent, mpHeight));
            GUI.DrawTexture(new Rect(0f, 0f, mpWidth, mpHeight), manaBarTexture);
            GUI.EndGroup();
        }

        GUI.Box(new Rect(12f, Screen.height - 116f, 560f, 100f), GUIContent.none, panelStyle);
        GUI.Label(new Rect(24f, Screen.height - 109f, 520f, 88f), hintText, hintStyle);

        if (!readableStoryVisible && !ReadableStoriesBlocked && !string.IsNullOrWhiteSpace(interactionPrompt))
        {
            float promptWidth = Mathf.Min(300f, Screen.width - 40f);
            Rect promptRect = new((Screen.width - promptWidth) * 0.5f, Screen.height - 106f, promptWidth, 36f);
            GUI.Box(promptRect, GUIContent.none, panelStyle);
            GUI.Label(new Rect(promptRect.x + 12f, promptRect.y + 7f, promptRect.width - 24f, 24f), interactionPrompt, promptStyle);
        }

        if (!readableStoryVisible && showCenterMessage)
        {
            float width = Mathf.Min(780f, Screen.width - 80f);
            float height = Mathf.Min(260f, Screen.height - 120f);
            Rect panelRect = new((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            GUI.Box(panelRect, GUIContent.none, panelStyle);
            GUI.Label(new Rect(panelRect.x + 24f, panelRect.y + 24f, panelRect.width - 48f, panelRect.height - 48f), centerMessage, centerStyle);
        }

        if (!readableStoryVisible && trophyVisible)
        {
            DrawTrophyToast();
        }

        if (readableStoryVisible)
        {
            DrawReadableStory();
        }
    }

    private void TickReadableStory()
    {
        if (!readableStoryFullyRevealed)
        {
            readableVisibleCharacterCount = Mathf.Min(
                readableStoryBody.Length,
                Mathf.FloorToInt((Time.unscaledTime - readableStoryStartedAt) * TypewriterCharactersPerSecond));

            if (readableVisibleCharacterCount >= readableStoryBody.Length)
            {
                readableStoryFullyRevealed = true;
            }
        }

        if (!WasReadPressedThisFrame())
        {
            return;
        }

        if (!readableStoryFullyRevealed)
        {
            readableVisibleCharacterCount = readableStoryBody.Length;
            readableStoryFullyRevealed = true;
            return;
        }

        HideReadableStory();
    }

    private void HideReadableStory()
    {
        readableStoryVisible = false;
        readableStoryTitle = string.Empty;
        readableStoryBody = string.Empty;
        readableVisibleCharacterCount = 0;
        readableStoryFullyRevealed = false;

        if (readableStoryLocksInput)
        {
            player?.SetInputLocked(false);
        }

        readableStoryLocksInput = false;
    }

    private void DrawReadableStory()
    {
        GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none, fullScreenOverlayStyle);

        float horizontalMargin = Mathf.Clamp(Screen.width * 0.08f, 80f, 220f);
        Rect titleRect = new(horizontalMargin, Screen.height * 0.18f, Screen.width - horizontalMargin * 2f, Screen.height * 0.2f);

        int visibleCharacters = readableStoryFullyRevealed
            ? readableStoryBody.Length
            : Mathf.Clamp(readableVisibleCharacterCount, 0, readableStoryBody.Length);
        string visibleBody = readableStoryBody.Substring(0, visibleCharacters);

        Rect bodyRect = new(horizontalMargin, Screen.height * 0.42f, Screen.width - horizontalMargin * 2f, Screen.height * 0.32f);
        int titleSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height * 0.105f, 108f, 150f));
        int bodySize = Mathf.RoundToInt(Mathf.Clamp(Screen.height * 0.058f, 62f, 92f));
        FitTextStyle(storyTitleStyle, readableStoryTitle, titleRect, titleSize, 78);
        FitTextStyle(storyBodyStyle, readableStoryBody, bodyRect, bodySize, 48);

        DrawTextWithShadow(titleRect, readableStoryTitle, storyTitleStyle, StoryTextColor, StoryShadowColor, 5f);
        DrawTextWithShadow(bodyRect, visibleBody, storyBodyStyle, StoryTextColor, StoryShadowColor, 4f);

        string footerText = readableStoryFullyRevealed ? "Press W to close" : "Press W to reveal";
        Rect footerRect = new(0f, Screen.height - 92f, Screen.width, 42f);
        DrawTextWithShadow(footerRect, footerText, storyFooterStyle, new Color(0.78f, 0.78f, 0.72f, 1f), StoryShadowColor, 2f);
    }

    private void DrawTrophyToast()
    {
        float width = Mathf.Min(420f, Screen.width - 40f);
        Rect toastRect = new(Screen.width - width - 20f, 112f, width, 78f);
        GUI.Box(toastRect, GUIContent.none, panelStyle);
        GUI.Label(new Rect(toastRect.x + 18f, toastRect.y + 10f, toastRect.width - 36f, 32f), trophyTitle, trophyTitleStyle);
        GUI.Label(new Rect(toastRect.x + 18f, toastRect.y + 44f, toastRect.width - 36f, 24f), trophySubtitle, trophyBodyStyle);
    }

    private static void FitTextStyle(GUIStyle style, string text, Rect rect, int maxFontSize, int minFontSize)
    {
        style.fontSize = maxFontSize;
        GUIContent content = new(text);
        while (style.fontSize > minFontSize && style.CalcHeight(content, rect.width) > rect.height)
        {
            style.fontSize--;
        }
    }

    private static void DrawTextWithShadow(Rect rect, string text, GUIStyle style, Color textColor, Color shadowColor, float shadowOffset)
    {
        Color originalColor = style.normal.textColor;
        style.normal.textColor = shadowColor;
        GUI.Label(new Rect(rect.x + shadowOffset, rect.y + shadowOffset, rect.width, rect.height), text, style);
        style.normal.textColor = textColor;
        GUI.Label(rect, text, style);
        style.normal.textColor = originalColor;
    }

    private void DrawRainOverlay()
    {
        Color previousColor = GUI.color;
        float elapsed = Time.unscaledTime - rainStartedAt;
        int dropCount = Mathf.Clamp(Screen.width / 9, 72, 180);

        for (int i = 0; i < dropCount; i++)
        {
            float seed = Hash01(i);
            float speed = Mathf.Lerp(260f, 430f, Hash01(i + 41));
            float x = Mathf.Repeat(seed * Screen.width + elapsed * Mathf.Lerp(-26f, 18f, Hash01(i + 7)), Screen.width);
            float y = Mathf.Repeat(Hash01(i + 19) * (Screen.height + 120f) + elapsed * speed, Screen.height + 120f) - 80f;
            float height = Mathf.Lerp(14f, 28f, Hash01(i + 83));
            float width = Hash01(i + 131) > 0.78f ? 2f : 1f;

            GUI.color = new Color(RainColor.r, RainColor.g, RainColor.b, Mathf.Lerp(0.22f, RainColor.a, Hash01(i + 193)));
            GUI.DrawTexture(new Rect(x, y, width, height), rainTexture);
        }

        GUI.color = previousColor;
    }

    private void StartRain()
    {
        if (rainActive)
        {
            return;
        }

        rainActive = true;
        rainStartedAt = Time.unscaledTime;

        Camera camera = Camera.main;
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.56f, 0.66f, 0.75f, 1f);
        }
    }

    private void ShowTrophy(string title, string subtitle)
    {
        if (trophyShown)
        {
            return;
        }

        trophyShown = true;
        trophyVisible = true;
        trophyTitle = title;
        trophySubtitle = subtitle;
        trophyExpiresAt = Time.unscaledTime + 5.2f;
        GameAudioController.Play(AudioCue.Checkpoint);
    }

    private static bool WasReadPressedThisFrame()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.wKey.wasPressedThisFrame;
    }

    private static float Hash01(int seed)
    {
        return Mathf.Repeat(Mathf.Sin(seed * 12.9898f) * 43758.5453f, 1f);
    }

    private bool ReadableStoriesBlocked => storyPhase == StoryPhase.ChapterThree && !bossDefeated;

    private void UnlockAqueduct()
    {
        if (CanExit)
        {
            return;
        }

        CanExit = true;
        GameAudioController.Play(AudioCue.GateUnlocked);

        if (gateBarrier != null)
        {
            Collider2D barrierCollider = gateBarrier.GetComponent<Collider2D>();
            if (barrierCollider != null)
            {
                barrierCollider.enabled = false;
            }

            SpriteRenderer barrierRenderer = gateBarrier.GetComponent<SpriteRenderer>();
            if (barrierRenderer != null)
            {
                barrierRenderer.color = new Color(barrierRenderer.color.r, barrierRenderer.color.g, barrierRenderer.color.b, 0.2f);
            }
        }

        foreach (GameObject restoreObject in restoreObjects)
        {
            if (restoreObject != null)
            {
                restoreObject.SetActive(true);
            }
        }

        hintText = "Climb past the second valve and walk into the open reservoir gate.";
        UpdateObjective();
        ShowStoryMessage("Water answers the just. The reservoir gate stands open.", 4f);
    }

    private void BeginChapterTwoTransition()
    {
        storyPhase = StoryPhase.Transitioning;
        introVisible = false;
        showCenterMessage = true;
        centerMessage =
            "CHAPTER II\n\n" +
            "FLOODED VAULT\n\n" +
            "The reservoir gate gives way.\n" +
            "Stronger drowned wardens wait in the cistern below.";
        objectiveText = "Objective: Enter the flooded vault.";
        hintText = "Transitioning to Chapter II...";
        transitionTeleportAt = Time.unscaledTime + 0.55f;
        GameAudioController.Play(AudioCue.ChapterTransition);

        if (player != null)
        {
            TransitionStripAnimator.Spawn("Transitions/Desappearing96", player.transform.position + new Vector3(0f, 0.22f, -1f), 1.45f, 0.055f, 35);
            player.SetInputLocked(true);
            player.SetVisualHidden(true);
        }
    }

    private void EnterChapterTwo()
    {
        if (storyPhase != StoryPhase.Transitioning)
        {
            return;
        }

        if (chapterTwoRoot != null)
        {
            chapterTwoRoot.SetActive(true);
        }

        SimpleCameraFollow.SetHorizontalLimits(new Vector2(50f, 80f));

        checkpointPosition = chapterTwoSpawnPoint;

        if (player != null)
        {
            player.SetSpawnPoint(chapterTwoSpawnPoint);
            player.TeleportToSpawn();
            player.RestoreFullHealth();
            player.SetVisualHidden(false);
            player.SetInputLocked(false);
            TransitionStripAnimator.Spawn("Transitions/Appearing96", chapterTwoSpawnPoint + new Vector3(0f, 0.24f, -1f), 1.45f, 0.055f, 35);
        }

        storyPhase = StoryPhase.ChapterTwo;
        centerMessage =
            "CHAPTER II\n\n" +
            "FLOODED VAULT\n\n" +
            "The drowned wardens hit harder.\n" +
            "Break their line and seize the tidal seal.";
        showCenterMessage = true;
        centerMessageExpiresAt = Time.unscaledTime + 3.8f;
        hintText = "Chapter II: harder enemies deal more damage. Use air control and the long whip.";
        UpdateObjective();
    }

    private void BeginChapterThreeTransition()
    {
        storyPhase = StoryPhase.TransitioningChapterThree;
        introVisible = false;
        showCenterMessage = true;
        centerMessage =
            "CHAPTER III\n\n" +
            "WARDEN'S COURT\n\n" +
            "Past the tidal seal lies the warden of the drowned vault.\n" +
            "Strike only when its guard breaks.";
        objectiveText = "Objective: Break the warden of the drowned vault.";
        hintText = "Approaching the warden's court...";
        transitionTeleportAt = Time.unscaledTime + 0.55f;
        GameAudioController.Play(AudioCue.ChapterTransition);

        if (player != null)
        {
            TransitionStripAnimator.Spawn("Transitions/Desappearing96", player.transform.position + new Vector3(0f, 0.22f, -1f), 1.45f, 0.055f, 35);
            player.SetInputLocked(true);
            player.SetVisualHidden(true);
        }
    }

    private void EnterChapterThree()
    {
        if (storyPhase != StoryPhase.TransitioningChapterThree)
        {
            return;
        }

        if (chapterTwoRoot != null)
        {
            chapterTwoRoot.SetActive(false);
        }

        if (chapterThreeRoot != null)
        {
            chapterThreeRoot.SetActive(true);
        }

        SimpleCameraFollow.SetHorizontalLimits(chapterThreeCameraLimits);

        checkpointPosition = chapterThreeSpawnPoint;

        if (player != null)
        {
            player.SetSpawnPoint(chapterThreeSpawnPoint);
            player.TeleportToSpawn();
            player.RestoreFullHealth();
            player.SetVisualHidden(false);
            player.SetInputLocked(false);
            TransitionStripAnimator.Spawn("Transitions/Appearing96", chapterThreeSpawnPoint + new Vector3(0f, 0.24f, -1f), 1.45f, 0.055f, 35);
        }

        storyPhase = StoryPhase.ChapterThree;
        activeReadableTrigger = null;
        interactionPrompt = string.Empty;
        centerMessage =
            "CHAPTER III\n\n" +
            "WARDEN'S COURT\n\n" +
            "It is stun-immune.\n" +
            "It will only bleed when its cleave leaves it open.";
        showCenterMessage = true;
        centerMessageExpiresAt = Time.unscaledTime + 4.2f;
        hintText = "Dodge the cleave. Strike during the warden's recovery. Stay mobile.";
        UpdateObjective();
    }

    private void BuildUi()
    {
        healthText = "HP 6/6";
        manaText = "MP 3/3";
        objectiveText = "Objective: Reach the lower pumps and restore the twin valves (0/2).";
        centerMessage = string.Empty;
        hintText = DefaultHintText;
        interactionPrompt = string.Empty;
        showCenterMessage = false;
        readableStoryVisible = false;
        readableStoryLocksInput = false;
        bossDefeated = false;
        rainActive = false;
        trophyVisible = false;
        trophyShown = false;
    }

    private void UpdateObjective()
    {
        if (storyPhase == StoryPhase.ChapterThree)
        {
            objectiveText = bossDefeated
                ? "Objective: Rain Restored"
                : "Objective: Break the warden of the drowned vault.";
        }
        else if (storyPhase == StoryPhase.ChapterTwo)
        {
            objectiveText = "Objective: Survive the flooded vault and reach the tidal seal.";
        }
        else if (storyPhase == StoryPhase.Finished)
        {
            objectiveText = "Objective: Rain Restored";
        }
        else if (CanExit)
        {
            objectiveText = "Objective: Walk forward past the second valve to the reservoir gate.";
        }
        else
        {
            objectiveText = $"Objective: Reach the lower pumps and restore the twin valves ({activatedValves}/{totalValves}).";
        }
    }

    private void HideCenterText()
    {
        introVisible = false;
        showCenterMessage = false;
        centerMessage = string.Empty;
    }

    private void EnsureStyles()
    {
        Font uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        storyFont ??= Font.CreateDynamicFontFromOSFont("Menlo", 96);

        if (hudStyle == null)
        {
            hudStyle = new GUIStyle(GUI.skin.label)
            {
                font = uiFont,
                fontSize = 20
            };
            ApplyTextColor(hudStyle, UiTextColor);
        }

        if (hintStyle == null)
        {
            hintStyle = new GUIStyle(hudStyle)
            {
                font = uiFont,
                fontSize = 17
            };
            ApplyTextColor(hintStyle, UiTextColor);
        }

        if (centerStyle == null)
        {
            centerStyle = new GUIStyle(GUI.skin.label)
            {
                font = uiFont,
                fontSize = 28,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter
            };
            ApplyTextColor(centerStyle, UiTextColor);
        }

        if (panelStyle == null)
        {
            Texture2D background = new(1, 1);
            background.SetPixel(0, 0, new Color(0.97f, 0.88f, 0.68f, 0.9f));
            background.Apply();

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = background }
            };
        }

        if (fullScreenOverlayStyle == null)
        {
            fullScreenOverlayStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = CreateStyleTexture(new Color(0.03f, 0.035f, 0.045f, 0.96f)) }
            };
        }

        if (storyTitleStyle == null)
        {
            storyTitleStyle = new GUIStyle(centerStyle)
            {
                font = storyFont != null ? storyFont : uiFont,
                fontSize = 128,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            ApplyTextColor(storyTitleStyle, StoryTextColor);
        }

        if (storyBodyStyle == null)
        {
            storyBodyStyle = new GUIStyle(centerStyle)
            {
                font = storyFont != null ? storyFont : uiFont,
                fontSize = 76,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            ApplyTextColor(storyBodyStyle, StoryTextColor);
        }

        if (storyFooterStyle == null)
        {
            storyFooterStyle = new GUIStyle(hintStyle)
            {
                font = uiFont,
                fontSize = 30,
                alignment = TextAnchor.MiddleCenter
            };
            ApplyTextColor(storyFooterStyle, new Color(0.78f, 0.78f, 0.72f, 1f));
        }

        if (promptStyle == null)
        {
            promptStyle = new GUIStyle(hintStyle)
            {
                font = uiFont,
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            ApplyTextColor(promptStyle, UiTextColor);
        }

        if (trophyTitleStyle == null)
        {
            trophyTitleStyle = new GUIStyle(hudStyle)
            {
                font = uiFont,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            ApplyTextColor(trophyTitleStyle, UiTextColor);
        }

        if (trophyBodyStyle == null)
        {
            trophyBodyStyle = new GUIStyle(hintStyle)
            {
                font = uiFont,
                fontSize = 15,
                alignment = TextAnchor.MiddleLeft
            };
            ApplyTextColor(trophyBodyStyle, UiTextColor);
        }

        if (rainTexture == null)
        {
            rainTexture = CreateStyleTexture(Color.white);
        }
    }

    private static Texture2D CreateStyleTexture(Color color)
    {
        Texture2D texture = new(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private static void ApplyTextColor(GUIStyle style, Color color)
    {
        style.normal.textColor = color;
        style.hover.textColor = color;
        style.active.textColor = color;
        style.focused.textColor = color;
        style.onNormal.textColor = color;
        style.onHover.textColor = color;
        style.onActive.textColor = color;
        style.onFocused.textColor = color;
    }
}
