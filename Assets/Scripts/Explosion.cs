using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    SpriteRenderer sr;
    float sprSize;

    private void Awake()//자기 자신을 정의
    {
        sr = GetComponent<SpriteRenderer>();
        sprSize = sr.sprite.rect.width;
    }

    public void EndOfAnimation()//애니메이터에게 이 함수를 사용하면 오브젝트가 삭제된다고 전달 된 상황
    {
        Destroy(gameObject);
    }

    public void SetSize(float _size)
    {
        Vector3 vecScale = transform.localScale;
        vecScale *= (_size / sprSize) * 1.5f;
        transform.localScale = vecScale;
    }
}

