using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class StoryTrigger : MonoBehaviour
{
    private GameStateController gameState;
    private string message;
    private float duration;
    private bool fireOnce;
    private bool fired;

    public void Configure(GameStateController controller, string storyMessage, float messageDuration, bool onlyOnce)
    {
        gameState = controller;
        message = storyMessage;
        duration = messageDuration;
        fireOnce = onlyOnce;
    }

    private void Awake()
    {
        BoxCollider2D collider2D = GetComponent<BoxCollider2D>();
        collider2D.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((!fireOnce || !fired) && other.TryGetComponent(out PlayerController _))
        {
            fired = true;
            gameState?.ShowStoryMessage(message, duration);
        }
    }
}
