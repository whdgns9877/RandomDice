using UnityEngine;

public class InfoPanelController : MonoBehaviour
{
    [SerializeField] UIManager uiManager;
    // Update is called once per frame
    private void Update()
    {
        // InfoPanel�� Ȱ��ȭ�Ǿ������� ��� Ŭ���ϴ� �ش� �г��� ������
        if (Input.touchCount >= 1)
            uiManager.FloatingUI(Utils.INFOIMG, false);
    }
}
