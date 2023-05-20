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
        // Resources.LoadAll 메서드를 사용하여 Resources/Dice 폴더 내의 모든 Texture2D 객체를 가져옵니다.
        loadedTextures = Resources.LoadAll<Texture2D>("Dice");

        // 중복을 제외한 텍스처 리스트를 초기화합니다.
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
        // 랜덤하게 선택된 텍스처 중에서 하나를 사용하여 적용합니다.
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

        explainText.text = "5초뒤에 게임이 시작됩니다!";
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
        // 왼쪽(0) 다이스를 선택하면 플레이어덱
        if (what == 0)
        {
            // 덱에 이미지의 텍스쳐를 선택한 다이스로 교체해주고 버튼컴포넌트 추가후 게임매니저에서 받을 정보를 위한 int형 리스트에 해당 다이스 코드번호(int)를 추가한다
            // 버튼에는 플레이어일 경우에는 해당 주사위의 attackPower를 증가시키는 동작을
            // AI일 경우에는 해당 주사위의 정보를 띄우는 동작을 등록한다
            playerDecksImgs[selectCount - 1].texture = diceImgs[0].GetComponent<RawImage>().texture;
            playerDecksImgs[selectCount - 1].gameObject.AddComponent<Button>().onClick.AddListener(() => PlayerDiceUpgrade(GetNumFromTextureName(playerDecksImgs[selectCount - 1].texture.name)));
            playerDiceCodes.Add(GetNumFromTextureName(diceImgs[0].texture.name));
            
            aiDecksImgs[selectCount - 1].texture = diceImgs[1].GetComponent<RawImage>().texture;
            aiDecksImgs[selectCount - 1].gameObject.AddComponent<Button>().onClick.AddListener(() => OnClick_InfoBtn(aiDecksImgs[selectCount - 1].texture)); //diceImgs[0].GetComponent<RawImage>().texture)) ;
            aiDiceCodes.Add(GetNumFromTextureName(diceImgs[1].texture.name));
        }
        // 오른쪽(0) 다이스를 선택하면 AI덱
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

    // 텍스쳐의 이름에서 숫자만 추출 하여 int로 변환하여 return. ex) fire_01 -> 1
    private int GetNumFromTextureName(string name)
    {
        // 숫자만 추출하여 배열로 반환
        string[] numbersArray = Regex.Matches(name, @"\d+")
                                     .Cast<Match>()
                                     .Select(m => m.Value)
                                     .ToArray();

        // 배열의 모든 요소를 하나의 string으로 결합
        string numbersString = String.Join("", numbersArray);

        // string을 int로 변환
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

