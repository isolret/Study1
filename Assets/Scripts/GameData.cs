using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(gameObject);//새로운 씬을 구분해서 데이터를 저장
    }
}
