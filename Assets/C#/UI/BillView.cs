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

    #endregion

    #region 公开属性

    /// <summary>用户操作完成后为true，FlowBase通过此属性判断是否继续</summary>
    public bool IsCompleted { get; private set; }

    /// <summary>面板是否处于打开状态</summary>
    public bool IsOpen => gameObject.activeSelf;

    #endregion

    #region 运行时状态

    private Interactables.ActionType _stepAction;
    private List<Interactables.ActionType> _roleButtons = new List<Interactables.ActionType>();
    private BillData _billData;

    #endregion

    #region 生命周期

    private void Awake()
    {
        AutoFindUnassignedReferences();
        BindButtons();
        HideConfirmPopup();
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
    public void Open(Interactables.ActionType stepAction, List<Interactables.ActionType> roleButtons, BillData billData = null)
    {
        _stepAction = stepAction;
        _roleButtons = roleButtons ?? new List<Interactables.ActionType>();
        _billData = billData;
        IsCompleted = false;

        HideConfirmPopup();
        HideAlertPopup();
        HideBanner();

        // 计算并应用按钮显隐
        var visibleButtons = ComputeVisibleButtons(_stepAction, _roleButtons);
        ApplyButtonVisibility(visibleButtons);

        Debug.Log($"[BillView] 打开单据 | action={_stepAction} | 可见按钮={string.Join(",", visibleButtons)}");
    }

    /// <summary>
    /// 关闭单据
    /// </summary>
    public void Close()
    {
        gameObject.SetActive(false);
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
        ShowBanner(_billData != null ? _billData.fillBannerText : "已填写");
        Debug.Log("[BillView] 填写");
    }

    private void OnSaveClicked()
    {
        ShowBanner(_billData != null ? _billData.saveBannerText : "已保存成功！");
        Debug.Log("[BillView] 保存");
    }

    private void OnSubmitClicked()
    {
        ShowBanner(_billData != null ? _billData.submitBannerText : "已提交！");
        Debug.Log("[BillView] 提交 — 完成");
        IsCompleted = true;
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
                ShowBanner(pushDownText);
                Debug.Log("[BillView] 审核 + 下推 — 完成");
                IsCompleted = true;
            },
            onNo: () =>
            {
                Debug.Log("[BillView] 审核 — 不下推，完成");
                IsCompleted = true;
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

        ShowBanner(_billData != null ? _billData.shipBannerText : "已通知仓库发货");
        Debug.Log("[BillView] 发货 — 完成");
        IsCompleted = true;
    }

    private void OnSignClicked()
    {
        ShowBanner(_billData != null ? _billData.signBannerText : "已签名！");
        Debug.Log("[BillView] 签名 — 完成");
        IsCompleted = true;
    }

    private void OnExitClicked()
    {
        Debug.Log("[BillView] 退出");
        IsCompleted = true;
    }

    /// <summary>
    /// 发货条件检查。子类可重写以实现业务逻辑。
    /// 默认直接通过。
    /// </summary>
    protected virtual bool CheckShipConditions(out string failReason)
    {
        failReason = null;
        return true;
    }

    #endregion

    #region 反馈系统

    private void ShowBanner(string message)
    {
        if (bannerText == null) return;
        bannerText.text = message;
        bannerText.gameObject.SetActive(true);
    }

    private void HideBanner()
    {
        if (bannerText != null) bannerText.gameObject.SetActive(false);
    }

    private void ShowConfirm(string message, Action onYes, Action onNo)
    {
        if (confirmPopup == null)
        {
            Debug.LogWarning("[BillView] 确认弹窗未配置，直接执行「否」");
            onNo?.Invoke();
            return;
        }

        if (confirmPopupText != null) confirmPopupText.text = message;

        if (confirmYesBtn != null)
        {
            confirmYesBtn.onClick.RemoveAllListeners();
            confirmYesBtn.onClick.AddListener(() =>
            {
                HideConfirmPopup();
                onYes?.Invoke();
            });
        }
        if (confirmNoBtn != null)
        {
            confirmNoBtn.onClick.RemoveAllListeners();
            confirmNoBtn.onClick.AddListener(() =>
            {
                HideConfirmPopup();
                onNo?.Invoke();
            });
        }

        confirmPopup.SetActive(true);
        SetButtonsInteractable(false);
    }

    private void HideConfirmPopup()
    {
        if (confirmPopup != null) confirmPopup.SetActive(false);
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
