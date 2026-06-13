using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

/// <summary>
/// 单据视图 — 统一轻量组件，替代 BillPanelBase / UIFormBase / QuoteFormManager 等
/// 挂载在每个单据Panel的根节点。职责：按钮显隐 + 点击逻辑 + 反馈提示
/// 玩家输入控制由 FlowBase 统一管理，BillView 不参与
/// </summary>
public class BillView : MonoBehaviour
{
    #region 序列化字段

    [Header("输入框与表格（可选，拖拽引用）")]
    [SerializeField] private InputBoxGenerator inputBoxGenerator;
    [SerializeField] private InputBoxGenerator inputBoxGenerator2;
    [SerializeField] private SettingTableGenerator settingTableGenerator;

    [Header("上传附件")]
    [SerializeField] private Image attachmentImage;

    [Header("7个标准按钮")]
    [SerializeField] private Button fillBtn;
    [SerializeField] private Button saveBtn;
    [SerializeField] private Button submitBtn;
    [SerializeField] private Button approveBtn;
    [SerializeField] private Button shipBtn;
    [SerializeField] private Button signBtn;
    [SerializeField] private Button exitBtn;

    [Header("反馈组件 — 横幅提示")]
    [SerializeField] private TMP_Text bannerText;

    [Header("反馈组件 — 确认弹窗（是/否）")]
    [SerializeField] private GameObject confirmPopup;
    [SerializeField] private TMP_Text confirmPopupText;
    [SerializeField] private Button confirmYesBtn;
    [SerializeField] private Button confirmNoBtn;

    [Header("反馈组件 — 警告弹窗（仅确定，单按钮）")]
    [SerializeField] private GameObject alertPopup;
    [SerializeField] private TMP_Text alertPopupText;
    [SerializeField] private Button alertOkBtn;

    [Header("签字区域提示文字")]
    [Tooltip("签字时显示的姓名（工人签字位置）")]
    [SerializeField] private TMP_Text signHintText;

    [Tooltip("填写时显示的姓名（仓管员填写位置）")]
    [SerializeField] private TMP_Text fillSignNameText;

    [Tooltip("签字按钮下方的固定文本（如声明文字），填写时显示")]
    [SerializeField] private TMP_Text fillSignFixedText;

    #endregion

    #region 公开属性

    /// <summary>用户操作完成后为true，FlowBase通过此属性判断是否继续</summary>
    public bool IsCompleted { get; private set; }

    /// <summary>用户点了退出（非提交/审核），FlowBase据此判断是否重新打开单据</summary>
    public bool WasCancelled { get; private set; }

    /// <summary>面板是否处于打开状态</summary>
    public bool IsOpen => gameObject.activeSelf;

    /// <summary>运行时发货开关，由流程在打开单据前设置</summary>
    public bool AllowShip { get; set; } = false;

    #endregion

    #region 运行时状态

    private Interactables.ActionType _stepAction;
    private List<Interactables.ActionType> _roleButtons = new List<Interactables.ActionType>();
    private BillData _billData;
    private string _currentRole;

    // 保存/恢复：退出后再次进入时保留已填数据
    private string[] _savedInputData;
    private string[][] _savedTableRows;
    private string _savedFillSignName; // 保留仓管员姓名，供工人签字步骤显示
    private bool _hasSavedData = false;
    private bool _hasFilled = false; // 是否点击过"填写"或恢复了已保存数据

    // CanvasGroup：弹窗显示时暂时禁用单据面板的交互
    private CanvasGroup _canvasGroup;

    #endregion

