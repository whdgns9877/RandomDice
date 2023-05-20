using System.Collections;
using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected TMP_Text healthTXT;
    [SerializeField] protected int health;
    [SerializeField] protected float speed = 5f;
    public float SetSpeed { get { return speed; } set { speed = value; } }

    protected int wayNum = 0;

    public float distance;

    public bool imEnemyOfPlayer;

    public int Health
    {
        get => health;
        set
        {
            health = value;
            healthTXT.text = health.ToString();
        }
    }

    public void Damaged(int damage, bool isPlayer)
    {
        Health -= damage;
        Health = Mathf.Max(0, Health);

        if (Health <= 0 && gameObject.activeSelf)
        {
            if(isPlayer == true)
                GameManager.Inst.PlayerTotalsp += 10;
            else
                GameManager.Inst.aiTotalsp += 10;

            gameObject.SetActive(false);
        }
    }

    protected void Start()
    {
        if (imEnemyOfPlayer == true)
            StartCoroutine(CO_MovePath(true));
        else
            StartCoroutine(CO_MovePath(false));
    }

    protected IEnumerator CO_MovePath(bool _imEnemyOfPlayer)
    {
        if (_imEnemyOfPlayer == true)
        {
            transform.position = Utils.Ways_enemyOfPlayer[wayNum];
            while (true)
            {
                transform.position = Vector2.MoveTowards(transform.position, Utils.Ways_enemyOfPlayer[wayNum], speed * Time.deltaTime);
                distance += Time.deltaTime * speed;

                if ((Vector2)transform.position == Utils.Ways_enemyOfPlayer[wayNum])
                    wayNum++;

                if (wayNum == Utils.Ways_enemyOfPlayer.Length)
                {
                    GameManager.Inst.DecreasePlayerHeart();
                    gameObject.SetActive(false);
                    yield break;
                }

                yield return null;
            }
        }
        else
        {
            transform.position = Utils.Ways_enemyOfAI[wayNum];
            while (true)
            {
                transform.position = Vector2.MoveTowards(transform.position, Utils.Ways_enemyOfAI[wayNum], speed * Time.deltaTime);
                distance += Time.deltaTime * speed;

                if ((Vector2)transform.position == Utils.Ways_enemyOfAI[wayNum])
                    wayNum++;

                if (wayNum == Utils.Ways_enemyOfAI.Length)
                {
                    GameManager.Inst.DecreaseAIHeart();
                    gameObject.SetActive(false);
                    yield break;
                }

                yield return null;
            }
        }
    }

    protected void OnDisable()
    {
        distance = 0;
        wayNum = 0;
        if (imEnemyOfPlayer == true)
            GameManager.Inst.enemiesOfPlayer.Remove(this);
        else
            GameManager.Inst.enemiesOfAI.Remove(this);
        ObjectPool.ReturnToPool(gameObject);    // 한 객체에 한번만 
        CancelInvoke();    // Monobehaviour에 Invoke가 있다면 
    }
}
