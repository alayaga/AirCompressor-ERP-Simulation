using UnityEngine;
using TMPro;

/// <summary>
/// 对话 UI 层 — 负责显示/隐藏面板、更新说话人名字和文本。
/// 不包含任何逻辑，只做展示。由 DialogueController 直接调用。
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Panel")]
    [Tooltip("整个对话UI的根节点，用于整体显示/隐藏")]
    public GameObject panel;

    [Header("Text")]
    [Tooltip("显示当前说话人的名字")]
    public TextMeshProUGUI speakerNameText;

    [Tooltip("显示当前对话文本")]
    public TextMeshProUGUI dialogueText;

    [Header("Continue Hint")]
    [Tooltip("提示玩家按键继续（比如\"按空格继续\"），不需要可留空")]
    public GameObject continueHint;

    [Header("HUD 控制（可选）")]
    [Tooltip("对话期间隐藏的准星UI，不需要可留空")]
    public CrosshairUI crosshairUI;

    /// <summary>
    /// 显示一句对话
    /// </summary>
    public void ShowLine(string speakerName, string text)
    {
        panel.SetActive(true);

        if (speakerNameText != null)
            speakerNameText.text = speakerName;

        if (dialogueText != null)
            dialogueText.text = text;
    }

    /// <summary>
    /// 显示/隐藏"继续"提示
    /// </summary>
    public void SetContinueHint(bool show)
    {
        if (continueHint != null)
            continueHint.SetActive(show);
    }

    /// <summary>
    /// 关闭对话面板，恢复HUD
    /// </summary>
    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);

        // 恢复准星显示
        if (crosshairUI != null)
            crosshairUI.enabled = true;
    }

    /// <summary>
    /// 显示对话面板时调用，隐藏HUD元素
    /// </summary>
    public void Show()
    {
        if (panel != null)
            panel.SetActive(true);

        // 隐藏准星（对话时不显示准星）
        if (crosshairUI != null)
            crosshairUI.enabled = false;
    }

    /// <summary>
    /// 对话是否正在显示
    /// </summary>
    public bool IsShowing => panel != null && panel.activeSelf;
}
