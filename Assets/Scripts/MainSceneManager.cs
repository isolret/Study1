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
    List<GameManager.cRank> listRank = new List<GameManager.cRank>();//0~9 ������ ��ŷ

    void Awake()
    {
        initBtns();
        initRank();
        onRank(false);

        Tool.IsEnterFirstScene = true;//���ξ����� �����ߴ��� Ȯ��
    }   

    /// <summary>
    /// ��ũ �����͸� �Է��մϴ�.
    /// </summary>
    private void initRank()
    {
        string rankValue = PlayerPrefs.GetString(keyRankData, string.Empty);//string.Empty == "";
        int count = 0;
        if (rankValue == string.Empty) // ��ũ����� ("";)����ִٸ�
        {
            count = 10;
            for (int iNum = 0; iNum < count; iNum++) // 10���� ����ִ� ��ũ�� �����Ҷ����� �ݺ�
            {
                listRank.Add(new GameManager.cRank());
            }

            rankValue = JsonConvert.SerializeObject(listRank);
            PlayerPrefs.SetString(keyRankData, rankValue);
        }
        else//string.Empty�� �ƴϾ��ٸ�
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
    /// ��ư���� �����մϴ�.
    /// </summary>
    private void initBtns()
    {
        ListBtns[0].onClick.AddListener(onStart);//���۹�ư
        ListBtns[1].onClick.AddListener(() => onRank(true));//��ŷ��ư
        ListBtns[2].onClick.AddListener(onExit);//�����ư
        ListBtns[3].onClick.AddListener(() => onRank(false));//��ŷ �ݱ��ư
    }

    private void onStart()
    {
        SceneManager.LoadSceneAsync((int)SceneNums.PlayScene);//�÷��̾����� �̵�
    }

    private void onRank(bool _value)//true�� ������ ��ũ�並 ����
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