using System.Collections.Generic;
using UnityEngine;

public class GameStateController : MonoBehaviour
{
    private enum StoryPhase
    {
        ChapterOne,
        Transitioning,
        ChapterTwo,
        Finished
    }

    private readonly List<GameObject> restoreObjects = new();
    private static readonly Color UiTextColor = new(0.09f, 0.11f, 0.13f, 1f);
    private const string DefaultHintText = "Move A/D or Arrows   Jump Space   Burst F / Mouse 1 / Enter   Follow the cracked spillway right";

    private PlayerController player;
    private string healthText = "HP 3/3";
    private string objectiveText = string.Empty;
    private string centerMessage = string.Empty;
    private string hintText = DefaultHintText;

    private GUIStyle hudStyle;
    private GUIStyle hintStyle;
    private GUIStyle centerStyle;
    private GUIStyle panelStyle;

    private Vector3 checkpointPosition;
    private Vector3 chapterTwoSpawnPoint;
    private GameObject gateBarrier;
    private GameObject chapterTwoRoot;
    private int totalValves;
    private int activatedValves;
    private float centerMessageExpiresAt;
    private float transitionTeleportAt;

    private bool introVisible;
    private bool showCenterMessage;
    private StoryPhase storyPhase;

    public bool CanExit { get; private set; }

    public void Initialize(int valveCount, Vector3 initialCheckpoint)
    {
        totalValves = valveCount;
        checkpointPosition = initialCheckpoint;
        storyPhase = StoryPhase.ChapterOne;
        BuildUi();
        UpdateObjective();
        ShowIntro();
    }

    public void SetPlayer(PlayerController controlledPlayer)
    {
        player = controlledPlayer;
        UpdateHealth(player.CurrentHealth, player.MaxHealth);
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

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        healthText = $"HP {currentHealth}/{maxHealth}";
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
            ShowTransientMessage("Restore the twin valves to awaken the reservoir gate.", 2.6f);
            return;
        }

        if (storyPhase != StoryPhase.ChapterOne)
        {
            return;
        }

        BeginChapterTwoTransition();
    }

    public void CompleteGame()
    {
        if (storyPhase == StoryPhase.Finished)
        {
            return;
        }

        storyPhase = StoryPhase.Finished;
        showCenterMessage = true;
        centerMessage =
            "CHAPTER II SECURED\n\n" +
            "The drowned vault yields.\n" +
            "Its wardens break under the tide-bearer's reach,\n" +
            "and a harsher road opens beyond.\n\n" +
            "Prototype arc complete.";
        objectiveText = "Objective: Prototype Complete";
        hintText = "Prototype arc complete. Press Play again to restart from Chapter I.";
        player?.SetInputLocked(false);
        player?.SetVisualHidden(false);
    }

    private void Update()
    {
        if (storyPhase == StoryPhase.Transitioning && Time.unscaledTime >= transitionTeleportAt)
        {
            EnterChapterTwo();
        }

        if (storyPhase != StoryPhase.Finished &&
            storyPhase != StoryPhase.Transitioning &&
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

        GUI.Box(new Rect(12f, 12f, 325f, 86f), GUIContent.none, panelStyle);
        GUI.Label(new Rect(24f, 20f, 250f, 28f), healthText, hudStyle);
        GUI.Label(new Rect(24f, 48f, 720f, 44f), objectiveText, hudStyle);

        GUI.Box(new Rect(12f, Screen.height - 56f, 560f, 40f), GUIContent.none, panelStyle);
        GUI.Label(new Rect(24f, Screen.height - 49f, 520f, 28f), hintText, hintStyle);

        if (showCenterMessage)
        {
            float width = Mathf.Min(780f, Screen.width - 80f);
            float height = Mathf.Min(260f, Screen.height - 120f);
            Rect panelRect = new((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            GUI.Box(panelRect, GUIContent.none, panelStyle);
            GUI.Label(new Rect(panelRect.x + 24f, panelRect.y + 24f, panelRect.width - 48f, panelRect.height - 48f), centerMessage, centerStyle);
        }
    }

    private void UnlockAqueduct()
    {
        if (CanExit)
        {
            return;
        }

        CanExit = true;

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

        hintText = "Take the wide blue steps on the right, then cross the reopened aqueduct.";
        UpdateObjective();
        ShowStoryMessage("Water answers the just. Return to the upper aqueduct.", 4f);
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

    private void BuildUi()
    {
        healthText = "HP 3/3";
        objectiveText = "Objective: Reach the lower pumps and restore the twin valves (0/2).";
        centerMessage = string.Empty;
        hintText = DefaultHintText;
        showCenterMessage = false;
    }

    private void UpdateObjective()
    {
        if (storyPhase == StoryPhase.ChapterTwo)
        {
            objectiveText = "Objective: Survive the flooded vault and reach the tidal seal.";
        }
        else if (storyPhase == StoryPhase.Finished)
        {
            objectiveText = "Objective: Prototype Complete";
        }
        else if (CanExit)
        {
            objectiveText = "Objective: Return to the upper aqueduct and reach the reservoir gate.";
        }
        else
        {
            objectiveText = $"Objective: Reach the lower pumps and restore the twin valves ({activatedValves}/{totalValves}).";
        }
    }

    private void ShowIntro()
    {
        introVisible = true;
        showCenterMessage = true;
        centerMessage =
            "DESERT AQUEDUCT\n\n" +
            "The wells were sealed by decree.\n" +
            "Drop through the cracked spillway, restore the twin valves,\n" +
            "and let justice flow.\n\n" +
            "Burst wardens and machinery with water.";
    }

    private void HideCenterText()
    {
        introVisible = false;
        showCenterMessage = false;
        centerMessage = string.Empty;
    }

    private void EnsureStyles()
    {
        if (hudStyle == null)
        {
            hudStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18
            };
            ApplyTextColor(hudStyle, UiTextColor);
        }

        if (hintStyle == null)
        {
            hintStyle = new GUIStyle(hudStyle)
            {
                fontSize = 15
            };
            ApplyTextColor(hintStyle, UiTextColor);
        }

        if (centerStyle == null)
        {
            centerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
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
