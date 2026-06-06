using UnityEngine;
using UnityEngine.UI;

public class InputBoxScript : MonoBehaviour
{
    #region 公共引用
    [Header("填写框UI元素")]
    [SerializeField]
    private Text titleText;         // 标题文本
    
    [SerializeField]
    private Text contentText;       // 内容文本
    #endregion
    public void SetInputBoxInfo(string title, string content)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }
        
        if (contentText != null)
        {
            contentText.text = content;
        }
    }
    
    /// <summary>
    /// 获取填写框的标题
    /// </summary>
    public string GetTitle()
    {
        return titleText != null ? titleText.text : string.Empty;
    }

    /// <summary>
    /// 获取填写框的内容
    /// </summary>
    public string GetContent()
    {
        return contentText != null ? contentText.text : string.Empty;
    }
}