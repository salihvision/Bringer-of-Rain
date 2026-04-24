using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ExitTrigger : MonoBehaviour
{
    private GameStateController gameState;

    public void Configure(GameStateController controller)
    {
        gameState = controller;
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

        if (gameState != null && gameState.CanExit)
        {
            gameState.CompleteGame();
        }
        else
        {
            gameState?.ShowTransientMessage("Restore the twin valves to awaken the reservoir gate.", 2.6f);
        }
    }
}
