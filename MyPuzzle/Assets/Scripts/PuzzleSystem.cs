using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectedBubbleIdx
{
    public int row = -1, cul = -1;
}

public class PuzzleSystem : MonoBehaviour
{
    //������Ʈ Ǯ ����
    public static PuzzleSystem Instance;
    int BubbleNum;
    static int PrefabNum;
    [SerializeField]
    public List<GameObject> poolingObjectPrefabs;
    List<List<Bubble>> poolingObjectsList; //2���迭

    //���� ���� ������
    public Vector3 StartPosition; 
    public int RowNum;
    public int CulNum;
    static GameObject BubbleParent = null;

    //���� �ý���
    ScoreSystem scoreSystem;

    //������ 2���� ����
    public SelectedBubbleIdx[] SelectedBubbleIdxs;
    public bool EnableInput = true; // ����ڰ� �Է��� ��������
    bool bIsMatching = false; //������ ��ġ�� ����, �����̰� ���� ���� �÷���

    //����� ����
    public List<RuntimeAnimatorController> BubbleAnimContsList;
    public List<Sprite> BubbleSpritesList;
    Queue<List<Bubble>> MatchedBubbleQueue;

    int typeCnt = 1;

    private void Awake()
    {
        poolingObjectsList = new List<List<Bubble>>();
        BubbleParent = GameObject.Find("BubbleParent");
        PrefabNum = poolingObjectPrefabs.Count;
        BubbleNum = RowNum * CulNum;
        Instance = this;
        MatchedBubbleQueue = new Queue<List<Bubble>>();

        // ���� ���� �ε��� �ʱ�ȭ
        InitSelectedBubbleIdxs();

        //���� �ý���
        scoreSystem = GameObject.Find("Score System").GetComponent<ScoreSystem>();
    }

    private void InitPuzzle()
    {
        //���� ù ���� : ��ġ�� Ȯ���� ������ ����� ��, = ������ ���� Ÿ���� 3�� �̻� �������� �������� �ʾƾ� ��.

        for (int i = 0; i < RowNum; i++)
        {
            List<Bubble> rowBubbles = new List<Bubble>();

            for (int j = 0; j < CulNum; j++)
            {
                BubbleType randomType = GetRandomPuzzleType(i, j);
     
                rowBubbles.Add(CreateNewObject(i,j,(int)randomType));
                GetObject(i, j).GetBubbleInfo().SetType(randomType); 
            }
            poolingObjectsList.Add(rowBubbles);
        }

    }

    static BubbleType GetRandomPuzzleType(int row, int cul) 
    {
        List<BubbleType> possibleTypes = new List<BubbleType>(PrefabNum);

        //��� ���� Ÿ�� �߰�
        for (int i = 0; i < PrefabNum; i++)
        {
            possibleTypes.Add((BubbleType)i);
        }

        //���� �࿡�� ���� �� ������ ������ Ÿ������ Ȯ�� & �����ϸ� �ش� Ÿ�� ����
        if (row >= 2)
        {
            BubbleType prevType1 = GetObject(row - 1, cul).GetBubbleInfo().GetType_();
            BubbleType prevType2 = GetObject(row - 2, cul).GetBubbleInfo().GetType_();

            if (prevType1 == prevType2)
            {
                possibleTypes.Remove(prevType1);
                possibleTypes.Remove(prevType2);
            }
        }
        // ���� ������ ���� �� ������ ������ Ÿ������ Ȯ�� & �����ϸ� �ش� Ÿ�� ���� 
        if (cul >= 2)
        {
            BubbleType prevType1 = GetObject(row, cul - 1).GetBubbleInfo().GetType_();
            BubbleType prevType2 = GetObject(row, cul - 2).GetBubbleInfo().GetType_();

            if (prevType1 == prevType2) 
            {
                possibleTypes.Remove(prevType1);
                possibleTypes.Remove(prevType2);
            }
            
        }

        // ������ Ÿ�� �߿��� �������� ����
        int randomIndex = Random.Range(0, possibleTypes.Count);
        return possibleTypes[randomIndex];
    }

