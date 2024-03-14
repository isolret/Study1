using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    [SerializeField] float speed;
    float damage = 1.0f;
    bool isEnemyMissile = true;//적의 미사일인지
    bool isHit = false;

    //콜라이더와 리지드바디를 가지고 있을때 다른 물리콜라이더와 닿았는지 체크하고 싶을때
    private void OnTriggerEnter2D(Collider2D collision)//collision은 내 콜라이더에 닿은 상대방
    {
        if (isHit == true) return;

        if (isEnemyMissile == false && collision.tag == Tool.GetGameTag(GameTag.Enemy))
        {
            isHit = true;

            Enemy enemySc = collision.GetComponent<Enemy>();
            enemySc.Hit(damage);

            Destroy(gameObject);
        }
        else if(isEnemyMissile == true && collision.tag == Tool.GetGameTag(GameTag.Player))
        {
            isHit = true;

            Player playerSc = collision.GetComponent<Player>();
            playerSc.Hit();

            Destroy(gameObject);
        }
    }

    //렌더링 되고있다가 더이상 렌더링 되지 않게 되었을때
    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    void Update()
    {
        //transform.position = transform.position + Vector3.up * Time.deltaTime * speed; 
        transform.position = transform.position + transform.up * Time.deltaTime * speed;
    }

    public void SetMissile(float _speed, float _damege)
    {
        speed = _speed;
        damage = _damege;
        isEnemyMissile = false;
    }
}
