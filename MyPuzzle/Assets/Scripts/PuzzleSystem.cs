using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    Queue<Bubble> poolingObjectQueue = new Queue<Bubble>();

    private void Awake()
    {
        BubbleParent = GameObject.Find("BubbleParent");
        PrefabNum = poolingObjectPrefabs.Count;
        BubbleNum = RowNum * CulNum;
        Instance = this;
        Init(BubbleNum);
    }

    private void Init(int initCount)
    {
        for (int i = 0; i < initCount; i++)
        {
            poolingObjectQueue.Enqueue(CreateNewObject()); //���� ����
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

    public static Bubble GetObject()
    {
        if (Instance.poolingObjectQueue.Count > 0)
        {
            var obj = Instance.poolingObjectQueue.Dequeue(); //ť���� ������Ʈ ������

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

    public static void ReturnObject(Bubble obj)
    {
        obj.gameObject.SetActive(false);

        obj.transform.SetParent(BubbleParent.transform);
        //obj.transform.SetParent(Instance.transform);
        Instance.poolingObjectQueue.Enqueue(obj);
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
                var obj = GetObject();
                obj.transform.localPosition = tmp1;
                tmp1 += new Vector3(0, -30, 0);

                //������ ����
                obj.transform.localScale = new Vector3(40, 40, 40);

            }
            tmp2 += new Vector3(30, 0, 0);
        }
      
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
