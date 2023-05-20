using System;
using UnityEngine;

[Serializable]
public class DiceData
{
    public int code;
    public Sprite sprite;
    public Color color;
    public int basicAttackDamage;
    public int attackSpeed;
    public int attackIncrement;
}

[CreateAssetMenu(fileName = "DiceSO", menuName = "Scriptable Object/DiceSO")]
public class DiceScriptableObject : ScriptableObject
{
    [SerializeField] Vector2[] playerOriginDicePosition;

    [SerializeField] Vector2[] aIOriginDicePosition;

    public DiceData[] diceDatas;

    public DiceData GetDiceData(int code) => Array.Find(diceDatas, x => x.code == code);

    public Vector2 GetPlayerOriginDicePosition(int idx) => playerOriginDicePosition[idx];

    public Vector2 GetAIOriginDicePosition(int idx) => aIOriginDicePosition[idx];
}
