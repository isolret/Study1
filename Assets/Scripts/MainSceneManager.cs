using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainSceneManager : MonoBehaviour
{
    [SerializeField] List<Button> ListBtns;

    [SerializeField] GameObject objMainView;
    [SerializeField] GameObject objRankView;

    [SerializeField] GameObject fabRankData;
    [SerializeField] Transform trsContents;

    string keyRankData = "rankData";
    List<GameManager.cRank> listRank = new List<GameManager.cRank>();//0~9 유저의 랭킹

    void Awake()
    {
        initBtns();
        initRank();
        onRank(false);

        Tool.IsEnterFirstScene = true;//메인씬에서 실행했는지 확인
    }   

    /// <summary>
    /// 랭크 데이터를 입력합니다.
    /// </summary>
    private void initRank()
    {
        string rankValue = PlayerPrefs.GetString(keyRankData, string.Empty);//string.Empty == "";
        int count = 0;
        if (rankValue == string.Empty) // 랭크밸류가 ("";)비어있다면
        {
            count = 10;
            for (int iNum = 0; iNum < count; iNum++) // 10개의 비어있는 랭크를 생성할때까지 반복
            {
                listRank.Add(new GameManager.cRank());
            }

            rankValue = JsonConvert.SerializeObject(listRank);
            PlayerPrefs.SetString(keyRankData, rankValue);
        }
        else//string.Empty가 아니었다면
        {
            listRank = JsonConvert.DeserializeObject<List<GameManager.cRank>>(rankValue);
        }

        count = listRank.Count;
        for (int iNum = 0; iNum < count; ++iNum)
        {
            GameManager.cRank rank = listRank[iNum];

            GameObject go = Instantiate(fabRankData, trsContents);
            RankData goSc = go.GetComponent<RankData>();
            goSc.SetData(iNum + 1, rank.name, rank.score);
        }
    }

    /// <summary>
    /// 버튼들을 세팅합니다.
    /// </summary>
    private void initBtns()
    {
        ListBtns[0].onClick.AddListener(onStart);//시작버튼
        ListBtns[1].onClick.AddListener(() => onRank(true));//랭킹버튼
        ListBtns[2].onClick.AddListener(onExit);//종료버튼
        ListBtns[3].onClick.AddListener(() => onRank(false));//랭킹 닫기버튼
    }

    private void onStart()
    {
        SceneManager.LoadSceneAsync((int)SceneNums.PlayScene);//플레이씬으로 이동
    }

    private void onRank(bool _value)//true로 들어오면 랭크뷰를 켜줌
    {
        objMainView.SetActive(!_value);
        objRankView.SetActive(_value);
    }

    private void onExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();      
#endif
    }
}