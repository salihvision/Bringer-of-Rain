using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ValveController : MonoBehaviour, IWaterReactive
{
    private GameStateController gameState;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer glowRenderer;
    private string valveLabel;
    private bool activated;

    private static readonly Color InactiveColor = new(0.85f, 0.85f, 0.92f, 1f);
    private static readonly Color ActiveColor = new(0.7f, 0.95f, 1f, 1f);
    private static Sprite cachedValveSprite;

    public void Configure(string label, GameStateController controller, SpriteRenderer glow)
    {
        valveLabel = label;
        gameState = controller;
        glowRenderer = glow;

        if (glowRenderer != null)
        {
            glowRenderer.enabled = false;
        }
    }

    public void ReactToWaterBurst(WaterBurstData burst)
    {
        if (activated)
        {
            return;
        }

        activated = true;
        spriteRenderer.color = ActiveColor;
        GameAudioController.Play(AudioCue.ValveActivated);

        if (glowRenderer != null)
        {
            glowRenderer.enabled = true;
            glowRenderer.color = new Color(ActiveColor.r, ActiveColor.g, ActiveColor.b, 0.65f);
        }

        gameState?.RegisterValveActivated(valveLabel);
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (cachedValveSprite == null)
        {
            Texture2D tex = Resources.Load<Texture2D>("Tiles/Valve");
            if (tex != null)
            {
                tex.filterMode = FilterMode.Point;
                cachedValveSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect);
            }
        }
        spriteRenderer.sprite = cachedValveSprite != null ? cachedValveSprite : PrimitiveSpriteLibrary.SquareSprite;
        spriteRenderer.color = InactiveColor;
        spriteRenderer.sortingOrder = 8;

        BoxCollider2D collider2D = GetComponent<BoxCollider2D>();
        collider2D.isTrigger = true;
        collider2D.size = new Vector2(0.85f, 1.5f);
    }
}