    private Bubble CreateNewObject(int row, int cul, int type)
    {
        Bubble newObj = Instantiate(poolingObjectPrefabs[type]).GetComponent<Bubble>();

        newObj.gameObject.SetActive(false);
        newObj.transform.SetParent(BubbleParent.transform);
        newObj.GetBubbleInfo().Init(row, cul);
        newObj.GetBubbleInfo().SetType((BubbleType)type);
        newObj.name = $"Bubble{newObj.GetBubbleInfo().GetType()}_{row}_{cul}";

        return newObj;
    }

    // ��(row)�� ��(cul)�� �ش��ϴ� ���� ������ ������Ʈ Ǯ���� ������
    public static Bubble GetObject(int row, int cul)
    {
        //if (Instance.poolingObjectsList.Count > 0)
        if(Instance.poolingObjectsList != null && row >= 0 && row < Instance.poolingObjectsList.Count
            && cul >= 0 && cul < Instance.poolingObjectsList[0].Count && Instance.poolingObjectsList[row][cul] != null)
        {
            var obj = Instance.poolingObjectsList[row][cul];
            obj.transform.SetParent(BubbleParent.transform);
            obj.gameObject.SetActive(true);

            return obj;
        }
        else //�� ������ ���� �� 
        {
            //var newObj = Instance.CreateNewObject(0, 0);
            BubbleType randomType = GetRandomPuzzleType(row, cul);
            var newObj = Instance.CreateNewObject(row, cul, (int)randomType);
            newObj.gameObject.SetActive(true);
            newObj.transform.SetParent(BubbleParent.transform);

            return newObj;
        }
    }

    public static void ReturnObject(int row, int cul)
    {
        var obj = Instance.poolingObjectsList[row][cul];
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(BubbleParent.transform);
    }


    void Start()
    {
        InitPuzzle();
        //ScanUntilNoMatch();
        SetPuzzleLocation();
    }

