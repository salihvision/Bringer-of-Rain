using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1000)]
public class DesertAqueductBootstrap : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    private const int GroundLayer = 3;
    private const int WaterLayer = 4;

    private static readonly Color SkyColor = new(0.95f, 0.82f, 0.58f, 1f);
    private static readonly Color SandColor = new(0.76f, 0.62f, 0.35f, 1f);
    private static readonly Color StoneColor = new(0.46f, 0.33f, 0.21f, 1f);
    private static readonly Color RustColor = new(0.57f, 0.27f, 0.15f, 1f);
    private static readonly Color WaterColor = new(0.16f, 0.86f, 1f, 0.88f);
    private static readonly Color HazardColor = new(0.72f, 0.21f, 0.14f, 0.9f);
    private static readonly Color ShadowColor = new(0.33f, 0.22f, 0.12f, 1f);

    private readonly List<GameObject> unlockOnRestore = new();

    private Transform worldRoot;
    private GameObject chapterTwoRoot;
    private GameObject chapterThreeRoot;
    private GameStateController gameState;

    private void Awake()
    {
        if (inputActions == null)
        {
            Debug.LogError("DesertAqueductBootstrap requires the project InputActionAsset.");
            enabled = false;
            return;
        }

        ClearPrototypeScene();
        ConfigureLighting();

        worldRoot = new GameObject("RuntimeWorld").transform;
        gameState = gameObject.AddComponent<GameStateController>();

        Vector3 spawnPoint = new(-6f, -3.1f, 0f);
        gameState.Initialize(2, spawnPoint);

        BuildBackdrop();
        BuildLevelGeometry();
        BuildChapterTwoGeometry();
        BuildChapterThreeGeometry();

        PlayerController player = BuildPlayer(spawnPoint);
        BuildCamera(player.transform);

        gameState.SetPlayer(player);
    }

    private void ClearPrototypeScene()
    {
        string[] obsoleteObjects = { "Capsule", "Square", "GameObject", "RuntimeWorld", "HUD" };
        foreach (string objectName in obsoleteObjects)
        {
            GameObject existing = GameObject.Find(objectName);
            if (existing != null && existing != gameObject)
            {
                existing.SetActive(false);
                Destroy(existing);
            }
        }
    }

    private void ConfigureLighting()
    {
        Camera camera = Camera.main;
        if (camera != null)
        {
            camera.backgroundColor = SkyColor;
            camera.orthographicSize = 5.4f;
        }

        GameObject globalLight = GameObject.Find("Global Light 2D");
        if (globalLight != null)
        {
            foreach (Component component in globalLight.GetComponents<Component>())
            {
                if (component is Behaviour behaviour && behaviour.GetType().Name.Contains("Light"))
                {
                    behaviour.enabled = true;
                }
            }
        }
    }

    private void BuildBackdrop()
    {
        CreateVisual("Sun", new Vector2(33f, 10f), new Vector2(6f, 6f), new Color(1f, 0.93f, 0.65f, 0.7f), false, -20);
        CreateVisual("DuneFarA", new Vector2(8f, -1.2f), new Vector2(30f, 8f), new Color(0.87f, 0.72f, 0.45f, 0.35f), false, -18);
        CreateVisual("DuneFarB", new Vector2(34f, -0.8f), new Vector2(28f, 7f), new Color(0.8f, 0.63f, 0.38f, 0.32f), false, -18);
        CreateVisual("AqueductShadow", new Vector2(24f, 1.7f), new Vector2(50f, 0.7f), ShadowColor, false, -10);
        CreateVisual("LowerShadow", new Vector2(30f, -12.2f), new Vector2(38f, 2f), new Color(0.2f, 0.14f, 0.08f, 0.45f), false, -8);
    }

    private void BuildLevelGeometry()
    {
        CreateSolid("EntryFloor", new Vector2(0f, -4.5f), new Vector2(16f, 1f), SandColor);
        CreateSolid("JunctionFloor", new Vector2(12f, -4.5f), new Vector2(8f, 1f), SandColor);
        CreateOneWayPlatform("UpperLedge", new Vector2(19f, 0.25f), new Vector2(6f, 1f), StoneColor);
        CreateOneWayPlatform("UpperBridgeLeft", new Vector2(28f, 0.25f), new Vector2(10f, 1f), StoneColor);
        CreateOneWayPlatform("UpperBridgeRight", new Vector2(41f, 0.25f), new Vector2(10f, 1f), StoneColor);

        GameObject gateBarrier = CreateSolid("JunctionGate", new Vector2(22.5f, -0.35f), new Vector2(0.8f, 3.6f), RustColor);
        GameObject restoredBridge = CreateOneWayPlatform("RestoredWaterBridge", new Vector2(34.5f, 0.25f), new Vector2(4.6f, 1f), WaterColor);
        restoredBridge.SetActive(false);
        unlockOnRestore.Add(restoredBridge);

        GameObject stepOne = CreateOneWayPlatform("WaterStep01", new Vector2(37.6f, -8.55f), new Vector2(3.4f, 0.55f), WaterColor);
        GameObject stepTwo = CreateOneWayPlatform("WaterStep02", new Vector2(39.4f, -7.0f), new Vector2(3.4f, 0.55f), WaterColor);
        GameObject stepThree = CreateOneWayPlatform("WaterStep03", new Vector2(41.2f, -5.45f), new Vector2(3.5f, 0.55f), WaterColor);
        GameObject stepFour = CreateOneWayPlatform("WaterStep04", new Vector2(42.8f, -3.9f), new Vector2(3.6f, 0.55f), WaterColor);
        GameObject stepFive = CreateOneWayPlatform("WaterStep05", new Vector2(43.9f, -2.35f), new Vector2(4f, 0.55f), WaterColor);
        GameObject stepSix = CreateOneWayPlatform("WaterStep06", new Vector2(41.6f, -1.0f), new Vector2(6.8f, 0.55f), WaterColor);
        stepOne.SetActive(false);
        stepTwo.SetActive(false);
        stepThree.SetActive(false);
        stepFour.SetActive(false);
        stepFive.SetActive(false);
        stepSix.SetActive(false);
        unlockOnRestore.Add(stepOne);
        unlockOnRestore.Add(stepTwo);
        unlockOnRestore.Add(stepThree);
        unlockOnRestore.Add(stepFour);
        unlockOnRestore.Add(stepFive);
        unlockOnRestore.Add(stepSix);

        CreateSolid("ShaftLeftLip", new Vector2(15.5f, -4.1f), new Vector2(1f, 2f), StoneColor);
        CreateSolid("ShaftRightLip", new Vector2(22.5f, -4.1f), new Vector2(1f, 2f), StoneColor);
        CreateSolid("LeftBoundary", new Vector2(-8.5f, -2.5f), new Vector2(1f, 10f), StoneColor);
        CreateSolid("RightBoundary", new Vector2(46.5f, -2.5f), new Vector2(1f, 10f), StoneColor);

        CreateSolid("LowerFloorLeft", new Vector2(19f, -10.5f), new Vector2(8f, 1f), SandColor);
        CreateSolid("LowerFloorMiddle", new Vector2(29f, -10.5f), new Vector2(12f, 1f), SandColor);
        CreateSolid("LowerFloorRight", new Vector2(40.5f, -10.5f), new Vector2(11f, 1f), SandColor);
        CreateVisual("SpillwayGlow", new Vector2(18.8f, -6.6f), new Vector2(5.8f, 0.16f), new Color(WaterColor.r, WaterColor.g, WaterColor.b, 0.22f), false, 1);

        CreateSolid("ValveAPedestal", new Vector2(30f, -8.5f), new Vector2(3f, 3f), RustColor);
        CreateSolid("ReservoirStepA", new Vector2(38.8f, -9.25f), new Vector2(2.6f, 1.5f), RustColor);
        CreateSolid("ReservoirStepB", new Vector2(41.8f, -7.25f), new Vector2(2.6f, 2f), RustColor);
        CreateSolid("ValveBPedestal", new Vector2(44.5f, -5.15f), new Vector2(3f, 3.8f), RustColor);

        CreateVisual("UpperPipeA", new Vector2(28f, 1.8f), new Vector2(10f, 0.35f), RustColor, false, 4);
        CreateVisual("UpperPipeB", new Vector2(41f, 1.8f), new Vector2(10f, 0.35f), RustColor, false, 4);

        GameObject waterFlowA = CreateVisual("WaterFlowLeft", new Vector2(28f, 1.2f), new Vector2(10f, 0.18f), WaterColor, false, 5);
        GameObject waterFlowBridge = CreateVisual("WaterFlowBridge", new Vector2(34.5f, 1.2f), new Vector2(3f, 0.18f), WaterColor, false, 5);
        GameObject waterFlowB = CreateVisual("WaterFlowRight", new Vector2(41f, 1.2f), new Vector2(10f, 0.18f), WaterColor, false, 5);
        waterFlowA.SetActive(false);
        waterFlowBridge.SetActive(false);
        waterFlowB.SetActive(false);
        unlockOnRestore.Add(waterFlowA);
        unlockOnRestore.Add(waterFlowBridge);
        unlockOnRestore.Add(waterFlowB);

        CreateEnemy("SentryScout", new Vector2(4.5f, -3.42f), 2.6f, 6.8f, 1.45f, "MaskDude");
        CreateEnemy("SentryA", new Vector2(19.5f, -9.42f), 17.2f, 21.4f, 1.8f, "MaskDude");
        CreateEnemy("SentryB", new Vector2(26.2f, -9.42f), 24.5f, 28f, 1.95f, "MaskDude");
        CreateEnemy("SentryGate", new Vector2(35.5f, -9.42f), 33.2f, 37.2f, 2.05f, "MaskDude");

        CreateHazard("DrySpikes", new Vector2(35f, -10f), new Vector2(2f, 0.7f), false);
        CreateHazard("KillPlane", new Vector2(20f, -18f), new Vector2(80f, 4f), true);

        CreateValve("Valve of Mercy", new Vector2(30f, -6.85f), "Valve of Mercy");
        CreateValve("Valve of Witness", new Vector2(44.5f, -1.95f), "Valve of Witness");

        CreateCheckpoint(new Vector2(33.6f, -8.7f), new Vector3(33.6f, -9.1f, 0f));
        CreateStorySign(
            new Vector2(10.5f, -2.75f),
            "DECREE",
            "Wardens above.\nPumps below.",
            "The cracked spillway is the only open path into the pumps below.",
            new Vector2(3.4f, 3f));

        CreateExit("ReservoirGate", new Vector2(45f, 1.8f), new Vector2(2.2f, 4f), ExitTrigger.ExitMode.AdvanceChapter);

        gameState.ConfigureProgression(gateBarrier, unlockOnRestore);
    }

    private void BuildChapterTwoGeometry()
    {
        Transform previousRoot = worldRoot;
        chapterTwoRoot = new GameObject("ChapterTwoWorld");
        chapterTwoRoot.transform.SetParent(previousRoot, false);
        worldRoot = chapterTwoRoot.transform;

        Vector3 chapterTwoSpawn = new(53.6f, -3.1f, 0f);
        Color vaultStone = new(0.21f, 0.31f, 0.35f, 1f);
        Color vaultWater = new(0.18f, 0.82f, 0.98f, 0.92f);
        Color vaultMist = new(0.48f, 0.83f, 0.91f, 0.18f);

        CreateVisual("VaultBackdrop", new Vector2(62.5f, -0.6f), new Vector2(24f, 10f), new Color(0.16f, 0.28f, 0.31f, 0.14f), false, -16);
        CreateVisual("VaultMist", new Vector2(63.5f, 1.8f), new Vector2(20f, 2.6f), vaultMist, false, -15);
        CreateSolid("VaultFloor", new Vector2(62.5f, -4.5f), new Vector2(22f, 1f), vaultStone);
        CreateSolid("VaultRightBoundary", new Vector2(78.3f, -1.8f), new Vector2(1f, 12f), vaultStone);
        CreateSolid("VaultColumnA", new Vector2(59.8f, -2.4f), new Vector2(1.1f, 4.2f), StoneColor);
        CreateSolid("VaultColumnB", new Vector2(68.6f, -2.0f), new Vector2(1.1f, 5f), StoneColor);
        CreateSolid("VaultSealPedestal", new Vector2(75.2f, -2.6f), new Vector2(3.8f, 3.8f), RustColor);

        CreateOneWayPlatform("VaultStepA", new Vector2(56.8f, -1.5f), new Vector2(3.6f, 0.6f), vaultWater);
        CreateOneWayPlatform("VaultStepB", new Vector2(61.9f, -0.2f), new Vector2(4.4f, 0.6f), vaultWater);
        CreateOneWayPlatform("VaultStepC", new Vector2(66.4f, -2.3f), new Vector2(3.8f, 0.6f), vaultWater);
        CreateOneWayPlatform("VaultStepD", new Vector2(71.2f, -0.9f), new Vector2(4.2f, 0.6f), vaultWater);

        CreateHazard("VaultSpikesA", new Vector2(64.3f, -4.03f), new Vector2(2.4f, 0.7f), false);
        CreateHazard("VaultSpikesB", new Vector2(72.2f, -4.03f), new Vector2(2.1f, 0.7f), false);

        CreateEnemy("VaultHunterA", new Vector2(57f, -3.42f), 54.6f, 59.6f, 2.15f, "NinjaFrog", 1.35f);
        CreateEnemy("VaultCrusherA", new Vector2(63.4f, -3.42f), 61.2f, 66.2f, 2.25f, "MaskDude", 1.55f);
        CreateEnemy("VaultHunterB", new Vector2(71.2f, -0.02f), 69.3f, 73.1f, 2.4f, "NinjaFrog", 1.7f);
        CreateEnemy("VaultCrusherB", new Vector2(72.1f, -3.42f), 71.2f, 73.0f, 2.5f, "MaskDude", 1.82f);

        CreateCheckpoint(new Vector2(53.8f, -3.7f), chapterTwoSpawn);
        CreateStorySign(
            new Vector2(56.2f, -2.75f),
            "VAULT",
            "Flooded below.\nTeeth ahead.",
            "Chapter II: the drowned vault. These wardens strike harder and dash faster.",
            new Vector2(3.4f, 3f));

        CreateExit("TidalSeal", new Vector2(76.2f, -0.85f), new Vector2(2.1f, 3.2f), ExitTrigger.ExitMode.FinalSeal);

        worldRoot = previousRoot;
        chapterTwoRoot.SetActive(false);
        gameState.ConfigureChapterTwo(chapterTwoRoot, chapterTwoSpawn);
    }

    private void BuildChapterThreeGeometry()
    {
        Transform previousRoot = worldRoot;
        chapterThreeRoot = new GameObject("ChapterThreeWorld");
        chapterThreeRoot.transform.SetParent(previousRoot, false);
        worldRoot = chapterThreeRoot.transform;

        Vector3 chapterThreeSpawn = new(85.5f, -3.42f, 0f);
        const float arenaCenterX = 94f;
        const float arenaLeftBound = 83.5f;
        const float arenaRightBound = 104.5f;
        const float arenaFloorY = -4f;

        Color courtBackdrop = new(0.12f, 0.07f, 0.16f, 1f);
        Color courtFloor = new(0.18f, 0.13f, 0.22f, 1f);
        Color courtPillar = new(0.34f, 0.18f, 0.36f, 1f);
        Color courtMist = new(0.62f, 0.34f, 0.78f, 0.18f);
        Color courtAccent = new(0.92f, 0.36f, 0.42f, 0.85f);

        CreateVisual("CourtBackdrop", new Vector2(arenaCenterX, -0.6f), new Vector2(24f, 11f), courtBackdrop, false, -16);
        CreateVisual("CourtMistA", new Vector2(arenaCenterX - 4f, 1.4f), new Vector2(14f, 2.4f), courtMist, false, -14);
        CreateVisual("CourtMistB", new Vector2(arenaCenterX + 5f, 2.2f), new Vector2(12f, 1.8f), courtMist, false, -14);
        CreateVisual("CourtAccent", new Vector2(arenaCenterX, 3.4f), new Vector2(20f, 0.18f), courtAccent, false, -12);

        CreateSolid("CourtFloor", new Vector2(arenaCenterX, -4.5f), new Vector2(22f, 1f), courtFloor);
        CreateSolid("CourtLeftWall", new Vector2(arenaLeftBound - 0.5f, -1f), new Vector2(1f, 9f), courtPillar);
        CreateSolid("CourtRightWall", new Vector2(arenaRightBound + 0.5f, -1f), new Vector2(1f, 9f), courtPillar);
        CreateSolid("CourtCeiling", new Vector2(arenaCenterX, 4.4f), new Vector2(22f, 0.6f), courtPillar);
        CreateSolid("CourtPillarLeft", new Vector2(89.5f, -2.6f), new Vector2(0.7f, 2.6f), courtPillar);
        CreateSolid("CourtPillarRight", new Vector2(98.5f, -2.6f), new Vector2(0.7f, 2.6f), courtPillar);

        CreateHazard("CourtKillPlane", new Vector2(arenaCenterX, -18f), new Vector2(28f, 4f), true);

        CreateCheckpoint(new Vector2(85.4f, -3.7f), chapterThreeSpawn);
        CreateStorySign(
            new Vector2(86.5f, -2.75f),
            "WARDEN",
            "Stun-immune.\nStrike on the slam.",
            "The warden of the drowned vault. Strike only when its guard breaks.",
            new Vector2(3.4f, 3f));

        BuildBoss(new Vector2(arenaCenterX, -2f), arenaLeftBound, arenaRightBound, arenaFloorY);

        worldRoot = previousRoot;
        chapterThreeRoot.SetActive(false);
        gameState.ConfigureChapterThree(chapterThreeRoot, chapterThreeSpawn, new Vector2(82f, 107f));
    }

    private void BuildBoss(Vector2 position, float leftBound, float rightBound, float groundY)
    {
        GameObject bossObject = new("DrownedWarden");
        bossObject.transform.SetParent(worldRoot, false);
        bossObject.transform.position = position;
        bossObject.transform.localScale = new Vector3(2.2f, 2.4f, 1f);

        bossObject.AddComponent<SpriteRenderer>();
        bossObject.AddComponent<BoxCollider2D>();
        bossObject.AddComponent<Rigidbody2D>();

        BossController boss = bossObject.AddComponent<BossController>();
        boss.Configure(gameState, worldRoot, leftBound, rightBound, groundY);

        CreateChildVisual(bossObject.transform, "BossShadow", new Vector2(0f, -0.62f), new Vector2(0.92f, 0.16f), new Color(0.05f, 0.02f, 0.08f, 0.55f), 8);
        CreateChildVisual(bossObject.transform, "BossCrest", new Vector2(0f, 0.42f), new Vector2(0.55f, 0.16f), new Color(1f, 0.84f, 0.32f, 0.95f), 12);
        CreateChildVisual(bossObject.transform, "BossEye", new Vector2(0f, 0.08f), new Vector2(0.18f, 0.18f), new Color(1f, 0.42f, 0.32f, 0.95f), 13);
    }

    private PlayerController BuildPlayer(Vector3 spawnPoint)
    {
        GameObject playerObject = new("TideBearer");
        playerObject.transform.SetParent(worldRoot, false);
        playerObject.transform.position = spawnPoint;

        SpriteRenderer bodyRenderer = playerObject.AddComponent<SpriteRenderer>();
        bodyRenderer.sortingOrder = 10;
        bodyRenderer.sprite = PrimitiveSpriteLibrary.SquareSprite;

        Rigidbody2D body = playerObject.AddComponent<Rigidbody2D>();
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        CapsuleCollider2D capsule = playerObject.AddComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(0.95f, 1.8f);

        CreateChildVisual(playerObject.transform, "GroundShadow", new Vector2(0f, -0.8f), new Vector2(0.7f, 0.16f), new Color(0.11f, 0.08f, 0.06f, 0.28f), 8);
        CreateChildVisual(playerObject.transform, "BackGlow", new Vector2(-0.08f, -0.04f), new Vector2(0.52f, 0.96f), new Color(0.18f, 0.86f, 1f, 0.15f), 10);

        PlayerController controller = playerObject.AddComponent<PlayerController>();
        controller.Configure(inputActions, 1 << GroundLayer, 1 << 0, gameState, spawnPoint);
        playerObject.AddComponent<CharacterSpriteAnimator>();
        return controller;
    }

    private void BuildCamera(Transform playerTarget)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        SimpleCameraFollow follow = camera.GetComponent<SimpleCameraFollow>();
        if (follow == null)
        {
            follow = camera.gameObject.AddComponent<SimpleCameraFollow>();
        }

        follow.Configure(playerTarget, new Vector3(2.5f, 1f, -10f), 4.2f, new Vector2(-2f, 71f));
    }

    private GameObject CreateSolid(string objectName, Vector2 position, Vector2 size, Color color)
    {
        GameObject solid = CreateVisual(objectName, position, size, color, true, 0);
        solid.layer = GroundLayer;
        return solid;
    }

    private GameObject CreateOneWayPlatform(string objectName, Vector2 position, Vector2 size, Color color)
    {
        GameObject platform = CreateVisual(objectName, position, size, new Color(color.r, color.g, color.b, 0.86f), true, 2);
        platform.layer = GroundLayer;

        BoxCollider2D collider2D = platform.GetComponent<BoxCollider2D>();
        collider2D.usedByEffector = true;

        PlatformEffector2D effector = platform.AddComponent<PlatformEffector2D>();
        effector.useOneWay = true;
        effector.useOneWayGrouping = true;
        effector.surfaceArc = 170f;
        effector.sideArc = 0f;

        Color topLipColor = Color.Lerp(color, Color.white, 0.42f);
        Color undersideColor = Color.Lerp(color, ShadowColor, 0.35f);
        CreateChildVisual(platform.transform, "TopLip", new Vector2(0f, 0.38f), new Vector2(0.98f, 0.18f), new Color(topLipColor.r, topLipColor.g, topLipColor.b, 0.98f), 3);
        CreateChildVisual(platform.transform, "Underside", new Vector2(0f, -0.34f), new Vector2(0.92f, 0.22f), new Color(undersideColor.r, undersideColor.g, undersideColor.b, 0.92f), 1);
        return platform;
    }

    private GameObject CreateVisual(string objectName, Vector2 position, Vector2 size, Color color, bool addCollider, int sortingOrder)
    {
        GameObject visual = new(objectName);
        visual.transform.SetParent(worldRoot, false);
        visual.transform.position = position;
        visual.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;

        if (addCollider)
        {
            BoxCollider2D collider2D = visual.AddComponent<BoxCollider2D>();
            collider2D.size = Vector2.one;
        }

        return visual;
    }

    private GameObject CreateChildVisual(Transform parent, string objectName, Vector2 localPosition, Vector2 size, Color color, int sortingOrder)
    {
        GameObject visual = new(objectName);
        visual.transform.SetParent(parent, false);
        visual.transform.localPosition = localPosition;
        visual.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return visual;
    }

    private void CreateEnemy(string objectName, Vector2 position, float minX, float maxX, float speed, string spriteSet, float difficultyScale = 1f)
    {
        GameObject enemyObject = new(objectName);
        enemyObject.transform.SetParent(worldRoot, false);
        enemyObject.transform.position = position;
        enemyObject.transform.localScale = new Vector3(1.28f, 1.28f, 1f);

        enemyObject.AddComponent<SpriteRenderer>();
        enemyObject.AddComponent<BoxCollider2D>();
        enemyObject.AddComponent<Rigidbody2D>();

        EnemyPatrol enemy = enemyObject.AddComponent<EnemyPatrol>();
        enemy.Configure(minX, maxX, speed, difficultyScale);
        EnemySpriteAnimator animator = enemyObject.AddComponent<EnemySpriteAnimator>();
        animator.Configure(spriteSet);

        CreateChildVisual(enemyObject.transform, "GroundShadow", new Vector2(0f, -0.38f), new Vector2(0.7f, 0.12f), new Color(0.11f, 0.08f, 0.06f, 0.22f), 8);
        CreateChildVisual(enemyObject.transform, "ThreatGlow", new Vector2(0f, -0.02f), new Vector2(0.66f, 0.9f), new Color(0.16f, 0.86f, 1f, 0.08f), 9);
    }

    private void CreateValve(string objectName, Vector2 position, string label)
    {
        GameObject valveRoot = new(objectName);
        valveRoot.transform.SetParent(worldRoot, false);
        valveRoot.transform.position = position;
        valveRoot.transform.localScale = new Vector3(0.9f, 1.5f, 1f);

        SpriteRenderer bodyRenderer = valveRoot.AddComponent<SpriteRenderer>();
        bodyRenderer.sortingOrder = 8;
        valveRoot.AddComponent<BoxCollider2D>();

        GameObject glow = CreateChildVisual(valveRoot.transform, "Glow", new Vector2(0f, 0.25f), new Vector2(1f, 1f), WaterColor, 7);
        SpriteRenderer glowRenderer = glow.GetComponent<SpriteRenderer>();

        CreateChildVisual(valveRoot.transform, "Handle", new Vector2(0f, 0.55f), new Vector2(1.2f, 0.18f), new Color(0.75f, 0.58f, 0.34f, 1f), 9);

        ValveController valve = valveRoot.AddComponent<ValveController>();
        valve.Configure(label, gameState, glowRenderer);
    }

    private void CreateHazard(string objectName, Vector2 position, Vector2 size, bool instantRespawn)
    {
        GameObject hazard = CreateVisual(objectName, position, size, instantRespawn ? new Color(0.15f, 0.1f, 0.08f, 0.01f) : HazardColor, false, 2);
        hazard.layer = WaterLayer;

        BoxCollider2D collider2D = hazard.AddComponent<BoxCollider2D>();
        collider2D.isTrigger = true;

        HazardZone zone = hazard.AddComponent<HazardZone>();
        zone.Configure(1, instantRespawn, 0.75f);

        if (!instantRespawn)
        {
            CreateChildVisual(hazard.transform, "SpikeA", new Vector2(-0.5f, 0.35f), new Vector2(0.3f, 0.3f), HazardColor, 3);
            CreateChildVisual(hazard.transform, "SpikeB", new Vector2(0f, 0.35f), new Vector2(0.3f, 0.3f), HazardColor, 3);
            CreateChildVisual(hazard.transform, "SpikeC", new Vector2(0.5f, 0.35f), new Vector2(0.3f, 0.3f), HazardColor, 3);
        }
    }

    private void CreateCheckpoint(Vector2 position, Vector3 respawnPoint)
    {
        GameObject checkpoint = new("Checkpoint");
        checkpoint.transform.SetParent(worldRoot, false);
        checkpoint.transform.position = position;
        checkpoint.layer = WaterLayer;

        BoxCollider2D collider2D = checkpoint.AddComponent<BoxCollider2D>();
        collider2D.isTrigger = true;
        collider2D.size = new Vector2(1.4f, 2.2f);

        GameObject beacon = CreateChildVisual(checkpoint.transform, "Beacon", new Vector2(0f, 0.6f), new Vector2(0.8f, 1.2f), new Color(0.36f, 0.27f, 0.2f, 1f), 6);
        SpriteRenderer beaconRenderer = beacon.GetComponent<SpriteRenderer>();
        CreateChildVisual(checkpoint.transform, "Post", new Vector2(0f, -0.25f), new Vector2(0.22f, 1.7f), StoneColor, 5);

        CheckpointTrigger trigger = checkpoint.AddComponent<CheckpointTrigger>();
        trigger.Configure(respawnPoint, gameState, beaconRenderer);
    }

    private void CreateStorySign(Vector2 position, string title, string visibleText, string triggerText, Vector2 triggerSize)
    {
        GameObject sign = new("StorySign");
        sign.transform.SetParent(worldRoot, false);
        sign.transform.position = position;
        sign.layer = WaterLayer;

        CreateChildVisual(sign.transform, "Post", new Vector2(0f, -0.5f), new Vector2(0.2f, 1.8f), StoneColor, 5);
        CreateChildVisual(sign.transform, "Plaque", new Vector2(0f, 0.25f), new Vector2(2.4f, 1.2f), RustColor, 6);
        CreateChildVisual(sign.transform, "MarkerTop", new Vector2(0f, 0.52f), new Vector2(1.35f, 0.12f), new Color(1f, 0.91f, 0.72f, 1f), 7);
        CreateChildVisual(sign.transform, "MarkerMid", new Vector2(0f, 0.22f), new Vector2(1.8f, 0.12f), new Color(0.16f, 0.86f, 1f, 0.95f), 7);
        CreateChildVisual(sign.transform, "MarkerBottom", new Vector2(0f, -0.08f), new Vector2(1.5f, 0.1f), new Color(0.96f, 0.93f, 0.86f, 1f), 7);
        CreateChildVisual(sign.transform, "MarkerDot", new Vector2(0.8f, -0.32f), new Vector2(0.14f, 0.14f), new Color(0.96f, 0.93f, 0.86f, 1f), 7);

        BoxCollider2D collider2D = sign.AddComponent<BoxCollider2D>();
        collider2D.isTrigger = true;
        collider2D.size = triggerSize;

        StoryTrigger storyTrigger = sign.AddComponent<StoryTrigger>();
        storyTrigger.Configure(gameState, triggerText, 3.5f, true);
    }

    private void CreateExit(string objectName, Vector2 position, Vector2 triggerSize, ExitTrigger.ExitMode exitMode = ExitTrigger.ExitMode.AdvanceChapter)
    {
        GameObject exitRoot = new(objectName);
        exitRoot.transform.SetParent(worldRoot, false);
        exitRoot.transform.position = position;
        exitRoot.layer = WaterLayer;

        CreateChildVisual(exitRoot.transform, "ArchLeft", new Vector2(-0.9f, -0.4f), new Vector2(0.35f, 3.5f), StoneColor, 6);
        CreateChildVisual(exitRoot.transform, "ArchRight", new Vector2(0.9f, -0.4f), new Vector2(0.35f, 3.5f), StoneColor, 6);
        CreateChildVisual(exitRoot.transform, "ArchTop", new Vector2(0f, 1.2f), new Vector2(2.1f, 0.35f), StoneColor, 6);
        CreateChildVisual(exitRoot.transform, "ReservoirGlow", new Vector2(0f, 0.1f), new Vector2(1.4f, 2.2f), new Color(WaterColor.r, WaterColor.g, WaterColor.b, 0.5f), 5);

        BoxCollider2D collider2D = exitRoot.AddComponent<BoxCollider2D>();
        collider2D.isTrigger = true;
        collider2D.size = triggerSize;

        ExitTrigger exitTrigger = exitRoot.AddComponent<ExitTrigger>();
        exitTrigger.Configure(gameState, exitMode);
    }

    private TextMesh CreateWorldText(Transform parent, string content, Vector2 localPosition, float characterSize, TextAlignment alignment)
    {
        GameObject textObject = new("WorldText");
        textObject.transform.SetParent(parent, false);
        textObject.transform.localPosition = localPosition;
        textObject.transform.localScale = Vector3.one;

        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = content;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = alignment;
        textMesh.characterSize = characterSize;
        textMesh.fontSize = 64;
        textMesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        MeshRenderer renderer = textObject.GetComponent<MeshRenderer>();
        if (textMesh.font != null)
        {
            renderer.sharedMaterial = textMesh.font.material;
        }
        else
        {
            Debug.LogWarning("Built-in font LegacyRuntime.ttf could not be loaded for world text.");
        }

        renderer.sortingOrder = 7;
        return textMesh;
    }
}
