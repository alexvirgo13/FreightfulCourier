using UnityEngine;

public class SkinManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.Find("Skins")
                 .GetChild(
                    PlayerPrefs.GetInt("SelectedCharacter", 0)
                ).gameObject
                 .SetActive(true);
    }
}
