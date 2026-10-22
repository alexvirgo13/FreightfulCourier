using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    
    public static void OpenMenu()
    {
        // Load the main menu scene 
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1f;
    }
    
    public static void OpenGuide()
    {
        // Load the main menu scene 
        SceneManager.LoadScene("Guide");
        Time.timeScale = 1f;
    }

    public static void PlayGame()
    {
        // Load the game's menu scene
        SceneManager.LoadScene("LevelSelection");
        Time.timeScale = 1f;
    }

    public static void QuitGame()
    {
        // Quit the application
        Application.Quit();
    }

    public static void PlayCredits()
    {
        // Load the credits scene
        SceneManager.LoadScene("Credits");
        Time.timeScale = 1f;
    }

    public static void ChooseCharacter()
    {
        // Load the character selection scene
        SceneManager.LoadScene("CharacterSelection");
        Time.timeScale = 1f;
    }

    public static void PlayLevel(int levelIndex)
    {
        // Load the specified level scene
        SceneManager.LoadScene($"Level{levelIndex}");
        Time.timeScale = 1f;
    }

    public static void Reload()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
        Time.timeScale = 1f;
    }

    public static void SelectCharacter(int characterIndex)
    {
        // Store the selected character index in PlayerPrefs
        PlayerPrefs.SetInt("SelectedCharacter", characterIndex);
        PlayerPrefs.Save(); // Save the PlayerPrefs to ensure the selection is persisted
        OpenMenu();
    }
}
