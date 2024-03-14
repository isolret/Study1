using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;
using Newtonsoft.Json;
using UnityEditor.Experimental.GraphView;

public enum SceneNums
{
    MainScene,
    PlayScene,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;//아무이름이나 적으셔도 됨

    [Header("적기생성")]
    [SerializeField] bool isSpawn = false;
    [SerializeField] List<GameObject> listEnemy;//적기의 종류
    List<GameObject> listSpawnEnemy = new List<GameObject>();//생성된 적기들

    [SerializeField, Range(0.1f, 2.0f)] float spawnTime = 1.0f;
    float sTimer = 0.0f;//스폰타이머 
    Transform trsSpawnPoint;

    [Header("적기생성 카메라리밋")]
    [SerializeField] Vector2 vecCamMinMax;//기획자가 설정하는 위치값, 카메라로부터
    Vector2 vecSpawnLimit;//월드 포지션 기준 생성 리밋 위치값, 월드포지션

    [Header("아이템드롭")]
    [SerializeField, Range(0.0f, 100.0f)] float itemDropRate = 0.0f;//0.0~100.0f
    [SerializeField] List<GameObject> listItem;

    Camera mainCam;

    [SerializeField] GameObject objPlayer;

    [Header("게이지")]
    [SerializeField] Slider slider;
    [SerializeField] Image sliderFill;
    [SerializeField] TMP_Text textTimer;

    float bossSpawnTime = 60f;
    [SerializeField] float gameTime = 0f;
    bool spawnBoss = false;
    float colorRatio = 0;//0일때는 일반상태, 1일때는 보스 체력상태

    [SerializeField] Color colorTimer;
    [SerializeField] Color colorBossHp;


    [SerializeField] int killCountBossSpawn = 60;
    [SerializeField] int killCount = 0;

    [Header("점수")]
    [SerializeField] TMP_Text textScore;//점수 텍스트
    int score; //실제 점수

    [Header("게임오버메뉴")]
    [SerializeField] GameObject objGameoverMenu;
    [SerializeField] TMP_Text textGameover;
    [SerializeField] TMP_InputField iFGameover;
    [SerializeField] Button btnGameover;
    int rank = -1;
    string keyRankData = "rankData";


    public class cRank
    {
        public int score = 0;
        public string name = "";
    }

    List<cRank> listRank = new List<cRank>();//0~9 유저의 랭킹

    private void Awake()
    {
        if(Tool.IsEnterFirstScene == false)//플레이씬에서 실행하면 메인씬으로 가게 해주는 코드
        {
            SceneManager.LoadScene((int)SceneNums.MainScene);
            return;
        }

        if (Instance == null)//싱글턴, 싱글톤, 디자인패턴, 단하나만 존재하는 스크립트 만들어야 할때
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        initGameoverMenu();
        initRank();
    }

    //플레이어가 사망했을때 현재 점수가 랭크로부터 몇등인지 가져오는 함수.
    private int GetRank()//점수가 더 낮은걸 덮어씌우기
    {
        rank = listRank.Count;
        for (int iNum = 0; iNum < rank; iNum++)
        {
            cRank rank = listRank[iNum];
            if (rank.score < score)//랭크의 점수가 현재 점수보다 낮다면 그 자리를 현재 점수가 차지
            {
                return iNum + 1;
            }
        }
        return -1;
    }

    private void initGameoverMenu()
    {
        if (objGameoverMenu.activeSelf == true)//오브젝트가 active true(활성화) 되어있다면
        {
            objGameoverMenu.SetActive(false);//비활성화
            //objGameoverMenu.SetActive(true);//활성화
        }

        btnGameover.onClick.AddListener(saveAndNextScene);
    }

    private void initRank()
    {
        string rankValue = PlayerPrefs.GetString(keyRankData, string.Empty);//string.Empty == "";
        if (rankValue == string.Empty) // 랭크밸류가 ("";)비어있다면
        {
            int count = 10;
            for (int iNum = 0; iNum < count; iNum++) // 10개의 비어있는 랭크를 생성할때까지 반복
            {
                listRank.Add(new cRank());
            }

            rankValue = JsonConvert.SerializeObject(listRank);
            PlayerPrefs.SetString(keyRankData, rankValue);
        }
        else//string.Empty가 아니었다면
        {
            listRank = JsonConvert.DeserializeObject<List<cRank>>(rankValue);
        }
    }

