using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 开始界面与流程选择管理器
/// 已集成：标准/定制流程选择 > 通知 DemandManager 固定需求(2台) > 加载主场景
/// </summary>
public class SceneManage : MonoBehaviour
{
    [Header("音频与菜单")]
    public AudioSource audio;
    public GameObject selectionMenu;

    /// <summary>
    /// 点击 "开始体验" 后，弹出选择菜单
    /// </summary>
    public void GoToMainScene()
    {
        Debug.Log("打开流程选择菜单");

        if (audio != null) audio.Stop();

        if (selectionMenu != null)
        {
            selectionMenu.SetActive(true);
        }
        else
        {
            Debug.LogWarning("SelectionMenu 未赋值！请在 Inspector 中拖入该 GameObject");
        }
    }

    /// <summary>
    /// 选择 "标准产品流程"
    /// </summary>
    public void SelectStandardFlow()
    {
        Debug.Log("选择了标准产品流程");
        PrepareDemandForFlow(DemandManager.WorkflowType.Standard);
        SceneManager.LoadScene("加载场景");
    }

    /// <summary>
    /// 选择 "定制产品流程"
    /// </summary>
    public void SelectCustomFlow()
    {
        Debug.Log("选择了定制产品流程");
        PrepareDemandForFlow(DemandManager.WorkflowType.Custom);
        SceneManager.LoadScene("加载场景");
    }

    /// <summary>
    /// 核心对接方法：设置流程类型并生成固定调试需求
    /// </summary>
    private void PrepareDemandForFlow(DemandManager.WorkflowType type)
    {
        var demandMgr = DemandManager.Instance;
        if (demandMgr != null)
        {
            // 1. 透传流程类型
            demandMgr.SetWorkflowType(type);

            // 2. 调试阶段：固定生成 2 台空压机需求（内部已做 isDebugMode 判断）
            demandMgr.GenerateNewDemand();

            var current = demandMgr.GetCurrentDemand();
            Debug.Log($"[SceneManage] 流程预设置: {type} | 客户:{current.customerName} | 数量:{current.airCompressorCount}台 | 单价:{current.unitPrice}");
        }
        else
        {
            Debug.LogError("[SceneManage]  未找到 DemandManager！请确保场景中已挂载且未丢失单例引用。");
        }
    }

    /// <summary>
    /// 选择菜单里的 "退出" 按钮 - 返回主菜单
    /// </summary>
    public void ExitFromSelection()
    {
        Debug.Log("返回主菜单");
        if (selectionMenu != null) selectionMenu.SetActive(false);
    }

    /// <summary>
    /// 加载主菜单场景（保留原功能，备用）
    /// </summary>
    public void LoadMainMenuScene()
    {
        Debug.Log("加载主菜单场景");
        if (audio != null) audio.Stop();
        SceneManager.LoadScene("开始界面");
    }

    /// <summary>
    /// 退出游戏（开始界面的退出按钮用这个）
    /// </summary>
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