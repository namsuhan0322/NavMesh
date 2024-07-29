using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class AreaFloorBaker : MonoBehaviour
{
    [SerializeField] private NavMeshSurface Surface;                                // NavMesh 생성에 사용될 Surface    
    [SerializeField] private GameObject Player;                                     // NavMesh가 따라가 플레이어 오브젝트
    [SerializeField] private float UpdateRate = 0.1f;                               // NavMesh 업데이트 주기
    [SerializeField] private float MovementThredshold = 3f;                         // NavMesh 를 다시 생성할 플레이어 이동 거리의 임계 값
    [SerializeField] private Vector3 MavMeshSize = new Vector3 (20, 20, 20);        // NavMesh 크기

    private Vector3 WorldAnchor;                                                    // NavMesh 중심점
    private NavMeshData navMeshData;                                                // NavMesh 데이터
    private List<NavMeshBuildSource> Sources = new List<NavMeshBuildSource>();      // NavMesh 생성에 사용될 소스들

    void Start()
    {
        // NavMesh 데이터 초기화 및 생성
        navMeshData = new NavMeshData();
        NavMesh.AddNavMeshData(navMeshData);
        BuildNavMesh(false);
        StartCoroutine(CheckPlayerMovement());
    }

    // 플레이어 움직임 체크 코루틴
    private IEnumerator CheckPlayerMovement()
    {
        WaitForSeconds Wait = new WaitForSeconds(UpdateRate);

        while (true)
        {
            // 플레이어가 일정 거리 이상 이동 했는지 체크
            if (Vector3.Distance(WorldAnchor, Player.transform.position) > MovementThredshold)
            {
                BuildNavMesh(true);
                WorldAnchor = Player.transform.position;
            }
            yield return Wait;
        }
    }

    // NavMesh 생성 메서드
    private void BuildNavMesh(bool Async)
    {
        // NavMesh 경계 설정
        Bounds navMeshBounds = new Bounds(Player.transform.position, MavMeshSize);
        List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();
        List<NavMeshModifier> modifiers;

        // NavMeshModifier 컴포넌트 수집
        if (Surface.collectObjects == CollectObjects.Children)
        {
            modifiers = new List<NavMeshModifier>(GetComponentsInChildren<NavMeshModifier>());
        }
        else
        {
            modifiers = NavMeshModifier.activeModifiers;
        }
        // NavMeshBuildMarkup 설정
        for (int i = 0; i < modifiers.Count; i++)
        {
            if (((Surface.layerMask & (1 << modifiers[i].gameObject.layer)) == 1) &&
                modifiers[i].AffectsAgentType(Surface.agentTypeID))
            {
                markups.Add(new NavMeshBuildMarkup()
                {
                    root = modifiers[i].transform,
                    overrideArea =modifiers[i].overrideArea,
                    area = modifiers[i].area,
                    ignoreFromBuild = modifiers[i].ignoreFromBuild,
                });
            }
        }

        // NavMesh 소스 수집
        if (Surface.collectObjects == CollectObjects.Children)
        {
            NavMeshBuilder.CollectSources(transform, Surface.layerMask, Surface.useGeometry, Surface.defaultArea, markups, Sources);
        }
        else
        {
            NavMeshBuilder.CollectSources(navMeshBounds, Surface.layerMask, Surface.useGeometry, Surface.defaultArea, markups, Sources);
        }

        // NavMeshAgent 컴포넌트가 있는 소스를 제거
        Sources.RemoveAll(source => source.component != null && source.component.gameObject.GetComponent<NavMeshAgent>() != null);

        // NavMesh 데이터 업데이트 (비동기 버전과 동기버전)
        if (Async)
        {
            NavMeshBuilder.UpdateNavMeshDataAsync
                (navMeshData, Surface.GetBuildSettings(), Sources, new Bounds(Player.transform.position, MavMeshSize));
        }
        else
        {
            NavMeshBuilder.UpdateNavMeshData
                (navMeshData, Surface.GetBuildSettings(), Sources, new Bounds(Player.transform.position, MavMeshSize));
        }
    }
}
