using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManage : MonoBehaviour
{
    // 音频组件
    public AudioSource audio;

    // 拖入你的 SelectionMenu 面板
    public GameObject selectionMenu;

    // 点击"开始体验"后，弹出选择菜单
    public void GoToMainScene()
    {
        Debug.Log("打开流程选择菜单");

        // 停止背景音乐（可选）
        if (audio != null)
        {
            audio.Stop();
        }

        // 显示 SelectionMenu 菜单
        if (selectionMenu != null)
        {
            selectionMenu.SetActive(true);
        }
        else
        {
            Debug.LogWarning("SelectionMenu 未赋值！请在 Inspector 中拖入该 GameObject");
        }
    }

    // 选择"标准产品流程"
    public void SelectStandardFlow()
    {
        Debug.Log("选择了标准产品流程");
        SceneManager.LoadScene("加载场景");
    }

    // 选择"定制产品流程"
    public void SelectCustomFlow()
    {
        Debug.Log("选择了定制产品流程");
        SceneManager.LoadScene("加载场景");
    }

    // 选择菜单里的"退出"按钮 - 返回主菜单
    public void ExitFromSelection()
    {
        Debug.Log("返回主菜单");

        // 隐藏选择菜单
        if (selectionMenu != null)
        {
            selectionMenu.SetActive(false);
        }
    }

    // 加载主菜单场景（保留原功能，备用）
    public void LoadMainMenuScene()
    {
        Debug.Log("加载主菜单场景");
        if (audio != null)
        {
            audio.Stop();
        }
        SceneManager.LoadScene("开始界面");
    }

    // 退出游戏（开始界面的退出按钮用这个）
    public void ExitGame()
    {
        Debug.Log("游戏退出！");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}