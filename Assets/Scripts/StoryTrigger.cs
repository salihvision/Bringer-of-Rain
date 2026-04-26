using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class StoryTrigger : MonoBehaviour
{
    private GameStateController gameState;
    private string title;
    private string body;
    private string prompt;
    private bool lockPlayerInput;

    public void Configure(GameStateController controller, string storyTitle, string storyBody, bool lockInput = true, string promptText = "Press W to read")
    {
        gameState = controller;
        title = storyTitle;
        body = storyBody;
        lockPlayerInput = lockInput;
        prompt = promptText;
    }

    public void Configure(GameStateController controller, string storyMessage, float messageDuration, bool onlyOnce)
    {
        Configure(controller, string.Empty, storyMessage);
    }

    private void Awake()
    {
        BoxCollider2D collider2D = GetComponent<BoxCollider2D>();
        collider2D.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController _))
        {
            gameState?.SetReadableTarget(this, prompt);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController _))
        {
            gameState?.ClearReadableTarget(this);
        }
    }

    public void Open()
    {
        gameState?.ShowReadableStory(title, body, lockPlayerInput);
    }
}
