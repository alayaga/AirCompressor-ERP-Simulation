using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// 单据面板运行模式（由流程步骤 ActionType 决定）
/// </summary>
public enum BillMode
{
    Editing,     // Fill → 可填写
    Reviewing,   // Approve → 审核模式
    Viewing,     // View → 纯查看
    Picking,     // Pick → 领取模式
    Delivering   // Deliver → 交付模式
}

/// <summary>
/// 单据内部状态
/// </summary>
public enum BillState
{
    Initial,     // 刚打开
    Filled,      // 已点击填写
    Saved,       // 已保存
    Submitted,   // 已提交
    Approved,    // 已审核
    Shipped,     // 已发货
    Signed       // 已签名
}

/// <summary>
/// 单据面板统一基类
/// 管理按钮显隐、状态机、反馈系统（横幅提示 + 确认弹窗）
/// 所有输入框只读，填写 = 系统自动填入预配置数据
/// </summary>
public class BillPanelBase : MonoBehaviour
{
    #region 序列化字段

    [Header("输入框与表格（可选）")]
    [SerializeField] protected InputBoxGenerator inputBoxGenerator;
    [SerializeField] protected InputBoxGenerator inputBoxGenerator2;
    [SerializeField] protected SettingTableGenerator settingTableGenerator;

    [Header("7个标准按钮")]
    [SerializeField] protected Button fillBtn;
    [SerializeField] protected Button saveBtn;
    [SerializeField] protected Button submitBtn;
    [SerializeField] protected Button approveBtn;
    [SerializeField] protected Button shipBtn;
    [SerializeField] protected Button signBtn;
    [SerializeField] protected Button exitBtn;

    [Header("反馈组件")]
    [SerializeField] protected Text bannerText;
    [SerializeField] protected GameObject confirmPopup;
    [SerializeField] protected Text confirmPopupText;
    [SerializeField] protected Button confirmYesBtn;
    [SerializeField] protected Button confirmNoBtn;

    [Header("预配置数据（点击填写后自动填入）")]
    [SerializeField] protected string[] prefillData;
    [SerializeField] protected string[][] prefillTableData;

    #endregion

    #region 运行时状态

    protected BillMode _currentMode = BillMode.Viewing;
    protected BillState _billState = BillState.Initial;
    protected bool _isCompleted = false;
    protected bool _hasUnsavedChanges = false;
    protected List<Interactables.ActionType> _roleButtons = new List<Interactables.ActionType>();

    // 数据快照（用于退出未保存时还原）
    protected string[] _snapshotData;
    protected string[] _snapshotData2;
    protected List<string[]> _snapshotTableData;

    public BillMode CurrentMode => _currentMode;
    public BillState CurrentBillState => _billState;
    public bool IsCompleted => _isCompleted;

    #endregion

    #region 生命周期

    protected virtual void Awake()
    {
        BindAllButtons();
        HideConfirmPopup();
    }

    protected virtual void OnDestroy()
    {
        UnbindAllButtons();
    }

    #endregion

    #region 按钮绑定

    protected virtual void BindAllButtons()
    {
        if (fillBtn != null) fillBtn.onClick.AddListener(OnFillClicked);
        if (saveBtn != null) saveBtn.onClick.AddListener(OnSaveClicked);
        if (submitBtn != null) submitBtn.onClick.AddListener(OnSubmitClicked);
        if (approveBtn != null) approveBtn.onClick.AddListener(OnApproveClicked);
        if (shipBtn != null) shipBtn.onClick.AddListener(OnShipClicked);
        if (signBtn != null) signBtn.onClick.AddListener(OnSignClicked);
        if (exitBtn != null) exitBtn.onClick.AddListener(OnExitClicked);
    }

    protected virtual void UnbindAllButtons()
    {
        if (fillBtn != null) fillBtn.onClick.RemoveAllListeners();
        if (saveBtn != null) saveBtn.onClick.RemoveAllListeners();
        if (submitBtn != null) submitBtn.onClick.RemoveAllListeners();
        if (approveBtn != null) approveBtn.onClick.RemoveAllListeners();
        if (shipBtn != null) shipBtn.onClick.RemoveAllListeners();
        if (signBtn != null) signBtn.onClick.RemoveAllListeners();
        if (exitBtn != null) exitBtn.onClick.RemoveAllListeners();
    }

    #endregion

    #region 公开入口

