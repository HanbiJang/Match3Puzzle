using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageInform : MonoBehaviour
{
    int StageNum;

    public int GetStageNum() {
        return StageNum;
    }

    // Start is called before the first frame update
    void Start()
    {
        StageNum = 1;
    }


}
