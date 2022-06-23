using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BtnStage : MonoBehaviour
{
    string SceneName;
    int MaxStageNum;
    int StageNum;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.GetComponent<StageInform>())
            gameObject.GetComponent<StageInform>().GetStageNum();
        MaxStageNum = 3;
        SceneName = "GameScene";
    }

    // Update is called once per frame
    public void BtnStageGame()
    {
        SceneManager.LoadScene(SceneName);
    }
}
