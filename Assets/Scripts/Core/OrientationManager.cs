using UnityEngine;

namespace MazeGame
{
    /// <summary>
    /// 管理游戏屏幕方向
    /// 强制横屏模式
    /// </summary>
    public class OrientationManager : MonoBehaviour
    {
        private void Awake()
        {
            // 强制横屏模式
            SetLandscapeOrientation();
        }

        private void Start()
        {
            // 再次确保横屏（有些设备在 Awake 设置可能被重置）
            SetLandscapeOrientation();
        }

        /// <summary>
        /// 设置为横屏模式
        /// </summary>
        private void SetLandscapeOrientation()
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;

            Debug.Log($"Orientation set to Landscape. Current: {Screen.orientation}, Size: {Screen.width}x{Screen.height}");
        }
    }
}
