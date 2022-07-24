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
        int randomNum = (int) Random.Range(0, PrefabNum);
        var newObj = Instantiate(poolingObjectPrefabs[randomNum]).GetComponent<Bubble>(); //����
        newObj.gameObject.SetActive(false);
        newObj.transform.SetParent(BubbleParent.transform);

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
                var obj = GetObject(j,i);
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
        if (SelectedBubbleIdxs[1].row != -1 && SelectedBubbleIdxs[1].cul != -1) 
        {
            //�� ������ �ε����� 1���� ��� �ڸ��� �ٲ۴�
            if ( Mathf.Abs(SelectedBubbleIdxs[0].row - SelectedBubbleIdxs[1].row) <= 1 && Mathf.Abs(SelectedBubbleIdxs[0].cul - SelectedBubbleIdxs[1].cul) <= 1) {
                Vector3 postmp = GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).transform.localPosition;
                GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).transform.localPosition = GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).transform.localPosition;
                GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).transform.localPosition = postmp;
            }
            
            //���� �ʱ�ȭ
            SelectedBubbleIdxs[0].row = -1;
            SelectedBubbleIdxs[0].cul = -1;
            SelectedBubbleIdxs[1].row = -1;
            SelectedBubbleIdxs[1].cul = -1;
        }
    }
}