    /// <summary>
    /// 由 FlowBase.WaitForBillComplete 调用，打开单据面板
    /// </summary>
    /// <param name="stepAction">流程步骤的 ActionType，决定 BillMode</param>
    /// <param name="roleButtons">BillButtonConfig 返回的角色可见按钮列表</param>
    public virtual void OpenBill(Interactables.ActionType stepAction, List<Interactables.ActionType> roleButtons)
    {
        _roleButtons = roleButtons ?? new List<Interactables.ActionType>();
        _currentMode = ActionTypeToBillMode(stepAction);
        _billState = BillState.Initial;
        _isCompleted = false;
        _hasUnsavedChanges = false;
        HideConfirmPopup();
        HideBanner();

        // 计算并应用按钮显隐
        var visibleButtons = ComputeVisibleButtons(_currentMode, _roleButtons);
        ApplyButtonVisibility(visibleButtons);

        // 所有输入框只读
        SetInputsInteractable(false);

        // 子类可在此做额外初始化
        OnBillOpened();

        Debug.Log($"[BillPanelBase] 打开单据 | Mode={_currentMode} | State={_billState} | 可见按钮={string.Join(",", visibleButtons)}");
    }

    /// <summary>
    /// 子类重写以在打开单据时做额外初始化
    /// </summary>
    protected virtual void OnBillOpened() { }

    #endregion

    #region 按钮事件（虚方法，子类可按需重写）

    /// <summary>填写 — 自动填入预配置数据。仅首次生效。</summary>
    protected virtual void OnFillClicked()
    {
        if (_billState >= BillState.Filled)
        {
            ShowBanner("已填写过，无需重复填写", Color.yellow);
            return;
        }

        CaptureSnapshot(); // 填写前快照
        ApplyPrefillData();
        _billState = BillState.Filled;
        _hasUnsavedChanges = true;
        ShowBanner("已填写");
        Debug.Log("[BillPanelBase] 填写 — 数据已自动填入");
    }

    /// <summary>保存 — 保存当前状态，不退出</summary>
    protected virtual void OnSaveClicked()
    {
        if (_billState < BillState.Filled)
        {
            ShowBanner("请先点击填写", Color.red);
            return;
        }

        _billState = BillState.Saved;
        _hasUnsavedChanges = false;
        ShowBanner("已保存成功！");
        Debug.Log("[BillPanelBase] 保存");
    }

    /// <summary>提交 — 已填写→完成退出，未填写→提示</summary>
    protected virtual void OnSubmitClicked()
    {
        if (_billState < BillState.Filled)
        {
            ShowBanner("<color=red>未填写必填项！</color>");
            Debug.Log("[BillPanelBase] 提交失败 — 未填写");
            return;
        }

        _billState = BillState.Submitted;
        _hasUnsavedChanges = false;
        ShowBanner("已提交！");
        Debug.Log("[BillPanelBase] 提交 — 完成");
        DelayedComplete();
    }

    /// <summary>审核 — 提示已审核 + 弹窗是否下推</summary>
    protected virtual void OnApproveClicked()
    {
        _billState = BillState.Approved;
        _hasUnsavedChanges = false;
        ShowBanner("已审核！");
        Debug.Log("[BillPanelBase] 审核");

        // 弹窗确认是否下推
        ShowConfirm("是否下推？",
            onYes: () =>
            {
                ShowBanner("已下推！");
                Debug.Log("[BillPanelBase] 审核 + 下推 — 完成");
                DelayedComplete();
            },
            onNo: () =>
            {
                Debug.Log("[BillPanelBase] 审核 — 不下推，完成");
                DelayedComplete();
            }
        );
    }

    /// <summary>发货 — 条件检查 + 完成</summary>
    protected virtual void OnShipClicked()
    {
        if (!CheckShipConditions(out string failReason))
        {
            ShowConfirm(failReason,
                onYes: () => { /* 关闭弹窗，返回表单 */ },
                onNo: () => { /* 关闭弹窗，返回表单 */ }
            );
            Debug.Log($"[BillPanelBase] 发货失败 — {failReason}");
            return;
        }

        _billState = BillState.Shipped;
        _hasUnsavedChanges = false;
        ShowBanner("已通知仓库发货");
        Debug.Log("[BillPanelBase] 发货 — 完成");
        DelayedComplete();
    }

    /// <summary>签名 — 完成</summary>
    protected virtual void OnSignClicked()
    {
        _billState = BillState.Signed;
        _hasUnsavedChanges = false;
        ShowBanner("已签名！");
        Debug.Log("[BillPanelBase] 签名 — 完成");
        DelayedComplete();
    }