    private void saveAndNextScene()
    {
        //랭크에 들었었다면 name과 score를 rankList에 저장해주고 11등은 삭제
        if (rank != -1)
        {
            cRank data = new cRank();
            data.score = score;
            data.name = iFGameover.text;

            listRank.Insert(rank - 1, data);
            listRank.RemoveAt(listRank.Count - 1);

            string saveValue = JsonConvert.SerializeObject(listRank);
            PlayerPrefs.SetString(keyRankData, saveValue);//유니티에 저장됨
        }

        //랭크에 들지 못했다면 다른 씬으로 이동(다른씬은 메인씬)
        //오버로드 오버라이드 찾아보기
        SceneManager.LoadSceneAsync((int)SceneNums.MainScene);//메인씬으로 이동

    }

    private void Start()
    {
        mainCam = Camera.main;
        trsSpawnPoint = transform.Find("SpawnPoint");

        vecSpawnLimit.x = mainCam.ViewportToWorldPoint(
            new Vector3(vecCamMinMax.x, 0f)
            ).x;

        vecSpawnLimit.y = mainCam.ViewportToWorldPoint(
            new Vector3(vecCamMinMax.y, 0f)
            ).x;

        initSlider();
    }

    void Update()
    {
        checkSpawn();
        checkTime();
    }

    private void checkSpawn()//적기를 소환해도 되는지 체크
    {
        if (isSpawn == false || spawnBoss == true) return;

        sTimer += Time.deltaTime;
        if (sTimer >= spawnTime && gameTime < bossSpawnTime - 1)
        {
            sTimer = 0.0f;
            createEnemy();//적기생산
        }
    }

    private void checkTime()
    {
        if (spawnBoss == true)
        {
            if (sliderFill.color != colorBossHp)
            {
                if (colorRatio != 1.0f)
                {
                    colorRatio += Time.deltaTime;
                    if (colorRatio > 1.0f)
                    {
                        colorRatio = 1.0f;
                    }
                }
                sliderFill.color = Color.Lerp(colorTimer, colorBossHp, colorRatio);

            }
        }
        else
        {
            //색 변경
            if (sliderFill.color != colorTimer)
            {
                if (colorRatio != 0.0f)
                {
                    colorRatio -= Time.deltaTime;
                    if (colorRatio < 0.0f)
                    {
                        colorRatio = 0.0f;
                    }
                }
                sliderFill.color = Color.Lerp(colorTimer, colorBossHp, colorRatio);
            }
            gameTime += Time.deltaTime;

            if (gameTime > bossSpawnTime)
            {
                gameTime = bossSpawnTime;
                spawnBoss = true;
                isSpawn = false;

                //모든 적기들을 삭제
                clearAllEnemy();
                //보스가 출현
                createBoss();
            }
            modifySlider();
        }
    }

    public void BossKill()
    {
        spawnBoss = false;
        isSpawn = true;
        initTimer();
    }

    public void HitBoss(int _curHp, int _maxHp)
    {
        textTimer.text = $"{_curHp.ToString("D4")} / {_maxHp.ToString("D4")}";
        slider.maxValue = _maxHp;
        slider.value = _curHp;
    }
    private void initTimer()
    {
        gameTime = 0;
        bossSpawnTime += 60;

        if (spawnTime != 0.1f)
        {
            spawnTime -= 0.1f;
        }
        initSlider();
    }
    private void checkKillCount()
    {
        if (spawnBoss == false && killCount == killCountBossSpawn)
        {
            spawnBoss = true;

            clearAllEnemy();
            createBoss();
        }
        modifySlider();
    }

    private void initSlider()
    {
        slider.maxValue = bossSpawnTime;
        //slider.maxValue = killCountBossSpawn;
        slider.value = 0;
        textTimer.text = $"{((int)slider.value).ToString("D2")} / {((int)slider.maxValue).ToString("D2")}";
        //textTimer.text = $"{0.ToString("D2")} / {((int)slider.maxValue).ToString("D2")}";
    }

