using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BtnBackGame : MonoBehaviour
{
    string CurSceneName;
    string PrevSceneName_Stage;
    string PrevSceneName_Game;

    // Start is called before the first frame update
    void Start()
    {
        CurSceneName = SceneManager.GetActiveScene().name;
        PrevSceneName_Stage = "StartScene";
        PrevSceneName_Game = "StageScene";
    }

    public void BtnBack()
    {
        switch (CurSceneName) {
            case "StageScene":
                SceneManager.LoadScene(PrevSceneName_Stage);
                break;
            case "GameScene":
                SceneManager.LoadScene(PrevSceneName_Game);
                break;
        
        }
        
    }
}
