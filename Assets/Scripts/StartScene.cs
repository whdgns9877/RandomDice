using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float minSize = 1f;
    [SerializeField] private float maxSize = 2f;
    [SerializeField] private float lerpDuration = 1f;

    private float timer = 0f;

    private void Start()
    {
        text.fontSize = minSize;
    }

    private void Update()
    {
        TextAnim();

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {

            SceneManager.LoadScene("GameScene");
        }
    }

    private void TextAnim()
    {
        timer += Time.deltaTime;

        float t = timer / lerpDuration;
        t = Mathf.Clamp01(t);

        float size = Mathf.Lerp(minSize, maxSize, t);

        text.fontSize = size;

        if (t >= 1f)
        {
            timer = 0f;
            float temp = minSize;
            minSize = maxSize;
            maxSize = temp;
        }
    }
}