    /// <summary>退出 — 未保存→还原数据，已保存→直接退出</summary>
    protected virtual void OnExitClicked()
    {
        if (_hasUnsavedChanges)
        {
            RevertSnapshot();
            Debug.Log("[BillPanelBase] 退出 — 未保存，数据已还原");
        }
        else
        {
            Debug.Log("[BillPanelBase] 退出");
        }
        _isCompleted = true;
    }

    #endregion

    #region 发货条件检查（子类重写）

    /// <summary>
    /// 检查发货条件。默认直接通过。子类重写以实现业务逻辑。
    /// </summary>
    /// <returns>true=可以发货</returns>
    protected virtual bool CheckShipConditions(out string failReason)
    {
        failReason = null;
        return true;
    }

    #endregion

    #region 按钮显隐

    /// <summary>
    /// 根据 BillMode 和角色按钮配置，计算最终可见按钮
    /// </summary>
    protected virtual List<Interactables.ActionType> ComputeVisibleButtons(BillMode mode, List<Interactables.ActionType> roleButtons)
    {
        var result = new List<Interactables.ActionType> { Interactables.ActionType.Exit }; // 始终可用

        switch (mode)
        {
            case BillMode.Editing:
                result.Add(Interactables.ActionType.Fill);
                result.Add(Interactables.ActionType.Save);
                result.Add(Interactables.ActionType.Submit);
                break;
            case BillMode.Reviewing:
                result.Add(Interactables.ActionType.Approve);
                break;
            case BillMode.Picking:
                result.Add(Interactables.ActionType.Pick);
                break;
            case BillMode.Delivering:
                result.Add(Interactables.ActionType.Deliver);
                break;
            case BillMode.Viewing:
                break; // 仅 Exit
        }

        // Ship: 仅 Editing / Delivering 模式下且角色允许时显示
        if (roleButtons.Contains(Interactables.ActionType.Ship) &&
            (mode == BillMode.Editing || mode == BillMode.Delivering))
        {
            result.Add(Interactables.ActionType.Ship);
        }

        // Sign: 仅 Reviewing / Viewing 模式下且角色允许时显示
        if (roleButtons.Contains(Interactables.ActionType.Sign) &&
            (mode == BillMode.Reviewing || mode == BillMode.Viewing))
        {
            result.Add(Interactables.ActionType.Sign);
        }

        return result;
    }

    /// <summary>
    /// 将可见按钮列表应用到实际 Button GameObject
    /// </summary>
    protected virtual void ApplyButtonVisibility(List<Interactables.ActionType> visibleButtons)
    {
        SetBtn(fillBtn,    visibleButtons.Contains(Interactables.ActionType.Fill));
        SetBtn(saveBtn,    visibleButtons.Contains(Interactables.ActionType.Save));
        SetBtn(submitBtn,  visibleButtons.Contains(Interactables.ActionType.Submit));
        SetBtn(approveBtn, visibleButtons.Contains(Interactables.ActionType.Approve));
        SetBtn(shipBtn,    visibleButtons.Contains(Interactables.ActionType.Ship));
        SetBtn(signBtn,    visibleButtons.Contains(Interactables.ActionType.Sign));
        SetBtn(exitBtn,    visibleButtons.Contains(Interactables.ActionType.Exit));
    }

    protected void SetBtn(Button btn, bool show)
    {
        if (btn != null) btn.gameObject.SetActive(show);
    }

    #endregion

    #region 数据管理

    /// <summary>应用预配置的填写数据到输入框和表格</summary>
    protected virtual void ApplyPrefillData()
    {
        if (inputBoxGenerator != null && prefillData != null && prefillData.Length > 0)
            inputBoxGenerator.SetAllInputBoxContents(prefillData);

        if (inputBoxGenerator2 != null && prefillData != null && prefillData.Length > 0)
            inputBoxGenerator2.SetAllInputBoxContents(prefillData);

        if (settingTableGenerator != null && prefillTableData != null && prefillTableData.Length > 0)
        {
            settingTableGenerator.ClearTable();
            settingTableGenerator.CreateHeaderRow();
            foreach (var rowData in prefillTableData)
                settingTableGenerator.AddRow(rowData);
        }
    }

    /// <summary>快照当前数据（填写前调用）</summary>
    protected virtual void CaptureSnapshot()
    {
        _snapshotData = inputBoxGenerator?.GetAllContents();
        _snapshotData2 = inputBoxGenerator2?.GetAllContents();
        Debug.Log("[BillPanelBase] 已捕获数据快照");
    }

