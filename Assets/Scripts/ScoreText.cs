using UnityEngine;
using TMPro;

public class ScoreText : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text scoreShadowText;

    public TMP_Text highscoreText;
    public TMP_Text highscoreShadowText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scoreText.text = "SCORE: " + PlayerPrefs.GetInt("Score") + "00";   
        scoreShadowText.text = "SCORE: " + PlayerPrefs.GetInt("Score") + "00";
        highscoreText.text = "HIGHSCORE: " + PlayerPrefs.GetInt("Highscore") + "00";
        highscoreShadowText.text = "HIGHSCORE: " + PlayerPrefs.GetInt("Highscore") + "00";
    }
}
