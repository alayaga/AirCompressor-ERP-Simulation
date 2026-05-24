using UnityEngine;

/// <summary>
/// 可交互物体组件
/// 挂在场景中的NPC、桌子、物体上，供玩家按E交互
/// </summary>
public class Interactables : MonoBehaviour
{
    #region 枚举定义

    /// <summary>
    /// 操作类型
    /// </summary>
    public enum ActionType
    {
        None,
        Fill,       // 填写
        Approve,    // 审核
        PushDown,   // 下推
        View,       // 查看
        Pick,       // 领取
        Deliver     // 交付
    }

    #endregion

    #region Inspector配置

    [Header("=== 交互信息 ===")]

    [Tooltip("NPC/角色名称，如：销售员、销售主管")]
    public string npcName = "";

    [Tooltip("所在位置，如：销售办公室、生产车间")]
    public string location = "";

    [Tooltip("操作类型")]
    public ActionType actionType = ActionType.None;

    [Tooltip("屏幕显示的交互提示文字")]
    public string interactText = "按E交互";

    #endregion

    #region Unity生命周期

    private void Awake()
    {
        // 可选：初始化时显示/隐藏提示
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 被玩家交互时调用（由 InteractionManager 调用）
    /// </summary>
    public virtual void OnInteract()
    {
        Debug.Log($"[交互] {npcName} - {actionType}");

        // 最简原型：直接完成当前步骤
        FlowStepTracker.CompleteStep();
    }

    /// <summary>
    /// 获取显示信息（供UI使用）
    /// </summary>
    public string GetDisplayInfo()
    {
        return $"【{npcName}】{location} - {actionType}";
    }

    #endregion

    #region 可选：编辑器辅助

    /// <summary>
    /// 在Scene视图中显示标签，方便调试
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        #if UNITY_EDITOR
        // 在Scene视图中显示交互范围（如果将来需要）
        // Gizmos.DrawWireSphere(transform.position, 1f);
        #endif
    }

    #endregion
}
