using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct SelectedBubbleIdx
{
    public int row, cul;

    public SelectedBubbleIdx(int r, int c) { row = r; cul = c; }
}

public class PuzzleSystem : MonoBehaviour
{
    //������Ʈ Ǯ ����
    public static PuzzleSystem Instance;
    int BubbleNum;
    int PrefabNum;
    public Vector3 StartPosition; //���� ���� ������
    public int RowNum;
    public int CulNum;
    static GameObject BubbleParent = null;

    [SerializeField]
    public List<GameObject> poolingObjectPrefabs;

    //Queue<Bubble> poolingObjectQueue = new Queue<Bubble>();

    List<List<Bubble>> poolingObjectsList; //2���迭

    //������ ���� �����ϱ� (ũ�� ����)
    public SelectedBubbleIdx[] SelectedBubbleIdxs;

    ScoreSystem scoreSystem; // ���� �ý���

    public bool EnableInput = true; // �Է��� ��������

    // ����� ����
    public RuntimeAnimatorController AnimCont_Blue;
    public RuntimeAnimatorController AnimCont_Green;
    public RuntimeAnimatorController AnimCont_Orange;
    public RuntimeAnimatorController AnimCont_Red;
    public RuntimeAnimatorController AnimCont_Yellow;

    // ����� �̹���
    public Sprite Sprite_Blue;
    public Sprite Sprite_Green;
    public Sprite Sprite_Orange;
    public Sprite Sprite_Red;
    public Sprite Sprite_Yellow;

    bool IsMoving = false;

    int typeCnt = 1;

    string strtest;

    private void Awake()
    {
        poolingObjectsList = new List<List<Bubble>>();
        BubbleParent = GameObject.Find("BubbleParent");
        PrefabNum = poolingObjectPrefabs.Count;
        BubbleNum = RowNum * CulNum;
        Instance = this;
        Init(BubbleNum); //���� ������ֱ�

        //���� ���� �ε���
        SelectedBubbleIdx tmp = new SelectedBubbleIdx();
        tmp.row = -1; tmp.cul = -1;
        SelectedBubbleIdxs = new SelectedBubbleIdx[2];
        SelectedBubbleIdxs[0] = tmp;
        SelectedBubbleIdxs[1] = tmp;

        // ���� �ý���
        scoreSystem = GameObject.Find("Score System").GetComponent<ScoreSystem>();
    }

    private void Init(int initCount)
    {
        /*       //ť ����
               for (int i = 0; i < initCount; i++)
               {
                   poolingObjectQueue.Enqueue(CreateNewObject()); //���� ����
               }*/

        for (int i = 0; i < RowNum; i++)
        {
            List<Bubble> tmpList = new List<Bubble>();
            //�� �����
            for (int j = 0; j < CulNum; j++)
            {
                tmpList.Add(CreateNewObject(i, j));
            }
            poolingObjectsList.Add(tmpList);

        }


    }

    private Bubble CreateNewObject(int row, int cul)
    {
        int randomNum = (int)Random.Range(0, PrefabNum);
        var newObj = Instantiate(poolingObjectPrefabs[randomNum]).GetComponent<Bubble>(); //����
        newObj.gameObject.SetActive(false);
        newObj.transform.SetParent(BubbleParent.transform);

        //Ÿ�� �����ϱ�
        newObj.m_type = (BubbleType)randomNum; //int -> enum

        // �̸� ����
        newObj.name = "Bubble" + newObj.m_type + " _" + row + " _" + cul;

        return newObj;
    }

