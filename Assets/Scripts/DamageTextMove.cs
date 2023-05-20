using System.Collections;
using TMPro;
using UnityEngine;

public class DamageTextMove : MonoBehaviour
{
    [SerializeField] TMP_Text damageText;
    [SerializeField] private float minOffsetY;
    [SerializeField] private float maxOffsetY;

    private Transform target;
    private float totalTime;

    public void SetUp(Transform target, int damageAmount)
    {
        gameObject.SetActive(true);
        this.target = target;
        totalTime = 0f;
        damageText.text = damageAmount.ToString();

        StartCoroutine(CO_DamageTextMove(damageAmount));
    }

    private IEnumerator CO_DamageTextMove(int damageAmount)
    {
        while (totalTime <= 0.35f)
        {
            if (target != null)
            {
                damageText.fontSize += totalTime * 0.03f;
                Vector2 targetPos = target.position;
                targetPos.y += minOffsetY;
                transform.position = targetPos;
            }

            totalTime += Time.deltaTime;
            yield return null;
        }

        totalTime = 0f;

        while (totalTime <= 0.3f)
        {
            if (target != null)
            {
                float lerpTime = totalTime * 2.5f;

                Vector2 targetPos = target.position;
                targetPos.y += Mathf.Lerp(minOffsetY, maxOffsetY, lerpTime);
                transform.position = targetPos;

                damageText.color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), lerpTime);
            }

            totalTime += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        GameManager.Inst.damageTexts.Remove(this);
        target = null;
        totalTime = 0f;
        damageText.text = "";
        ObjectPool.ReturnToPool(gameObject);    // 한 객체에 한번만 
        CancelInvoke();
    }
}
