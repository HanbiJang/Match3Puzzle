using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BtnStartGame : MonoBehaviour
{
    string GameSceneName;

    // Start is called before the first frame update
    void Start()
    {
        GameSceneName = "StageScene";
    }

    public void BtnStart()
    {
        SceneManager.LoadScene(GameSceneName);
    }

}
