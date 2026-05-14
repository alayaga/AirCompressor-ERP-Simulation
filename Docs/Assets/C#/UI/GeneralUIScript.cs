using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 通用UI脚本
/// </summary>
public class GeneralUIScript : UIFormBase
{
    [Header("文本")]
    [SerializeField] protected Text titleText;
    [SerializeField] protected Text contentText;
    
    [Header("按钮")]
    [SerializeField] protected Button fillButton;
    [SerializeField] protected Button approveButton;
    [SerializeField] protected Button rejectButton;
    [SerializeField] protected Button forwardButton;
    
    private bool _isFillButtonClicked = false;
    private bool _isApproveButtonClicked = false;
    private bool _isRejectButtonClicked = false;
    private bool _isForwardButtonClicked = false;

    protected override void Awake()
    {        
        base.Awake();
        AddButtonListeners();
    }
    
    protected virtual void OnDestroy()
    {        
        RemoveButtonListeners();
    }

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
    
    private void AddButtonListeners()
    {        
        if (fillButton != null) fillButton.onClick.AddListener(OnFillButtonClicked);
        if (approveButton != null) approveButton.onClick.AddListener(OnApproveButtonClicked);
        if (rejectButton != null) rejectButton.onClick.AddListener(OnRejectButtonClicked);
        if (forwardButton != null) forwardButton.onClick.AddListener(OnForwardButtonClicked);
    }
    
    private void RemoveButtonListeners()
    {        
        if (fillButton != null) fillButton.onClick.RemoveListener(OnFillButtonClicked);
        if (approveButton != null) approveButton.onClick.RemoveListener(OnApproveButtonClicked);
        if (rejectButton != null) rejectButton.onClick.RemoveListener(OnRejectButtonClicked);
        if (forwardButton != null) forwardButton.onClick.RemoveListener(OnForwardButtonClicked);
    }
    
    private void OnFillButtonClicked() { _isFillButtonClicked = true; }
    private void OnApproveButtonClicked() { _isApproveButtonClicked = true; }
    private void OnRejectButtonClicked() { _isRejectButtonClicked = true; }
    private void OnForwardButtonClicked() { _isForwardButtonClicked = true; }
    
    public IEnumerator WaitForFillButtonClick()
    {        
        _isFillButtonClicked = false;
        while (!_isFillButtonClicked) yield return null;
    }
    
    public IEnumerator WaitForApproveButtonClick()
    {        
        _isApproveButtonClicked = false;
        while (!_isApproveButtonClicked) yield return null;
    }
    
    public IEnumerator WaitForForwardButtonClick()
    {        
        _isForwardButtonClicked = false;
        while (!_isForwardButtonClicked) yield return null;
    }
}
