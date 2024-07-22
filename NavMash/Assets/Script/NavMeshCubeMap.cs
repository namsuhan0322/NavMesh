using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class NavMeshCubeMap : MonoBehaviour
{
    public int gridSize = 10;                   // 가로 세로 큐브 개수
    public float cellSize = 1.0f;               // 각 큐브 셀 크기
    public float dropProbability = 0.1f;        // 큐브가 떨어질 확률
    public float dropDuration = 3.0f;           // 큐브가 떨어지는데 걸리는 시간
    public float terrainChangeInterval = 10f;   // 지형 변화 간격
    public GameObject cubePrefabs;              // 큐브 프리펩
    public NavMeshSurface surface;              // NavMesh를 생성할 표면
    public GameObject playerPrefab;             // 플레이어 프리팹

    private GameObject[,] grid;                 // 그리드를 저장할 2차원 배열
    private GameObject player;                  // 플레이어 게임 오브젝트
    private NavMeshAgent playerAgent;           // 플레이어 에이전트
    private float terraionChangeTimer;          // 지형 변화 타이머
    private List<DroppingCube> droppingCubes = new List<DroppingCube>();        // 떨어지고 있는 큐브리스트

    private struct DroppingCube
    {
        public GameObject cube;             // 큐브 게임 오브젝트
        public Vector3 startPos;           // 시작위치
        public Vector3 endPos;             // 끝 위치
        public float dropTimer;             // 떨어지는 시간을 측정하는 타이머
        public int z, x;                    // 그리드 내 위치
    }

    void Start()
    {
        CreateGrid();
        SpawnPlayer();
        surface.BuildNavMesh();                         // 시작할때 네브메시를 생성한다.
        terraionChangeTimer = terrainChangeInterval;
    }

    void Update()
    {
        UpdateDroppingCubes();
        HandleTerraionChanges();
    }

    // 초기 그리드 생성
    void CreateGrid()
    {
        grid = new GameObject[gridSize, gridSize];              // 그리드 사이즈 만큼 2차원 배열에 할당

        for (int x = 0; x < gridSize; x++)                      // 2중 포문을 작성
        {
            for (int z = 0; z < gridSize; z++)
            {
                Vector3 position = new Vector3(x * cellSize, 0, z * cellSize);                  // 각 큐브 위치 계산
                GameObject cube = Instantiate(cubePrefabs, position, Quaternion.identity);      // 큐브 인스턴스 화
                cube.transform.localScale = new Vector3(cellSize, cellSize, cellSize);          // 큐브 크기 설정
                grid[x, z] = cube;                                                              // 그리드에 큐브 저장
            }
        }
    }

    void SpawnPlayer()
    {
        Vector3 spawnPosition = new Vector3(gridSize / 2 * cellSize, cellSize, gridSize / 2 * cellSize);    // 그리드 중앙
        player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);                             // 플레이어 인스턴스 화
        playerAgent = player.GetComponent<NavMeshAgent>();                                                  // NavMeshAgent 컴포넌트 가져오기
    }

    void HandleTerraionChanges()                        // 지형 변화 처리
    {
        terraionChangeTimer -= Time.deltaTime;          // 타이머 감소
        if (terraionChangeTimer <= 0)
        {
            bool terrainChange = false;

            for (int x = 0; x < gridSize; x++)                       // 이중 포문을 작성
            {
                for (int z = 0; z < gridSize; z++)
                {
                    if (Random.value < dropProbability && grid[x, z] != null)
                    {
                        StartDropCube(x, z);
                        terrainChange = true;
                    }
                }
            }

            if (terrainChange)                  // 지형이 변경 되었다면 NavMesh 재구축
            {
                StartCoroutine(BakeMapNavMesh());
            }

            terraionChangeTimer = terrainChangeInterval;
        }    
    }

    IEnumerator BakeMapNavMesh()
    {
        yield return new WaitForSeconds(0.2f);
        surface.BuildNavMesh();
    }

    void UpdateDroppingCubes()          // 떨어지는 큐브들 업데이트
    {
        for (int i = droppingCubes.Count - 1; i >= 0; i--)
        {
            DroppingCube droppingCube = droppingCubes[i];
            droppingCube.dropTimer += Time.deltaTime;

            // 떨어지는 시간이 완료되면
            if (droppingCube.dropTimer >= dropDuration)
            {
                Destroy(droppingCube.cube);
                grid[droppingCube.x, droppingCube.z] = null;
                droppingCubes.RemoveAt(i);
            }
            else
            {
                droppingCube.cube.transform.position = Vector3.Lerp(droppingCube.startPos, droppingCube.endPos,
                    droppingCube.dropTimer / dropDuration);
                droppingCubes[i] = droppingCube;                // 큐브 위치 업데이트
            }
        }
    }


    void StartDropCube(int x, int z)            // 큐브를 떨어뜨리기 시작
    {
        GameObject cube = grid[x, z];
        Vector3 startPos = cube.transform.position;
        Vector3 endPos = startPos - Vector3.up * 5;

        droppingCubes.Add(new DroppingCube          // 구조체에 맞는 형태로 리스트에 넣는다. 
        {
            cube = cube,
            startPos = startPos,
            endPos = endPos,
            dropTimer = 0,
            x = x,
            z = z
        });
    }
}
