using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 退出游戏UI脚本
/// 提供退出游戏和返回主菜单功能
/// </summary>
public class ExitGameUI : MonoBehaviour
{
    [Header("退出UI设置")]
    [SerializeField]
    private Button exitGameButton; // 退出游戏按钮
    
    [SerializeField]
    private Button backToMainMenuButton; // 返回主菜单按钮
    
    private SceneManage sceneManager; // 场景管理器引用
    
     void Awake()
    {
        
        // 查找场景管理器
        sceneManager = FindObjectOfType<SceneManage>();
        if (sceneManager == null)
        {
            Debug.LogError("找不到SceneManage组件，请确保场景中已添加该组件！");
        }
        
        // 设置按钮事件监听
        if (exitGameButton != null)
        {
            exitGameButton.onClick.AddListener(OnExitGameButtonClicked);
        }
        
        if (backToMainMenuButton != null)
        {
            backToMainMenuButton.onClick.AddListener(OnBackToMainMenuButtonClicked);
        }
        
        // 初始化时隐藏UI
        gameObject.SetActive(false);
    }

    private void OnExitGameButtonClicked()
    {
        Debug.Log("点击了退出游戏按钮");
        
        // 调用场景管理器的退出游戏方法
        if (sceneManager != null)
        {
            sceneManager.ExitGame();
        }
        else
        {
            // 备用方案：直接退出游戏
            Debug.Log("场景管理器不存在，使用Application.Quit()退出游戏");
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
    
    /// <summary>
    /// 返回主菜单按钮点击事件
    /// </summary>
    private void OnBackToMainMenuButtonClicked()
    {
        Debug.Log("点击了返回主菜单按钮");
        
        // 调用场景管理器的返回主菜单方法
        if (sceneManager != null)
        {
            sceneManager.LoadMainMenuScene();
        }
        else
        {
            Debug.LogError("无法返回主菜单，场景管理器不存在！");
        }
    }
    
    private void OnDestroy()
    {
        // 清理按钮事件监听，防止内存泄漏
        if (exitGameButton != null)
        {
            exitGameButton.onClick.RemoveListener(OnExitGameButtonClicked);
        }
        
        if (backToMainMenuButton != null)
        {
            backToMainMenuButton.onClick.RemoveListener(OnBackToMainMenuButtonClicked);
        }
    }
}