using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 轻量对话控制器 — 管理对话状态机和输入。
/// 用法：拖入 DialogueUI 和 DialogueData，调用 StartDialogue() 即可。
/// 自动对接项目的玩家控制器、交互系统和流程追踪器。
/// </summary>
public class DialogueController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("对话 UI 组件（同一个 Canvas 上）")]
    public DialogueUI dialogueUI;

    [Header("Input")]
    [Tooltip("推进对话的按键")]
    public KeyCode advanceKey = KeyCode.Space;

    [Tooltip("对话结束后延迟恢复输入的时间（秒），防止空格键误触跳跃")]
    public float endDelay = 0.5f;

    [Header("Player & Interaction")]
    [Tooltip("玩家第一人称控制器（对话期间禁用移动/视角）")]
    public SimpleFirstPersonController playerController;

    [Tooltip("交互管理器（对话期间禁用E键交互提示）")]
    public InteractionManager interactionManager;

    [Header("Legacy - Disable During Dialogue")]
    [Tooltip("[兼容] 对话期间禁用的组件，对话结束自动恢复")]
    public MonoBehaviour[] disableDuringDialogue;

    [Header("Events")]
    [Tooltip("对话开始时触发")]
    public UnityEvent onDialogueStart;
    [Tooltip("对话结束时触发")]
    public UnityEvent onDialogueEnd;

    // 内部状态
    private DialogueData currentDialogue;
    private int currentIndex;
    private bool isActive;
    private bool isEndingDelayed; // 延迟结束中，忽略输入

    /// <summary>
    /// 当前对话是否活跃中
    /// </summary>
    public bool IsActive => isActive;

    /// <summary>
    /// 启动一段对话
    /// </summary>
    public void StartDialogue(DialogueData data)
    {
        if (data == null || data.lines == null || data.lines.Count == 0)
        {
            Debug.LogWarning("DialogueController: 对话数据为空");
            return;
        }

        currentDialogue = data;
        currentIndex = 0;
        isActive = true;

        // 禁用玩家输入
        SetPlayerInputEnabled(false);

        // 禁用交互系统
        SetInteractionEnabled(false);

        // 禁用旧版指定组件
        SetComponentsEnabled(false);

        // 显示对话UI
        if (dialogueUI != null)
            dialogueUI.Show();

        // 显示第一句
        ShowCurrentLine();

        onDialogueStart?.Invoke();
    }

    /// <summary>
    /// 启动一段对话（指定模式，为 LLM 预留）
    /// </summary>
    public void StartDialogue(DialogueData data, DialogueMode mode)
    {
        if (mode == DialogueMode.LLM)
        {
            Debug.LogWarning("[DialogueController] LLM 对话模式尚未实现，回退到 Static 模式");
            // 未来：从 LLM 服务获取对话内容，动态填充 DialogueData
        }
        StartDialogue(data);
    }

    /// <summary>
    /// 推进到下一句。由 Update 检测按键后调用，也可外部直接调用。
    /// </summary>
    public void NextLine()
    {
        if (!isActive) return;

        currentIndex++;

        if (currentIndex >= currentDialogue.lines.Count)
        {
            // 延迟结束，防止空格键同时触发跳跃
            StartCoroutine(DelayedEnd());
        }
        else
        {
            ShowCurrentLine();
        }
    }

    private System.Collections.IEnumerator DelayedEnd()
    {
        isEndingDelayed = true;

        // 隐藏"继续"提示
        if (dialogueUI != null)
            dialogueUI.SetContinueHint(false);

        yield return new WaitForSeconds(endDelay);

        isEndingDelayed = false;
        EndDialogue();
    }

    /// <summary>
    /// 立即结束对话
    /// </summary>
    public void EndDialogue()
    {
        isActive = false;

        if (dialogueUI != null)
            dialogueUI.Hide();

        // 恢复玩家输入
        SetPlayerInputEnabled(true);

        // 恢复交互系统
        SetInteractionEnabled(true);

        // 恢复旧版指定组件
        SetComponentsEnabled(true);

        // 对话结束始终完成当前流程步骤
        // （流程步骤通过 DialogueConfig 控制是否播放对话，不再依赖资产字段）
        FlowStepTracker.CompleteStep();

        currentDialogue = null;
        currentIndex = 0;

        onDialogueEnd?.Invoke();
    }

    private void ShowCurrentLine()
    {
        if (dialogueUI == null) return;

        DialogueLine line = currentDialogue.lines[currentIndex];

        // 先显示"继续"提示，让玩家看清内容后再按键
        dialogueUI.SetContinueHint(true);
        dialogueUI.ShowLine(line.speakerName, line.text);
    }

    private void Update()
    {
        if (!isActive) return;
        if (isEndingDelayed) return; // 延迟结束中，忽略按键

        if (Input.GetKeyDown(advanceKey))
        {
            NextLine();
        }
    }

    /// <summary>
    /// 控制玩家移动/视角输入
    /// </summary>
    private void SetPlayerInputEnabled(bool enabled)
    {
        if (playerController != null)
            playerController.SetPlayerInputEnabled(enabled);
    }

    /// <summary>
    /// 控制交互提示和E键交互
    /// </summary>
    private void SetInteractionEnabled(bool enabled)
    {
        if (interactionManager != null)
            interactionManager.SetInteractionEnabled(enabled);
    }

    /// <summary>
    /// [兼容] 控制旧版 disableDuringDialogue 列表中的组件
    /// </summary>
    private void SetComponentsEnabled(bool enabled)
    {
        if (disableDuringDialogue == null) return;

        foreach (var comp in disableDuringDialogue)
        {
            if (comp != null)
                comp.enabled = enabled;
        }
    }
}
