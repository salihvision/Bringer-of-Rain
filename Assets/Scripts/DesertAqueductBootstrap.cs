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
        GameAudioController.EnsureInScene();

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
        CreateVisual("CleanDesertSky", new Vector2(44f, 0.8f), new Vector2(120f, 47f), new Color(0.78f, 0.66f, 0.44f, 1f), false, -22);
    }

    private void BuildLevelGeometry()
    {
        CreateSolid("EntryFloor", new Vector2(0f, -4.5f), new Vector2(16f, 1f), SandColor);
        CreateTiledOverlay("EntryFloorTiles", new Vector2(0f, -4.5f), new Vector2(16f, 1f), "Tiles/PalmIsland", 1, 0);
        CreateSolid("JunctionFloor", new Vector2(12f, -4.5f), new Vector2(8f, 1f), SandColor);
        CreateTiledOverlay("JunctionFloorTiles", new Vector2(12f, -4.5f), new Vector2(8f, 1f), "Tiles/PalmIsland", 1, 0);
        CreateSolid("ShaftLeftLip", new Vector2(15.5f, -4.1f), new Vector2(1f, 2f), StoneColor);
        CreateTiledOverlay("ShaftLeftLipTiles", new Vector2(15.5f, -4.1f), new Vector2(1f, 2f), "Tiles/PalmIsland", 4, 2);
        CreateSolid("ShaftRightLip", new Vector2(22.5f, -4.1f), new Vector2(1f, 2f), StoneColor);
        CreateTiledOverlay("ShaftRightLipTiles", new Vector2(22.5f, -4.1f), new Vector2(1f, 2f), "Tiles/PalmIsland", 4, 2);
        CreateSolid("LeftBoundary", new Vector2(-8.5f, -2.5f), new Vector2(1f, 10f), StoneColor);
        CreateTiledOverlay("LeftBoundaryTiles", new Vector2(-8.5f, -2.5f), new Vector2(1f, 10f), "Tiles/PalmIsland", 4, 2);
        CreateSolid("RightBoundary", new Vector2(51.5f, -2.5f), new Vector2(1f, 10f), StoneColor);
        CreateTiledOverlay("RightBoundaryTiles", new Vector2(51.5f, -2.5f), new Vector2(1f, 10f), "Tiles/PalmIsland", 4, 2);

        CreateSolid("LowerFloorLeft", new Vector2(19f, -10.5f), new Vector2(8f, 1f), SandColor);
        CreateTiledOverlay("LowerFloorLeftTiles", new Vector2(19f, -10.5f), new Vector2(8f, 1f), "Tiles/PalmIsland", 1, 0);
        CreateSolid("LowerFloorMiddle", new Vector2(29f, -10.5f), new Vector2(12f, 1f), SandColor);
        CreateTiledOverlay("LowerFloorMiddleTiles", new Vector2(29f, -10.5f), new Vector2(12f, 1f), "Tiles/PalmIsland", 1, 0);
        CreateSolid("LowerFloorRight", new Vector2(40.5f, -10.5f), new Vector2(11f, 1f), SandColor);
        CreateTiledOverlay("LowerFloorRightTiles", new Vector2(40.5f, -10.5f), new Vector2(11f, 1f), "Tiles/PalmIsland", 1, 0);
        CreateVisual("SpillwayGlow", new Vector2(18.8f, -6.6f), new Vector2(5.8f, 0.16f), new Color(WaterColor.r, WaterColor.g, WaterColor.b, 0.22f), false, 1);

        CreateSolid("ValveAPedestal", new Vector2(30f, -8.5f), new Vector2(3f, 3f), StoneColor);
        CreateTiledOverlay("ValveAPedestalTiles", new Vector2(30f, -8.5f), new Vector2(3f, 3f), "Tiles/PalmIsland", 4, 2);
        CreateSolid("ReservoirStepA", new Vector2(38.8f, -9.25f), new Vector2(2.6f, 1.5f), StoneColor);
        CreateTiledOverlay("ReservoirStepATiles", new Vector2(38.8f, -9.25f), new Vector2(2.6f, 1.5f), "Tiles/PalmIsland", 4, 2);
        CreateSolid("ReservoirStepB", new Vector2(41.8f, -7.25f), new Vector2(2.6f, 2f), StoneColor);
        CreateTiledOverlay("ReservoirStepBTiles", new Vector2(41.8f, -7.25f), new Vector2(2.6f, 2f), "Tiles/PalmIsland", 4, 2);
        CreateSolid("ValveBPedestal", new Vector2(44.5f, -5.15f), new Vector2(3f, 3.8f), StoneColor);
        CreateTiledOverlay("ValveBPedestalTiles", new Vector2(44.5f, -5.15f), new Vector2(3f, 3.8f), "Tiles/PalmIsland", 4, 2);
        CreateSolid("ReservoirWalkway", new Vector2(48.25f, -3.6f), new Vector2(4.5f, 0.7f), SandColor);
        CreateTiledOverlay("ReservoirWalkwayTiles", new Vector2(48.25f, -3.6f), new Vector2(4.5f, 0.7f), "Tiles/PalmIsland", 1, 0);

        CreateElder(new Vector2(-4.25f, -3.0f));
        CreateDesertProp("EntryCactus", new Vector2(7.2f, -3.2f), 11, 5, new Vector2(1.3f, 2.2f));
        CreateDesertProp("LowerBarrel", new Vector2(47.8f, -3.05f), 14, 5, new Vector2(1.0f, 1.25f));

        CreateEnemy("SentryScout", new Vector2(4.5f, -3.42f), 2.6f, 6.8f, 1.45f, "MaskDude");
        CreateEnemy("SentryA", new Vector2(19.5f, -9.42f), 17.2f, 21.4f, 1.8f, "MaskDude");
        CreateEnemy("SentryB", new Vector2(26.2f, -9.42f), 24.5f, 28f, 1.95f, "MaskDude");
        CreateEnemy("SentryGate", new Vector2(35.5f, -9.42f), 33.2f, 37.2f, 2.05f, "MaskDude");

        CreateHazard("DrySpikes", new Vector2(35f, -10f), new Vector2(2f, 0.7f), false);
        CreateHazard("KillPlane", new Vector2(20f, -18f), new Vector2(80f, 4f), true);

        CreateValve("Valve of Mercy", new Vector2(30f, -6.25f), "Valve of Mercy");
        CreateValve("Valve of Witness", new Vector2(44.5f, -2.5f), "Valve of Witness");

        CreateCheckpoint(new Vector2(33.6f, -9.0f), new Vector3(33.6f, -9.1f, 0f));
        CreateStorySign(
            new Vector2(10.5f, -3.0f),
            "DECREE",
            "Pumps below.\nValves twin.",
            "Two valves are sealed below.\n\nDrop through the broken spillway. Restore both valves. Then return to the reservoir gate.",
            new Vector2(3.4f, 3f));

        CreateExit("ReservoirGate", new Vector2(49.5f, -1.1f), new Vector2(2.2f, 3.4f), ExitTrigger.ExitMode.AdvanceChapter);

        gameState.ConfigureProgression(null, unlockOnRestore);
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

        CreateVisual("VaultBackdrop", new Vector2(64f, -0.6f), new Vector2(50f, 20f), new Color(0.13f, 0.22f, 0.26f, 0.98f), false, -16);
        CreateSolid("VaultFloor", new Vector2(62.5f, -4.5f), new Vector2(22f, 1f), vaultStone);
        CreateTiledOverlay("VaultFloorTiles", new Vector2(62.5f, -4.5f), new Vector2(22f, 1f), "Tiles/PalmIsland", 1, 0);
        CreateSolid("VaultRightBoundary", new Vector2(78.3f, -1.8f), new Vector2(1f, 12f), vaultStone);
        CreateTiledOverlay("VaultRightBoundaryTiles", new Vector2(78.3f, -1.8f), new Vector2(1f, 12f), "Tiles/PalmIsland", 4, 2);
        CreateSolid("VaultColumnA", new Vector2(59.8f, -2.45f), new Vector2(1.1f, 3.1f), StoneColor);
        CreateTiledOverlay("VaultColumnATiles", new Vector2(59.8f, -2.45f), new Vector2(1.1f, 3.1f), "Tiles/PalmIsland", 4, 2);
        CreateSolid("VaultColumnB", new Vector2(68.6f, -2.25f), new Vector2(1.1f, 3.5f), StoneColor);
        CreateTiledOverlay("VaultColumnBTiles", new Vector2(68.6f, -2.25f), new Vector2(1.1f, 3.5f), "Tiles/PalmIsland", 4, 2);
        CreateSolid("VaultSealPedestal", new Vector2(75.2f, -2.6f), new Vector2(3.8f, 3.8f), RustColor);

        CreateHazard("VaultSpikesA", new Vector2(64.3f, -4.03f), new Vector2(2.4f, 0.7f), false);
        CreateHazard("VaultSpikesB", new Vector2(72.2f, -4.03f), new Vector2(2.1f, 0.7f), false);

        CreateEnemy("VaultCrusherA", new Vector2(63.4f, -3.42f), 61.2f, 66.2f, 2.25f, "MaskDude", 1.55f);
        CreateEnemy("VaultHunterB", new Vector2(70.5f, -3.42f), 68.8f, 73.0f, 2.4f, "NinjaFrog", 1.7f);
        CreateEnemy("VaultCrusherB", new Vector2(72.1f, -3.42f), 71.2f, 73.0f, 2.5f, "MaskDude", 1.82f);

        CreateCheckpoint(new Vector2(53.8f, -3.7f), chapterTwoSpawn);
        CreateStorySign(
            new Vector2(56.2f, -3.0f),
            "VAULT",
            "Flooded below.\nTeeth ahead.",
            "The vault is still guarded.\n\nMove carefully. The wardens hit harder here. Reach the tidal seal at the far end.",
            new Vector2(3.4f, 3f));

        CreateExit("TidalSeal", new Vector2(76.2f, 1.45f), new Vector2(2.1f, 3.2f), ExitTrigger.ExitMode.FinalSeal);

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

        Vector3 chapterThreeSpawn = new(85.5f, -3.0f, 0f);
        const float arenaCenterX = 94f;
        const float arenaLeftBound = 83.5f;
        const float arenaRightBound = 104.5f;
        const float arenaFloorY = -4f;

        Color courtBackdrop = new(0.12f, 0.07f, 0.16f, 1f);
        Color courtFloor = new(0.18f, 0.13f, 0.22f, 1f);
        Color courtPillar = new(0.34f, 0.18f, 0.36f, 1f);
        Color courtAccent = new(0.92f, 0.36f, 0.42f, 0.85f);

        CreateVisual("CourtBackdrop", new Vector2(arenaCenterX, -0.6f), new Vector2(50f, 22f), courtBackdrop, false, -16);
        CreateVisual("CourtAccent", new Vector2(arenaCenterX, 3.4f), new Vector2(20f, 0.18f), courtAccent, false, -12);

        // Floor split into two pieces with a 3-unit gate marker at x=99.
        // The current jam ending resolves immediately after the boss, so no post-boss route is built.
        CreateSolid("CourtFloorLeft", new Vector2(90.25f, -4.5f), new Vector2(14.5f, 1f), courtFloor);
        CreateTiledOverlay("CourtFloorLeftTiles", new Vector2(90.25f, -4.5f), new Vector2(14.5f, 1f), "Tiles/PalmIsland", 1, 0);
        CreateSolid("CourtFloorRight", new Vector2(102.75f, -4.5f), new Vector2(4.5f, 1f), courtFloor);
        CreateTiledOverlay("CourtFloorRightTiles", new Vector2(102.75f, -4.5f), new Vector2(4.5f, 1f), "Tiles/PalmIsland", 1, 0);
        Color afterBossGateColor = new(0.62f, 0.36f, 0.18f, 1f);
        CreateSolid("afterbossgate", new Vector2(99f, -4.5f), new Vector2(3f, 1f), afterBossGateColor);
        CreateSolid("CourtLeftWall", new Vector2(arenaLeftBound - 0.5f, -1f), new Vector2(1f, 9f), courtPillar);
        CreateTiledOverlay("CourtLeftWallTiles", new Vector2(arenaLeftBound - 0.5f, -1f), new Vector2(1f, 9f), "Tiles/PalmIsland", 4, 2);
        CreateSolid("CourtRightWall", new Vector2(arenaRightBound + 0.5f, -1f), new Vector2(1f, 9f), courtPillar);
        CreateTiledOverlay("CourtRightWallTiles", new Vector2(arenaRightBound + 0.5f, -1f), new Vector2(1f, 9f), "Tiles/PalmIsland", 4, 2);
        CreateSolid("CourtCeiling", new Vector2(arenaCenterX, 4.4f), new Vector2(22f, 0.6f), courtPillar);
        CreateTiledOverlay("CourtCeilingTiles", new Vector2(arenaCenterX, 4.4f), new Vector2(22f, 0.6f), "Tiles/PalmIsland", 4, 2);

        // Kill plane lives well below the court so falling out of the arena respawns at the chapter 3 checkpoint.
        CreateHazard("CourtKillPlane", new Vector2(120f, -50f), new Vector2(80f, 4f), true);

        CreateCheckpoint(new Vector2(85.4f, -3.7f), chapterThreeSpawn);
        CreateStorySign(
            new Vector2(86.5f, -3.0f),
            "WARDEN",
            "Stun-immune.\nStrike during recovery.",
            "The demon cannot be stunned.\n\nDodge the cleave. Strike when it recovers.",
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
        bossObject.transform.localScale = new Vector3(1.3f, 1.3f, 1f);

        bossObject.AddComponent<SpriteRenderer>();
        bossObject.AddComponent<BoxCollider2D>();
        bossObject.AddComponent<Rigidbody2D>();

        BossController boss = bossObject.AddComponent<BossController>();
        boss.Configure(gameState, worldRoot, leftBound, rightBound, groundY);
        bossObject.AddComponent<BossSpriteAnimator>();

        CreateChildVisual(bossObject.transform, "BossShadow", new Vector2(0f, -0.62f), new Vector2(0.92f, 0.16f), new Color(0.05f, 0.02f, 0.08f, 0.55f), 8);
        CreateChildVisual(bossObject.transform, "BossCrest", new Vector2(0f, 0.62f), new Vector2(0.42f, 0.12f), new Color(1f, 0.84f, 0.32f, 0.95f), 12);
    }

    private static readonly Dictionary<string, Sprite> CachedBackdropSprites = new();

    private static Sprite GetBackdropSprite(string resourcePath, float pixelsPerUnit)
    {
        if (CachedBackdropSprites.TryGetValue(resourcePath, out Sprite cached))
        {
            return cached;
        }

        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            return null;
        }

        texture.filterMode = FilterMode.Point;
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit);
        CachedBackdropSprites[resourcePath] = sprite;
        return sprite;
    }

    private void CreateTiledOverlay(string objectName, Vector2 position, Vector2 worldSize, string sheetPath, int column, int row, int tilePixelSize = 32, int sortingOrder = 6)
    {
        ResolveDesertTile(objectName, ref sheetPath, ref column, ref row);
        Sprite tile = TileSheetSlicer.GetTile(sheetPath, column, row, tilePixelSize, tilePixelSize, tilePixelSize);
        if (tile == null)
        {
            return;
        }

        GameObject overlay = new(objectName);
        overlay.transform.SetParent(worldRoot, false);
        overlay.transform.position = new Vector3(position.x, position.y, 0f);
        overlay.transform.localScale = Vector3.one;

        SpriteRenderer renderer = overlay.AddComponent<SpriteRenderer>();
        renderer.sprite = tile;
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.size = worldSize;
        renderer.sortingOrder = sortingOrder;
    }

    private static void ResolveDesertTile(string objectName, ref string sheetPath, ref int column, ref int row)
    {
        if (sheetPath != "Tiles/PalmIsland")
        {
            return;
        }

        bool isWall = objectName.Contains("Wall") ||
                      objectName.Contains("Boundary") ||
                      objectName.Contains("Column") ||
                      objectName.Contains("Lip") ||
                      objectName.Contains("Pedestal") ||
                      objectName.Contains("Ceiling");

        if (objectName.Contains("Vault") || objectName.Contains("Court"))
        {
            sheetPath = "Desert/BrickTiles";
            column = isWall ? 1 : 1;
            row = isWall ? 0 : 1;
            return;
        }

        sheetPath = "Desert/MainTiles";
        column = isWall ? 0 : 2;
        row = isWall ? 2 : 0;
    }

    private void AddTextureBackdrop(string objectName, string resourcePath, Vector2 position, Vector2 targetSize, int sortingOrder, float alpha, float pixelsPerUnit = 16f)
    {
        Sprite sprite = GetBackdropSprite(resourcePath, pixelsPerUnit);
        if (sprite == null)
        {
            return;
        }

        GameObject backdrop = new(objectName);
        backdrop.transform.SetParent(worldRoot, false);
        backdrop.transform.position = new Vector3(position.x, position.y, 0f);

        float spriteWidth = sprite.bounds.size.x;
        float spriteHeight = sprite.bounds.size.y;
        float scaleX = spriteWidth > 0f ? targetSize.x / spriteWidth : 1f;
        float scaleY = spriteHeight > 0f ? targetSize.y / spriteHeight : 1f;
        backdrop.transform.localScale = new Vector3(scaleX, scaleY, 1f);

        SpriteRenderer renderer = backdrop.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = new Color(1f, 1f, 1f, alpha);
        renderer.sortingOrder = sortingOrder;
    }

    private void AddTextureRegionBackdrop(string objectName, string resourcePath, Rect normalizedTopLeftRect, Vector2 position, Vector2 targetSize, int sortingOrder, float alpha, float pixelsPerUnit = 32f)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            return;
        }

        texture.filterMode = FilterMode.Point;
        float x = Mathf.Clamp(normalizedTopLeftRect.x * texture.width, 0f, texture.width - 1f);
        float yTop = Mathf.Clamp(normalizedTopLeftRect.y * texture.height, 0f, texture.height - 1f);
        float width = Mathf.Clamp(normalizedTopLeftRect.width * texture.width, 1f, texture.width - x);
        float height = Mathf.Clamp(normalizedTopLeftRect.height * texture.height, 1f, texture.height - yTop);
        Rect unityRect = new(x, texture.height - yTop - height, width, height);
        Sprite sprite = Sprite.Create(texture, unityRect, new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.FullRect);

        GameObject backdrop = new(objectName);
        backdrop.transform.SetParent(worldRoot, false);
        backdrop.transform.position = new Vector3(position.x, position.y, 0f);

        float spriteWidth = sprite.bounds.size.x;
        float spriteHeight = sprite.bounds.size.y;
        float scaleX = spriteWidth > 0f ? targetSize.x / spriteWidth : 1f;
        float scaleY = spriteHeight > 0f ? targetSize.y / spriteHeight : 1f;
        backdrop.transform.localScale = new Vector3(scaleX, scaleY, 1f);

        SpriteRenderer renderer = backdrop.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = new Color(1f, 1f, 1f, alpha);
        renderer.sortingOrder = sortingOrder;
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

    private void CreateElder(Vector2 position)
    {
        GameObject elderObject = new("VillageElder");
        elderObject.transform.SetParent(worldRoot, false);
        elderObject.transform.position = position;
        elderObject.transform.localScale = new Vector3(1.42f, 1.42f, 1f);
        elderObject.layer = WaterLayer;

        elderObject.AddComponent<SpriteRenderer>();
        elderObject.AddComponent<VillagerSpriteAnimator>();

        BoxCollider2D collider2D = elderObject.AddComponent<BoxCollider2D>();
        collider2D.size = new Vector2(1.65f, 1.95f);
        collider2D.offset = new Vector2(0f, 0.15f);
        collider2D.isTrigger = true;

        CreateChildVisual(elderObject.transform, "GroundShadow", new Vector2(0f, -0.72f), new Vector2(0.6f, 0.1f), new Color(0.11f, 0.08f, 0.06f, 0.28f), 8);

        StoryTrigger trigger = elderObject.AddComponent<StoryTrigger>();
        trigger.Configure(
            gameState,
            "VILLAGE ELDER",
            "Please... the wells are dry. The fire demon took the water below the aqueduct.\n\nFind the old valves. Open the gate. Bring the rain back to us.",
            true,
            "Press W to listen");
    }

    private void CreateDesertProp(string objectName, Vector2 position, int column, int row, Vector2 size)
    {
        Sprite sprite = TileSheetSlicer.GetTile("Desert/MainTiles", column, row, 32, 32, 32f);
        if (sprite == null)
        {
            return;
        }

        GameObject prop = new(objectName);
        prop.transform.SetParent(worldRoot, false);
        prop.transform.position = new Vector3(position.x, position.y, 0f);

        float scaleX = sprite.bounds.size.x > 0f ? size.x / sprite.bounds.size.x : 1f;
        float scaleY = sprite.bounds.size.y > 0f ? size.y / sprite.bounds.size.y : 1f;
        prop.transform.localScale = new Vector3(scaleX, scaleY, 1f);

        SpriteRenderer renderer = prop.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 7;
    }

    private void CreateValve(string objectName, Vector2 position, string label)
    {
        GameObject valveRoot = new(objectName);
        valveRoot.transform.SetParent(worldRoot, false);
        valveRoot.transform.position = position;

        valveRoot.AddComponent<SpriteRenderer>();
        valveRoot.AddComponent<BoxCollider2D>();

        GameObject glow = new("Glow");
        glow.transform.SetParent(valveRoot.transform, false);
        glow.transform.localPosition = new Vector3(0f, 0.31f, 0f);
        SpriteRenderer glowRenderer = glow.AddComponent<SpriteRenderer>();
        Texture2D glowTex = Resources.Load<Texture2D>("Tiles/ValveGlow");
        if (glowTex != null)
        {
            glowTex.filterMode = FilterMode.Point;
            glowRenderer.sprite = Sprite.Create(glowTex, new Rect(0f, 0f, glowTex.width, glowTex.height), new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect);
        }
        else
        {
            glowRenderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
            glowRenderer.color = WaterColor;
        }
        glowRenderer.sortingOrder = 9;

        ValveController valve = valveRoot.AddComponent<ValveController>();
        valve.Configure(label, gameState, glowRenderer);
    }

    private void CreateHazard(string objectName, Vector2 position, Vector2 size, bool instantRespawn)
    {
        GameObject hazard = CreateVisual(objectName, position, size, instantRespawn ? new Color(0.15f, 0.1f, 0.08f, 0.01f) : new Color(0.16f, 0.13f, 0.18f, 0.85f), false, 2);
        hazard.layer = WaterLayer;

        BoxCollider2D collider2D = hazard.AddComponent<BoxCollider2D>();
        collider2D.isTrigger = true;

        HazardZone zone = hazard.AddComponent<HazardZone>();
        zone.Configure(1, instantRespawn, 0.75f);

        if (!instantRespawn)
        {
            int spikeCount = Mathf.Max(3, Mathf.RoundToInt(size.x * 1.4f));
            for (int i = 0; i < spikeCount; i++)
            {
                float localX = -0.5f + (i + 0.5f) / spikeCount;
                CreateChildSpike(hazard.transform, $"Spike{i}", new Vector2(localX, 0.35f), new Vector2(0.95f / spikeCount, 0.85f));
            }
        }
    }

    private static Sprite cachedSpikeSprite;

    private void CreateChildSpike(Transform parent, string objectName, Vector2 localPosition, Vector2 size)
    {
        GameObject spike = new(objectName);
        spike.transform.SetParent(parent, false);
        spike.transform.localPosition = localPosition;
        spike.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = spike.AddComponent<SpriteRenderer>();
        if (cachedSpikeSprite == null)
        {
            Texture2D texture = Resources.Load<Texture2D>("Tiles/Spike");
            if (texture != null)
            {
                texture.filterMode = FilterMode.Point;
                cachedSpikeSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect);
            }
        }
        renderer.sprite = cachedSpikeSprite != null ? cachedSpikeSprite : PrimitiveSpriteLibrary.SquareSprite;
        renderer.color = cachedSpikeSprite != null ? Color.white : new Color(0.45f, 0.45f, 0.55f, 1f);
        renderer.sortingOrder = 3;
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

        GameObject pillar = new("Pillar");
        pillar.transform.SetParent(checkpoint.transform, false);
        pillar.transform.localPosition = Vector3.zero;
        SpriteRenderer pillarRenderer = pillar.AddComponent<SpriteRenderer>();
        Texture2D pillarTex = Resources.Load<Texture2D>("Tiles/Checkpoint");
        if (pillarTex != null)
        {
            pillarTex.filterMode = FilterMode.Point;
            pillarRenderer.sprite = Sprite.Create(pillarTex, new Rect(0f, 0f, pillarTex.width, pillarTex.height), new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect);
        }
        else
        {
            pillarRenderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
            pillarRenderer.color = StoneColor;
        }
        pillarRenderer.sortingOrder = 5;

        GameObject beacon = new("Beacon");
        beacon.transform.SetParent(checkpoint.transform, false);
        beacon.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        SpriteRenderer beaconRenderer = beacon.AddComponent<SpriteRenderer>();
        Texture2D beaconTex = Resources.Load<Texture2D>("Tiles/CheckpointBeacon");
        if (beaconTex != null)
        {
            beaconTex.filterMode = FilterMode.Point;
            beaconRenderer.sprite = Sprite.Create(beaconTex, new Rect(0f, 0f, beaconTex.width, beaconTex.height), new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect);
        }
        else
        {
            beaconRenderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
        }
        beaconRenderer.sortingOrder = 7;

        CheckpointTrigger trigger = checkpoint.AddComponent<CheckpointTrigger>();
        trigger.Configure(respawnPoint, gameState, beaconRenderer);
    }

    private void CreateStorySign(Vector2 position, string title, string visibleText, string triggerText, Vector2 triggerSize)
    {
        GameObject sign = new("StorySign");
        sign.transform.SetParent(worldRoot, false);
        sign.transform.position = position;
        sign.layer = WaterLayer;

        SpriteRenderer signRenderer = sign.AddComponent<SpriteRenderer>();
        Texture2D signTex = Resources.Load<Texture2D>("Tiles/Sign");
        if (signTex != null)
        {
            signTex.filterMode = FilterMode.Point;
            signRenderer.sprite = Sprite.Create(signTex, new Rect(0f, 0f, signTex.width, signTex.height), new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect);
        }
        else
        {
            signRenderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
            signRenderer.color = RustColor;
        }
        signRenderer.sortingOrder = 5;

        BoxCollider2D collider2D = sign.AddComponent<BoxCollider2D>();
        collider2D.isTrigger = true;
        collider2D.size = triggerSize;

        StoryTrigger storyTrigger = sign.AddComponent<StoryTrigger>();
        storyTrigger.Configure(gameState, title, triggerText, true, "Press W to read");
    }

    private void CreateExit(string objectName, Vector2 position, Vector2 triggerSize, ExitTrigger.ExitMode exitMode = ExitTrigger.ExitMode.AdvanceChapter)
    {
        GameObject exitRoot = new(objectName);
        exitRoot.transform.SetParent(worldRoot, false);
        exitRoot.transform.position = position;
        exitRoot.layer = WaterLayer;

        CreateChildVisual(exitRoot.transform, "PortalInterior", new Vector2(0f, -0.4f), new Vector2(1.6f, 3.5f), WaterColor, 4);
        CreateChildVisual(exitRoot.transform, "ArchLeft", new Vector2(-0.9f, -0.4f), new Vector2(0.5f, 3.5f), StoneColor, 6);
        CreateChildVisual(exitRoot.transform, "ArchRight", new Vector2(0.9f, -0.4f), new Vector2(0.5f, 3.5f), StoneColor, 6);
        CreateChildVisual(exitRoot.transform, "ArchTop", new Vector2(0f, 1.3f), new Vector2(2.4f, 0.55f), StoneColor, 6);
        CreateChildVisual(exitRoot.transform, "ArchKeystone", new Vector2(0f, 1.3f), new Vector2(0.55f, 0.7f), new Color(StoneColor.r * 1.15f, StoneColor.g * 1.15f, StoneColor.b * 1.15f, 1f), 7);

        BoxCollider2D collider2D = exitRoot.AddComponent<BoxCollider2D>();
        collider2D.isTrigger = true;
        collider2D.size = triggerSize;

        ExitTrigger exitTrigger = exitRoot.AddComponent<ExitTrigger>();
        exitTrigger.Configure(gameState, exitMode);
    }

    private void BuildPostBossDepths(float originX, float originY)
    {
        Color depthsBackdrop = new(0.06f, 0.04f, 0.10f, 1f);
        Color depthsFloor = new(0.18f, 0.13f, 0.22f, 1f);
        Color depthsWall = new(0.34f, 0.18f, 0.36f, 1f);
        Color depthsPlatform = new(0.22f, 0.15f, 0.26f, 1f);
        Color depthsDoor = new(0.62f, 0.36f, 0.18f, 1f);

        CreateVisual("DepthsBackdrop", new Vector2(originX + 25f, originY - 17f), new Vector2(60f, 38f), depthsBackdrop, false, -16);

        // Local helpers: mirror X (eastward), translate by origin, then build with palm-island tiles.
        void Slab(string name, float x, float y, float w, float h, Color color)
        {
            CreateBlockoutSurface(name, new Vector2(originX - x, originY + y), new Vector2(w, h), color);
        }

        void OneWay(string name, float x, float y, float w, float h, Color color)
        {
            CreateOneWayPlatform(name, new Vector2(originX - x, originY + y), new Vector2(w, h), color);
        }

        // Start area (where the player drops in)
        Slab("Depths_StartRightWall", 2f, -6f, 1f, 13f, depthsWall);
        Slab("Depths_StartLeftWall", -2f, -3.76f, 1f, 11f, depthsWall);

        // Corridor 1 with pit and one-way platform
        Slab("Depths_Corr1Ceiling", -6f, -8.83f, 9f, 1f, depthsWall);
        Slab("Depths_Corr1FloorRight", 0f, -12f, 5f, 1f, depthsFloor);
        Slab("Depths_Corr1FloorLeft", -9f, -12f, 3f, 1f, depthsFloor);
        Slab("Depths_Corr1PitRightWall", -2.5f, -13f, 1f, 3f, depthsWall);
        Slab("Depths_Corr1PitLeftWall", -7.5f, -13f, 1f, 3f, depthsWall);
        Slab("Depths_Corr1PitFloor", -5f, -15f, 6f, 1f, depthsFloor);
        OneWay("Depths_Corr1PitOneWay", -5f, -13.5f, 2f, 0.5f, depthsPlatform);

        // Central chamber walls + ceiling + floor
        Slab("Depths_CentralCeiling", -18f, -4.66f, 17f, 1f, depthsWall);
        Slab("Depths_CentralRightWallUpper", -10f, -6.71f, 1f, 5f, depthsWall);
        Slab("Depths_CentralLeftWallUpper", -26f, -7.07f, 1f, 5f, depthsWall);
        Slab("Depths_CentralRightWallLower", -10f, -21f, 1f, 19f, depthsWall);
        Slab("Depths_CentralLeftWallLower", -26f, -21f, 1f, 19f, depthsWall);
        Slab("Depths_CentralFloor", -18f, -31f, 17f, 1f, depthsFloor);

        // Climbing platforms in the central chamber
        Slab("Depths_PlatColA1", -22.97f, -14.02f, 4f, 0.5f, depthsPlatform);
        Slab("Depths_PlatColB1", -13.08f, -14.05f, 4f, 0.5f, depthsPlatform);
        Slab("Depths_PlatColA2", -23.01f, -18.68f, 4f, 0.5f, depthsPlatform);
        Slab("Depths_PlatColB2", -13.12f, -18.71f, 4f, 0.5f, depthsPlatform);
        Slab("Depths_PlatColA3", -22.88f, -23.16f, 4f, 0.5f, depthsPlatform);
        Slab("Depths_PlatColB3", -12.99f, -23.19f, 4f, 0.5f, depthsPlatform);
        Slab("Depths_PlatColA4", -22.84f, -27.73f, 4f, 0.5f, depthsPlatform);
        Slab("Depths_PlatColB4", -12.95f, -27.76f, 4f, 0.5f, depthsPlatform);
        Slab("Depths_PlatColC4", -21f, -29.31f, 4f, 0.5f, depthsPlatform);
        Slab("Depths_PlatColD4", -15.51f, -29.33f, 4f, 0.5f, depthsPlatform);
        Slab("Depths_PlatColC3", -21.04f, -24.74f, 4f, 0.5f, depthsPlatform);
        Slab("Depths_PlatColD3", -15.55f, -24.76f, 4f, 0.5f, depthsPlatform);
        Slab("Depths_PlatColC2", -21.17f, -20.26f, 4f, 0.5f, depthsPlatform);
        Slab("Depths_PlatColD2", -15.68f, -20.28f, 4f, 0.5f, depthsPlatform);
        Slab("Depths_PlatColC1", -21.13f, -15.6f, 4f, 0.5f, depthsPlatform);
        Slab("Depths_PlatColD1", -15.64f, -15.62f, 4f, 0.5f, depthsPlatform);
        Slab("Depths_PlatWide1", -17.995f, -12.32f, 13.031f, 0.5f, depthsPlatform);
        Slab("Depths_PlatWide2", -18.035f, -16.98f, 13.031f, 0.5f, depthsPlatform);
        Slab("Depths_PlatWide3", -17.905f, -21.46f, 13.031f, 0.5f, depthsPlatform);
        Slab("Depths_PlatWide4", -17.865f, -26.03f, 13.031f, 0.5f, depthsPlatform);

        // Corridor 2 leading to the breakable door
        Slab("Depths_Corr2Ceiling", -28.5f, -9.22f, 6f, 1f, depthsWall);
        Slab("Depths_Corr2Floor", -38.386f, -12f, 25.773f, 1f, depthsFloor);
        CreateBreakableBarrier("Depths_BreakableDoor", new Vector2(originX - (-28.5f), originY + (-10.56f)), new Vector2(0.5f, 2f), depthsDoor);

        // Sealed final chamber (FarLeft in the original blockout = far east here)
        Slab("Depths_FarCeiling", -41f, -4.12f, 20f, 1f, depthsWall);
        Slab("Depths_FarNearWall", -31f, -6.52f, 1f, 6f, depthsWall);
        Slab("Depths_FarEndWall", -51f, -8.444f, 1f, 7.888f, depthsWall);
    }

    private void CreateBlockoutSurface(string objectName, Vector2 position, Vector2 size, Color color)
    {
        CreateSolid(objectName, position, size, color);
        bool vertical = size.y > size.x;
        int column = vertical ? 4 : 1;
        int row = vertical ? 2 : 0;
        CreateTiledOverlay(objectName + "Tiles", position, size, "Tiles/PalmIsland", column, row);
    }

    private void CreateBreakableBarrier(string objectName, Vector2 position, Vector2 size, Color color)
    {
        GameObject doorObject = new(objectName);
        doorObject.transform.SetParent(worldRoot, false);
        doorObject.transform.position = new Vector3(position.x, position.y, 0f);
        doorObject.layer = GroundLayer;

        SpriteRenderer doorRenderer = doorObject.AddComponent<SpriteRenderer>();
        doorRenderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
        doorRenderer.color = color;
        doorRenderer.drawMode = SpriteDrawMode.Sliced;
        doorRenderer.size = size;
        doorRenderer.sortingOrder = 5;

        BoxCollider2D doorCollider = doorObject.AddComponent<BoxCollider2D>();
        doorCollider.size = size;

        doorObject.AddComponent<BreakableDoor>();
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

[RequireComponent(typeof(SpriteRenderer))]
public class VillagerSpriteAnimator : MonoBehaviour
{
    private static readonly Dictionary<string, Sprite[]> CachedAnimations = new();

    private SpriteRenderer spriteRenderer;
    private Sprite[] activeFrames;
    private float nextFrameAt;
    private int frameIndex;

    private const string ResourceFolder = "Villagers/OldMan";
    private const int CellSize = 48;
    private const float PixelsPerUnit = 34f;
    private const float IdleFrameDuration = 0.28f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 10;
    }

    private void Start()
    {
        activeFrames = LoadFrames("Idle");
        frameIndex = 0;
        spriteRenderer.sprite = activeFrames.Length > 0 ? activeFrames[0] : PrimitiveSpriteLibrary.SquareSprite;
        nextFrameAt = Time.time + IdleFrameDuration;
    }

    private void Update()
    {
        if (activeFrames == null || activeFrames.Length == 0 || Time.time < nextFrameAt)
        {
            return;
        }

        frameIndex = (frameIndex + 1) % activeFrames.Length;
        spriteRenderer.sprite = activeFrames[frameIndex];
        nextFrameAt = Time.time + IdleFrameDuration;
    }

    private static Sprite[] LoadFrames(string clipName)
    {
        string cacheKey = $"{ResourceFolder}/{clipName}";
        if (CachedAnimations.TryGetValue(cacheKey, out Sprite[] frames))
        {
            return frames;
        }

        Texture2D texture = Resources.Load<Texture2D>(cacheKey);
        if (texture == null)
        {
            texture = Resources.Load<Texture2D>($"{ResourceFolder}/Still");
        }

        if (texture == null)
        {
            Debug.LogWarning($"Villager sprite sheet not found at Resources/{cacheKey}.");
            frames = new[] { PrimitiveSpriteLibrary.SquareSprite };
            CachedAnimations[cacheKey] = frames;
            return frames;
        }

        texture.filterMode = FilterMode.Point;
        int frameCount = Mathf.Max(1, texture.width / CellSize);
        frames = new Sprite[frameCount];

        for (int i = 0; i < frameCount; i++)
        {
            Rect rect = new(i * CellSize, 0f, CellSize, CellSize);
            Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = $"OldMan_{clipName}_{i}";
            frames[i] = sprite;
        }

        CachedAnimations[cacheKey] = frames;
        return frames;
    }
}
