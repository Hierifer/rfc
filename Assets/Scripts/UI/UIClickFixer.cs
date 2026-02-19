using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MazeGame
{
    /// <summary>
    /// 修复 UI 点击问题的工具
    /// </summary>
    public class UIClickFixer : MonoBehaviour
    {
        private void Start()
        {
            // 延迟执行以确保所有 UI 都已初始化
            Invoke("DiagnoseAndFix", 0.5f);
        }

        private void DiagnoseAndFix()
        {
            Debug.Log("=== UI Click Diagnostics ===");

            // 1. 检查 EventSystem
            EventSystem eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                Debug.LogError("❌ No EventSystem found! Creating one...");
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
            }
            else
            {
                Debug.Log($"✓ EventSystem found: {eventSystem.gameObject.name}");

                // 检查 Input Module
                StandaloneInputModule inputModule = eventSystem.GetComponent<StandaloneInputModule>();
                if (inputModule == null)
                {
                    Debug.LogError("❌ No StandaloneInputModule! Adding one...");
                    eventSystem.gameObject.AddComponent<StandaloneInputModule>();
                }
                else
                {
                    Debug.Log("✓ StandaloneInputModule found");
                }
            }

            // 2. 检查 Canvas
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            Debug.Log($"Found {canvases.Length} Canvas(es)");

            foreach (Canvas canvas in canvases)
            {
                Debug.Log($"Canvas: {canvas.gameObject.name}, RenderMode: {canvas.renderMode}");

                // 检查 GraphicRaycaster
                GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
                if (raycaster == null)
                {
                    Debug.LogError($"❌ Canvas '{canvas.gameObject.name}' missing GraphicRaycaster! Adding...");
                    canvas.gameObject.AddComponent<GraphicRaycaster>();
                }
                else
                {
                    Debug.Log($"✓ Canvas '{canvas.gameObject.name}' has GraphicRaycaster");
                }

                // 如果是 Screen Space - Camera 模式，检查相机
                if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    if (canvas.worldCamera == null)
                    {
                        Debug.LogError($"❌ Canvas '{canvas.gameObject.name}' is in Camera mode but has no camera assigned!");
                    }
                }
            }

            // 3. 检查所有按钮
            Button[] buttons = FindObjectsOfType<Button>(true);
            Debug.Log($"Found {buttons.Length} Button(s)");

            foreach (Button button in buttons)
            {
                Debug.Log($"\nChecking Button: {button.gameObject.name}");
                Debug.Log($"  Active: {button.gameObject.activeInHierarchy}");
                Debug.Log($"  Interactable: {button.interactable}");

                // 检查 Image 组件
                Image image = button.GetComponent<Image>();
                if (image == null)
                {
                    Debug.LogError($"  ❌ Button '{button.gameObject.name}' has no Image component! Adding...");
                    image = button.gameObject.AddComponent<Image>();
                    image.color = new Color(0.2f, 0.3f, 0.4f, 0.9f);
                }
                else
                {
                    Debug.Log($"  ✓ Has Image component");
                }

                // 检查 raycastTarget
                if (image != null && !image.raycastTarget)
                {
                    Debug.LogWarning($"  ⚠️ Image raycastTarget is disabled! Enabling...");
                    image.raycastTarget = true;
                }
                else if (image != null)
                {
                    Debug.Log($"  ✓ raycastTarget enabled");
                }

                // 检查 Canvas Group
                CanvasGroup canvasGroup = button.GetComponentInParent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    if (!canvasGroup.interactable)
                    {
                        Debug.LogWarning($"  ⚠️ Parent CanvasGroup is not interactable!");
                    }
                    if (canvasGroup.blocksRaycasts == false)
                    {
                        Debug.LogWarning($"  ⚠️ Parent CanvasGroup blocks raycasts!");
                    }
                }
            }

            Debug.Log("=== Diagnostics Complete ===");
        }

        // 在屏幕上显示点击测试信息
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log($"\n=== CLICK DEBUG at {Input.mousePosition} ===");

                // 检查 EventSystem
                if (EventSystem.current == null)
                {
                    Debug.LogError("EventSystem.current is NULL!");
                    return;
                }

                PointerEventData eventData = new PointerEventData(EventSystem.current);
                eventData.position = Input.mousePosition;

                var results = new System.Collections.Generic.List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, results);

                Debug.Log($"Hit {results.Count} UI elements:");
                foreach (var result in results)
                {
                    Button button = result.gameObject.GetComponent<Button>();
                    string buttonInfo = button != null ? $" [Button, interactable={button.interactable}]" : "";
                    Debug.Log($"  {result.depth}: {GetGameObjectPath(result.gameObject)}{buttonInfo}");
                }

                if (results.Count == 0)
                {
                    Debug.LogWarning("No UI elements hit! Check if GraphicRaycaster is working.");
                }
            }
        }

        // 获取 GameObject 的完整路径
        private string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform current = obj.transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }
    }
}