    /// <summary>还原快照数据（退出未保存时调用）</summary>
    protected virtual void RevertSnapshot()
    {
        if (inputBoxGenerator != null && _snapshotData != null)
            inputBoxGenerator.SetAllInputBoxContents(_snapshotData);
        if (inputBoxGenerator2 != null && _snapshotData2 != null)
            inputBoxGenerator2.SetAllInputBoxContents(_snapshotData2);
        Debug.Log("[BillPanelBase] 数据已还原");
    }

    /// <summary>设置输入框全部只读</summary>
    protected virtual void SetInputsInteractable(bool interactable)
    {
        // 输入框始终只读 — 玩家不手动输入
        // 子类可按需重写
    }

    #endregion

    #region 反馈系统

    /// <summary>显示顶部横幅提示</summary>
    protected void ShowBanner(string message, Color? color = null)
    {
        if (bannerText == null) return;
        bannerText.text = message;
        if (color.HasValue) bannerText.color = color.Value;
        bannerText.gameObject.SetActive(true);
        Debug.Log($"[BillPanelBase] 横幅: {message}");
    }

    /// <summary>隐藏横幅</summary>
    protected void HideBanner()
    {
        if (bannerText != null) bannerText.gameObject.SetActive(false);
    }

    /// <summary>显示确认弹窗</summary>
    protected void ShowConfirm(string message, Action onYes, Action onNo)
    {
        if (confirmPopup == null)
        {
            Debug.LogWarning("[BillPanelBase] 确认弹窗未配置，直接执行「否」");
            onNo?.Invoke();
            return;
        }

        if (confirmPopupText != null) confirmPopupText.text = message;

        // 清除旧监听
        if (confirmYesBtn != null) confirmYesBtn.onClick.RemoveAllListeners();
        if (confirmNoBtn != null) confirmNoBtn.onClick.RemoveAllListeners();

        // 绑定新事件
        if (confirmYesBtn != null) confirmYesBtn.onClick.AddListener(() =>
        {
            HideConfirmPopup();
            onYes?.Invoke();
        });
        if (confirmNoBtn != null) confirmNoBtn.onClick.AddListener(() =>
        {
            HideConfirmPopup();
            onNo?.Invoke();
        });

        confirmPopup.SetActive(true);
        SetButtonsInteractable(false); // 弹窗显示时禁用按钮栏
        Debug.Log($"[BillPanelBase] 弹窗: {message}");
    }

    /// <summary>隐藏确认弹窗</summary>
    protected void HideConfirmPopup()
    {
        if (confirmPopup != null) confirmPopup.SetActive(false);
        SetButtonsInteractable(true);
    }

    /// <summary>禁用/启用按钮栏（弹窗时禁用）</summary>
    protected void SetButtonsInteractable(bool interactable)
    {
        if (fillBtn != null) fillBtn.interactable = interactable;
        if (saveBtn != null) saveBtn.interactable = interactable;
        if (submitBtn != null) submitBtn.interactable = interactable;
        if (approveBtn != null) approveBtn.interactable = interactable;
        if (shipBtn != null) shipBtn.interactable = interactable;
        if (signBtn != null) signBtn.interactable = interactable;
        if (exitBtn != null) exitBtn.interactable = interactable;
    }

    #endregion

    #region 工具方法

    /// <summary>ActionType → BillMode 映射</summary>
    protected BillMode ActionTypeToBillMode(Interactables.ActionType action)
    {
        switch (action)
        {
            case Interactables.ActionType.Fill:    return BillMode.Editing;
            case Interactables.ActionType.Approve: return BillMode.Reviewing;
            case Interactables.ActionType.View:    return BillMode.Viewing;
            case Interactables.ActionType.Pick:    return BillMode.Picking;
            case Interactables.ActionType.Deliver:  return BillMode.Delivering;
            default: return BillMode.Viewing;
        }
    }

    /// <summary>延迟完成（等待横幅/弹窗显示后再标记 IsCompleted）</summary>
    protected void DelayedComplete()
    {
        StartCoroutine(DelayedCompleteCoroutine());
    }

    private System.Collections.IEnumerator DelayedCompleteCoroutine()
    {
        yield return new WaitForSeconds(0.8f);
        _isCompleted = true;
    }

    /// <summary>重置完成标记（每次打开单据前调用）</summary>
    public void ResetCompleted() => _isCompleted = false;

    /// <summary>子类可调用以直接设置完成</summary>
    protected void MarkCompleted() => _isCompleted = true;

    #endregion
}
