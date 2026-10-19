using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    [SerializeField] private Transform buttonsParent;
    void Start()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1); // Get the highest unlocked level from PlayerPrefs, default to 1 if not set
        int i = 1;

        Debug.Log($"Last unlocked: {unlockedLevel}");

        foreach (Transform child in buttonsParent)
        {
            child.GetComponent<Button>().interactable = (i <= unlockedLevel); // Enable the button if the level is unlocked, otherwise disable it
            i++;
        }
    }
}
