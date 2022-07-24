using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BubbleType {
    Blue,
    Green,
    Orange,
    Red,
    Yellow,
}

public enum BubbleState {
    UnMatched,
    Matched,
}

public class BubbleInfo {
    int row;
    int cul;

    public int GetRow() { return row; }
    public int GetCul() { return cul; }
    public void SetRow(int row_) { row = row_; }
    public void SetCul(int cul_) { cul = cul_; }

    public void Init(int r, int c) {
        row = r;
        cul = c;
    }
}

public class Bubble : MonoBehaviour
{

    public BubbleType m_type;
    public BubbleState m_state;
    bool isSelected; //선택 되었는지 여부
    public BubbleInfo m_info;
    PuzzleSystem puzzleSystem;

    //스캔에 필요한 변수
    public bool visited = false;

    void Awake() {
        isSelected = false;
        m_info = new BubbleInfo();
        puzzleSystem = GameObject.Find("Puzzle System").GetComponent<PuzzleSystem>();
    }

    bool GetIsSelected() { return isSelected; }
    void SetIsSelected(bool b) { isSelected = b; }

    public void BtnBubbleOnClicked() {

        //puzzle system 객체를 찾아내어서 SelectedBubbleIdxs의 값을 바꾼다
        if (puzzleSystem) {

            //row 와 cul 받아오기
            if (puzzleSystem.SelectedBubbleIdxs[0].row == -1 && puzzleSystem.SelectedBubbleIdxs[0].cul == -1)
            {
                puzzleSystem.SelectedBubbleIdxs[0].row = m_info.GetRow();
                puzzleSystem.SelectedBubbleIdxs[0].cul = m_info.GetCul();
                return;
            }
            else 
            {
                if (puzzleSystem.SelectedBubbleIdxs[1].row == -1 && puzzleSystem.SelectedBubbleIdxs[1].cul == -1)
                {
                    puzzleSystem.SelectedBubbleIdxs[1].row = m_info.GetRow();
                    puzzleSystem.SelectedBubbleIdxs[1].cul = m_info.GetCul();
                    return;
                }
            }

        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
