using System.Collections;
using System.Collections.Generic;
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

    ScoreSystem scoreSystem;

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

        for (int i = 1; i <= RowNum; i++)
        {
            List<Bubble> tmpList = new List<Bubble>();
            //�� �����
            for (int j = 1; j <= CulNum; j++)
            {
                tmpList.Add(CreateNewObject());
            }
            poolingObjectsList.Add(tmpList);

        }


    }

    private Bubble CreateNewObject()
    {
        int randomNum = (int)Random.Range(0, PrefabNum);
        var newObj = Instantiate(poolingObjectPrefabs[randomNum]).GetComponent<Bubble>(); //����
        newObj.gameObject.SetActive(false);
        newObj.transform.SetParent(BubbleParent.transform);

        //Ÿ�� �����ϱ�
        newObj.m_type = (BubbleType)randomNum; //int -> enum

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
            var newObj = Instance.CreateNewObject();

            newObj.gameObject.SetActive(true);
            newObj.transform.SetParent(BubbleParent.transform);
            //newObj.transform.SetParent(null);

            return newObj;
        }
    }

    public static void ReturnObject(Bubble obj, int row, int cul)
    {
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
                obj.m_info.SetRow(j);
                obj.m_info.SetCul(i);

            }
            tmp2 += new Vector3(30, 0, 0);
        }


    }

    // Update is called once per frame
    void Update()
    {
        ChangeBubblePosition();
        //ScanBubbles(); // ��ġ�Ǵ� ������ �ִ��� Ž���ϱ�

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
                // ��ġ ����
                Vector3 postmp_0 = GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).transform.localPosition;
                Vector3 postmp_1 = GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).transform.localPosition;
                GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).transform.localPosition = postmp_1;
                GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).transform.localPosition = postmp_0;

                //�ε����� �ٲ۴�
                int row_0 = GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).m_info.GetRow();
                int cul_0 = GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).m_info.GetCul();
                int row_1 = GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).m_info.GetRow();
                int cul_1 = GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).m_info.GetCul();

                //�� Instance�� �迭�� �ε������� ���빰 �ٲ�
                var tmp = Instance.poolingObjectsList[row_0][cul_0];
                Instance.poolingObjectsList[row_0][cul_0] = Instance.poolingObjectsList[row_1][cul_1];
                Instance.poolingObjectsList[row_1][cul_1] = tmp;

                // ��ġ�� ���� ���� ��
                if (!ScanBubbles())
                {
                    // ��ġ ���� (�ʱ�ȭ)
                    GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).transform.localPosition = postmp_1;
                    GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).transform.localPosition = postmp_0;

                    // �� �迭 �ε��� �ʱ�ȭ
                    var tmp_ = Instance.poolingObjectsList[row_0][cul_0];
                    Instance.poolingObjectsList[row_0][cul_0] = Instance.poolingObjectsList[row_1][cul_1];
                    Instance.poolingObjectsList[row_1][cul_1] = tmp;
                }
                else // ��ġ�� �� ��
                {
                    // ������ �ε��� ����(info) ����
                    GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).m_info.SetRow(row_1);
                    GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).m_info.SetCul(cul_1);
                    GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).m_info.SetRow(row_0);
                    GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).m_info.SetCul(cul_0);

                }

            }

            //���� �ʱ�ȭ
            SelectedBubbleIdxs[0].row = -1;
            SelectedBubbleIdxs[0].cul = -1;
            SelectedBubbleIdxs[1].row = -1;
            SelectedBubbleIdxs[1].cul = -1;
            Debug.Log("select init");
        }
    }

    // ��ü ������ ��ĵ�ϱ�
    bool ScanBubbles()
    {
        Bubble root;
        bool HasMatchedBubbles = false; // ��ġ�Ǵ� ������ �ִ��� �Ǵ�
        Queue queue = new Queue(); //�湮�� ť

        for (int i = 0; i < RowNum; i++)
        {
            for (int j = 0; j < CulNum; j++)
            {
                // root ����
                root = GetObject(i, j);
                int typeCnt = 1;
                queue.Enqueue(root); //ť�� ���� Enqueue
                root.visited = true; // (�湮�� ���� üũ)

                // 3. ť�� ������ ������ ����Ѵ�.
                while (queue.Count != 0)
                {
                    Bubble r = (Bubble)queue.Dequeue(); // ť�� �տ��� ��� ����

                    //visit(r); //ť���� ������ ��� �湮

                    //ť���� Dequeue�� ����� ���� ������ ��� ���ʷ� �湮�Ѵ�.

                    //��Ʈ ����� row�� cul
                    int r_row = r.m_info.GetRow();
                    int r_cul = r.m_info.GetCul();

                    Bubble b; //���� ���

                    // [1] ����
                    if ((r_row >= 0 && r_row < RowNum) && (r_cul - 1 >= 0 && r_cul - 1 < CulNum))
                    {
                        b = Instance.poolingObjectsList[r_row][r_cul - 1];
                        if (b.visited == false) // �湮���� �ʾҴٸ�
                        {
                            b.visited = true; // �湮�� ��� üũ

                            // Ÿ���� ���ٸ�
                            if (b.m_type == r.m_type)
                            {
                                typeCnt++;
                                queue.Enqueue(b); // ť�� ���� Enqueue
                            }

                        }
                    }


                    // [2] ������
                    if ((r_row >= 0 && r_row < RowNum) && (r_cul + 1 >= 0 && r_cul + 1 < CulNum))
                    {
                        b = Instance.poolingObjectsList[r_row][r_cul + 1];
                        if (b.visited == false) // �湮���� �ʾҴٸ�
                        {
                            b.visited = true; // �湮�� ��� üũ

                            // Ÿ���� ���ٸ�
                            if (b.m_type == r.m_type)
                            {
                                typeCnt++;
                                queue.Enqueue(b); // ť�� ���� Enqueue
                            }
                        }
                    }


                    // [3] ��
                    if ((r_row - 1 >= 0 && r_row - 1 < RowNum) && (r_cul >= 0 && r_cul < CulNum))
                    {
                        b = Instance.poolingObjectsList[r_row - 1][r_cul];
                        if (b.visited == false) // �湮���� �ʾҴٸ�
                        {
                            b.visited = true; // �湮�� ��� üũ

                            // Ÿ���� ���ٸ�
                            if (b.m_type == r.m_type)
                            {
                                typeCnt++;
                                queue.Enqueue(b); // ť�� ���� Enqueue
                            }
                        }
                    }

                    // [4] �Ʒ�
                    if ((r_row + 1 >= 0 && r_row + 1 < RowNum) && (r_cul >= 0 && r_cul < CulNum))
                    {
                        b = Instance.poolingObjectsList[r_row + 1][r_cul];
                        if (b.visited == false) // �湮���� �ʾҴٸ�
                        {
                            b.visited = true; // �湮�� ��� üũ

                            // Ÿ���� ���ٸ�
                            if (b.m_type == r.m_type)
                            {
                                typeCnt++;
                                queue.Enqueue(b); // ť�� ���� Enqueue
                            }
                        }
                    }
                }

                if (typeCnt >= 3)
                {
                    HasMatchedBubbles = true;

                    // ��ġ�Ǵ� ������� Ÿ�� ���� 
                    /*            int randomNum = (int)Random.Range(0, PrefabNum);
                                //Ÿ�� �����ϱ�
                                newObj.m_type = (BubbleType)randomNum; //int -> enum*/


                    // ���� 10�� ����
                    if (scoreSystem)
                    {
                        scoreSystem.ChangeScore(10);
                    }
                }
            }
        }

        //��� visit �ʱ�ȭ
        InitAllVisited();

        return HasMatchedBubbles;
    }

    void InitAllVisited()
    {
        for (int i = 0; i < RowNum; i++)
        {
            for (int j = 0; j < CulNum; j++)
            {
                GetObject(i, j).visited = false;
            }
        }

    }
}
