using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum UIFormMode
{
    Fill,
    Approve,
    PushDown,
    ViewOnly,
    Sign
}

/// <summary>
/// UI表单基类 — 支持按角色+单据类型动态显示按钮组
/// </summary>
public class UIFormBase : MonoBehaviour
{
    [Header("模式设置")]
    [SerializeField] protected UIFormMode currentMode = UIFormMode.Fill;

    [Header("子面板引用")]
    [SerializeField] protected GameObject fillModeUI;
    [SerializeField] protected GameObject approveModeUI;
    [SerializeField] protected GameObject pushDownModeUI;

    [Header("按钮引用（按单据需要拖拽）")]
    [SerializeField] protected Button saveBtn;
    [SerializeField] protected Button submitBtn;
    [SerializeField] protected Button fillBtn;
    [SerializeField] protected Button exitBtn;
    [SerializeField] protected Button shipBtn;
    [SerializeField] protected Button approveBtn;
    [SerializeField] protected Button signBtn;

    protected bool isVisible = false;
    private bool _isCompleted = false;
    public bool IsCompleted => _isCompleted;

    protected virtual void Awake()
    {
        SetAllUIVisibility(false);
        BindButtonEvents();
    }

    /// <summary>
    /// 绑定按钮点击事件（子类可重写添加自定义逻辑）
    /// </summary>
    protected virtual void BindButtonEvents()
    {
        if (saveBtn != null) saveBtn.onClick.AddListener(OnSaveClicked);
        if (submitBtn != null) submitBtn.onClick.AddListener(OnSubmitClicked);
        if (fillBtn != null) fillBtn.onClick.AddListener(OnFillClicked);
        if (exitBtn != null) exitBtn.onClick.AddListener(OnExitClicked);
        if (shipBtn != null) shipBtn.onClick.AddListener(OnShipClicked);
        if (approveBtn != null) approveBtn.onClick.AddListener(OnApproveClicked);
        if (signBtn != null) signBtn.onClick.AddListener(OnSignClicked);
    }

    protected virtual void OnDestroy()
    {
        if (saveBtn != null) saveBtn.onClick.RemoveListener(OnSaveClicked);
        if (submitBtn != null) submitBtn.onClick.RemoveListener(OnSubmitClicked);
        if (fillBtn != null) fillBtn.onClick.RemoveListener(OnFillClicked);
        if (exitBtn != null) exitBtn.onClick.RemoveListener(OnExitClicked);
        if (shipBtn != null) shipBtn.onClick.RemoveListener(OnShipClicked);
        if (approveBtn != null) approveBtn.onClick.RemoveListener(OnApproveClicked);
        if (signBtn != null) signBtn.onClick.RemoveListener(OnSignClicked);
    }

    #region 按钮事件（子类可重写）
    protected virtual void OnSaveClicked()   { Debug.Log($"[UIFormBase] 保存"); }
    protected virtual void OnSubmitClicked() { Debug.Log($"[UIFormBase] 提交"); _isCompleted = true; }
    protected virtual void OnFillClicked()   { Debug.Log($"[UIFormBase] 填写"); }
    protected virtual void OnExitClicked()   { Debug.Log($"[UIFormBase] 退出"); _isCompleted = true; }
    protected virtual void OnShipClicked()   { Debug.Log($"[UIFormBase] 发货"); _isCompleted = true; }
    protected virtual void OnApproveClicked(){ Debug.Log($"[UIFormBase] 审核"); _isCompleted = true; }
    protected virtual void OnSignClicked()   { Debug.Log($"[UIFormBase] 签名"); _isCompleted = true; }
    #endregion

    /// <summary>
    /// 按角色配置显示/隐藏按钮组
    /// </summary>
    public void ConfigureButtons(List<Interactables.ActionType> visibleButtons)
    {
        // 先全部隐藏
        SetButtonVisible(saveBtn, false);
        SetButtonVisible(submitBtn, false);
        SetButtonVisible(fillBtn, false);
        SetButtonVisible(exitBtn, false);
        SetButtonVisible(shipBtn, false);
        SetButtonVisible(approveBtn, false);
        SetButtonVisible(signBtn, false);

        // 按配置显示
        foreach (var action in visibleButtons)
        {
            switch (action)
            {
                case Interactables.ActionType.Save:    SetButtonVisible(saveBtn, true); break;
                case Interactables.ActionType.Submit:  SetButtonVisible(submitBtn, true); break;
                case Interactables.ActionType.Fill:    SetButtonVisible(fillBtn, true); break;
                case Interactables.ActionType.Exit:    SetButtonVisible(exitBtn, true); break;
                case Interactables.ActionType.Ship:    SetButtonVisible(shipBtn, true); break;
                case Interactables.ActionType.Approve: SetButtonVisible(approveBtn, true); break;
                case Interactables.ActionType.Sign:    SetButtonVisible(signBtn, true); break;
            }
        }
    }

    private void SetButtonVisible(Button btn, bool visible)
    {
        if (btn != null) btn.gameObject.SetActive(visible);
    }

    /// <summary>
    /// 重置完成标记（每次打开单据前调用）
    /// </summary>
    public void ResetCompleted()
    {
        _isCompleted = false;
    }

    public virtual void ShowUIMode(UIFormMode mode)
    {
        currentMode = mode;
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
                // 纯查看模式：只显示退出按钮
                break;
            case UIFormMode.Sign:
                // 签名模式
                break;
        }

        gameObject.SetActive(true);
        isVisible = true;

        var player = ObjectManager.Instance?.GetObject(ObjectManager.ObjectType.Player);
        player?.GetComponent<SimpleFirstPersonController>()?.SetPlayerInputEnabled(false);
    }

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
        if (isVisible)
            ShowUIMode(mode);
        else
            currentMode = mode;
    }
}