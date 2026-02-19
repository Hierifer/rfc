using UnityEngine;
using UnityEngine.UI;

namespace MazeGame
{
    /// <summary>
    /// 关闭面板按钮
    /// 直接控制 GameObject 的激活状态
    /// </summary>
    public class ClosePanelButton : MonoBehaviour
    {
        [SerializeField] private GameObject panelToClose;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnClick);
            }
            else
            {
                Debug.LogError("ClosePanelButton: No Button component found!");
            }
        }

        private void OnClick()
        {
            Debug.Log($"=== ClosePanelButton clicked ===");

            if (panelToClose != null)
            {
                Debug.Log($"Closing panel: {panelToClose.name}");
                panelToClose.SetActive(false);
                Debug.Log($"Panel active state: {panelToClose.activeSelf}");
            }
            else
            {
                Debug.LogError("ClosePanelButton: panelToClose is null!");
            }
        }

        /// <summary>
        /// 设置要关闭的面板（用于代码动态设置）
        /// </summary>
        public void SetPanelToClose(GameObject panel)
        {
            panelToClose = panel;
            Debug.Log($"ClosePanelButton: Panel set to {panel?.name}");
        }
    }
}
