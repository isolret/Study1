using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHp : MonoBehaviour
{
    private GameManager gameManager;
    private Image Hp;
    private Image Effect;
    private Transform trsPlayer;
    [SerializeField] private float curPlayerHp; 
    [SerializeField] private float maxPlayerHp; 

    private void Awake()
    {
        Hp = transform.Find("Hp").GetComponent<Image>();
        Effect = transform.Find("Effect").GetComponent<Image>();
    }

    void Start()
    {
        gameManager = GameManager.Instance;
        trsPlayer = gameManager.GetPlayerTransform(); 
    }

    
    void Update()
    {
        checkPostion();
        checkPlayerHp();
        isDestorying();
    }

    private void checkPostion()
    {
        if (trsPlayer == null) return;

        Vector3 movePos = trsPlayer.position;//플레이어의 위치
        movePos.y -= 0.7f;
        transform.position = movePos;
    }
    private void checkPlayerHp()
    {
        if (Hp.fillAmount < Effect.fillAmount)//데미지를 입어 체력이 감소했을때
        {
            Effect.fillAmount -= Time.deltaTime * 0.5f; //이펙트의 붉은 부분이 감소됨

            if (Effect.fillAmount <= Hp.fillAmount)//만약 Hp값보다 작아져 버린다면
            {
                Effect.fillAmount = Hp.fillAmount;//이펙트를 Hp값으로 변경
            }
        }
        else if (Hp.fillAmount > Effect.fillAmount)//체력을 회복했을때
        {
            Effect.fillAmount = Hp.fillAmount;//이펙트를 Hp값으로 변경
        }

        float value = curPlayerHp / maxPlayerHp;//남은 hp의 비율
        if (Hp.fillAmount > value) //데미지를 입음
        {
            Hp.fillAmount -= Time.deltaTime;
            if (Hp.fillAmount <= value)
            {
                Hp.fillAmount = value;
            }
        }
        else if (Hp.fillAmount < value)//회복 되었을때
        {
            Hp.fillAmount += Time.deltaTime;
            if (Hp.fillAmount >= value)
            {
                Hp.fillAmount = value;
            }
        }
    }

    private void isDestorying()
    {
        if (Effect.fillAmount <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetPlayerHp(int _hp, int _maxHp)
    {
        curPlayerHp = _hp;
        maxPlayerHp = _maxHp;
    }
}
