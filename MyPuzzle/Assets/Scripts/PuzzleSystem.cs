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
    //오브젝트 풀 개발
    public static PuzzleSystem Instance;
    int BubbleNum;
    int PrefabNum;
    public Vector3 StartPosition; //버블 생성 포지션
    public int RowNum;
    public int CulNum;
    static GameObject BubbleParent = null;

    [SerializeField]
    public List<GameObject> poolingObjectPrefabs;

    //Queue<Bubble> poolingObjectQueue = new Queue<Bubble>();

    List<List<Bubble>> poolingObjectsList; //2차배열

    //선택한 버블 구분하기 (크기 한정)
    public SelectedBubbleIdx[] SelectedBubbleIdxs; 


    private void Awake()
    {
        poolingObjectsList = new List<List<Bubble>>();
        BubbleParent = GameObject.Find("BubbleParent");
        PrefabNum = poolingObjectPrefabs.Count;
        BubbleNum = RowNum * CulNum;
        Instance = this;
        Init(BubbleNum); //버블 만들어주기

        //버블 선택 인덱스
        SelectedBubbleIdx tmp = new SelectedBubbleIdx();
        tmp.row = -1; tmp.cul = -1;
        SelectedBubbleIdxs = new SelectedBubbleIdx[2];
        SelectedBubbleIdxs[0] = tmp;
        SelectedBubbleIdxs[1] = tmp;
    }

    private void Init(int initCount)
    {
        /*       //큐 버전
               for (int i = 0; i < initCount; i++)
               {
                   poolingObjectQueue.Enqueue(CreateNewObject()); //버블 생성
               }*/

        for (int i = 1; i <= RowNum; i++) 
        {
            List<Bubble> tmpList = new List<Bubble>();
            //열 만들기
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
        var newObj = Instantiate(poolingObjectPrefabs[randomNum]).GetComponent<Bubble>(); //생성
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
            //var obj = Instance.poolingObjectQueue.Dequeue(); //큐에서 오브젝트 꺼내기

            //obj.transform.SetParent(null);
            obj.transform.SetParent(BubbleParent.transform);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else //다 꺼내고 없을 때 
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

        //Instance.poolingObjectQueue.Enqueue(obj); //큐에 되돌려 주는 작업을 주석 처리
    }


    // Start is called before the first frame update
    void Start()
    {

        //위치 배치하기
        Vector3 tmp1 = StartPosition;
        Vector3 tmp2 = StartPosition;

        //위치와 스케일 설정해주기
        for (int i = 0; i < CulNum; i++)
        {
            tmp1 = tmp2;
            for (int j = 0; j < RowNum; j++)
            {
                var obj = GetObject(j,i);
                obj.transform.localPosition = tmp1;
                tmp1 += new Vector3(0, -30, 0);

                //스케일 설정
                obj.transform.localScale = new Vector3(40, 40, 40);

                //row 와 cul 설정해주기
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
            //두 버블의 인덱스가 1차이 라면 자리를 바꾼다
            if ( Mathf.Abs(SelectedBubbleIdxs[0].row - SelectedBubbleIdxs[1].row) <= 1 && Mathf.Abs(SelectedBubbleIdxs[0].cul - SelectedBubbleIdxs[1].cul) <= 1) {
                Vector3 postmp = GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).transform.localPosition;
                GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).transform.localPosition = GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).transform.localPosition;
                GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).transform.localPosition = postmp;
            }
            
            //선택 초기화
            SelectedBubbleIdxs[0].row = -1;
            SelectedBubbleIdxs[0].cul = -1;
            SelectedBubbleIdxs[1].row = -1;
            SelectedBubbleIdxs[1].cul = -1;
        }
    }
}