    private void modifySlider()
    {
        slider.value = gameTime;
        textTimer.text = $"{((int)gameTime).ToString("D2")} / {((int)bossSpawnTime).ToString("D2")}";

        //slider.value = killCount;
        //textTimer.text = $"{killCount.ToString("D2")} / {killCountBossSpawn.ToString("D2")}";
    }

    public void AddKillCount()
    {
        return;//킬카운트로 사용할때는 리턴 해제

        killCount++;
        if (killCount > killCountBossSpawn)
        {
            killCount = killCountBossSpawn;
        }

        checkKillCount();
    }


    private void clearAllEnemy()
    {
        int count = listSpawnEnemy.Count;
        for (int iNum = count - 1; iNum > -1; --iNum)
        {
            Destroy(listSpawnEnemy[iNum]);
        }
        listSpawnEnemy.Clear();
    }

    private void createBoss()
    {
        GameObject go = listEnemy[listEnemy.Count - 1];//[3]넣어도 됨
        Instantiate(go, trsSpawnPoint.position, Quaternion.identity);
    }

    private void createEnemy()//적기를 생산합니다.
    {
        float rand = Random.Range(0.0f, 100.0f);
        GameObject objEnemy = listEnemy[0];
        if (rand < 50.0f)
        {
            objEnemy = listEnemy[0];
        }
        else if (rand < 75.0f)
        {
            objEnemy = listEnemy[1];
        }
        else
        {
            objEnemy = listEnemy[2];
        }

        Vector3 newPos = trsSpawnPoint.position;
        newPos.x = Random.Range(vecSpawnLimit.x, vecSpawnLimit.y);
        GameObject go = Instantiate(objEnemy, newPos, Quaternion.identity);

        //listSpawnEnemy.Add(go);//직접 리스트에 등록하는 방법

        float rate = Random.Range(0.0f, 100.0f);
        if (rate <= itemDropRate)
        {
            Enemy goSc = go.GetComponent<Enemy>();
            goSc.SetHaveItem();


        }
    }

    public void DropItem(Vector3 _pos)
    {
        int raniNum = Random.Range(0, listItem.Count);//0~1
        GameObject obj = listItem[raniNum];
        Instantiate(obj, _pos, Quaternion.identity);
    }

    public Transform GetPlayerTransform()
    {
        if (objPlayer == null)
        {
            return null;
        }
        else
        {
            return objPlayer.transform;
        }
    }

    public GameObject GetPlayerObject()
    {
        return objPlayer;
    }

    public void AddSpawnEnemyList(GameObject _value)
    {
        //if (listSpawnEnemy.Exists((x) => x == _value) == false)
        //{ 
        //    listSpawnEnemy.Add(_value);
        //}
        listSpawnEnemy.Add(_value);
    }

    public void RemoveSpawnEnemyList(GameObject _value)
    {
        listSpawnEnemy.Remove(_value);
    }

    public void DestroyEnemy(Enemy.enumEnemy _value)
    {
        switch (_value)
        {
            case Enemy.enumEnemy.EnemyA:
                score += 1;
                break;
            case Enemy.enumEnemy.EnemyB:
                score += 2;
                break;
            case Enemy.enumEnemy.EnemyC:
                score += 3;
                break;
            case Enemy.enumEnemy.Boss:
                score += 4;
                break;
        }
        textScore.text = score.ToString("D8");
    }

    public void GameOver()//플레이어가 사망했을때 동작
    {
        rank = GetRank();//랭크에 들지 않았다면 -1

        //랭크에 들었다면 이름과 점수를 기록할 수 있는 메뉴가
        if (rank != -1)
        {
            textGameover.text = $"{rank}등 달성!";
            iFGameover.gameObject.SetActive(true);
        }
        //랭크에 들지 않았다면 확인메뉴만
        else//-1일때
        {
            textGameover.text = "순위권 내에 들지 못했습니다.";
            iFGameover.gameObject.SetActive(false);
        }
        objGameoverMenu.gameObject.SetActive(true);
        iFGameover.Select();
    }
}
