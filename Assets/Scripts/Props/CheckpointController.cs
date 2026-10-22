using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float fuelRecharge = 15.0f;
    [SerializeField] private bool isFinishLine = false;
    [SerializeField] private int pointsWorth = 500;

    private const int LAST_LEVEL = 2;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        BoxCollider2D playerCollider = player.GetComponentInChildren<BoxCollider2D>();


        if(collider == playerCollider)
        {
            if(isFinishLine)
            {
                Invoke(nameof(FinishRace), 2);
                player.GetComponent<ScoreManager>().FinalScore();
            }
            else
            {
                player.GetComponentInChildren<MovementController>().AddFuel(fuelRecharge);
                player.GetComponent<ScoreManager>().BonusPoints(pointsWorth);
                fuelRecharge = 0;
            }
        }
    }

    private void FinishRace()
    {
        int currentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex; //MainMenu va delante de los niveles
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        Debug.Log($"Last unlocked: {unlockedLevel}");
        Debug.Log($"Current Index: {currentLevel}");


        if (currentLevel >= LAST_LEVEL)
        {
            MenuController.OpenMenu();
        } 
        else 
        {
            if (unlockedLevel < currentLevel + 1)
                PlayerPrefs.SetInt("UnlockedLevel", currentLevel + 1);

            MenuController.PlayLevel(currentLevel + 1);
        }
    }
}
