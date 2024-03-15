using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    public enum enumEnemy
    {
        EnemyA,
        EnemyB,
        EnemyC,
        Boss
    }

    [SerializeField] enumEnemy enemyType;

    [SerializeField] bool boss;//보스인지 판별
    public bool Boss => boss;
    [SerializeField] Vector2 vecCamMinMax;

    bool bossStartMove = false;//첫 등장시 연출을 했는지, 무적상태
    private float startPosY;//본인이 생성된 y위치
    float ratioY;//2.5
    bool swayRight = false;//좌우 true가 되면 우측으로 false가 되면 좌측으로
    Animator anim;


    [SerializeField] float hp;
    float hpMax;
    [SerializeField] float speed;

    SpriteRenderer spriteRenderer;
    Sprite spriteDefault;
    [SerializeField] Sprite spriteHit;

    [SerializeField] GameObject fabExplosion;

    GameManager gameManager;

    [Header("아이템")]
    [SerializeField] bool hasItem = false;//아이템을 가지고 있는지
    bool dropItem = false;//아이템을 드롭했는지

    [Header("보스 미사일 패턴")]
    //좌우로 이동하면 위치 기준 전방으로 총알을 발사
    [SerializeField] int pattern1Count = 8;
    [SerializeField] float pattern1Reloadt = 1f;
    [SerializeField] GameObject pattern1Missile;
    //샷건 처럼 각도를 가지며 퍼지는 총알을 발사
    [SerializeField] int pattern2Count = 5;
    [SerializeField] float pattern2Reloadt = 0.8f;
    [SerializeField] GameObject pattern2Missile;
    //플레이어를 정확히 조준해서 빠르게 총알을 발사
    [SerializeField] int pattern3Count = 40;
    [SerializeField] float pattern3Reloadt = 0.3f;
    [SerializeField] GameObject pattern3Missile;

    int curPattern = 1;//현재 사용중인 패턴
    int curPatternShootCount = 0;//패턴 사용 횟수
    float patternTimer;//패턴 타이머
    bool patternChange = false;//패턴을 바꿔야하는 상황인지
    [SerializeField] float PatternChangeTime = 1f;//패턴을 바꿀때 딜레이되는 시간

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.RemoveSpawnEnemyList(gameObject);
        }
    }

    private void OnBecameInvisible()
    {
        if (boss == false)
        { Destroy(gameObject); }
    }


    private void Awake()
    {
        anim = GetComponent<Animator>();
        hpMax = hp;
    }


    void Start()
    {

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteDefault = spriteRenderer.sprite;

        if (hasItem == true)
        {
            spriteRenderer.color = new Color(0f, 0.4f, 1f, 1f);
        }

        gameManager = GameManager.Instance;
        gameManager.AddSpawnEnemyList(gameObject);
        if (boss == true)
        {
            gameManager.HitBoss((int)hp, (int)hpMax);
        }

        startPosY = transform.position.y;
    }

    void Update()
    {
        moving();
        shoot();
    }
    private void moving()
    {
        if (boss == false)
        {
            transform.position += -transform.up * Time.deltaTime * speed;
        }
        else
        {
            if (bossStartMove == false)
            {
                bossStartMoving();
            }
            else
            {
                bossSwayMoving();
            }
        }
    }

    private void shoot()
    {
        if (boss == false || bossStartMove == false) return;

        patternTimer += Time.deltaTime;
        if (patternChange == true)//패턴이 바뀔때 잠시 멈춰있는시간(보스 무력화)
        {
            if (patternTimer >= PatternChangeTime)
            {
                patternTimer = 0;
                patternChange = false;
            }
            return;
        }

        switch (curPattern)
        {
            case 1://전방으로 총알을 발사
                if (patternTimer >= pattern1Reloadt)
                {
                    patternTimer = 0.0f;
                    shootStraight();
                    if (curPatternShootCount >= pattern1Count)
                    {
                        curPattern++;
                        //curPattern = 2;

                        //int rand = Random.Range(1, 4);//패턴 랜덤
                        //while (curPattern == rand) //현재 사용중인 패턴과 같다면 랜덤 한번 더돌리기
                        //{
                        //    rand = Random.Range(1, 4);//
                        //}
                        //curPattern = rand;

                        patternChange = true;
                        curPatternShootCount = 0;
                    }
                }
                break;
            case 2:
                if (patternTimer >= pattern2Reloadt)
                {
                    patternTimer = 0.0f;
                    shootShotgun();
                    if (curPatternShootCount >= pattern2Count)
                    {

                        curPattern++;
                        patternChange = true;
                        curPatternShootCount = 0;
                    }
                }
                break;
            case 3:
                if (patternTimer >= pattern3Reloadt)
                {
                    patternTimer = 0.0f;
                    shootGatling();
                    if (curPatternShootCount >= pattern3Count)
                    {

                        curPattern = 1;
                        patternChange = true;
                        curPatternShootCount = 0;
                    }
                }
                break;
        }
    }

    private void shootStraight()
    {
        curPatternShootCount++;
        createMissile(transform.position, new Vector3(0f, 0f, 180f), pattern1Missile);
        createMissile(transform.position + new Vector3(-1f, 0f, 0f), new Vector3(0f, 0f, 180f), pattern1Missile);
        createMissile(transform.position + new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, 180f), pattern1Missile);
    }

    private void shootShotgun()
    {
        curPatternShootCount++;
        createMissile(transform.position, new Vector3(0f, 0f, 180f), pattern2Missile);
        createMissile(transform.position, new Vector3(0f, 0f, 180f - 15f), pattern2Missile);
        createMissile(transform.position, new Vector3(0f, 0f, 180f + 15f), pattern2Missile);
        createMissile(transform.position, new Vector3(0f, 0f, 180f - 30f), pattern2Missile);
        createMissile(transform.position, new Vector3(0f, 0f, 180f + 30f), pattern2Missile);
        createMissile(transform.position, new Vector3(0f, 0f, 180f - 45f), pattern2Missile);
        createMissile(transform.position, new Vector3(0f, 0f, 180f + 45f), pattern2Missile);
        createMissile(transform.position, new Vector3(0f, 0f, 180f - 60f), pattern2Missile);
        createMissile(transform.position, new Vector3(0f, 0f, 180f + 60f), pattern2Missile);

    }

    private void shootGatling()
    {
        curPatternShootCount++;
        Transform trsPlayer = gameManager.GetPlayerTransform();
        if (trsPlayer == null) return;
        Vector3 playerPos = trsPlayer.position;

        float angle = Quaternion.FromToRotation(Vector3.up, playerPos - transform.position).eulerAngles.z; //각도 활용할때 자주 쓸 예정

        createMissile(transform.position, new Vector3(0f, 0f, angle), pattern3Missile);
    }

    private void createMissile(Vector3 _pos, Vector3 _rot, GameObject _instObj)
    {
        GameObject objMissile = Instantiate(_instObj, _pos, Quaternion.Euler(_rot));
    }

    private void bossStartMoving()
    {
        ratioY += Time.deltaTime * 0.5f;
        if (ratioY >= 1.0f)
        {
            bossStartMove = true;
        }

        Vector3 curPos = transform.position;
        //curPos = Vector3.Lerp(new Vector3(0f,startPosY,0f), new Vector3(0, 2.5f, 0), ratioY);
        curPos.y = Mathf.SmoothStep(startPosY, 2.5f, ratioY);
        transform.position = curPos;
    }

    private void bossSwayMoving()
    {
        if (swayRight)
        {
            transform.position += transform.right * Time.deltaTime * speed;
        }
        else
        {
            transform.position -= transform.right * Time.deltaTime * speed;
        }
        checkMoveLimit();
    }

    private void checkMoveLimit()
    {
        Vector3 curPos = Camera.main.WorldToViewportPoint(transform.position);
        if (swayRight == false && curPos.x < vecCamMinMax.x)
        {
            swayRight = true;
        }
        else if (swayRight == true && curPos.x > vecCamMinMax.y)
        {
            swayRight = false;
        }
    }

    public void Hit(float _damage)
    {
        if (boss == true && bossStartMove == false) return;
        hp -= _damage;

        if (hp <= 0)
        {
            destroyFunction();

            if ((hasItem == true || boss == true) && dropItem == false)//내가 아이템을 보유하고 있고 드랍하지 않았다면
            {
                dropItem = true;
                GameManager.Instance.DropItem(transform.position);
            }

            gameManager.AddKillCount();

            if (boss == true)
            {
                gameManager.BossKill();
            }

            gameManager.DestroyEnemy(enemyType);
        }
        else
        {
            if (boss == false)
            {
                spriteRenderer.sprite = spriteHit;
                Invoke("setSpriteDefault", 0.1f);
            }
            else//보스일때만
            {
                if (checkAnim() == true) ;
                {
                    anim.SetTrigger("Hit");

                }
                gameManager.HitBoss((int)hp, (int)hpMax);
                ;
            }
        }
    }
    private bool checkAnim()
    {
        bool isNameBosshit = anim.GetCurrentAnimatorStateInfo(0).IsName("BossHit");//이름이 보스히트라면 ture 아니라면 false
        //anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f // 0~1 1이라면 애니메이션이 끝까지 구동되었음
        return !isNameBosshit;

    }
    public void DestroyOnBodySlam()
    {
        if (boss == false)
        {
            destroyFunction();
        }
    }

    private void destroyFunction()
    {
        Destroy(gameObject);

        GameObject expObj = Instantiate(fabExplosion, transform.position, Quaternion.identity);
        Explosion expSc = expObj.GetComponent<Explosion>();
        expSc.SetSize(spriteDefault.rect.width);
    }

    private void setSpriteDefault()
    {
        spriteRenderer.sprite = spriteDefault;
    }

    public void SetHaveItem()
    {
        hasItem = true;
    }
}
