using DG.Tweening;
using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private RawImage gameWinImg;
    [SerializeField] private RawImage gameLoseImg;
    [SerializeField] private RawImage surrenderImg;
    [SerializeField] private RawImage infoImg;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text explainText;
    [SerializeField] private RawImage startSelectBackGroundImg;
    [SerializeField] private RectTransform[] dicePos;
    [SerializeField] private RectTransform[] playerDeckPos;
    [SerializeField] private RectTransform[] aiDeckPos;
    [SerializeField] private Button[] diceButtons;
    [SerializeField] private RawImage[] diceImgs;
    [SerializeField] private Transform[] decks;
    [SerializeField] private RawImage[] playerDecksImgs;
    [SerializeField] private RawImage[] aiDecksImgs;
    [SerializeField] private RawImage infoImgToDisplay;

    [SerializeField] private List<int> playerDiceCodes = new List<int>();
    public List<int> PlayerDiceCodes { get { return playerDiceCodes; } private set { } }

    [SerializeField] private List<int> aiDiceCodes = new List<int>();
    public List<int> AiDiceCodes { get { return aiDiceCodes; } private set { } }

    private Texture2D[] loadedTextures;
    private List<Texture2D> availableTextures;

    private int selectCount;

    private void Start()
    {
        // Resources.LoadAll �޼��带 ����Ͽ� Resources/Dice ���� ���� ��� Texture2D ��ü�� �����ɴϴ�.
        loadedTextures = Resources.LoadAll<Texture2D>("Dice");

        // �ߺ��� ������ �ؽ�ó ����Ʈ�� �ʱ�ȭ�մϴ�.
        availableTextures = new List<Texture2D>(loadedTextures);

        SetTexture();
        ControlButtonEnable(true, false);
        StartCoroutine(SelectDiceForDeck());
    }

    public void FloatingUI(int floatingUiNum, bool active)
    {
        switch (floatingUiNum)
        {
            case Utils.GAMEWINIMG:
                StartCoroutine(CO_Floating(gameWinImg, active));
                break;

            case Utils.GAMELOSEIMG:
                StartCoroutine(CO_Floating(gameLoseImg, active));
                break;

            case Utils.SURRENDERIMG:
                StartCoroutine(CO_Floating(surrenderImg, active));
                break;

            case Utils.INFOIMG:
                StartCoroutine(CO_Floating(infoImg, active));
                break;
        }
    }

    private IEnumerator CO_Floating(RawImage floatingObj, bool active)
    {
        float time = 0f;
        if (active)
        {
            floatingObj.gameObject.SetActive(true);
            floatingObj.gameObject.transform.localScale = Vector3.zero;
            while ((floatingObj.gameObject.transform.localScale - Vector3.one).sqrMagnitude >= 0.01f)
            {
                floatingObj.gameObject.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, time);
                time += Time.deltaTime * 2f;
                yield return null;
            }
            floatingObj.gameObject.transform.localScale = Vector3.one;
        }
        else
        {
            floatingObj.gameObject.transform.localScale = Vector3.one;
            while ((floatingObj.gameObject.transform.localScale - Vector3.zero).sqrMagnitude >= 0.01f)
            {
                floatingObj.gameObject.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time);
                time += Time.deltaTime * 2f;
                yield return null;
            }
            floatingObj.gameObject.transform.localScale = Vector3.zero;
            floatingObj.gameObject.SetActive(false);
        }
    }

    public void SetWaveNum(int waveNum) => waveText.text = $"WAVE {GameManager.Inst.CurWave}";

    public void OnClick_SurrenderBtn(bool active) => FloatingUI(Utils.SURRENDERIMG, active);

    public void OnClick_CancleBtn() => FloatingUI(Utils.SURRENDERIMG, false);

    public void OnClick_CheckBtn() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);


    private void SetTexture()
    {
        // �����ϰ� ���õ� �ؽ�ó �߿��� �ϳ��� ����Ͽ� �����մϴ�.
        for (int i = 0; i < diceImgs.Length; ++i)
        {
            Texture2D randomTexture = availableTextures[UnityEngine.Random.Range(0, availableTextures.Count)];
            diceImgs[i].texture = randomTexture;
            availableTextures.Remove(randomTexture);
        }
    }

    private IEnumerator SelectDiceForDeck()
    {
        selectCount = 0;

        yield return new WaitUntil(() => selectCount >= 5);

        explainText.text = "5�ʵڿ� ������ ���۵˴ϴ�!";
        yield return new WaitForSeconds(5f);

        GameManager.Inst.isGameStart = true;
        decks[0].transform.SetParent(GameObject.Find("PlayerUI").transform);
        decks[0].transform.localPosition = new Vector3(0, -156 * 1.5f, 0);
        decks[0].transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

        decks[1].transform.SetParent(GameObject.Find("EnemyUI").transform);
        decks[1].transform.localPosition = new Vector3(190, 0, 0);
        decks[1].transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

        startSelectBackGroundImg.gameObject.SetActive(false);
    }

    public void OnClickSelectDice(int what)
    {
        if (selectCount >= 5)
        {
            ControlButtonEnable(false, false);
            return;
        }

        ControlButtonEnable(false, false);

        if (what == 0)
        {
            diceImgs[0].transform.DOMove(playerDeckPos[selectCount].position, 1f).OnComplete(() => CreateDice(what));
            diceImgs[1].transform.DOMove(aiDeckPos[selectCount].position, 1f).OnComplete(() => ControlButtonEnable(true, false));
        }
        else
        {
            diceImgs[0].transform.DOMove(aiDeckPos[selectCount].position, 1f).OnComplete(() => CreateDice(what));
            diceImgs[1].transform.DOMove(playerDeckPos[selectCount].position, 1f).OnComplete(() => ControlButtonEnable(true, false));
        }

        selectCount++;
    }

    private void CreateDice(int what)
    {
        // ����(0) ���̽��� �����ϸ� �÷��̾
        if (what == 0)
        {
            // ���� �̹����� �ؽ��ĸ� ������ ���̽��� ��ü���ְ� ��ư������Ʈ �߰��� ���ӸŴ������� ���� ������ ���� int�� ����Ʈ�� �ش� ���̽� �ڵ��ȣ(int)�� �߰��Ѵ�
            // ��ư���� �÷��̾��� ��쿡�� �ش� �ֻ����� attackPower�� ������Ű�� ������
            // AI�� ��쿡�� �ش� �ֻ����� ������ ���� ������ ����Ѵ�
            playerDecksImgs[selectCount - 1].texture = diceImgs[0].GetComponent<RawImage>().texture;
            playerDecksImgs[selectCount - 1].gameObject.AddComponent<Button>().onClick.AddListener(() => PlayerDiceUpgrade(GetNumFromTextureName(playerDecksImgs[selectCount - 1].texture.name)));
            playerDiceCodes.Add(GetNumFromTextureName(diceImgs[0].texture.name));
            
            aiDecksImgs[selectCount - 1].texture = diceImgs[1].GetComponent<RawImage>().texture;
            aiDecksImgs[selectCount - 1].gameObject.AddComponent<Button>().onClick.AddListener(() => OnClick_InfoBtn(aiDecksImgs[selectCount - 1].texture)); //diceImgs[0].GetComponent<RawImage>().texture)) ;
            aiDiceCodes.Add(GetNumFromTextureName(diceImgs[1].texture.name));
        }
        // ������(0) ���̽��� �����ϸ� AI��
        else
        {
            playerDecksImgs[selectCount - 1].texture = diceImgs[1].GetComponent<RawImage>().texture;
            playerDecksImgs[selectCount - 1].gameObject.AddComponent<Button>().onClick.AddListener(() => PlayerDiceUpgrade(GetNumFromTextureName(playerDecksImgs[selectCount - 1].texture.name)));
            playerDiceCodes.Add(GetNumFromTextureName(diceImgs[1].texture.name));

            aiDecksImgs[selectCount - 1].texture = diceImgs[0].GetComponent<RawImage>().texture;
            aiDecksImgs[selectCount - 1].gameObject.AddComponent<Button>().onClick.AddListener(() => OnClick_InfoBtn(aiDecksImgs[selectCount - 1].texture)); //diceImgs[1].GetComponent<RawImage>().texture)) ;
            aiDiceCodes.Add(GetNumFromTextureName(diceImgs[0].texture.name));
        }
        ControlButtonEnable(true, true);

        if (selectCount == 5)
        {
            diceImgs[0].gameObject.SetActive(false);
            diceImgs[1].gameObject.SetActive(false);
        }
    }

    private void ControlButtonEnable(bool enable, bool setTexture)
    {
        for (int i = 0; i < diceButtons.Length; ++i)
        {
            diceButtons[i].enabled = enable;
        }

        for (int i = 0; i < diceImgs.Length; ++i)
        {
            diceImgs[i].transform.position = dicePos[i].position;
        }

        if (setTexture == false) return;
        SetTexture();
    }

    // �ؽ����� �̸����� ���ڸ� ���� �Ͽ� int�� ��ȯ�Ͽ� return. ex) fire_01 -> 1
    private int GetNumFromTextureName(string name)
    {
        // ���ڸ� �����Ͽ� �迭�� ��ȯ
        string[] numbersArray = Regex.Matches(name, @"\d+")
                                     .Cast<Match>()
                                     .Select(m => m.Value)
                                     .ToArray();

        // �迭�� ��� ��Ҹ� �ϳ��� string���� ����
        string numbersString = String.Join("", numbersArray);

        // string�� int�� ��ȯ
        int result = int.Parse(numbersString);
        return result;
    }

    public void OnClick_InfoBtn(Texture texture)
    {
        infoImgToDisplay.texture = texture;
        FloatingUI(Utils.INFOIMG, true);
    }

    public void PlayerDiceUpgrade(int code)
    {
        DiceData playerData = GameManager.Inst.playerDiceDatas.Find(x => x.code == code);
        playerData.basicAttackDamage += playerData.attackIncrement;
    }
}

