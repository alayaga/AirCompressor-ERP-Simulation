/// <summary>
/// 对话模式枚举
/// 决定流程步骤的对话内容来源
/// </summary>
public enum DialogueMode
{
    /// <summary>无对话，交互直接完成步骤</summary>
    None,
    /// <summary>静态对话，从 DialogueData ScriptableObject 资产读取</summary>
    Static,
    /// <summary>（预留）大模型动态生成对话</summary>
    LLM
}
