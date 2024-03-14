using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RankData : MonoBehaviour
{
    [SerializeField] TMP_Text textRank;
    [SerializeField] TMP_Text textName;
    [SerializeField] TMP_Text textScore;

    public void SetData(int _rank, string _name, int _score)
    {
        textRank.text = _rank.ToString();
        textName.text = _name;
        textScore.text = _score.ToString("D8");
    }
}
