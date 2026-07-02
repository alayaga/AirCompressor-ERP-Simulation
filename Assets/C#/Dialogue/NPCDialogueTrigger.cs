using UnityEngine;

/// <summary>
/// 挂到 NPC 上，持有对话数据。
/// 继承自 Interactables，当玩家按 E 交互时触发对话。
///
/// 【架构变更】对话资产不再由 NPC 持有，改为从当前流程步骤读取。
/// 同一 NPC 在不同流程中可播放不同对话。
///
/// 配置说明：
///   - npcName: 在 Interactables 基类设置，匹配流程系统的目标NPC
///   - interactText: 在 Interactables 基类设置（默认"对话"）
///   - 对话内容在流程步骤的 StepData.dialogueConfig 中配置
/// </summary>
public class NPCDialogueTrigger : Interactables
{
    [Header("Controller (Optional)")]
    [Tooltip("场景中的 DialogueController，如果留空会自动查找")]
    public DialogueController dialogueController;

    private void Start()
    {
        if (dialogueController == null)
        {
            dialogueController = FindObjectOfType<DialogueController>();
        }

        // 如果未设置交互文本，使用默认值
        if (string.IsNullOrEmpty(interactText))
        {
            interactText = "对话";
        }
    }

    /// <summary>
    /// 玩家按下交互键时调用（由 InteractionManager 触发）
    /// 从当前流程步骤读取对话配置，有对话则播放，无对话则直接完成步骤
    /// </summary>
    public override void OnInteract()
    {
        if (dialogueController == null)
        {
            Debug.LogWarning($"[NPCDialogueTrigger] {npcName}: 找不到 DialogueController，请确保场景中有该组件");
            return;
        }

        // 如果对话已在活跃中，不重复触发
        if (dialogueController.IsActive)
        {
            Debug.Log($"[NPCDialogueTrigger] {npcName}: 对话已在进行中，忽略重复交互");
            return;
        }

        // 从当前流程步骤读取对话配置
        DialogueConfig config = GetDialogueConfigFromFlow();

        if (!config.HasDialogue)
        {
            // 当前步骤无对话配置，直接完成步骤（兼容非对话 NPC 交互）
            Debug.Log($"[NPCDialogueTrigger] {npcName}: 当前步骤无对话配置，直接完成步骤");
            FlowStepTracker.CompleteStep();
            return;
        }

        Debug.Log($"[NPCDialogueTrigger] {npcName}: 启动对话 (mode={config.mode})");
        dialogueController.StartDialogue(config.data, config.mode);
        // 步骤完成由 DialogueController.EndDialogue() 触发
    }

    /// <summary>
    /// 从当前流程步骤读取对话配置
    /// 自动处理分支流程转发（StandardSalesFlow / CustomSalesFlow）
    /// </summary>
    private DialogueConfig GetDialogueConfigFromFlow()
    {
        FlowBase flow = FlowTaskIntegration.Instance?.GetCurrentFlow();
        if (flow == null) return DialogueConfig.None;

        return flow.GetCurrentStepDialogueConfig();
    }
}
