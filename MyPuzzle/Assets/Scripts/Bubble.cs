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

    public void SetVisited(bool b) { visited = b; }
    public bool GetVisited() { return visited; }

    public BubbleType GetMType() { return m_type; }
    public void SetMType(BubbleType type) { m_type = type; }

    public BubbleInfo GetBubbleInfo() { return m_info; }
    public void SetBubbleInfo(BubbleInfo bi) { m_info = bi; }



    // 버블의 스프라이트
    SpriteRenderer m_Img;
    // 버블의 애니메이터
    Animator m_Animator;

    void Awake() {
        m_info = new BubbleInfo();
        puzzleSystem = GameObject.Find("Puzzle System").GetComponent<PuzzleSystem>();

        m_Img = GetComponentInChildren<SpriteRenderer>();
        m_Animator = GetComponent<Animator>();
    }

    public void BtnBubbleOnClicked() {

        //puzzle system 객체를 찾아내어서 SelectedBubbleIdxs의 값을 바꾼다
        if (puzzleSystem && puzzleSystem.EnableInput) 
        { //입력 가능하면

            //row 와 cul 받아오기
            if (puzzleSystem.SelectedBubbleIdxs[0].row == -1 && puzzleSystem.SelectedBubbleIdxs[0].cul == -1)
            {
                puzzleSystem.SelectedBubbleIdxs[0].row = m_info.GetRow();
                puzzleSystem.SelectedBubbleIdxs[0].cul = m_info.GetCul();
            }
            else 
            {
                if (puzzleSystem.SelectedBubbleIdxs[1].row == -1 && puzzleSystem.SelectedBubbleIdxs[1].cul == -1)
                {
                    puzzleSystem.SelectedBubbleIdxs[1].row = m_info.GetRow();
                    puzzleSystem.SelectedBubbleIdxs[1].cul = m_info.GetCul();
                }
            }

            //puzzleSystem.ChangePosOnceonly = false;

            Debug.Log("Clicked");
        }
    }

/*    IEnumerator TurnBlue() 
    {
        int cnt = 0;
        while ( cnt <= 10) 
        {
            cnt++;
            this.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.blue;
            yield return new WaitForSeconds(0.1f);
        }

        this.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.white;

        yield return null;
    }*/

    public void ChangeTypeAndImg(int type) 
    {
        // 타입 변경
        m_type = (BubbleType)type;
        StartCoroutine(ChangeImgColor(type));

        // 이름도 변경
        Debug.Log(" m_info.GetRow()" + m_info.GetRow() + "  m_info.GetCul() : " + m_info.GetCul());
        gameObject.name = "Bubble" + m_type + " _" + m_info.GetRow() + " _" + m_info.GetCul();
    }

    IEnumerator ChangeImgColor(int type) 
    {
        int cnt = 0;
        float alpha = 1;

        while (cnt < 20) 
        {
            m_Img.color = new Color(m_Img.color.r, m_Img.color.g, m_Img.color.b, alpha);
            alpha -= 0.05f;
            cnt++;
            yield return new WaitForSeconds(0.03f);
        }

        switch (type)
        {
            case 0:
                // 애니메이터 변경              
                m_Animator.runtimeAnimatorController = puzzleSystem.AnimCont_Blue;
                // 이미지 변경
                m_Img.sprite = puzzleSystem.Sprite_Blue;
                break;
            case 1:
                // 애니메이터 변경
                m_Animator.runtimeAnimatorController = puzzleSystem.AnimCont_Green;
                // 이미지 변경
                m_Img.sprite = puzzleSystem.Sprite_Green;
                break;
            case 2:
                // 애니메이터 변경
                m_Animator.runtimeAnimatorController = puzzleSystem.AnimCont_Orange;
                // 이미지 변경
                m_Img.sprite = puzzleSystem.Sprite_Orange;
                break;
            case 3:
                // 애니메이터 변경
                m_Animator.runtimeAnimatorController = puzzleSystem.AnimCont_Red;
                // 이미지 변경
                m_Img.sprite = puzzleSystem.Sprite_Red;
                break;
            case 4:
                // 애니메이터 변경
                m_Animator.runtimeAnimatorController = puzzleSystem.AnimCont_Yellow;
                // 이미지 변경
                m_Img.sprite = puzzleSystem.Sprite_Yellow;
                break;
        }

        cnt = 0;
        alpha = 0;
        while (cnt < 20)
        {
            m_Img.color = new Color(m_Img.color.r, m_Img.color.g, m_Img.color.b, alpha);
            alpha += 0.05f;

            cnt++;
            yield return new WaitForSeconds(0.03f);
        }

        yield return null;
    }


}
