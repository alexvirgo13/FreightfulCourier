using UnityEngine;
using UnityEngine.InputSystem;

public class PauseController : MonoBehaviour
{
    [SerializeField] private Canvas pauseMenu;

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current != null 
            && Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePause();
    }

    public void TogglePause()
    {
        pauseMenu.enabled = !pauseMenu.enabled;
        Time.timeScale = pauseMenu.enabled?0f:1f;
    }
}
