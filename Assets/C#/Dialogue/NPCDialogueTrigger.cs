using UnityEngine;

/// <summary>
/// 挂到 NPC 上，持有对话数据。
/// 继承自 Interactables，当玩家按 E 交互时触发对话。
/// 配置说明：
///   - npcName: 在 Interactables 基类设置，匹配流程系统的目标NPC
///   - interactText: 在 Interactables 基类设置（默认"对话"）
///   - dialogueData: 要播放的对话数据（ScriptableObject）
/// </summary>
public class NPCDialogueTrigger : Interactables
{
    [Header("Dialogue")]
    [Tooltip("要播放的对话数据（在 Assets 中右键 → Dialogue → Dialogue Data 创建）")]
    public DialogueData dialogueData;

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
    /// </summary>
    public override void OnInteract()
    {
        if (dialogueController == null)
        {
            Debug.LogWarning($"[NPCDialogueTrigger] {npcName}: 找不到 DialogueController，请确保场景中有该组件");
            return;
        }

        if (dialogueData == null)
        {
            Debug.LogWarning($"[NPCDialogueTrigger] {npcName}: 没有指定 DialogueData");
            return;
        }

        // 如果对话已在活跃中，不重复触发
        if (dialogueController.IsActive)
        {
            Debug.Log($"[NPCDialogueTrigger] {npcName}: 对话已在进行中，忽略重复交互");
            return;
        }

        dialogueController.StartDialogue(dialogueData);
    }
}