    void SetPuzzleLocation()
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
    }

    void ScanUntilNoMatch()
    {
        StartCoroutine(ScanUntilNoMatch_Co());
    }

    // ��ĵ�Ǵ� ������ ���� ������ �ݺ��ؼ� ��ĵ
    IEnumerator ScanUntilNoMatch_Co()
    {  
        while (true)
        {
            if (ScanBubbles() == true)
            {
                yield return new WaitForSeconds(2f);
            }
            else {
                break;
            }
        }

        //���� �ʱ�ȭ
        InitSelectedBubbleIdxs();
        bIsMatching = false;

        yield return null;
    }
    

    // Update is called once per frame
    void Update()
    {

        if (bIsMatching) 
            EnableInput = false;
        else
        {
            EnableInput = true;
            ChangeBubblePosition();
        }

    }


    // ������ ������ ��ġ �ٲٱ�
    void ChangeBubblePosition()
    {
        if (SelectedBubbleIdxs[0].row != -1 && SelectedBubbleIdxs[0].cul != -1
            && SelectedBubbleIdxs[1].row != -1 && SelectedBubbleIdxs[1].cul != -1)
        {
            //�� ������ �ε����� 1���� ��� �ڸ��� �ٲ۴�
            if (Mathf.Abs(SelectedBubbleIdxs[0].row - SelectedBubbleIdxs[1].row) <= 1 && Mathf.Abs(SelectedBubbleIdxs[0].cul - SelectedBubbleIdxs[1].cul) <= 1)
            {
                bIsMatching = true;

                // [2] �ڷ�ƾ���� �̵���Ű��
                Vector3 pos_0 = GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).transform.localPosition;
                Vector3 pos_1 = GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).transform.localPosition;
                StartCoroutine(SwapPos(pos_0, pos_1));
            }
            else 
            {
                InitSelectedBubbleIdxs(); //���� ��ҷ� �����
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

            bIsMatching = false;
        }
        else // ��ġ�� �� ��
        {
            //bIsMatching = false;
            ScanUntilNoMatch();
        }

        InitSelectedBubbleIdxs();

        yield return null;
    }

    void InitSelectedBubbleIdxs()
    {
        if (SelectedBubbleIdxs == null) {
            SelectedBubbleIdxs = new SelectedBubbleIdx[2];

            // �� ��ҿ� SelectedBubbleIdx �ν��Ͻ� �Ҵ� (��ü�� �����ڰ� ȣ����� ����)
            for (int i = 0; i < SelectedBubbleIdxs.Length; i++)
            {
                SelectedBubbleIdxs[i] = new SelectedBubbleIdx();
            }
        }
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
                if (root.GetBubbleInfo().GetState() == BubbleState.Matched) continue; // ��ġ�� �� ���¸� �н��ϱ�

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

                //��ġ�� 3�� �̻� �Ǹ�
                if (typeCnt >= 3)
                {

                    SameTypeBubbles = SameTypeBubbles.Distinct().ToList();

                    HasMatchedBubbles = true;

                    //���� 10�� ���� ����
                    if (scoreSystem) scoreSystem.ChangeScore(10 * SameTypeBubbles.Count);

                    //��Ī�� ���� ����Ʈ�� ���� ���� Ÿ�� ���� ����Ʈ�� �߰��ϱ�
                    for (int s = 0; s < SameTypeBubbles.Count; s++)
                    {
                        SameTypeBubbles[s].GetBubbleInfo().SetState(BubbleState.Matched); // ��ġ�� ���·� ����
                        MatchedBubbles.Add(SameTypeBubbles[s]);
                    }
                }

                // ���� Ÿ�� ��Ī �ʱ�ȭ
                typeCnt = 1;
                SameTypeBubbles.Clear();

                // ��� visit �ʱ�ȭ
                InitAllVisited();
            }
        }

        StartCoroutine(ChangeMatchedBubbles(MatchedBubbles));

        return HasMatchedBubbles;
    }

    IEnumerator ChangeMatchedBubbles(List<Bubble> MatchedBubbles)
    {
        MatchedBubbleQueue.Enqueue(MatchedBubbles);
        

        // ��ġ�� ������� Ÿ�԰� �̹��� ����
        while (MatchedBubbleQueue.Count > 0)
        {
            List<Bubble> curList = MatchedBubbleQueue.Dequeue();

            for(int i=0; i<curList.Count; i++)
            {
                int randomNum = (int)Random.Range(0, PrefabNum);
                curList[i].ChangeTypeAndLooks(randomNum);
                curList[i].GetBubbleInfo().SetState(BubbleState.UnMatched);
               
            }
            yield return new WaitForSeconds(0.5f);
        }

        // ��Ī�� ����� ����Ʈ ����
        MatchedBubbles.Clear();

        yield return null;
    }


    void AddToMatchedBubbles(Bubble r, int r_row, int r_cul, Queue queue, List<Bubble> SameTypeBubbles)
    {
        Bubble b = GetObject(r_row, r_cul);

        if (b.GetVisited() == false && b.GetBubbleInfo().GetState() == BubbleState.UnMatched) // �湮���� �ʾҰ� + ��ġ���� �ʾҴٸ�
        {
            b.SetVisited(true); // �湮�� ��� üũ

            // Ÿ���� ���ٸ�
            if (b.GetBubbleInfo().GetType_() == r.GetBubbleInfo().GetType_())
            {
                typeCnt++;
                queue.Enqueue(b); // ť�� ���� Enqueue

                SameTypeBubbles.Add(r); // ��Ʈ�� ���� Ÿ���� ���� (��Ʈ��) �߰�
            }
        }
    }

    void InitAllVisited()
    {
        for (int i = 0; i < RowNum; i++) for (int j = 0; j < CulNum; j++) GetObject(i, j).SetVisited(false);
    }

}
