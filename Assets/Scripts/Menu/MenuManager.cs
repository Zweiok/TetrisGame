using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class MenuManager : MonoBehaviour
{
    /// <summary>
    /// scoreItem sample for cloning
    /// </summary>
    [SerializeField] ScoreItem scorePref;

    /// <summary>
    /// gradient for score items
    /// </summary>
    [SerializeField] Gradient scoreGradient;

    private void Start()
    {
        InitScore();
    }

    /// <summary>
    /// initialize score list
    /// </summary>
    void InitScore()
    {
        HashSet<float> scores = new HashSet<float>();
        
        if (PlayerPrefs.HasKey("ScoreData"))
        {
            // gettings scores from prefs
            scores = new HashSet<float>(JsonUtility.FromJson<ScoreData>(PlayerPrefs.GetString("ScoreData", "")).scores.OrderByDescending(x => x));
        }
        
        int count = 0;


        //spawn score items
        foreach (float score in scores)
        {
            count++;
            ScoreItem item = Instantiate(scorePref, scorePref.transform.parent);
            item.score.text = score.ToString();
            item.gameObject.SetActive(true);
            item.image.color = scoreGradient.Evaluate(((float)scores.Count / (float)count) / (float)scores.Count); // setup color for score item
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }
}
