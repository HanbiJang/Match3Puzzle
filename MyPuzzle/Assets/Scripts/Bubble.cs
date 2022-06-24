using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BubbleType {
    Normal,
    Ice,
    Bomb,
    Lock,
}

public enum BubbleState {
    UnMatched,
    Matched,
}

public class Bubble : MonoBehaviour
{

    public BubbleType m_type;
    public BubbleState m_state;
    bool isSelected; //선택 되었는지 여부

    void Awake() {
        isSelected = false;
    }

    bool GetIsSelected() { return isSelected; }
    void SetIsSelected(bool b) { isSelected = b; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