    #region 生命周期

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        AutoFindUnassignedReferences();
        BindButtons();
        HideConfirmPopup();
    }

    private void OnDisable()
    {
        // 面板被隐藏时强制清理横幅和弹窗（防止协程被 Unity 停掉导致残留）
        HideBanner();
        HideConfirmPopup();
        HideAlertPopup();
        HideSignHint();
    }

    /// <summary>
    /// 自动按名称查找所有未在Inspector中绑定的按钮/组件引用。
    /// 解决嵌套prefab无法拖拽引用的问题 — 每个BillView只搜索自己的子层级。
    /// </summary>
    private void AutoFindUnassignedReferences()
    {
        if (fillBtn    == null) fillBtn    = FindButtonInChildren("填写");
        if (saveBtn    == null) saveBtn    = FindButtonInChildren("保存");
        if (submitBtn  == null) submitBtn  = FindButtonInChildren("提交");
        if (approveBtn == null) approveBtn = FindButtonInChildren("审核");
        if (shipBtn    == null) shipBtn    = FindButtonInChildren("发货");
        if (signBtn    == null) signBtn    = FindButtonInChildren("签字");
        if (exitBtn    == null) exitBtn    = FindButtonInChildren("退出");

        if (inputBoxGenerator  == null) inputBoxGenerator  = GetComponentInChildren<InputBoxGenerator>(true);
        if (inputBoxGenerator2 == null) inputBoxGenerator2 = null; // 手动绑定的不覆盖
        if (settingTableGenerator == null) settingTableGenerator = GetComponentInChildren<SettingTableGenerator>(true);

        if (bannerText    == null) bannerText    = FindTextInChildren("Banner");
        if (signHintText == null) signHintText = FindTextInChildren("SignHint");
        if (fillSignNameText == null) fillSignNameText = FindTextInChildren("FillSignName");
        if (fillSignFixedText == null) fillSignFixedText = FindTextInChildren("SignFixed");
        if (confirmPopup  == null) confirmPopup  = FindChildByName("ConfirmPopup");
        if (alertPopup    == null) alertPopup    = FindChildByName("AlertPopup");

        if (confirmPopup != null)
        {
            if (confirmYesBtn == null) confirmYesBtn = FindButtonInChild(confirmPopup.transform, "是");
            if (confirmNoBtn  == null) confirmNoBtn  = FindButtonInChild(confirmPopup.transform, "否");
            if (confirmPopupText == null)
            {
                var t = confirmPopup.GetComponentInChildren<TMP_Text>(true);
                if (t != null && t.gameObject != confirmPopup) confirmPopupText = t;
            }
        }

        if (alertPopup != null)
        {
            if (alertOkBtn == null) alertOkBtn = FindButtonInChild(alertPopup.transform, "确定");
            if (alertPopupText == null)
            {
                var t = alertPopup.GetComponentInChildren<TMP_Text>(true);
                if (t != null && t.gameObject != alertPopup) alertPopupText = t;
            }
        }
    }

    private Button FindButtonInChildren(string nameContains)
    {
        foreach (var btn in GetComponentsInChildren<Button>(true))
            if (btn.name.Contains(nameContains)) return btn;
        return null;
    }

    private Button FindButtonInChild(Transform parent, string nameContains)
    {
        foreach (var btn in parent.GetComponentsInChildren<Button>(true))
            if (btn.name.Contains(nameContains)) return btn;
        return null;
    }

    private TMP_Text FindTextInChildren(string nameContains)
    {
        foreach (var t in GetComponentsInChildren<TMP_Text>(true))
            if (t.name.Contains(nameContains)) return t;
        return null;
    }

    private GameObject FindChildByName(string nameContains)
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
            if (child.name.Contains(nameContains)) return child.gameObject;
        return null;
    }

    private void OnDestroy()
    {
        UnbindButtons();
    }

    #endregion

    #region 公开入口

    /// <summary>
    /// 打开单据。由 FlowBase.WaitForBillComplete 调用。
    /// </summary>
    /// <param name="stepAction">流程步骤的 ActionType</param>
    /// <param name="roleButtons">BillData 返回的角色可见按钮列表</param>
    /// <param name="billData">单据配置数据（弹窗文字等）</param>
    /// <param name="roleName">当前角色名，用于匹配角色预填数据覆盖</param>
    public void Open(Interactables.ActionType stepAction, List<Interactables.ActionType> roleButtons, BillData billData = null, string roleName = null)
    {
        _stepAction = stepAction;
        _roleButtons = roleButtons ?? new List<Interactables.ActionType>();
        _billData = billData;
        _currentRole = roleName;
        IsCompleted = false;
        WasCancelled = false;
        _hasFilled = false;

        HideConfirmPopup();
        HideAlertPopup();
        HideBanner();
        HideSignHint();

        // 计算并应用按钮显隐
        var visibleButtons = ComputeVisibleButtons(_stepAction, _roleButtons);
        ApplyButtonVisibility(visibleButtons);

        // 打开时预显示：固定文本始终显示，仓管员姓名优先用保存值
        if (_billData != null && _billData.showSignHintOnFill)
        {
            ShowFillSignFixedText();
            RestoreFillSignName();
        }

        // 打开时预显示表格预览数据（部分数据，不点填写就能看到）
        ShowPreviewTable();

        // 如果之前保存过数据（退出后重新进入），自动恢复
        if (_hasSavedData)
        {
            RestoreSavedData();
        }

        Debug.Log($"[BillView] 打开单据 | action={_stepAction} | 可见按钮={string.Join(",", visibleButtons)} | hasSaved={_hasSavedData}");
    }

    /// <summary>
    /// 关闭单据
    /// </summary>
    public void Close()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 打开单据时预显示表格预览数据（部分数据），不点填写就能看到
    /// </summary>
    private void ShowPreviewTable()
    {
        if (settingTableGenerator == null || _billData == null) return;
        var previewData = _billData.GetPreviewTableForRole(_currentRole);
        if (previewData == null || previewData.Length == 0) return;

        var columnHeaders = _billData.tableColumnHeaders;
        if (columnHeaders != null && columnHeaders.Length > 0)
            settingTableGenerator.SetColumnHeaders(columnHeaders);

        settingTableGenerator.ClearTable();
        settingTableGenerator.CreateHeaderRow();
        foreach (var row in previewData)
            settingTableGenerator.AddRow(row.columns);
        settingTableGenerator.UpdateContentSize();
        Debug.Log($"[BillView] 预览表格数据已显示 {previewData.Length} 行");
    }

    /// <summary>
    /// 填入预配置数据到输入框和表格（FlowBase 在 Fill 模式下自动调用）
    /// </summary>
    public void FillData(string[] inputData, BillData.TableRow[] tableData = null, string[] columnHeaders = null)
    {
        if (inputBoxGenerator != null && inputData != null && inputData.Length > 0)
        {
            inputBoxGenerator.SetAllInputBoxContents(inputData);
            Debug.Log($"[BillView] 已填入 {inputData.Length} 个输入框数据");
        }

        if (inputBoxGenerator2 != null && inputData != null && inputData.Length > 0)
        {
            inputBoxGenerator2.SetAllInputBoxContents(inputData);
        }

        if (settingTableGenerator != null && tableData != null && tableData.Length > 0)
        {
            if (columnHeaders != null && columnHeaders.Length > 0)
                settingTableGenerator.SetColumnHeaders(columnHeaders);

            settingTableGenerator.ClearTable();
            settingTableGenerator.CreateHeaderRow();
            foreach (var row in tableData)
                settingTableGenerator.AddRow(row.columns);
            settingTableGenerator.UpdateContentSize();
            Debug.Log($"[BillView] 已填入 {tableData.Length} 行表格数据");
        }
    }

    #endregion

    #region 按钮显隐

    /// <summary>
    /// 根据 ActionType 和角色按钮配置，计算最终可见按钮集合
    /// </summary>
    private List<Interactables.ActionType> ComputeVisibleButtons(
        Interactables.ActionType stepAction, List<Interactables.ActionType> roleButtons)
    {
        var result = new List<Interactables.ActionType> { Interactables.ActionType.Exit }; // 始终可用

        switch (stepAction)
        {
            case Interactables.ActionType.Fill:
                result.Add(Interactables.ActionType.Fill);
                result.Add(Interactables.ActionType.Save);
                result.Add(Interactables.ActionType.Submit);
                break;
            case Interactables.ActionType.Approve:
                result.Add(Interactables.ActionType.Approve);
                break;
            case Interactables.ActionType.Pick:
            case Interactables.ActionType.Deliver:
                // Pick/Deliver 模式下显示对应按钮 + 可能发货
                if (roleButtons.Contains(Interactables.ActionType.Ship))
                    result.Add(Interactables.ActionType.Ship);
                break;
            case Interactables.ActionType.View:
                break; // View模式仅 Exit
        }

        // Ship: Fill 模式下角色允许时显示
        if (roleButtons.Contains(Interactables.ActionType.Ship) &&
            stepAction == Interactables.ActionType.Fill)
        {
            result.Add(Interactables.ActionType.Ship);
        }

        // Sign: Approve/View 模式下角色允许时显示
        if (roleButtons.Contains(Interactables.ActionType.Sign) &&
            (stepAction == Interactables.ActionType.Approve || stepAction == Interactables.ActionType.View))
        {
            result.Add(Interactables.ActionType.Sign);
        }

        return result;
    }

    private void ApplyButtonVisibility(List<Interactables.ActionType> visibleButtons)
    {
        SetBtnActive(fillBtn,    visibleButtons.Contains(Interactables.ActionType.Fill));
        SetBtnActive(saveBtn,    visibleButtons.Contains(Interactables.ActionType.Save));
        SetBtnActive(submitBtn,  visibleButtons.Contains(Interactables.ActionType.Submit));
        SetBtnActive(approveBtn, visibleButtons.Contains(Interactables.ActionType.Approve));
        SetBtnActive(shipBtn,    visibleButtons.Contains(Interactables.ActionType.Ship));
        SetBtnActive(signBtn,    visibleButtons.Contains(Interactables.ActionType.Sign));
        SetBtnActive(exitBtn,    visibleButtons.Contains(Interactables.ActionType.Exit));
    }

    private void SetBtnActive(Button btn, bool active)
    {
        if (btn != null) btn.gameObject.SetActive(active);
    }

    #endregion

    #region 按钮事件

    private void BindButtons()
    {
        if (fillBtn != null)    fillBtn.onClick.AddListener(OnFillClicked);
        if (saveBtn != null)    saveBtn.onClick.AddListener(OnSaveClicked);
        if (submitBtn != null)  submitBtn.onClick.AddListener(OnSubmitClicked);
        if (approveBtn != null) approveBtn.onClick.AddListener(OnApproveClicked);
        if (shipBtn != null)    shipBtn.onClick.AddListener(OnShipClicked);
        if (signBtn != null)    signBtn.onClick.AddListener(OnSignClicked);
        if (exitBtn != null)    exitBtn.onClick.AddListener(OnExitClicked);
    }

    private void UnbindButtons()
    {
        if (fillBtn != null)    fillBtn.onClick.RemoveAllListeners();
        if (saveBtn != null)    saveBtn.onClick.RemoveAllListeners();
        if (submitBtn != null)  submitBtn.onClick.RemoveAllListeners();
        if (approveBtn != null) approveBtn.onClick.RemoveAllListeners();
        if (shipBtn != null)    shipBtn.onClick.RemoveAllListeners();
        if (signBtn != null)    signBtn.onClick.RemoveAllListeners();
        if (exitBtn != null)    exitBtn.onClick.RemoveAllListeners();
    }

    private void OnFillClicked()
    {
        // 已有保存的表格数据 >不覆盖
        bool hasSavedTable = _savedTableRows != null && _savedTableRows.Length > 0;
        if (hasSavedTable)
        {
            ShowBanner("已有保存数据，无需重复填写");
            Debug.Log("[BillView] 填写 — 已有保存表格数据，跳过");
            _hasFilled = true;
            return;
        }
        // 从模板填充（优先角色覆盖数据）
        if (_billData != null)
        {
            var inputData = _billData.GetPrefillInputForRole(_currentRole) ?? _billData.prefillInputData;
            var tableData = _billData.GetPrefillTableForRole(_currentRole) ?? _billData.prefillTableData;
            FillData(inputData, tableData, _billData.tableColumnHeaders);

            // 切换上传附件图片
            if (_billData.fillAttachmentSprite != null && attachmentImage != null)
                attachmentImage.sprite = _billData.fillAttachmentSprite;

            Debug.Log("[BillView] 填写 — 已填入预配置数据");
        }
        _hasFilled = true;

        // 部分单据（如领料单、退料入库单）点填写即显示填写位姓名 + 固定文本
        if (_billData != null && _billData.showSignHintOnFill)
        {
            ShowFillSignName();
            ShowFillSignFixedText();
        }

        ShowBanner(_billData != null ? _billData.fillBannerText : "已填写");
        Debug.Log("[BillView] 填写");
    }

    private void OnSaveClicked()
    {
        // 保存当前输入框和表格数据
        _hasSavedData = true;
        _savedInputData = inputBoxGenerator != null
            ? inputBoxGenerator.GetAllContents()
            : null;
        _savedTableRows = settingTableGenerator != null && settingTableGenerator.HasRows
            ? settingTableGenerator.GetAllRowData()
            : null;
        ShowBanner(_billData != null ? _billData.saveBannerText : "已保存成功！");
        Debug.Log($"[BillView] 保存 — inputLen={_savedInputData?.Length}, tableRows={_savedTableRows?.Length}");
    }

    private void OnSubmitClicked()
    {
        if (!_hasFilled && !_hasSavedData)
        {
            ShowAlert("请先点击「填写」录入数据后再提交");
            Debug.Log("[BillView] 提交被拒绝 — 未填写数据");
            return;
        }
        ClearSavedData();
        ShowBanner(_billData != null ? _billData.submitBannerText : "已提交！");
        Debug.Log("[BillView] 提交 — 完成");
        StartCoroutine(DelayedComplete());
    }

    private void OnApproveClicked()
    {
        ShowBanner(_billData != null ? _billData.approveBannerText : "已审核！");
        Debug.Log("[BillView] 审核");

        var confirmText = _billData != null ? _billData.approveConfirmText : "是否下推？";
        var pushDownText = _billData != null ? _billData.pushDownBannerText : "已下推！";

        ShowConfirm(confirmText,
            onYes: () =>
            {
                ClearSavedData();
                ShowBanner(pushDownText);
                Debug.Log("[BillView] 审核 + 下推 — 完成");
                StartCoroutine(DelayedComplete());
            },
            onNo: () =>
            {
                ClearSavedData();
                Debug.Log("[BillView] 审核 — 不下推，完成");
                StartCoroutine(DelayedComplete());
            }
        );
    }

    private void OnShipClicked()
    {
        if (!CheckShipConditions(out string failReason))
        {
            ShowAlert(failReason);
            Debug.Log($"[BillView] 发货失败 — {failReason}");
            return;
        }

        ClearSavedData();
        ShowBanner(_billData != null ? _billData.shipBannerText : "已通知仓库发货");
        Debug.Log("[BillView] 发货 — 完成");
        StartCoroutine(DelayedComplete());
    }

    private void OnSignClicked()
    {
        // 签字时：恢复仓管员姓名 + 固定文本 + 签字位工人姓名
        RestoreFillSignName();
        ShowFillSignFixedText();
        ShowSignHint();
        ClearSavedData();
        _savedFillSignName = null; // 签字完成后清除，不再需要保留
        ShowBanner(_billData != null ? _billData.signBannerText : "已签名！");
        Debug.Log("[BillView] 签名 — 完成");
        StartCoroutine(DelayedComplete());
    }

    /// <summary>延迟完成步骤，让横幅充分展示后再推进流程</summary>
    private System.Collections.IEnumerator DelayedComplete()
    {
        // 禁用按钮防止重复点击
        SetButtonsInteractable(false);
        yield return new WaitForSeconds(1.5f);
        IsCompleted = true;
    }

    private void OnExitClicked()
    {
        // View 步骤：退出即视为完成，3 秒后自动推进流程
        if (_stepAction == Interactables.ActionType.View)
        {
            Debug.Log("[BillView] 查看完成，3s后自动推进");
            StartCoroutine(DelayedViewComplete());
            return;
        }

        Debug.Log("[BillView] 退出（不完成步骤）");
        WasCancelled = true;
        gameObject.SetActive(false);
    }

    /// <summary>View 步骤退出后延迟 3 秒完成</summary>
    private System.Collections.IEnumerator DelayedViewComplete()
    {
        SetButtonsInteractable(false);
        yield return new WaitForSeconds(3f);
        IsCompleted = true;
    }

    /// <summary>恢复已保存的数据到输入框和表格</summary>
    private void RestoreSavedData()
    {
        if (!_hasSavedData) return;
        _hasFilled = true; // 恢复数据视为已填写
        Debug.Log("[BillView] 恢复已保存数据");
        if (_savedInputData != null && inputBoxGenerator != null)
            inputBoxGenerator.SetAllInputBoxContents(_savedInputData);
        if (_savedTableRows != null && settingTableGenerator != null && _savedTableRows.Length > 0)
        {
            settingTableGenerator.ClearTable();
            settingTableGenerator.CreateHeaderRow();
            foreach (var row in _savedTableRows)
                settingTableGenerator.AddRow(row);
            settingTableGenerator.UpdateContentSize();
        }
    }

    private void ClearSavedData()
    {
        _hasSavedData = false;
        _hasFilled = false;
        _savedInputData = null;
        _savedTableRows = null;
        // 注意：_savedFillSignName 不在这里清除，需跨步骤保留到签字完成
    }

    /// <summary>
    /// 发货条件检查。子类可重写以实现业务逻辑。
    /// </summary>
    protected virtual bool CheckShipConditions(out string failReason)
    {
        // 运行时开关（流程控制）或 BillData 静态配置，任一允许即可
        if (AllowShip || (_billData != null && _billData.shipAllowed))
        {
            failReason = "";
            return true;
        }
        failReason = _billData != null ? _billData.shipFailAlertText : "条件不满足，无法发货";
        return false;
    }

    #endregion

    #region 反馈系统

    private Coroutine _bannerCoroutine;

    private void ShowBanner(string message)
    {
        if (bannerText == null) return;
        if (_bannerCoroutine != null) StopCoroutine(_bannerCoroutine);
        bannerText.text = message;
        bannerText.gameObject.SetActive(true);
        _bannerCoroutine = StartCoroutine(AutoHideBanner(3f));
    }

    private System.Collections.IEnumerator AutoHideBanner(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bannerText != null) bannerText.gameObject.SetActive(false);
        _bannerCoroutine = null;
    }

    private void HideBanner()
    {
        if (_bannerCoroutine != null)
        {
            StopCoroutine(_bannerCoroutine);
            _bannerCoroutine = null;
        }
        if (bannerText != null) bannerText.gameObject.SetActive(false);
    }

    /// <summary>显示填写位的姓名（如仓管员）。若无独立 FillSignName 则回退到 SignHint。同时保存供后续步骤恢复</summary>
    private void ShowFillSignName()
    {
        var target = fillSignNameText != null ? fillSignNameText : signHintText;
        if (target == null)
        {
            Debug.LogWarning("[BillView] ShowFillSignName: fillSignNameText 和 signHintText 都为 null，请检查预制体中是否有命名为 FillSignName 或 SignHint 的 TMP_Text");
            return;
        }
        var hint = _billData != null ? _billData.GetSignHintForRole(_currentRole) : "";
        Debug.Log($"[BillView] ShowFillSignName: target={target.name}, role={_currentRole}, hint={hint}");
        if (!string.IsNullOrEmpty(hint))
        {
            target.text = hint;
            target.gameObject.SetActive(true);
            _savedFillSignName = hint;
        }
        else
        {
            Debug.LogWarning($"[BillView] ShowFillSignName: 角色 '{_currentRole}' 的 signHintText 为空，请在 BillData 的 RolePrefillOverrides 中配置");
        }
    }

    /// <summary>恢复已保存的仓管员姓名（用于工人签字步骤打开时）</summary>
    private void RestoreFillSignName()
    {
        if (string.IsNullOrEmpty(_savedFillSignName)) return;
        var target = fillSignNameText != null ? fillSignNameText : signHintText;
        if (target == null) return;
        target.text = _savedFillSignName;
        target.gameObject.SetActive(true);
    }

    /// <summary>显示签字位的姓名（如工人），签字按钮点击时调用</summary>
    private void ShowSignHint()
    {
        if (signHintText == null) return;
        var hint = _billData != null ? _billData.GetSignHintForRole(_currentRole) : "";
        if (!string.IsNullOrEmpty(hint))
        {
            signHintText.text = hint;
            signHintText.gameObject.SetActive(true);
        }
    }

    /// <summary>显示签字按钮下方的固定文本（声明文字），填写时显示</summary>
    private void ShowFillSignFixedText()
    {
        if (fillSignFixedText == null || _billData == null) return;
        if (!string.IsNullOrEmpty(_billData.fillSignFixedContent))
        {
            fillSignFixedText.text = _billData.fillSignFixedContent;
            fillSignFixedText.gameObject.SetActive(true);
        }
    }

    /// <summary>隐藏签字区域所有提示文字</summary>
    private void HideSignHint()
    {
        if (signHintText != null) signHintText.gameObject.SetActive(false);
        if (fillSignNameText != null) fillSignNameText.gameObject.SetActive(false);
        if (fillSignFixedText != null) fillSignFixedText.gameObject.SetActive(false);
    }

    private void ShowConfirm(string message, Action onYes, Action onNo)
    {
        if (confirmPopup == null)
        {
            Debug.LogWarning("[BillView] 确认弹窗未配置，直接执行「否」");
            onNo?.Invoke();
            return;
        }

        // 确保父级链都激活
        var t = confirmPopup.transform.parent;
        while (t != null)
        {
            if (!t.gameObject.activeSelf)
            {
                Debug.LogWarning($"[BillView] 激活弹窗父级: {t.name}");
                t.gameObject.SetActive(true);
            }
            t = t.parent;
        }

        // 暂时禁用单据面板的交互，让弹窗可以接收点击
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        confirmPopup.SetActive(true);
        Debug.Log($"[BillView] 确认弹窗已激活: {confirmPopup.name}, activeInHierarchy={confirmPopup.activeInHierarchy}");

        if (confirmPopupText != null) confirmPopupText.text = message;

        if (confirmYesBtn != null)
        {
            confirmYesBtn.onClick.RemoveAllListeners();
            confirmYesBtn.onClick.AddListener(() =>
            {
                Debug.Log($"[BillView] 确认弹窗 — 点击了「是/同意」");
                HideConfirmPopup();
                onYes?.Invoke();
            });
            Debug.Log($"[BillView] 确认弹窗 — 「是」按钮已绑定, interactable={confirmYesBtn.interactable}, raycastTarget={confirmYesBtn.targetGraphic?.raycastTarget}, active={confirmYesBtn.gameObject.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("[BillView] 确认弹窗 — confirmYesBtn 为 null！请在 Inspector 拖拽引用或确保子物体中有 Button");
        }

        if (confirmNoBtn != null)
        {
            confirmNoBtn.onClick.RemoveAllListeners();
            confirmNoBtn.onClick.AddListener(() =>
            {
                Debug.Log($"[BillView] 确认弹窗 — 点击了「否/拒绝」");
                HideConfirmPopup();
                onNo?.Invoke();
            });
            Debug.Log($"[BillView] 确认弹窗 — 「否」按钮已绑定, interactable={confirmNoBtn.interactable}, raycastTarget={confirmNoBtn.targetGraphic?.raycastTarget}, active={confirmNoBtn.gameObject.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("[BillView] 确认弹窗 — confirmNoBtn 为 null！请在 Inspector 拖拽引用或确保子物体中有 Button");
        }

        SetButtonsInteractable(false);
        Debug.Log("[BillView] 确认弹窗 — 已禁用主面板按钮");
    }

    private void HideConfirmPopup()
    {
        if (confirmPopup != null) confirmPopup.SetActive(false);
        // 恢复单据面板的交互
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
        SetButtonsInteractable(true);
    }

    /// <summary>显示警告弹窗（单按钮"确定"），点确定后自动关闭</summary>
    private void ShowAlert(string message)
    {
        if (alertPopup == null)
        {
            Debug.LogWarning($"[BillView] 警告弹窗未配置，消息: {message}");
            return;
        }

        if (alertPopupText != null) alertPopupText.text = message;

        if (alertOkBtn != null)
        {
            alertOkBtn.onClick.RemoveAllListeners();
            alertOkBtn.onClick.AddListener(HideAlertPopup);
        }

        alertPopup.SetActive(true);
        SetButtonsInteractable(false);
    }

    /// <summary>隐藏警告弹窗</summary>
    private void HideAlertPopup()
    {
        if (alertPopup != null) alertPopup.SetActive(false);
        SetButtonsInteractable(true);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (fillBtn != null)    fillBtn.interactable = interactable;
        if (saveBtn != null)    saveBtn.interactable = interactable;
        if (submitBtn != null)  submitBtn.interactable = interactable;
        if (approveBtn != null) approveBtn.interactable = interactable;
        if (shipBtn != null)    shipBtn.interactable = interactable;
        if (signBtn != null)    signBtn.interactable = interactable;
        if (exitBtn != null)    exitBtn.interactable = interactable;
    }

    #endregion
}
