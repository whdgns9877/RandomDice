using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using DG.Tweening;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Inst { get; private set; }
    private void Awake() => Inst = this;

    public DiceScriptableObject diceSO;
    public List<Enemy> enemiesOfPlayer;
    public List<Enemy> enemiesOfAI;
    public List<DamageTextMove> damageTexts;

    private List<int> playerDiceCodes;
    private List<int> aiDiceCodes;

    [SerializeField] private UIManager uiManager;
    [SerializeField] private int[] playerCanSpawnDiceCodes = new int[5];
    [SerializeField] private int[] aiCanSpawnDiceCodes = new int[5];
    [SerializeField] private SerializeDiceData[] playerSerializeDiceDatas;
    [SerializeField] private SerializeDiceData[] aiSerializeDiceDatas;
    [SerializeField] GameObject[] heartImgsOfPlayer;
    [SerializeField] GameObject[] heartImgsOfAI;
    [SerializeField] TMP_Text totalspText;
    [SerializeField] TMP_Text spawnspText;
    [SerializeField] public List<DiceData> playerDiceDatas = new List<DiceData>();
    [SerializeField] private List<DiceData> aiDiceDatas = new List<DiceData>();

    public DiceData GetPlayerRandomDiceData() => playerDiceDatas[UnityEngine.Random.Range(1, playerDiceDatas.Count)];
    public DiceData GetAiRandomDiceData() => aiDiceDatas[UnityEngine.Random.Range(1, aiDiceDatas.Count)];

    private int playerTotalsp;
    public int PlayerTotalsp
    {
        get => playerTotalsp;

        set
        {
            totalspText.text = value.ToString();
            playerTotalsp = value;
        }
    }

    private int playerSpawnsp;
    public int PlayerSpawnsp
    {
        get => playerSpawnsp;

        set
        {
            spawnspText.text = value.ToString();
            playerSpawnsp = value;
        }
    }

    public int aiTotalsp;
    private int aiSpawnsp;

    private bool isDead;
    public bool isGameStart;

    private int curWave;
    public int CurWave { get => curWave; private set { } }

    private bool isBossDead;

    private void SetDiceDatas()
    {
        for (int i = 0; i < diceSO.diceDatas.Length; i++)
        {
            for (int j = 0; j < playerCanSpawnDiceCodes.Length; j++)
            {
                if (diceSO.diceDatas[i].code == playerCanSpawnDiceCodes[j])
                {
                    playerDiceDatas.Add(diceSO.diceDatas[i]);
                }
            }

            for (int j = 0; j < aiCanSpawnDiceCodes.Length; j++)
            {
                if (diceSO.diceDatas[i].code == aiCanSpawnDiceCodes[j])
                {
                    aiDiceDatas.Add(diceSO.diceDatas[i]);
                }
            }
        }
    }

    private IEnumerator Start()
    {
        Time.timeScale = 1f;
        isGameStart = false;
        yield return new WaitUntil(() => isGameStart == true);
        SetSpawnCodes();
        SetDiceDatas();
        GameStart();
    }

    // Update is called once per frame
    private void Update()
    {
        ArrangeEnemies();
        ArrangeDamageText();
    }

    private void SetSpawnCodes()
    {
        playerCanSpawnDiceCodes = uiManager.PlayerDiceCodes.ToArray();
        aiCanSpawnDiceCodes = uiManager.AiDiceCodes.ToArray();
    }

    public void SetPlayerSerializeDiceData(SerializeDiceData serializeDiceData)
    {
        playerSerializeDiceDatas[serializeDiceData.index] = serializeDiceData;
    }

    public void SetAISerializeDiceData(SerializeDiceData serializeDiceData)
    {
        aiSerializeDiceDatas[serializeDiceData.index] = serializeDiceData;
    }

    private bool PlayerTryRandomSpawnDice(int level = 1)
    {
        SerializeDiceData[] emptySerializeDiceDatas = Array.FindAll(playerSerializeDiceDatas, x => x.isExist == false);

        if (emptySerializeDiceDatas.Length <= 0)
            return false;

        int randIdx = emptySerializeDiceDatas[UnityEngine.Random.Range(0, emptySerializeDiceDatas.Length)].index;
        GameObject diceObj = ObjectPool.SpawnFromPool("Dice", diceSO.GetPlayerOriginDicePosition(randIdx), Utils.QI);

        SerializeDiceData serializeDiceData = new SerializeDiceData(true, randIdx, GetPlayerRandomDiceData().code, level, diceObj);
        diceObj.GetComponent<Dice>().SetupDice(serializeDiceData, true);

        SetPlayerSerializeDiceData(serializeDiceData);

        return true;
    }

    private IEnumerator AITryRandomSpawnDice(int level = 1)
    {
        while (!isDead && aiTotalsp >= aiSpawnsp)
        {
            SerializeDiceData[] emptySerializeDiceDatas = Array.FindAll(aiSerializeDiceDatas, x => x.isExist == false);

            if (emptySerializeDiceDatas.Length <= 0)
                yield break;

            int randIdx = emptySerializeDiceDatas[UnityEngine.Random.Range(0, emptySerializeDiceDatas.Length)].index;
            GameObject diceObj = ObjectPool.SpawnFromPool("Dice", diceSO.GetAIOriginDicePosition(randIdx), Utils.QI);

            SerializeDiceData serializeDiceData = new SerializeDiceData(true, randIdx, GetAiRandomDiceData().code, level, diceObj);
            diceObj.GetComponent<Dice>().SetupDice(serializeDiceData, false);

            SetAISerializeDiceData(serializeDiceData);
            aiTotalsp -= aiSpawnsp;
            aiSpawnsp += 10;
            StartCoroutine(CheckAIDiceMerge());
            yield return Utils.delayAISpawn;
        }
    }

    private IEnumerator CheckAIDiceMerge()
    {
        List<SerializeDiceData> laidDiceSerializeDatas = new List<SerializeDiceData>();
        for (int i = 0; i < aiSerializeDiceDatas.Length; ++i)
        {
            if (aiSerializeDiceDatas[i].isExist == true)
            {
                laidDiceSerializeDatas.Add(aiSerializeDiceDatas[i]);
            }
        }

        if (laidDiceSerializeDatas.Count <= 1) yield break;

        for (int i = 0; i < laidDiceSerializeDatas.Count; ++i)
        {
            for (int j = i + 1; j < laidDiceSerializeDatas.Count; ++j)
            {
                if ((laidDiceSerializeDatas[i].code == laidDiceSerializeDatas[j].code)
                    && (laidDiceSerializeDatas[i].level == laidDiceSerializeDatas[j].level)
                    && laidDiceSerializeDatas[i].level <= Utils.MAX_DICE_LEVEL)
                {
                    bool moveDone = false;
                    Dice targetDice = laidDiceSerializeDatas[j].myObj.GetComponent<Dice>();
                    targetDice.transform.DOMove(laidDiceSerializeDatas[i].myObj.transform.position, 1f).OnComplete(() => moveDone = true);

                    yield return new WaitUntil(() => moveDone == true);
                    SerializeDiceData targetSerializeDiceData = new SerializeDiceData(true, targetDice.serializeDiceData.index, GetAiRandomDiceData().code, laidDiceSerializeDatas[i].level + 1,
                        targetDice.gameObject);
                    targetDice.SetupDice(targetSerializeDiceData, false);

                    SerializeDiceData curSerializeData = new SerializeDiceData(false, laidDiceSerializeDatas[i].index, 0, 0, laidDiceSerializeDatas[i].myObj);
                    laidDiceSerializeDatas[i].myObj.GetComponent<Dice>().SetupDice(curSerializeData, false);
                }
            }
        }
    }

    public void OnClick_SpawnBtn()
    {
        if (PlayerTotalsp >= PlayerSpawnsp)
        {
            if (PlayerTryRandomSpawnDice())
            {
                PlayerTotalsp -= PlayerSpawnsp;
                PlayerSpawnsp += 10;
            }
        }
    }

    private IEnumerator CO_StartWave(int waveLevel)
    {
        curWave = waveLevel;
        while (isDead == false)
        {
            uiManager.SetWaveNum(curWave);

            yield return Utils.delayWaveStart;

            for (int i = 0; i < 10; ++i)
            {
                SpawnEnemy();
                yield return Utils.delayWave;
            }
            yield return Utils.delayBossSpawn;
            // bool값이 있다면 보스 스폰
            SpawnEnemy(true);
            // 보스가 죽을때까지 웨이브 대기
            yield return new WaitUntil(() => isBossDead == true);
            curWave++;
        }
    }

    private void SpawnEnemy()
    {
        GameObject enemyObjOfPlayer = new GameObject();
        GameObject enemyObjOfAI = new GameObject();

        switch (GenerateRandomNumber())
        {
            case 1:
                enemyObjOfPlayer = ObjectPool.SpawnFromPool("Enemy_Normal", Utils.Ways_enemyOfPlayer[0], Utils.QI);
                enemyObjOfAI = ObjectPool.SpawnFromPool("Enemy_Normal", Utils.Ways_enemyOfAI[0], Utils.QI);
                break;

            case 2:
                enemyObjOfPlayer = ObjectPool.SpawnFromPool("Enemy_Small", Utils.Ways_enemyOfPlayer[0], Utils.QI);
                enemyObjOfAI = ObjectPool.SpawnFromPool("Enemy_Small", Utils.Ways_enemyOfAI[0], Utils.QI);
                break;

            case 3:
                enemyObjOfPlayer = ObjectPool.SpawnFromPool("Enemy_Big", Utils.Ways_enemyOfPlayer[0], Utils.QI);
                enemyObjOfAI = ObjectPool.SpawnFromPool("Enemy_Big", Utils.Ways_enemyOfAI[0], Utils.QI);
                break;
        }
        enemyObjOfPlayer.GetComponent<Enemy>().imEnemyOfPlayer = true;
        enemiesOfPlayer.Add(enemyObjOfPlayer.GetComponent<Enemy>());

        enemyObjOfAI.GetComponent<Enemy>().imEnemyOfPlayer = false;
        enemiesOfAI.Add(enemyObjOfAI.GetComponent<Enemy>());
    }

    // 보스를 스폰하기위해 기존 SpawnEnemy함수에서 오버로딩으로 만들어서 사용
    private bool SpawnEnemy(bool isBossSpawn)
    {
        GameObject enemyObjOfPlayer = new GameObject();
        GameObject enemyObjOfAI = new GameObject();

        switch (curWave)
        {
            case 1:
                enemyObjOfPlayer = ObjectPool.SpawnFromPool("Enemy_Boss_Snake", Utils.Ways_enemyOfPlayer[0], Utils.QI);
                enemyObjOfAI = ObjectPool.SpawnFromPool("Enemy_Boss_Snake", Utils.Ways_enemyOfAI[0], Utils.QI);
                break;

            case 2:
                enemyObjOfPlayer = ObjectPool.SpawnFromPool("Enemy_Boss_Snake", Utils.Ways_enemyOfPlayer[0], Utils.QI);
                enemyObjOfAI = ObjectPool.SpawnFromPool("Enemy_Boss_Snake", Utils.Ways_enemyOfAI[0], Utils.QI);
                break;

            case 3:
                enemyObjOfPlayer = ObjectPool.SpawnFromPool("Enemy_Boss_Snake", Utils.Ways_enemyOfPlayer[0], Utils.QI);
                enemyObjOfAI = ObjectPool.SpawnFromPool("Enemy_Boss_Snake", Utils.Ways_enemyOfAI[0], Utils.QI);
                break;
        }
        // isDead를 false로 만든후 isDead가 true가 되는 조건은 BossController에서 구현
        enemyObjOfPlayer.GetComponent<BossController>().imEnemyOfPlayer = true;
        enemyObjOfPlayer.GetComponent<BossController>().isDead = false;
        enemiesOfPlayer.Add(enemyObjOfPlayer.GetComponent<BossController>());

        enemyObjOfAI.GetComponent<BossController>().imEnemyOfPlayer = false;
        enemyObjOfAI.GetComponent<BossController>().isDead = false;
        enemiesOfAI.Add(enemyObjOfAI.GetComponent<BossController>());

        return isBossDead = false;
    }

    // 확률에 따라 일반, 빠른, 거대한 적 생성을위한 숫자 반환
    private int GenerateRandomNumber()
    {
        int randNum = UnityEngine.Random.Range(1, 101); // 1부터 100까지의 랜덤 정수 생성

        if (randNum <= 50)
        {
            return 1; // 50% 확률로 1 반환
        }
        else if (randNum <= 80)
        {
            return 2; // 30% 확률로 2 반환
        }
        else
        {
            return 3; // 20% 확률로 3 반환
        }
    }

    private void ArrangeEnemies()
    {
        enemiesOfPlayer.Sort((x, y) => x.distance.CompareTo(y.distance));

        for (int i = 0; i < enemiesOfPlayer.Count; ++i)
        {
            enemiesOfPlayer[i].GetComponent<SortingLayerOrder>().SetOrder(i);
        }

        enemiesOfAI.Sort((x, y) => x.distance.CompareTo(y.distance));

        for (int i = 0; i < enemiesOfAI.Count; ++i)
        {
            enemiesOfAI[i].GetComponent<SortingLayerOrder>().SetOrder(i);
        }
    }

    private void ArrangeDamageText()
    {
        for (int i = 0; i < damageTexts.Count; ++i)
        {
            damageTexts[i].GetComponent<SortingLayerOrder>().SetOrder(i);
        }
    }

    public Enemy GetFirstEnemyOfPlayer()
    {
        if (enemiesOfPlayer.Count <= 0)
            return null;

        return enemiesOfPlayer.Last();
    }

    public Enemy GetFirstEnemyOfAI()
    {
        if (enemiesOfAI.Count <= 0)
            return null;

        return enemiesOfAI.Last();
    }

    public void DecreasePlayerHeart()
    {
        if (isDead) return;

        for (int i = 0; i < heartImgsOfPlayer.Length; ++i)
        {
            if (heartImgsOfPlayer[i].activeSelf)
            {
                heartImgsOfPlayer[i].SetActive(false);
                break;
            }
        }

        if (Array.TrueForAll(heartImgsOfPlayer, x => x.activeSelf == false))
        {
            Lose();
            isDead = true;
        }
    }

    public void DecreaseAIHeart()
    {
        if (isDead) return;

        for (int i = 0; i < heartImgsOfAI.Length; ++i)
        {
            if (heartImgsOfAI[i].activeSelf)
            {
                heartImgsOfAI[i].SetActive(false);
                break;
            }
        }

        if (Array.TrueForAll(heartImgsOfAI, x => x.activeSelf == false))
        {
            Win();
            isDead = true;
        }
    }

    public void GameStart()
    {
        curWave = 1;
        PlayerTotalsp = 100;
        PlayerSpawnsp = 10;
        aiTotalsp = 100;
        aiSpawnsp = 10;
        Time.timeScale = 1f;
        StartCoroutine(AITryRandomSpawnDice());
        StartCoroutine(CO_StartWave(1));
    }

    public void Win()
    {
        Invoke(nameof(DelayStop), 2f);
        uiManager.FloatingUI(Utils.GAMEWINIMG, true);
    }

    public void Lose()
    {
        Invoke(nameof(DelayStop), 2f);
        uiManager.FloatingUI(Utils.GAMELOSEIMG, true);
    }

    private void DelayStop() => Time.timeScale = 0;
}
