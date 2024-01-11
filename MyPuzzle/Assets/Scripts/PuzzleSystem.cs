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
    //오브젝트 풀 개발
    public static PuzzleSystem Instance;
    int BubbleNum;
    static int PrefabNum;
    [SerializeField]
    public List<GameObject> poolingObjectPrefabs;
    List<List<Bubble>> poolingObjectsList; //2차배열

    //버블 생성 포지션
    public Vector3 StartPosition; 
    public int RowNum;
    public int CulNum;
    static GameObject BubbleParent = null;

    //점수 시스템
    ScoreSystem scoreSystem;

    //선택한 2개의 버블
    public SelectedBubbleIdx[] SelectedBubbleIdxs;
    public bool EnableInput = true; // 사용자가 입력이 가능한지
    bool bIsMatching = false; //버블이 매치로 인해, 움직이고 있을 시의 플래그

    //버블들 정보
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

        // 버블 선택 인덱스 초기화
        InitSelectedBubbleIdxs();

        //점수 시스템
        scoreSystem = GameObject.Find("Score System").GetComponent<ScoreSystem>();
    }

    private void InitPuzzle()
    {
        //퍼즐 첫 생성 : 매치가 확정된 퍼즐이 없어야 함, = 동일한 퍼즐 타입이 3개 이상 연속으로 존재하지 않아야 함.

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

        //모든 퍼즐 타입 추가
        for (int i = 0; i < PrefabNum; i++)
        {
            possibleTypes.Add((BubbleType)i);
        }

        //현재 행에서 이전 두 퍼즐이 동일한 타입인지 확인 & 동일하면 해당 타입 제외
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
        // 현재 열에서 이전 두 퍼즐이 동일한 타입인지 확인 & 동일하면 해당 타입 제외 
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

        // 가능한 타입 중에서 랜덤으로 선택
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

    // 행(row)과 열(cul)에 해당하는 퍼즐 버블을 오브젝트 풀에서 가져옴
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
        else //다 꺼내고 없을 때 
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
        //위치 배치하기
        Vector3 tmp1 = StartPosition;
        Vector3 tmp2 = StartPosition;

        //위치와 스케일 설정해주기
        for (int i = 0; i < CulNum; i++)
        {
            tmp1 = tmp2;
            for (int j = 0; j < RowNum; j++)
            {
                var obj = GetObject(j, i);
                obj.transform.localPosition = tmp1;
                tmp1 += new Vector3(0, -30, 0);

                //스케일 설정
                obj.transform.localScale = new Vector3(40, 40, 40);

                //row 와 cul 설정해주기
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

    // 스캔되는 버블이 없을 때까지 반복해서 스캔
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

        //선택 초기화
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


    // 선택한 버블의 위치 바꾸기
    void ChangeBubblePosition()
    {
        if (SelectedBubbleIdxs[0].row != -1 && SelectedBubbleIdxs[0].cul != -1
            && SelectedBubbleIdxs[1].row != -1 && SelectedBubbleIdxs[1].cul != -1)
        {
            //두 버블의 인덱스가 1차이 라면 자리를 바꾼다
            if (Mathf.Abs(SelectedBubbleIdxs[0].row - SelectedBubbleIdxs[1].row) <= 1 && Mathf.Abs(SelectedBubbleIdxs[0].cul - SelectedBubbleIdxs[1].cul) <= 1)
            {
                bIsMatching = true;

                // [2] 코루틴으로 이동시키기
                Vector3 pos_0 = GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).transform.localPosition;
                Vector3 pos_1 = GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).transform.localPosition;
                StartCoroutine(SwapPos(pos_0, pos_1));
            }
            else 
            {
                InitSelectedBubbleIdxs(); //선택 취소로 만들기
            }
        }
    }

    IEnumerator SwapPos(Vector3 origin, Vector3 target)
    {
        int cnt = 0;

        // 위치를 변경했으므로 원 배열에서의 행렬을 바꿔줘야함
        int row_0 = GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).GetBubbleInfo().GetRow(); // A의 원래 행렬
        int cul_0 = GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).GetBubbleInfo().GetCul();
        int row_1 = GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).GetBubbleInfo().GetRow(); // B의 원래 행렬
        int cul_1 = GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).GetBubbleInfo().GetCul();

        // 서로의 위치로 바꾸기
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
        Instance.poolingObjectsList[row_1][cul_1] = tmp; // A와 B를 poolingobject 리스트 상에서 교체

        // 버블 각각의 인덱스 정보(info)도 변경
        GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).GetBubbleInfo().SetRow(row_0); // A 자리에 있는 B에게 A의 행렬을 주기
        GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).GetBubbleInfo().SetCul(cul_0);
        GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).GetBubbleInfo().SetRow(row_1); // B 자리에 있는 A에게 B의 행렬을 주기
        GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).GetBubbleInfo().SetCul(cul_1);

        // 매치가 되지 않을 시
        if (!ScanBubbles())
        {

            // 위치 변경 (초기화)
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

            // 버블 각각의 인덱스 정보(info)도 변경 초기화
            GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).GetBubbleInfo().SetRow(row_1);
            GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).GetBubbleInfo().SetCul(cul_1);
            GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).GetBubbleInfo().SetRow(row_0);
            GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).GetBubbleInfo().SetCul(cul_0);

            // 원 배열 인덱스 초기화
            var tmp_ = Instance.poolingObjectsList[row_0][cul_0];
            Instance.poolingObjectsList[row_0][cul_0] = Instance.poolingObjectsList[row_1][cul_1];
            Instance.poolingObjectsList[row_1][cul_1] = tmp_;

            bIsMatching = false;
        }
        else // 매치가 될 시
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

            // 각 요소에 SelectedBubbleIdx 인스턴스 할당 (객체의 생성자가 호출되지 않음)
            for (int i = 0; i < SelectedBubbleIdxs.Length; i++)
            {
                SelectedBubbleIdxs[i] = new SelectedBubbleIdx();
            }
        }
        //선택 초기화
        SelectedBubbleIdxs[0].row = -1;
        SelectedBubbleIdxs[0].cul = -1;
        SelectedBubbleIdxs[1].row = -1;
        SelectedBubbleIdxs[1].cul = -1;
    }

    // 전체 버블을 스캔하기
    bool ScanBubbles()
    {
        bool HasMatchedBubbles = false; // 매치되는 버블이 있는지 판단

        Bubble root;
        Queue queue = new Queue(); //방문할 큐
        List<Bubble> MatchedBubbles = new List<Bubble>(); // 모든 매칭된 버블들

        for (int i = 0; i < RowNum; i++)
        {
            for (int j = 0; j < CulNum; j++)
            {
                // root 변경
                root = GetObject(i, j);
                if (root.GetVisited()) continue; // 가본 노드면 패스하기
                if (root.GetBubbleInfo().GetState() == BubbleState.Matched) continue; // 매치가 된 상태면 패스하기

                typeCnt = 1;
                queue.Enqueue(root); //큐의 끝에 Enqueue
                root.SetVisited(true); // (방문한 버블 체크)

                List<Bubble> SameTypeBubbles = new List<Bubble>();
                SameTypeBubbles.Add(root);

                // 큐가 소진될 때까지 계속 같은 타입을 찾는다
                while (queue.Count != 0)
                {
                    Bubble r = (Bubble)queue.Dequeue(); // 큐의 앞에서 노드 추출

                    //루트 노드의 row와 cul
                    int r_row = r.GetBubbleInfo().GetRow();
                    int r_cul = r.GetBubbleInfo().GetCul();

                    SameTypeBubbles.Add(r); // 루트와 같은 타입의 버블에 (루트를) 추가

                    //visit(r); //큐에서 추출한 노드 방문
                    //큐에서 Dequeue한 노드의 인접 노드들을 모두 차례로 방문한다.

                    // [1] 왼쪽
                    if ((r_row >= 0 && r_row < RowNum) && (r_cul - 1 >= 0 && r_cul - 1 < CulNum))
                        AddToMatchedBubbles(r, r_row, r_cul - 1, queue, SameTypeBubbles);
                    // [2] 오른쪽
                    if ((r_row >= 0 && r_row < RowNum) && (r_cul + 1 >= 0 && r_cul + 1 < CulNum))
                        AddToMatchedBubbles(r, r_row, r_cul + 1, queue, SameTypeBubbles);
                    // [3] 위
                    if ((r_row - 1 >= 0 && r_row - 1 < RowNum) && (r_cul >= 0 && r_cul < CulNum))
                        AddToMatchedBubbles(r, r_row - 1, r_cul, queue, SameTypeBubbles);
                    // [4] 아래
                    if ((r_row + 1 >= 0 && r_row + 1 < RowNum) && (r_cul >= 0 && r_cul < CulNum))
                        AddToMatchedBubbles(r, r_row + 1, r_cul, queue, SameTypeBubbles);
                }

                //매치가 3번 이상 되면
                if (typeCnt >= 3)
                {

                    SameTypeBubbles = SameTypeBubbles.Distinct().ToList();

                    HasMatchedBubbles = true;

                    //점수 10점 단위 증가
                    if (scoreSystem) scoreSystem.ChangeScore(10 * SameTypeBubbles.Count);

                    //매칭된 버블 리스트에 모은 같은 타입 버블 리스트를 추가하기
                    for (int s = 0; s < SameTypeBubbles.Count; s++)
                    {
                        SameTypeBubbles[s].GetBubbleInfo().SetState(BubbleState.Matched); // 매치된 상태로 변경
                        MatchedBubbles.Add(SameTypeBubbles[s]);
                    }
                }

                // 같은 타입 매칭 초기화
                typeCnt = 1;
                SameTypeBubbles.Clear();

                // 모든 visit 초기화
                InitAllVisited();
            }
        }

        StartCoroutine(ChangeMatchedBubbles(MatchedBubbles));

        return HasMatchedBubbles;
    }

    IEnumerator ChangeMatchedBubbles(List<Bubble> MatchedBubbles)
    {
        MatchedBubbleQueue.Enqueue(MatchedBubbles);
        

        // 매치된 버블들의 타입과 이미지 변경
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

        // 매칭된 버블들 리스트 삭제
        MatchedBubbles.Clear();

        yield return null;
    }


    void AddToMatchedBubbles(Bubble r, int r_row, int r_cul, Queue queue, List<Bubble> SameTypeBubbles)
    {
        Bubble b = GetObject(r_row, r_cul);

        if (b.GetVisited() == false && b.GetBubbleInfo().GetState() == BubbleState.UnMatched) // 방문하지 않았고 + 매치되지 않았다면
        {
            b.SetVisited(true); // 방문한 노드 체크

            // 타입이 같다면
            if (b.GetBubbleInfo().GetType_() == r.GetBubbleInfo().GetType_())
            {
                typeCnt++;
                queue.Enqueue(b); // 큐의 끝에 Enqueue

                SameTypeBubbles.Add(r); // 루트와 같은 타입의 버블에 (루트를) 추가
            }
        }
    }

    void InitAllVisited()
    {
        for (int i = 0; i < RowNum; i++) for (int j = 0; j < CulNum; j++) GetObject(i, j).SetVisited(false);
    }

}
