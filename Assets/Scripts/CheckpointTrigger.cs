using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CheckpointTrigger : MonoBehaviour
{
    private GameStateController gameState;
    private SpriteRenderer beaconRenderer;
    private Vector3 checkpointPosition;
    private bool activated;

    private static readonly Color ActiveColor = new(1f, 1f, 1f, 1f);

    public void Configure(Vector3 checkpoint, GameStateController controller, SpriteRenderer beacon)
    {
        checkpointPosition = checkpoint;
        gameState = controller;
        beaconRenderer = beacon;
        if (beaconRenderer != null)
        {
            beaconRenderer.enabled = false;
        }
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
            GameAudioController.Play(AudioCue.Checkpoint);
            beaconRenderer.enabled = true;
            beaconRenderer.color = ActiveColor;
        }
    }
}
