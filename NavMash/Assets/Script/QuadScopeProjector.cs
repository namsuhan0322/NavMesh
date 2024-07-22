using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadScopeProjector : MonoBehaviour
{
    public float size = 2.0f;
    public float fadeSpeed = 2.0f;
    public Material projectorMaterial;                      // 트렌스 메테리얼 사용

    private MeshRenderer quadRenderer;
    private bool isFading = false;

    void Awake()
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);           // 기본 도형 만들기
        quad.transform.SetParent(transform);
        quad.transform.localPosition = Vector3.zero;                                // 위치 초기화
        quad.transform.localRotation = Quaternion.Euler(90, 0, 0);                  // 바닥에 Quad 를 놓기 위해서 X = 90
        quad.transform.localScale = Vector3.one * size;                             // 사이즈 설정

        quadRenderer = quad.GetComponent<MeshRenderer>();
        quadRenderer.material = projectorMaterial;                                          // 받아온 메테리얼을 적용 시킨다.
        quadRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;       // 그림자를 코드로 끈다.
        quadRenderer.receiveShadows = false;

        Destroy(quad.GetComponent<Collider>());
    }

    void Update()
    {
       if (isFading)                                            // 페이딩 중이면
        {
            float currentAlpha = GetAlpha();                    // 함수를 통해 알파 값을 가져온다.
            currentAlpha -= fadeSpeed * Time.deltaTime;         // 시간이 지날수록 흐려진다.
            SetAlpha(currentAlpha);                     
            if (currentAlpha <= 0)                              // 0 이하이면
            {
                gameObject.SetActive(false);                    // 오브젝트르 끈다.
            }
        }
    }

    public void ShowAtPosition(Vector3 position)                    // 받은 포지션에서 오브젝트를 보여준다.
    {
        transform.position = position + Vector3.up * 0.1f;
        SetAlpha(1);
        isFading = false;
        gameObject.SetActive(true);
    }

    public void StartFading()
    {
        isFading = true;
    }

    private void SetAlpha(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);                       // 값을 0 ~ 1로
        Color color = projectorMaterial.color;              // 컬러의 알파 값을 접근
        color.a = alpha;                                    // 알파 값 할당
        projectorMaterial.color = color;
    }

    private float GetAlpha()                                 // 알파 값을 가져 오는 함수
    {
        return projectorMaterial.color.a;                   // 컬러 알파 값 리턴
    }
}
