using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(gameObject);//���ο� ���� �����ؼ� �����͸� ����
    }
}
