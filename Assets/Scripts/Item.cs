using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eItemType//열거형 자료형
{
    Recorvery,
    PowerUp,
}

public class Item : MonoBehaviour
{
    [SerializeField] eItemType itemType;
    private Vector3 moveDir;//움직일 방향
    private float speed;//움직이는 속도

    [SerializeField] Vector2 speedMinMax;//랜덤으로 움직일 속도 민,맥스 

    [Header("화면 리밋 비율")]
    [SerializeField][Tooltip("최소비율")] Vector2 minScreen;//최소비율
    [SerializeField, Tooltip("최대비율")] Vector2 maxScreen;//최대비율
    private Camera cam;

    void Start()
    {
        float dirX = Random.Range(-1.0f, 1.0f);
        float dirY = Random.Range(-1.0f, 1.0f);

        moveDir = new Vector2(dirX, dirY);//3에 2를 넣게되면 z는 0으로 
        //moveDir.Normalize();
        speed = Random.Range(speedMinMax.x, speedMinMax.y);

        cam = Camera.main;
    }

    void Update()
    {
        transform.position += moveDir * speed * Time.deltaTime;
        checkMovePosition();
    }

    private void checkMovePosition()
    {
        Vector3 curPos = cam.WorldToViewportPoint(transform.position);

        if (curPos.x < minScreen.x && moveDir.x < 0)//왼쪽 
        {
            moveDir = Vector3.Reflect(moveDir, Vector3.left);
        }
        else if (curPos.x > maxScreen.x && moveDir.x > 0)//오른쪽
        {
            moveDir = Vector3.Reflect(moveDir, Vector3.right);
        }

        if (curPos.y < minScreen.y && moveDir.y < 0)//아래
        {
            moveDir = Vector3.Reflect(moveDir, Vector3.down);
        }
        else if (curPos.y > maxScreen.y && moveDir.y > 0)//위
        {
            moveDir = Vector3.Reflect(moveDir, Vector3.up);
        }
    }

    public eItemType GetItemType()
    {
        return itemType;
    }
}
