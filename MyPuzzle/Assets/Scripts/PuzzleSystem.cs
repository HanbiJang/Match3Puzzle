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

    ScoreSystem scoreSystem; // 점수 시스템

    public bool EnableInput = true; // 입력이 가능한지

    // 버블들 정보
    public RuntimeAnimatorController AnimCont_Blue;
    public RuntimeAnimatorController AnimCont_Green;
    public RuntimeAnimatorController AnimCont_Orange;
    public RuntimeAnimatorController AnimCont_Red;
    public RuntimeAnimatorController AnimCont_Yellow;

    // 버블들 이미지
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
        Init(BubbleNum); //버블 만들어주기

        //버블 선택 인덱스
        SelectedBubbleIdx tmp = new SelectedBubbleIdx();
        tmp.row = -1; tmp.cul = -1;
        SelectedBubbleIdxs = new SelectedBubbleIdx[2];
        SelectedBubbleIdxs[0] = tmp;
        SelectedBubbleIdxs[1] = tmp;

        // 점수 시스템
        scoreSystem = GameObject.Find("Score System").GetComponent<ScoreSystem>();
    }

    private void Init(int initCount)
    {
        /*       //큐 버전
               for (int i = 0; i < initCount; i++)
               {
                   poolingObjectQueue.Enqueue(CreateNewObject()); //버블 생성
               }*/

        for (int i = 0; i < RowNum; i++)
        {
            List<Bubble> tmpList = new List<Bubble>();
            //열 만들기
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
        var newObj = Instantiate(poolingObjectPrefabs[randomNum]).GetComponent<Bubble>(); //생성
        newObj.gameObject.SetActive(false);
        newObj.transform.SetParent(BubbleParent.transform);

        //타입 설정하기
        newObj.m_type = (BubbleType)randomNum; //int -> enum

        // 이름 설정
        newObj.name = "Bubble" + newObj.m_type + " _" + row + " _" + cul;

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

        ScanUntilNoMatch();
    }

    void ScanUntilNoMatch()
    {
        StartCoroutine(ScanUntilNoMatch_Co());
    }

    IEnumerator ScanUntilNoMatch_Co()
    {

        // 스캔되는 버블이 없을 때까지 반복
        while (true)
        {
            yield return new WaitForSeconds(0.01f);
            if (!ScanBubbles())
            {
                yield return new WaitForSeconds(0.01f);
                break;
            }
        }

        //선택 초기화
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


    // 버블의 위치 바꾸기
    void ChangeBubblePosition()
    {
        if (SelectedBubbleIdxs[0].row != -1 && SelectedBubbleIdxs[0].cul != -1
            && SelectedBubbleIdxs[1].row != -1 && SelectedBubbleIdxs[1].cul != -1)
        {
            //두 버블의 인덱스가 1차이 라면 자리를 바꾼다
            if (Mathf.Abs(SelectedBubbleIdxs[0].row - SelectedBubbleIdxs[1].row) <= 1 && Mathf.Abs(SelectedBubbleIdxs[0].cul - SelectedBubbleIdxs[1].cul) <= 1)
            {
                IsMoving = true;

                // [2] 코루틴으로 이동시키기
                Vector3 pos_0 = GetObject(SelectedBubbleIdxs[0].row, SelectedBubbleIdxs[0].cul).transform.localPosition;
                Vector3 pos_1 = GetObject(SelectedBubbleIdxs[1].row, SelectedBubbleIdxs[1].cul).transform.localPosition;
                StartCoroutine(SwapPos(pos_0, pos_1));
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

            IsMoving = false;
        }
        else // 매치가 될 시
        {
            // 스캔되는 버블이 없을 때까지 반복
            //ScanUntilNoMatch();
            IsMoving = false;
        }

        InitSelectedBubbleIdxs();

        yield return null;
    }

    void InitSelectedBubbleIdxs()
    {
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
                if (root.m_state == BubbleState.Matched) continue; // 매치가 된 상태면 패스하기

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

                // 매치가 3번 이상 되면
                if (typeCnt >= 3)
                {

                    SameTypeBubbles = SameTypeBubbles.Distinct().ToList();

                    HasMatchedBubbles = true;

                    // 점수 10점 단위 증가
                    if (scoreSystem) scoreSystem.ChangeScore(10 * SameTypeBubbles.Count);

                    // 매칭된 버블 리스트에 모은 같은 타입 버블 리스트를 추가하기
                    for (int s = 0; s < SameTypeBubbles.Count; s++)
                    {
                        SameTypeBubbles[s].m_state = BubbleState.Matched; // 매치된 상태로 변경
                        MatchedBubbles.Add(SameTypeBubbles[s]);
                    }
                }

                // 같은 타입 매칭 초기화
                typeCnt = 1;

                // 같은 타입들 버블 초기화
                for (int s = 0; s < SameTypeBubbles.Count; s++)
                {
                    SameTypeBubbles.RemoveAt(s);
                }
                // 모든 visit 초기화
                InitAllVisited();
            }
        }

        // 매치된 버블들의 타입과 이미지 변경
        //Debug.Log(MatchedBubbles.Count);
        for (int m = 0; m < MatchedBubbles.Count; m++)
        {
            int randomNum = (int)Random.Range(0, PrefabNum);
            MatchedBubbles[m].ChangeTypeAndImg(randomNum);
        }

        // 매칭된 버블들의 매치 상태 초기화
        for (int m = 0; m < MatchedBubbles.Count; m++)
        {
            MatchedBubbles[m].m_state = BubbleState.UnMatched;
        }

        // 매칭된 버블들 리스트 삭제
        for (int m = 0; m < MatchedBubbles.Count; m++)
        {
            MatchedBubbles.RemoveAt(m);
        }

        return HasMatchedBubbles;
    }

    void AddToMatchedBubbles(Bubble r, int r_row, int r_cul, Queue queue, List<Bubble> SameTypeBubbles)
    {
        //Bubble b = Instance.poolingObjectsList[r_row][r_cul]; //인접 노드
        Bubble b = GetObject(r_row, r_cul);

        if (b.GetVisited() == false && b.m_state == BubbleState.UnMatched) // 방문하지 않았고 + 매치되지 않았다면
        {
            b.SetVisited(true); // 방문한 노드 체크

            // 타입이 같다면
            if (b.m_type == r.m_type)
            {
                typeCnt++;
                queue.Enqueue(b); // 큐의 끝에 Enqueue

                SameTypeBubbles.Add(r); // 루트와 같은 타입의 버블에 (루트를) 추가
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
