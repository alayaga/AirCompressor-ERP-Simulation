using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通用UI脚本 — 继承 UIFormBase → BillPanelBase
/// 添加标题/内容文本设置能力
/// </summary>
public class GeneralUIScript : UIFormBase
{
    [Header("文本")]
    [SerializeField] protected Text titleText;
    [SerializeField] protected Text contentText;

    public void SetAllTexts(string title, string content)
    {
        SetTitleText(title);
        SetContentText(content);
    }

    public void SetTitleText(string title)
    {
        if (titleText != null) titleText.text = title;
    }

    public void SetContentText(string content)
    {
        if (contentText != null) contentText.text = content;
    }
}
