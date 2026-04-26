using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1000)]
public class LevelTestBootstrap : MonoBehaviour
{
    public InputActionAsset inputActions;
    public Vector3 spawnPoint = new Vector3(0f, 0f, 0f);

    private void Awake()
    {
        if (inputActions == null)
        {
            Debug.LogError("LevelTestBootstrap requires the project InputActionAsset.");
            enabled = false;
            return;
        }

        GameAudioController.EnsureInScene();

        GameStateController gameState = gameObject.AddComponent<GameStateController>();
        gameState.Initialize(0, spawnPoint);

        PlayerController player = BuildPlayer(spawnPoint, gameState);
        BuildCamera(player.transform);

        gameState.SetPlayer(player);
    }

    private PlayerController BuildPlayer(Vector3 spawnPosition, GameStateController gameState)
    {
        GameObject playerObject = new GameObject("TideBearer");
        playerObject.transform.position = spawnPosition;

        SpriteRenderer bodyRenderer = playerObject.AddComponent<SpriteRenderer>();
        bodyRenderer.sortingOrder = 10;
        bodyRenderer.sprite = PrimitiveSpriteLibrary.SquareSprite;

        Rigidbody2D body = playerObject.AddComponent<Rigidbody2D>();
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        CapsuleCollider2D capsule = playerObject.AddComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(0.95f, 1.8f);

        PlayerController controller = playerObject.AddComponent<PlayerController>();
        controller.Configure(inputActions, 1 << 3, 1 << 0, gameState, spawnPosition);
        playerObject.AddComponent<CharacterSpriteAnimator>();

        GameObject glow = new GameObject("BackGlow");
        glow.transform.SetParent(playerObject.transform, false);
        glow.transform.localPosition = new Vector3(-0.08f, -0.04f, 0f);
        glow.transform.localScale = new Vector3(0.52f, 0.96f, 1f);
        SpriteRenderer glowRenderer = glow.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
        glowRenderer.color = new Color(0.18f, 0.86f, 1f, 0.15f);
        glowRenderer.sortingOrder = 10;

        return controller;
    }

    private void BuildCamera(Transform playerTarget)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camera = camObj.AddComponent<Camera>();
        }

        camera.backgroundColor = new Color(0.95f, 0.82f, 0.58f, 1f);
        camera.orthographicSize = 5.4f;

        SimpleCameraFollow follow = camera.GetComponent<SimpleCameraFollow>();
        if (follow == null)
        {
            follow = camera.gameObject.AddComponent<SimpleCameraFollow>();
        }

        follow.Configure(playerTarget, new Vector3(2.5f, 1f, -10f), 4.2f, new Vector2(-1000f, 1000f));
    }
}
