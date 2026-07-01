using System;
using UnityEngine;

/// <summary>
/// 对话配置结构体
/// 由流程步骤（StepData）持有，告诉 DialogueController 这一步播什么对话
/// </summary>
[Serializable]
public struct DialogueConfig
{
    [Tooltip("对话模式：None=无对话 / Static=静态资产 / LLM=大模型（预留）")]
    public DialogueMode mode;

    [Tooltip("静态对话资产（mode 为 Static 时需要）")]
    public DialogueData data;

    /// <summary>无对话配置的便捷常量</summary>
    public static DialogueConfig None => default;

    /// <summary>是否有有效的对话配置</summary>
    public bool HasDialogue => mode != DialogueMode.None && data != null;
}
