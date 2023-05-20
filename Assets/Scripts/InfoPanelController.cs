using UnityEngine;

public class InfoPanelController : MonoBehaviour
{
    [SerializeField] UIManager uiManager;
    // Update is called once per frame
    private void Update()
    {
        // InfoPanel이 활성화되어있을때 어디를 클릭하던 해당 패널이 닫힌다
        if (Input.touchCount >= 1)
            uiManager.FloatingUI(Utils.INFOIMG, false);
    }
}
