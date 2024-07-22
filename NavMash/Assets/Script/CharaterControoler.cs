using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharaterControoler : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float rotationSpeed = 10f;
    public string speedParameterName = "Speed";
    public GameObject scopeProjetorPrefabs;                                     // 캐릭터 이동 위치를 알려줄 프리팹
    public NavMeshAgent agent;                                                  // 네브매시 클래스 할당
    public Animator animator;
    private Camera mainCamara;                                                  // 레이케스트를 하기 위해 카메라를 가져온다.
    public QuadScopeProjector scopeProjector;

    void Start()
    {
        mainCamara = Camera.main;
        agent.speed = moveSpeed;                                                // 네브메시 스피드 할당

        GameObject projectorObj = Instantiate(scopeProjetorPrefabs);
        scopeProjector = projectorObj.GetComponent<QuadScopeProjector>();
        projectorObj.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamara.ScreenPointToRay(Input.mousePosition);         // 카메라에서 스크린 위치의 마우스포지션에서 레이케스팅을 한다.
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))                                  // 레이케스팅에 히팅이 되는것이 있을 경우
            {
                agent.SetDestination(hit.point);                                // 에이전트의 목적지는 히트 포인트로 한다.
                scopeProjector.gameObject.SetActive(true);
                scopeProjector.ShowAtPosition(hit.point);
            }
        }

        float currentSpeed = Mathf.Clamp01(agent.velocity.magnitude / agent.speed);     // 0 ~ 1 사이값으로 변경

        animator.SetFloat(speedParameterName, currentSpeed);                            // 블랜딩 애니메이션 값에 넣어 준다.

        if (agent.velocity.magnitude > 0.1f)
        {
            Quaternion toRotation = Quaternion.LookRotation(agent.desiredVelocity, Vector3.up);                     // 이동할 회전 값을 구한다.
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);  // 회전 보정을 해준다.
        }

        if (!agent.pathPending && agent.remainingDistance < 0.1f)       // 에이전트가 해다 위치에 다 도착할 때 쯤
        {
            scopeProjector.StartFading();
        }
    }
}
