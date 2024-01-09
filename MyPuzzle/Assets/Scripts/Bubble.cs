using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BubbleType 
{
    Blue, //0
    Green,
    Orange,
    Red,
    Yellow,
}

public enum BubbleState 
{
    UnMatched,
    Matched,
}

public class BubbleInfo 
{
    int row;
    int cul;
    BubbleState state; //매치 상태 정보
    BubbleType type; //버블의 타입

    public int GetRow() { return row; }
    public int GetCul() { return cul; }
    public void SetRow(int row_) { row = row_; }
    public void SetCul(int cul_) { cul = cul_; }
    public BubbleState GetState() { return state; }
    public void SetState(BubbleState state_) { state = state_; }
    public BubbleType GetType_() { return type; }
    public void SetType(BubbleType type_) { type = type_; }

    public void Init(int r, int c) {
        row = r;
        cul = c;
        state = BubbleState.UnMatched;
    }
}

public class Bubble : MonoBehaviour
{
    PuzzleSystem puzzleSystem;

    bool isSelected; //사용자에 의해 선택 되었는지 여부
    public BubbleInfo m_info;
    public bool visited = false; //매치 스캔에 필요한 변수

    public BubbleInfo GetBubbleInfo() { return m_info; }
    public void SetBubbleInfo(BubbleInfo binfo) { m_info = binfo; }

    public void SetVisited(bool b) { visited = b; }
    public bool GetVisited() { return visited; }
    
    SpriteRenderer m_Img; //버블의 스프라이트
    Animator m_Animator; //버블의 애니메이터

    void Awake() 
    {
        puzzleSystem = GameObject.Find("Puzzle System").GetComponent<PuzzleSystem>();
        m_info = new BubbleInfo();     
        m_Img = GetComponentInChildren<SpriteRenderer>();
        m_Animator = GetComponent<Animator>();
    }

    //버블을 버튼으로써 활용, 버튼 함수로 등록
    public void BtnBubbleOnClicked() 
    {
        if (puzzleSystem && puzzleSystem.EnableInput) 
        {
            SelectedBubbleIdx firstBubble = puzzleSystem.SelectedBubbleIdxs[0];
            SelectedBubbleIdx secondBubble = puzzleSystem.SelectedBubbleIdxs[1];

            if (firstBubble.row == -1 && firstBubble.cul == -1) //정해지지 않았을 시
            {
                firstBubble.row = m_info.GetRow();
                firstBubble.cul = m_info.GetCul();
            }
            else 
            {
                if (secondBubble.row == -1 && secondBubble.cul == -1)
                {
                    secondBubble.row = m_info.GetRow();
                    secondBubble.cul = m_info.GetCul();
                }
            }
        }
    }


    public void ChangeTypeAndLooks(int type) 
    {
        //타입 변경
        m_info.SetType((BubbleType)type);
        StartCoroutine(ChangeBubbleLooksWithAnim(type));

        //이름도 변경
        gameObject.name = "Bubble" + m_info.GetType() + " _" + m_info.GetRow() + " _" + m_info.GetCul();
    }

    IEnumerator ChangeBubbleLooksWithAnim(int type) 
    {
        //각 단계가 제대로 완료된 후에 다음 단계로
        yield return StartCoroutine(CoSetTransparent());
        ChangeBubbleLooks(type);
        yield return StartCoroutine(CoSetOpaque());
    }


    //투명해지기
    IEnumerator CoSetTransparent() 
    {
        int cnt = 0; //프레임 세는 용도
        float alpha = 1;

        while (cnt < 20) 
        {
            m_Img.color = new Color(m_Img.color.r, m_Img.color.g, m_Img.color.b, alpha);
            alpha -= 0.05f;
            cnt++;
            yield return new WaitForSeconds(0.03f);
        }
        yield return null;
    }

    //불투명해지기
    IEnumerator CoSetOpaque() 
    {
        int cnt = 0; //프레임 세는 용도
        float alpha = 0;

        while (cnt < 20) //불투명해지기
        {
            m_Img.color = new Color(m_Img.color.r, m_Img.color.g, m_Img.color.b, alpha);
            alpha += 0.05f;

            cnt++;
            yield return new WaitForSeconds(0.03f);
        }

        yield return null;
    }

    //퍼즐 외형 변경
    void ChangeBubbleLooks(int type) 
    {
        m_Animator.runtimeAnimatorController = puzzleSystem.BubbleAnimContsList[type];
        m_Img.sprite = puzzleSystem.BubbleSpritesList[type];
    }

}
