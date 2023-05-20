using System;
using UnityEngine;

[Serializable]
public class SerializeDiceData
{
    public bool isExist;
    public int index;
    public int code;
    public int level;
    public Vector2 originPos;
    public GameObject myObj;

    public SerializeDiceData(bool isExist, int index, int code, int level, GameObject myObj)
    {
        this.isExist = isExist;
        this.index = index;
        this.code = code;
        this.level = level;
        this.myObj = myObj;
    }
}

public class Utils
{
    public const int MAX_DICE_LEVEL = 6;
    public const int DICE_LAYERMASK = 6;

    public const int GAMEWINIMG = 1;
    public const int GAMELOSEIMG = 2;
    public const int SURRENDERIMG = 3;
    public const int INFOIMG = 4;

    public const int CRACKNUM = 13;
    public const int FIRENUM = 1;
    public const int ICENUM = 5;
    public const int POISONNUM = 4;
    public const int ELECTRICNUM = 2;

    public static readonly Quaternion QI = Quaternion.identity;

    public static Vector2 TouchPos
    {
        get
        {
            Touch touch = Input.GetTouch(0);
            return Camera.main.ScreenToWorldPoint(touch.position);
        }
    }

    public static GameObject[] GetRayCastAllObjs(int layerMask)
    {
        RaycastHit2D[] hitObjs = Physics2D.RaycastAll(TouchPos, Vector3.forward, float.MaxValue, 1 << layerMask);
        GameObject[] objs = Array.ConvertAll(hitObjs, x => x.collider.gameObject);

        return objs;
    }

    public static Vector2[] GetEyesPositions(int level) =>
        level switch
        {
            1 => new Vector2[] { Vector2.zero },
            2 => new Vector2[] { new Vector2(-0.11f, -0.11f), new Vector2(0.11f, 0.11f) },
            3 => new Vector2[] { new Vector2(-0.11f, -0.11f), new Vector2(0.11f, 0.11f), Vector2.zero },
            4 => new Vector2[] { new Vector2(-0.11f, -0.11f), new Vector2(0.11f, 0.11f), new Vector2(-0.11f, 0.11f), new Vector2(0.11f, -0.11f) },
            5 => new Vector2[] { new Vector2(-0.11f, -0.11f), new Vector2(0.11f, 0.11f), new Vector2(-0.11f, 0.11f), new Vector2(0.11f, -0.11f), Vector2.zero },
            6 => new Vector2[] { new Vector2(-0.11f, -0.11f), new Vector2(0.11f, 0.11f), new Vector2(-0.11f, 0.11f), new Vector2(0.11f, -0.11f), new Vector2(-0.11f, 0f), new Vector2(0.11f, 0f) },
            _ => new Vector2[] { Vector2.zero }
        };

    public static readonly Vector2[] Ways_enemyOfPlayer = { new Vector2(-2.2f, -3f), new Vector2(-2.2f, -0.24f), new Vector2(2.224f, -0.24f), new Vector2(2.224f, -3f) };

    public static readonly Vector2[] Ways_enemyOfAI = { new Vector2(2.224f, 4.023f), new Vector2(2.224f, 1.222f), new Vector2(-2.204f, 1.222f), new Vector2(-2.207f, 4.023f) };

    public static readonly WaitForSeconds delayWaveStart = new WaitForSeconds(3f);

    public static readonly WaitForSeconds delayWave = new WaitForSeconds(1f);

    public static readonly WaitForSeconds delayDiceBulletSpawn = new WaitForSeconds(1f);

    public static readonly WaitForSeconds delayAISpawn = new WaitForSeconds(2f);

    public static readonly WaitForSeconds delayBossSpawn = new WaitForSeconds(5f);

    public static int TotalAttackDamage(int basicAttackDamage, int level)
    {
        return basicAttackDamage + level * 3;
    }
}