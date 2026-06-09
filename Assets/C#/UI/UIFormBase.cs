using UnityEngine;

public enum UIFormMode
{
    Fill,
    Approve,
    PushDown,
    ViewOnly,
    Sign
}

/// <summary>
/// UI表单基类 — 继承 BillPanelBase，增加子面板模式切换
/// 所有按钮管理、状态机、反馈系统由 BillPanelBase 统一处理
/// </summary>
public class UIFormBase : BillPanelBase
{
    [Header("模式子面板")]
    [SerializeField] protected GameObject fillModeUI;
    [SerializeField] protected GameObject approveModeUI;
    [SerializeField] protected GameObject pushDownModeUI;

    protected bool isVisible = false;

    protected override void Awake()
    {
        base.Awake();
        SetAllUIVisibility(false);
    }

    /// <summary>
    /// 按模式显示对应子面板
    /// </summary>
    public virtual void ShowUIMode(UIFormMode mode)
    {
        SetAllUIVisibility(false);
        ResetCompleted();

        switch (mode)
        {
            case UIFormMode.Fill:
                if (fillModeUI != null) fillModeUI.SetActive(true);
                break;
            case UIFormMode.Approve:
                if (approveModeUI != null) approveModeUI.SetActive(true);
                break;
            case UIFormMode.PushDown:
                if (pushDownModeUI != null) pushDownModeUI.SetActive(true);
                break;
            case UIFormMode.ViewOnly:
            case UIFormMode.Sign:
                break;
        }

        gameObject.SetActive(true);
        isVisible = true;

        var player = ObjectManager.Instance?.GetObject(ObjectManager.ObjectType.Player);
        player?.GetComponent<SimpleFirstPersonController>()?.SetPlayerInputEnabled(false);
    }

    /// <summary>
    /// 隐藏整个UI面板
    /// </summary>
    public virtual void HideUI()
    {
        SetAllUIVisibility(false);
        gameObject.SetActive(false);
        isVisible = false;

        var player = ObjectManager.Instance?.GetObject(ObjectManager.ObjectType.Player);
        player?.GetComponent<SimpleFirstPersonController>()?.SetPlayerInputEnabled(true);
    }

    public void SetAllUIVisibility(bool visible)
    {
        if (fillModeUI != null) fillModeUI.SetActive(visible);
        if (approveModeUI != null) approveModeUI.SetActive(visible);
        if (pushDownModeUI != null) pushDownModeUI.SetActive(visible);
    }

    public virtual void SwitchMode(UIFormMode mode)
    {
        if (isVisible) ShowUIMode(mode);
    }
}