    public static Bubble GetObject(int row, int cul)
    {
        if (Instance.poolingObjectsList.Count > 0)
        //if (Instance.poolingObjectQueue.Count > 0)
        {
            var obj = Instance.poolingObjectsList[row][cul];
            //var obj = Instance.poolingObjectQueue.Dequeue(); //ť���� ������Ʈ ������

            //obj.transform.SetParent(null);
            obj.transform.SetParent(BubbleParent.transform);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else //�� ������ ���� �� 
        {
            var newObj = Instance.CreateNewObject(0, 0);

            newObj.gameObject.SetActive(true);
            newObj.transform.SetParent(BubbleParent.transform);
            //newObj.transform.SetParent(null);

            return newObj;
        }
    }

    public static void ReturnObject(int row, int cul)
    {
        var obj = Instance.poolingObjectsList[row][cul];

        obj.gameObject.SetActive(false);

        obj.transform.SetParent(BubbleParent.transform);
        //obj.transform.SetParent(Instance.transform);

        //Instance.poolingObjectQueue.Enqueue(obj); //ť�� �ǵ��� �ִ� �۾��� �ּ� ó��
    }


    // Start is called before the first frame update
    void Start()
    {

        //��ġ ��ġ�ϱ�
        Vector3 tmp1 = StartPosition;
        Vector3 tmp2 = StartPosition;

        //��ġ�� ������ �������ֱ�
        for (int i = 0; i < CulNum; i++)
        {
            tmp1 = tmp2;
            for (int j = 0; j < RowNum; j++)
            {
                var obj = GetObject(j, i);
                obj.transform.localPosition = tmp1;
                tmp1 += new Vector3(0, -30, 0);

                //������ ����
                obj.transform.localScale = new Vector3(40, 40, 40);

                //row �� cul �������ֱ�
                obj.GetBubbleInfo().SetRow(j);
                obj.GetBubbleInfo().SetCul(i);

            }
            tmp2 += new Vector3(30, 0, 0);
        }

        ScanUntilNoMatch();
    }

    void ScanUntilNoMatch()
    {
        StartCoroutine(ScanUntilNoMatch_Co());
    }

    IEnumerator ScanUntilNoMatch_Co()
    {

        // ��ĵ�Ǵ� ������ ���� ������ �ݺ�
        while (true)
        {
            yield return new WaitForSeconds(0.01f);
            if (!ScanBubbles())
            {
                yield return new WaitForSeconds(0.01f);
                break;
            }
        }

        //���� �ʱ�ȭ
        SelectedBubbleIdxs[0].row = -1;
        SelectedBubbleIdxs[0].cul = -1;
        SelectedBubbleIdxs[1].row = -1;
        SelectedBubbleIdxs[1].cul = -1;
        IsMoving = false;

        yield return null;
    }

    // Update is called once per frame
    void Update()
    {

        if (IsMoving)
            EnableInput = false;
        else
        {
            EnableInput = true;
            ChangeBubblePosition();
        }

    }


    // ������ ��ġ �ٲٱ�
    void ChangeBubblePosition()
    {
        if (SelectedBubbleIdxs[0].row != -1 && SelectedBubbleIdxs[0].cul != -1
            && SelectedBubbleIdxs[1].row != -1 && SelectedBubbleIdxs[1].cul != -1)
        {
            //�� ������ �ε����� 1���� ��� �ڸ��� �ٲ۴�
            if (Mathf.Abs(SelectedBubbleIdxs[0].row - SelectedBubbleIdxs[1].row) <= 1 && Mathf.Abs(SelectedBubbleIdxs[0].cul - SelectedBubbleIdxs[1].cul) <= 1)
            {
                IsMoving = true;

                // [2] �ڷ�ƾ���� �̵���Ű��
                Vector3 pos_0 = GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).transform.localPosition;
                Vector3 pos_1 = GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).transform.localPosition;
                StartCoroutine(SwapPos(pos_0, pos_1));
            }
        }
    }

    IEnumerator SwapPos(Vector3 origin, Vector3 target)
    {
        int cnt = 0;

        // ��ġ�� ���������Ƿ� �� �迭������ ����� �ٲ������
        int row_0 = GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).GetBubbleInfo().GetRow(); // A�� ���� ���
        int cul_0 = GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).GetBubbleInfo().GetCul();
        int row_1 = GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).GetBubbleInfo().GetRow(); // B�� ���� ���
        int cul_1 = GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).GetBubbleInfo().GetCul();

        // ������ ��ġ�� �ٲٱ�
        while (cnt < 20)
        {
            cnt++;
            GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).transform.localPosition = Vector3.Lerp(
                GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).transform.localPosition, target, 0.25f);
            GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).transform.localPosition = Vector3.Lerp(
               GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).transform.localPosition, origin, 0.25f);
            yield return new WaitForSeconds(0.03f);
        }

        var tmp = Instance.poolingObjectsList[row_0][cul_0];
        Instance.poolingObjectsList[row_0][cul_0] = Instance.poolingObjectsList[row_1][cul_1];
        Instance.poolingObjectsList[row_1][cul_1] = tmp; // A�� B�� poolingobject ����Ʈ �󿡼� ��ü

        // ���� ������ �ε��� ����(info)�� ����
        GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).GetBubbleInfo().SetRow(row_0); // A �ڸ��� �ִ� B���� A�� ����� �ֱ�
        GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).GetBubbleInfo().SetCul(cul_0);
        GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).GetBubbleInfo().SetRow(row_1); // B �ڸ��� �ִ� A���� B�� ����� �ֱ�
        GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).GetBubbleInfo().SetCul(cul_1);

        // ��ġ�� ���� ���� ��
        if (!ScanBubbles())
        {

            // ��ġ ���� (�ʱ�ȭ)
            cnt = 0;
            while (cnt < 20)
            {
                cnt++;
                GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).transform.localPosition = Vector3.Lerp(
                    GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).transform.localPosition, target, 0.25f);
                GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).transform.localPosition = Vector3.Lerp(
                   GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).transform.localPosition, origin, 0.25f);
                yield return new WaitForSeconds(0.03f);
            }

            // ���� ������ �ε��� ����(info)�� ���� �ʱ�ȭ
            GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).GetBubbleInfo().SetRow(row_1);
            GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).GetBubbleInfo().SetCul(cul_1);
            GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).GetBubbleInfo().SetRow(row_0);
            GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).GetBubbleInfo().SetCul(cul_0);

            // �� �迭 �ε��� �ʱ�ȭ
            var tmp_ = Instance.poolingObjectsList[row_0][cul_0];
            Instance.poolingObjectsList[row_0][cul_0] = Instance.poolingObjectsList[row_1][cul_1];
            Instance.poolingObjectsList[row_1][cul_1] = tmp_;

            IsMoving = false;
        }
        else // ��ġ�� �� ��
        {
            // ��ĵ�Ǵ� ������ ���� ������ �ݺ�
            //ScanUntilNoMatch();
            IsMoving = false;
        }

        InitSelectedBubbleIdxs();

        yield return null;
    }

    void InitSelectedBubbleIdxs()
    {
        //���� �ʱ�ȭ
        SelectedBubbleIdxs[0].row = -1;
        SelectedBubbleIdxs[0].cul = -1;
        SelectedBubbleIdxs[1].row = -1;
        SelectedBubbleIdxs[1].cul = -1;
    }

    // ��ü ������ ��ĵ�ϱ�
    bool ScanBubbles()
    {
        bool HasMatchedBubbles = false; // ��ġ�Ǵ� ������ �ִ��� �Ǵ�

        Bubble root;
        Queue queue = new Queue(); //�湮�� ť
        List<Bubble> MatchedBubbles = new List<Bubble>(); // ��� ��Ī�� �����

        for (int i = 0; i < RowNum; i++)
        {
            for (int j = 0; j < CulNum; j++)
            {
                // root ����
                root = GetObject(i, j);
                if (root.GetVisited()) continue; // ���� ���� �н��ϱ�
                if (root.m_state == BubbleState.Matched) continue; // ��ġ�� �� ���¸� �н��ϱ�

                typeCnt = 1;
                queue.Enqueue(root); //ť�� ���� Enqueue
                root.SetVisited(true); // (�湮�� ���� üũ)

                List<Bubble> SameTypeBubbles = new List<Bubble>();
                SameTypeBubbles.Add(root);

                // ť�� ������ ������ ��� ���� Ÿ���� ã�´�
                while (queue.Count != 0)
                {
                    Bubble r = (Bubble)queue.Dequeue(); // ť�� �տ��� ��� ����

                    //��Ʈ ����� row�� cul
                    int r_row = r.GetBubbleInfo().GetRow();
                    int r_cul = r.GetBubbleInfo().GetCul();

                    SameTypeBubbles.Add(r); // ��Ʈ�� ���� Ÿ���� ���� (��Ʈ��) �߰�

                    //visit(r); //ť���� ������ ��� �湮
                    //ť���� Dequeue�� ����� ���� ������ ��� ���ʷ� �湮�Ѵ�.

                    // [1] ����
                    if ((r_row >= 0 && r_row < RowNum) && (r_cul - 1 >= 0 && r_cul - 1 < CulNum))
                        AddToMatchedBubbles(r, r_row, r_cul - 1, queue, SameTypeBubbles);
                    // [2] ������
                    if ((r_row >= 0 && r_row < RowNum) && (r_cul + 1 >= 0 && r_cul + 1 < CulNum))
                        AddToMatchedBubbles(r, r_row, r_cul + 1, queue, SameTypeBubbles);
                    // [3] ��
                    if ((r_row - 1 >= 0 && r_row - 1 < RowNum) && (r_cul >= 0 && r_cul < CulNum))
                        AddToMatchedBubbles(r, r_row - 1, r_cul, queue, SameTypeBubbles);
                    // [4] �Ʒ�
                    if ((r_row + 1 >= 0 && r_row + 1 < RowNum) && (r_cul >= 0 && r_cul < CulNum))
                        AddToMatchedBubbles(r, r_row + 1, r_cul, queue, SameTypeBubbles);
                }

                // ��ġ�� 3�� �̻� �Ǹ�
                if (typeCnt >= 3)
                {

                    SameTypeBubbles = SameTypeBubbles.Distinct().ToList();

                    HasMatchedBubbles = true;

                    // ���� 10�� ���� ����
                    if (scoreSystem) scoreSystem.ChangeScore(10 * SameTypeBubbles.Count);

                    // ��Ī�� ���� ����Ʈ�� ���� ���� Ÿ�� ���� ����Ʈ�� �߰��ϱ�
                    for (int s = 0; s < SameTypeBubbles.Count; s++)
                    {
                        SameTypeBubbles[s].m_state = BubbleState.Matched; // ��ġ�� ���·� ����
                        MatchedBubbles.Add(SameTypeBubbles[s]);
                    }
                }

                // ���� Ÿ�� ��Ī �ʱ�ȭ
                typeCnt = 1;

                // ���� Ÿ�Ե� ���� �ʱ�ȭ
                for (int s = 0; s < SameTypeBubbles.Count; s++)
                {
                    SameTypeBubbles.RemoveAt(s);
                }
                // ��� visit �ʱ�ȭ
                InitAllVisited();
            }
        }

        // ��ġ�� ������� Ÿ�԰� �̹��� ����
        //Debug.Log(MatchedBubbles.Count);
        for (int m = 0; m < MatchedBubbles.Count; m++)
        {
            int randomNum = (int)Random.Range(0, PrefabNum);
            MatchedBubbles[m].ChangeTypeAndImg(randomNum);
        }

        // ��Ī�� ������� ��ġ ���� �ʱ�ȭ
        for (int m = 0; m < MatchedBubbles.Count; m++)
        {
            MatchedBubbles[m].m_state = BubbleState.UnMatched;
        }

        // ��Ī�� ����� ����Ʈ ����
        for (int m = 0; m < MatchedBubbles.Count; m++)
        {
            MatchedBubbles.RemoveAt(m);
        }

        return HasMatchedBubbles;
    }

    void AddToMatchedBubbles(Bubble r, int r_row, int r_cul, Queue queue, List<Bubble> SameTypeBubbles)
    {
        //Bubble b = Instance.poolingObjectsList[r_row][r_cul]; //���� ���
        Bubble b = GetObject(r_row, r_cul);

        if (b.GetVisited() == false && b.m_state == BubbleState.UnMatched) // �湮���� �ʾҰ� + ��ġ���� �ʾҴٸ�
        {
            b.SetVisited(true); // �湮�� ��� üũ

            // Ÿ���� ���ٸ�
            if (b.m_type == r.m_type)
            {
                typeCnt++;
                queue.Enqueue(b); // ť�� ���� Enqueue

                SameTypeBubbles.Add(r); // ��Ʈ�� ���� Ÿ���� ���� (��Ʈ��) �߰�
            }
        }
    }

    IEnumerator ChangeMatchedBubbles(List<Bubble> MatchedBubbles)
    {
        yield return null;
    }

    void InitAllVisited()
    {
        for (int i = 0; i < RowNum; i++)
        {
            for (int j = 0; j < CulNum; j++)
            {
                GetObject(i, j).SetVisited(false);
            }
        }

    }

}
