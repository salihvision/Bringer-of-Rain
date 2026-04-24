using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ExitTrigger : MonoBehaviour
{
    public enum ExitMode
    {
        AdvanceChapter,
        FinalSeal
    }

    private GameStateController gameState;
    private ExitMode exitMode;

    public void Configure(GameStateController controller, ExitMode mode = ExitMode.AdvanceChapter)
    {
        gameState = controller;
        exitMode = mode;
    }

    private void Awake()
    {
        BoxCollider2D collider2D = GetComponent<BoxCollider2D>();
        collider2D.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerController _))
        {
            return;
        }

        if (gameState == null)
        {
            return;
        }

        if (exitMode == ExitMode.FinalSeal)
        {
            gameState.CompleteGame();
            return;
        }

        gameState.ReachReservoirGate();
    }
}
