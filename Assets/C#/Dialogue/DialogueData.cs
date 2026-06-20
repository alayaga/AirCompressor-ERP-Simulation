using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 纯线性对话数据 — ScriptableObject，在 Inspector 里按顺序填表即可。
/// 第0句 → 第1句 → 第2句……全部播完对话结束。
/// </summary>
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public List<DialogueLine> lines;

    [Header("流程集成（可选）")]
    [Tooltip("对话结束后是否自动完成当前流程步骤")]
    public bool completeStepOnEnd = false;
}

/// <summary>
/// 对话中的一句话
/// </summary>
[System.Serializable]
public class DialogueLine
{
    [Tooltip("说话人名字，比如\"主角\" / \"村长\"")]
    public string speakerName;

    [Tooltip("这句话的文案")]
    [TextArea(2, 5)]
    public string text;
}
