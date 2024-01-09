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
    BubbleState state; //��ġ ���� ����
    BubbleType type; //������ Ÿ��

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

    bool isSelected; //����ڿ� ���� ���� �Ǿ����� ����
    public BubbleInfo m_info;
    public bool visited = false; //��ġ ��ĵ�� �ʿ��� ����

    public BubbleInfo GetBubbleInfo() { return m_info; }
    public void SetBubbleInfo(BubbleInfo binfo) { m_info = binfo; }

    public void SetVisited(bool b) { visited = b; }
    public bool GetVisited() { return visited; }
    
    SpriteRenderer m_Img; //������ ��������Ʈ
    Animator m_Animator; //������ �ִϸ�����

    void Awake() 
    {
        puzzleSystem = GameObject.Find("Puzzle System").GetComponent<PuzzleSystem>();
        m_info = new BubbleInfo();     
        m_Img = GetComponentInChildren<SpriteRenderer>();
        m_Animator = GetComponent<Animator>();
    }

    //������ ��ư���ν� Ȱ��, ��ư �Լ��� ���
    public void BtnBubbleOnClicked() 
    {
        if (puzzleSystem && puzzleSystem.EnableInput) 
        {
            SelectedBubbleIdx firstBubble = puzzleSystem.SelectedBubbleIdxs[0];
            SelectedBubbleIdx secondBubble = puzzleSystem.SelectedBubbleIdxs[1];

            if (firstBubble.row == -1 && firstBubble.cul == -1) //�������� �ʾ��� ��
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
        //Ÿ�� ����
        m_info.SetType((BubbleType)type);
        StartCoroutine(ChangeBubbleLooksWithAnim(type));

        //�̸��� ����
        gameObject.name = "Bubble" + m_info.GetType() + " _" + m_info.GetRow() + " _" + m_info.GetCul();
    }

    IEnumerator ChangeBubbleLooksWithAnim(int type) 
    {
        //�� �ܰ谡 ����� �Ϸ�� �Ŀ� ���� �ܰ��
        yield return StartCoroutine(CoSetTransparent());
        ChangeBubbleLooks(type);
        yield return StartCoroutine(CoSetOpaque());
    }


    //����������
    IEnumerator CoSetTransparent() 
    {
        int cnt = 0; //������ ���� �뵵
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

    //������������
    IEnumerator CoSetOpaque() 
    {
        int cnt = 0; //������ ���� �뵵
        float alpha = 0;

        while (cnt < 20) //������������
        {
            m_Img.color = new Color(m_Img.color.r, m_Img.color.g, m_Img.color.b, alpha);
            alpha += 0.05f;

            cnt++;
            yield return new WaitForSeconds(0.03f);
        }

        yield return null;
    }

    //���� ���� ����
    void ChangeBubbleLooks(int type) 
    {
        m_Animator.runtimeAnimatorController = puzzleSystem.BubbleAnimContsList[type];
        m_Img.sprite = puzzleSystem.BubbleSpritesList[type];
    }

}
