using System.Collections;
using UnityEngine;

public class DiceBullet : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SerializeDiceData serializeDiceData;
    [SerializeField] private GameObject[] Eff_SkillBullet;
    [SerializeField] private GameObject Eff_bullet;

    [SerializeField] private GameObject myBullet;
    [SerializeField] private SpriteRenderer Eff_BulletDieRenderer;
    [SerializeField] private float speed;

    public DiceData diceData => GameManager.Inst.diceSO.GetDiceData(serializeDiceData.code);

    protected Enemy targetEnemy;

    protected void OnEnable()
    {
        for(int i = 0; i < Eff_SkillBullet.Length; i++)
        {
            Eff_SkillBullet[i].SetActive(false);
        }

        Eff_bullet.SetActive(false);

        if(myBullet != null)
            myBullet.SetActive(false);
    }

    private void CheckMyBullet(SerializeDiceData serializeDiceData)
    {
        switch (serializeDiceData.code)
        {
            case Utils.CRACKNUM:
                myBullet = Eff_SkillBullet[0];
                break;

            case Utils.FIRENUM:
                myBullet = Eff_SkillBullet[1];
                break;

            case Utils.ICENUM:
                myBullet = Eff_SkillBullet[2];
                break;

            case Utils.POISONNUM:
                myBullet = Eff_SkillBullet[3];
                break;

            case Utils.ELECTRICNUM:
                myBullet = Eff_SkillBullet[4];
                break;

            default:
                myBullet = Eff_bullet;
                break;
        }

        Eff_BulletDieRenderer = myBullet.GetComponent<SpriteRenderer>();
    }

    public virtual void SetUpDiceBullet(SerializeDiceData serializeDiceData, Enemy targetEnemy, bool isPlayer)
    {
        CheckMyBullet(serializeDiceData);
        this.serializeDiceData = serializeDiceData;
        this.targetEnemy = targetEnemy;
        spriteRenderer.enabled = true;
        spriteRenderer.color = diceData.color;
        Eff_BulletDieRenderer.color = diceData.color;
        StartCoroutine(CO_Attack(isPlayer));
    }

    protected IEnumerator CO_Attack(bool isPlayer)
    {
        while (true)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetEnemy.transform.position, Time.deltaTime * speed);
            yield return null;

            if ((transform.position - targetEnemy.transform.position).sqrMagnitude < Time.deltaTime * speed * Time.deltaTime * speed)
            {
                transform.position = targetEnemy.transform.position;
                break;
            }
        }

        int totalAttackdamage = Utils.TotalAttackDamage(diceData.basicAttackDamage, serializeDiceData.level);

        if (targetEnemy != null)
        {
            targetEnemy.Damaged(totalAttackdamage, isPlayer);
            DamageTextMove damageTmp = ObjectPool.SpawnFromPool("DamageText", targetEnemy.transform.position, Utils.QI).GetComponent<DamageTextMove>();
            damageTmp.GetComponent<DamageTextMove>().SetUp(targetEnemy.transform, totalAttackdamage);
            GameManager.Inst.damageTexts.Add(damageTmp);
        }

        Die();
    }

    protected void Die()
    {
        myBullet.transform.position = targetEnemy.transform.position;
        myBullet.SetActive(true);
        spriteRenderer.enabled = false;
        Invoke(nameof(DelayDestroy), 1f);
    }

    private void DelayDestroy() => gameObject.SetActive(false);

    protected void OnDisable()
    {
        ObjectPool.ReturnToPool(gameObject);    // 한 객체에 한번만 
        CancelInvoke();    // Monobehaviour에 Invoke가 있다면 
    }
}
