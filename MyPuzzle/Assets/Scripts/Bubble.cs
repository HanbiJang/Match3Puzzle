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
    bool isSelected; //���� �Ǿ����� ����
    public BubbleInfo m_info;
    PuzzleSystem puzzleSystem;

    //��ĵ�� �ʿ��� ����
    public bool visited = false;

    public void SetVisited(bool b) { visited = b; }
    public bool GetVisited() { return visited; }

    public BubbleType GetMType() { return m_type; }
    public void SetMType(BubbleType type) { m_type = type; }

    public BubbleInfo GetBubbleInfo() { return m_info; }
    public void SetBubbleInfo(BubbleInfo bi) { m_info = bi; }



    // ������ ��������Ʈ
    SpriteRenderer m_Img;
    // ������ �ִϸ�����
    Animator m_Animator;

    void Awake() {
        m_info = new BubbleInfo();
        puzzleSystem = GameObject.Find("Puzzle System").GetComponent<PuzzleSystem>();

        m_Img = GetComponentInChildren<SpriteRenderer>();
        m_Animator = GetComponent<Animator>();
    }

    public void BtnBubbleOnClicked() {

        //puzzle system ��ü�� ã�Ƴ�� SelectedBubbleIdxs�� ���� �ٲ۴�
        if (puzzleSystem && puzzleSystem.EnableInput) 
        { //�Է� �����ϸ�

            //row �� cul �޾ƿ���
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
        // Ÿ�� ����
        m_type = (BubbleType)type;
        StartCoroutine(ChangeImgColor(type));

        // �̸��� ����
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
                // �ִϸ����� ����              
                m_Animator.runtimeAnimatorController = puzzleSystem.AnimCont_Blue;
                // �̹��� ����
                m_Img.sprite = puzzleSystem.Sprite_Blue;
                break;
            case 1:
                // �ִϸ����� ����
                m_Animator.runtimeAnimatorController = puzzleSystem.AnimCont_Green;
                // �̹��� ����
                m_Img.sprite = puzzleSystem.Sprite_Green;
                break;
            case 2:
                // �ִϸ����� ����
                m_Animator.runtimeAnimatorController = puzzleSystem.AnimCont_Orange;
                // �̹��� ����
                m_Img.sprite = puzzleSystem.Sprite_Orange;
                break;
            case 3:
                // �ִϸ����� ����
                m_Animator.runtimeAnimatorController = puzzleSystem.AnimCont_Red;
                // �̹��� ����
                m_Img.sprite = puzzleSystem.Sprite_Red;
                break;
            case 4:
                // �ִϸ����� ����
                m_Animator.runtimeAnimatorController = puzzleSystem.AnimCont_Yellow;
                // �̹��� ����
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
