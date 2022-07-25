using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreSystem : MonoBehaviour
{
    // Start is called before the first frame update
    public Text ScoreText;
    int Score = 0;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
         
    }

    public int GetScore() 
    {
        return Score;
    }

    public void ChangeScore(int Bonus) 
    {
        Score += Bonus;
        ScoreText.text = "Score : " + (Score);
    }
}
