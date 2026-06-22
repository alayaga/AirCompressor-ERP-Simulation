using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 步骤→对话数据映射。同一NPC在不同步骤可播放不同对话内容。
/// </summary>
[System.Serializable]
public class StepDialogueMapping
{
    [Tooltip("流程步骤名，如\"客户询单\"")]
    public string stepName;

    [Tooltip("该步骤对应的对话数据")]
    public DialogueData dialogueData;
}

/// <summary>
/// 挂到 NPC 上，持有对话数据。
/// 继承自 Interactables，当玩家按 E 交互时触发对话。
/// 配置说明：
///   - npcName: 在 Interactables 基类设置，匹配流程系统的目标NPC
///   - interactText: 在 Interactables 基类设置（默认"对话"）
///   - dialogueData: 默认对话数据（兜底）
///   - triggerOnStepNames: 限定只在指定步骤名时触发对话，留空则始终触发
///   - stepDialogueOverrides: 不同步骤使用不同对话（优先于默认 dialogueData）
/// </summary>
public class NPCDialogueTrigger : Interactables
{
    [Header("Dialogue")]
    [Tooltip("默认对话数据（当步骤没有在 Step Dialogue Overrides 中指定时使用）")]
    public DialogueData dialogueData;

    [Header("Trigger Condition")]
    [Tooltip("仅在指定步骤名时触发对话。留空则在有对话数据时始终触发")]
    public List<string> triggerOnStepNames = new List<string>();

    [Header("Per-Step Dialogue Overrides")]
    [Tooltip("不同步骤使用不同对话内容。匹配到的步骤用此处指定的对话，未匹配的用上方默认 Dialogue Data")]
    public List<StepDialogueMapping> stepDialogueOverrides = new List<StepDialogueMapping>();

    [Header("Controller (Optional)")]
    [Tooltip("场景中的 DialogueController，如果留空会自动查找")]
    public DialogueController dialogueController;

    private void Start()
    {
        if (dialogueController == null)
        {
            dialogueController = FindObjectOfType<DialogueController>();
        }

        if (string.IsNullOrEmpty(interactText))
        {
            interactText = "对话";
        }
    }

    /// <summary>
    /// 玩家按下交互键时调用（由 InteractionManager 触发）
    /// </summary>
    public override void OnInteract()
    {
        string currentStepName = GetCurrentFlowStepName();

        // 构建有效步骤名集合：triggerOnStepNames + stepDialogueOverrides 中的 stepName
        var allTriggerSteps = new HashSet<string>();
        if (triggerOnStepNames != null)
        {
            foreach (var s in triggerOnStepNames)
                if (!string.IsNullOrEmpty(s)) allTriggerSteps.Add(s);
        }
        foreach (var m in stepDialogueOverrides)
        {
            if (m != null && !string.IsNullOrEmpty(m.stepName))
                allTriggerSteps.Add(m.stepName);
        }

        // 如果配置了步骤过滤，检查当前步骤是否匹配
        if (allTriggerSteps.Count > 0)
        {
            if (string.IsNullOrEmpty(currentStepName) || !allTriggerSteps.Contains(currentStepName))
            {
                Debug.Log($"[NPCDialogueTrigger] 步骤 \"{currentStepName}\" 不在触发列表中，走普通交互");
                base.OnInteract();
                return;
            }
        }

        // 确定使用哪个对话数据：优先查映射表，其次用默认
        DialogueData dataToUse = dialogueData;
        if (stepDialogueOverrides != null && !string.IsNullOrEmpty(currentStepName))
        {
            var match = stepDialogueOverrides.FirstOrDefault(
                m => m != null && m.stepName == currentStepName);
            if (match != null && match.dialogueData != null)
            {
                dataToUse = match.dialogueData;
                Debug.Log($"[NPCDialogueTrigger] 步骤 \"{currentStepName}\" 匹配映射表，使用指定对话");
            }
        }

        TriggerDialogue(dataToUse);
    }

    private void TriggerDialogue(DialogueData data)
    {
        if (dialogueController == null)
        {
            Debug.LogWarning($"[NPCDialogueTrigger] {npcName}: 找不到 DialogueController");
            return;
        }

        if (data == null)
        {
            Debug.LogWarning($"[NPCDialogueTrigger] {npcName}: 没有指定 DialogueData");
            return;
        }

        if (dialogueController.IsActive)
        {
            Debug.Log($"[NPCDialogueTrigger] {npcName}: 对话已在进行中，忽略重复交互");
            return;
        }

        dialogueController.StartDialogue(data);
    }

    private string GetCurrentFlowStepName()
    {
        var flow = FlowTaskIntegration.Instance?.GetCurrentFlow();
        return flow?.GetCurrentStepName();
    }
}
