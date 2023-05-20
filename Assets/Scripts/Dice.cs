using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class Dice : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] SpriteRenderer[] eyeSpriteRenderer;
    [SerializeField] SortingLayerOrder sortingLayerOrder;

    [Header("Values")]
    [SerializeField] Transform[] eyes;
    public SerializeDiceData serializeDiceData { get; private set; }

    private int eyesCount;
    private bool isPlayer;
    private bool isAttack = false;

    public void SetupDice(SerializeDiceData serializeDiceData, bool isPlayer)
    {
        this.isPlayer = isPlayer;
        this.serializeDiceData = serializeDiceData;

        if (isPlayer == true)
            GameManager.Inst.SetPlayerSerializeDiceData(serializeDiceData);
        else
            GameManager.Inst.SetAISerializeDiceData(serializeDiceData);

        DiceData diceData = GameManager.Inst.diceSO.GetDiceData(serializeDiceData.code);
        spriteRenderer.sprite = diceData.sprite;
        SetDiceEyePos(serializeDiceData.level);

        for (int i = 0; i < Utils.MAX_DICE_LEVEL; i++)
        {
            eyeSpriteRenderer[i].color = diceData.color;
            //eyes[i].GetComponent<SpriteRenderer>().color = diceData.color;
        }

        if (serializeDiceData.code == 0)
            gameObject.SetActive(false);

        if (gameObject.activeSelf)
            StartCoroutine(CO_Attack(isPlayer));
    }

    public void SetDiceEyePos(int level)
    {
        Vector2[] positions = Utils.GetEyesPositions(level);
        int eyesCount = 0;

        for (int i = 0; i < Utils.MAX_DICE_LEVEL; ++i)
        {
            eyes[i].gameObject.SetActive(i < level);
            eyes[i].localPosition = i < level ? positions[i] : Vector2.zero;
            if (i < level)
                eyesCount++;
        }
        this.eyesCount = eyesCount;
    }

    public void OnMouseDown()
    {
        if (!isPlayer) return;
        sortingLayerOrder.SetMostFrontOrder(true);
    }

    public void OnMouseDrag()
    {
        if (!isPlayer) return;
        transform.position = Utils.TouchPos;
    }

    public void OnMouseUp()
    {
        if (!isPlayer) return;
        MoveTransform(GameManager.Inst.diceSO.GetPlayerOriginDicePosition(serializeDiceData.index), true, 0.2f, () => sortingLayerOrder.SetMostFrontOrder(false));

        GameObject[] rayHitObjs = Utils.GetRayCastAllObjs(Utils.DICE_LAYERMASK);
        GameObject targetDiceObj = Array.Find(rayHitObjs, x => x.gameObject != gameObject);

        if (targetDiceObj != null)
        {
            Dice targetDice = targetDiceObj.GetComponent<Dice>();
            int nextLevel = serializeDiceData.level + 1;

            if (serializeDiceData.code == targetDice.serializeDiceData.code
                && serializeDiceData.level == targetDice.serializeDiceData.level
                && nextLevel <= Utils.MAX_DICE_LEVEL)
            {
                SerializeDiceData targetSerializeDiceData = new SerializeDiceData(true, targetDice.serializeDiceData.index,
                    GameManager.Inst.GetPlayerRandomDiceData().code, nextLevel, gameObject);
                targetDice.SetupDice(targetSerializeDiceData, true);

                SerializeDiceData curSerializeData = new SerializeDiceData(false, serializeDiceData.index, 0, 0, gameObject);
                SetupDice(curSerializeData, true);
            }
        }
    }

    private void MoveTransform(Vector2 tartgetPos, bool useDoTween, float duration = 0f, TweenCallback todo = null)
    {
        if (useDoTween)
        {
            transform.DOMove(tartgetPos, duration).OnComplete(todo);
        }
        else
        {
            transform.position = tartgetPos;
        }
    }

    private IEnumerator CO_Attack(bool isPlayer)
    {
        WaitForSeconds delayDiceBulletSpawn = new WaitForSeconds(1f / eyesCount);
        while (true)
        {
            for (int i = 0; i < eyesCount; ++i)
            {
                Enemy targetEnemy;

                if (isPlayer == true)
                    targetEnemy = GameManager.Inst.GetFirstEnemyOfPlayer();
                else
                    targetEnemy = GameManager.Inst.GetFirstEnemyOfAI();

                if (targetEnemy != null)
                {
                    GameObject diceBulletObj = ObjectPool.SpawnFromPool("DiceBullet", eyes[i].position, Utils.QI);
                    if (isAttack == false) StartCoroutine(AttackAnim(eyeSpriteRenderer[i]));
                    diceBulletObj.GetComponent<DiceBullet>().SetUpDiceBullet(serializeDiceData, targetEnemy, isPlayer);
                    yield return Utils.delayDiceBulletSpawn;
                }
            }
            yield return null;
        }
    }

    private IEnumerator AttackAnim(SpriteRenderer spriteRenderer)
    {
        isAttack = true;
        float elapsedTime = 0f;
        float duration = 0.1f;
        float startScale = spriteRenderer.transform.localScale.x;
        float targetScale = startScale * 1.2f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            spriteRenderer.transform.localScale = Vector3.Lerp(new Vector3(startScale, startScale, startScale), new Vector3(targetScale, targetScale, targetScale), t);
            yield return null;
        }

        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            spriteRenderer.transform.localScale = Vector3.Lerp(new Vector3(targetScale, targetScale, targetScale), new Vector3(startScale, startScale, startScale), t);
            yield return null;
        }

        spriteRenderer.transform.localScale = new Vector3(startScale, startScale, startScale);
        isAttack = false;
    }

    private void OnDisable()
    {
        serializeDiceData = null;
        spriteRenderer.sprite = null;
        SetDiceEyePos(0);
        for (int i = 0; i < Utils.MAX_DICE_LEVEL; ++i)
        {
            eyes[i].GetComponent<SpriteRenderer>().color = Color.white;
        }
        ObjectPool.ReturnToPool(gameObject);    // 한 객체에 한번만 
        CancelInvoke();    // Monobehaviour에 Invoke가 있다면 
    }
}
