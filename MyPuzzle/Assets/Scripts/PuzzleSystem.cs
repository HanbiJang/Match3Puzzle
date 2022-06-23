using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleSystem : MonoBehaviour
{
    //오브젝트 풀 개발
    public static PuzzleSystem Instance;
    int BubbleNum;
    int PrefabNum;
    public Vector3 StartPosition; //버블 생성 포지션
    public int RowNum;
    public int CulNum;

    [SerializeField]
    public List<GameObject> poolingObjectPrefabs;

    Queue<Bubble> poolingObjectQueue = new Queue<Bubble>();

    private void Awake()
    {
        PrefabNum = poolingObjectPrefabs.Count;
        BubbleNum = RowNum * CulNum;
        Instance = this;
        Init(BubbleNum);
    }

    private void Init(int initCount)
    {
        for (int i = 0; i < initCount; i++)
        {
            poolingObjectQueue.Enqueue(CreateNewObject()); //버블 생성
        }

/*        //위치 배치하기
        Vector3 tmp1 = StartPosition;
        Vector3 tmp2 = StartPosition;

        //위치 설정해주기
        for (int i = 0; i < CulNum; i++)
        {
            tmp1 = tmp2;
            for (int j = 0; j < RowNum; j++)
            {
                GetObject().transform.position = tmp1;
                tmp1 += new Vector3(0, -10, 0);
            }
            tmp2 += new Vector3(10, 0, 0);
        }*/

    }

    private Bubble CreateNewObject()
    {
        int randomNum = (int) Random.Range(0, PrefabNum);
        var newObj = Instantiate(poolingObjectPrefabs[randomNum]).GetComponent<Bubble>(); //생성
        newObj.gameObject.SetActive(false);
        //newObj.transform.SetParent(transform);
        newObj.transform.SetParent(GameObject.Find("Puzzle System").transform);

        return newObj;
    }

    public static Bubble GetObject()
    {
        if (Instance.poolingObjectQueue.Count > 0)
        {
            var obj = Instance.poolingObjectQueue.Dequeue();

            //obj.transform.SetParent(null);
            obj.transform.SetParent(GameObject.Find("Puzzle System").transform);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            var newObj = Instance.CreateNewObject();

            newObj.gameObject.SetActive(true);
            newObj.transform.SetParent(GameObject.Find("Puzzle System").transform);
            //newObj.transform.SetParent(null);

            return newObj;
        }
    }

    public static void ReturnObject(Bubble obj)
    {
        obj.gameObject.SetActive(false);

        obj.transform.SetParent(GameObject.Find("Puzzle System").transform);
        //obj.transform.SetParent(Instance.transform);
        Instance.poolingObjectQueue.Enqueue(obj);
    }


    // Start is called before the first frame update
    void Start()
    {
        for(int i=0; i<BubbleNum; i++)
            GetObject();

        //위치 배치하기
        Vector3 tmp1 = StartPosition;
        Vector3 tmp2 = StartPosition;

        //위치와 스케일 설정해주기
        for (int i = 0; i < CulNum; i++)
        {
            tmp1 = tmp2;
            for (int j = 0; j < RowNum; j++)
            {
                var obj = GetObject();
                obj.transform.localPosition = tmp1;
                tmp1 += new Vector3(0, -30, 0);

                //스케일 설정
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
