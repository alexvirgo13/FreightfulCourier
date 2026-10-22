using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreboard;
    [SerializeField] int lossPerSecond = 25;

    [SerializeField] private float score = 5000;

    private bool activeCountdown = true;
    private int highscore;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        activeCountdown = true;
        highscore = PlayerPrefs.GetInt(
            $"HighscoreLvl{SceneManager.GetActiveScene().buildIndex}",
            0
        );
    }

    // Update is called once per frame
    void Update()
    {
        if (activeCountdown) DecreaseScore();
    }

    private void DecreaseScore ()
    {
        score -= Time.deltaTime * lossPerSecond;
        scoreboard.text = Mathf.Floor(score).ToString();
    }

    public void BonusPoints (int points)
    {
        if(points > 0)
        {
            score += points;
            scoreboard.text = Mathf.Floor(score).ToString();
        }
    }

    public void FinalScore()
    {
        activeCountdown = false;

        if (highscore < score)
        {
            PlayerPrefs.SetInt(
                $"HighscoreLvl{SceneManager.GetActiveScene().buildIndex}",
                (int) score
            );

            scoreboard.color = new Color(1.0f, 1.0f, 1.0f);
            //TODO: Show message congratulating
        }
    }

    internal void FreezeCounter()
    {
        activeCountdown = false;
    }

    internal void UnfreezeCounter()
    {
        activeCountdown = true;
    }
}
