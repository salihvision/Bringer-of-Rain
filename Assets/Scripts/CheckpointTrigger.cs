using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CheckpointTrigger : MonoBehaviour
{
    private GameStateController gameState;
    private SpriteRenderer beaconRenderer;
    private Vector3 checkpointPosition;
    private bool activated;

    private static readonly Color InactiveColor = new(0.36f, 0.27f, 0.2f, 1f);
    private static readonly Color ActiveColor = new(0.16f, 0.86f, 1f, 1f);

    public void Configure(Vector3 checkpoint, GameStateController controller, SpriteRenderer beacon)
    {
        checkpointPosition = checkpoint;
        gameState = controller;
        beaconRenderer = beacon;
        beaconRenderer.color = InactiveColor;
    }

    private void Awake()
    {
        BoxCollider2D collider2D = GetComponent<BoxCollider2D>();
        collider2D.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerController player))
        {
            return;
        }

        player.SetSpawnPoint(checkpointPosition);
        gameState?.SetCheckpoint(checkpointPosition);

        if (!activated && beaconRenderer != null)
        {
            activated = true;
            beaconRenderer.color = ActiveColor;
        }
    }
}
