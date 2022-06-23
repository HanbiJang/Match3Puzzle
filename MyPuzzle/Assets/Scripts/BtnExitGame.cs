using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtnExitGame : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void BtnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
