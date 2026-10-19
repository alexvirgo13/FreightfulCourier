using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float fuelRecharge = 15.0f;
    [SerializeField] private int pointsAwarded = 2500;
    [SerializeField] private bool isFinishLine = false;

    private const int LAST_LEVEL = 2;

    void OnTriggerEnter2D(Collider2D collider)
    {
        BoxCollider2D playerCollider = player.GetComponentInChildren<BoxCollider2D>();


        if(collider == playerCollider)
        {
            if(isFinishLine)
            {
                Invoke(nameof(FinishRace), 2);
            }
            else
            {
                player.GetComponentInChildren<MovementController>().AddFuel(fuelRecharge);
            }
        }
    }

    private void FinishRace()
    {
        int currentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        if (currentLevel >= LAST_LEVEL)
        {
            MenuController.OpenMenu();
            return;
        }

        if (unlockedLevel < currentLevel + 1)
            PlayerPrefs.SetInt("UnlockedLevel", currentLevel + 1);

        MenuController.PlayLevel(unlockedLevel + 1);

    }
}
