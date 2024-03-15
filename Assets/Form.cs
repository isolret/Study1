using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tool
{
    public static string GetGameTag(GameTag _value)
    {
        return _value.ToString();
    }

    public static bool IsEnterFirstScene = false;//시작씬을 어디서 하는지 체크해주는 스크립트
}

public enum GameTag
{
    None,
    Enemy,
    Player,
    Item
}
