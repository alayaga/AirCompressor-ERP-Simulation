using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 表单管理器 — 支持按角色动态显示按钮组
/// </summary>
public class FinishedProductOutboundManager : MonoBehaviour
{
    [Header("输入框生成器")]
    [SerializeField] private InputBoxGenerator inputBoxGenerator;
    [SerializeField] private InputBoxGenerator inputBoxGenerator2;

    [Header("表格生成器")]
    [SerializeField] private SettingTableGenerator settingTableGenerator;

    [Header("原按钮")]
    [SerializeField] private Button fillButton;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button approveButton;
    [SerializeField] private Button pushButton;

    [Header("新增按钮")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button shipButton;
    [SerializeField] private Button signButton;

    private bool _isFillButtonClicked = false;
    private bool _isSubmitButtonClicked = false;
    private bool _isApproveButtonClicked = false;
    private bool _isPushButtonClicked = false;
    private bool _isCompleted = false;
    public bool IsCompleted => _isCompleted;

    private void Awake()
    {
        if (fillButton != null) fillButton.onClick.AddListener(OnFillClick);
        if (submitButton != null) submitButton.onClick.AddListener(OnSubmitClick);
        if (approveButton != null) approveButton.onClick.AddListener(OnApproveClick);
        if (pushButton != null) pushButton.onClick.AddListener(OnPushClick);
        if (saveButton != null) saveButton.onClick.AddListener(OnSaveClick);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitClick);
        if (shipButton != null) shipButton.onClick.AddListener(OnShipClick);
        if (signButton != null) signButton.onClick.AddListener(OnSignClick);
    }

    private void OnDestroy()
    {
        if (fillButton != null) fillButton.onClick.RemoveAllListeners();
        if (submitButton != null) submitButton.onClick.RemoveAllListeners();
        if (approveButton != null) approveButton.onClick.RemoveAllListeners();
        if (pushButton != null) pushButton.onClick.RemoveAllListeners();
        if (saveButton != null) saveButton.onClick.RemoveAllListeners();
        if (exitButton != null) exitButton.onClick.RemoveAllListeners();
        if (shipButton != null) shipButton.onClick.RemoveAllListeners();
        if (signButton != null) signButton.onClick.RemoveAllListeners();
    }

    #region 按钮事件
    private void OnFillClick()
    {
        if (!_isModified)
        {
            _isModified = true;
            CaptureOriginalData();
        }
        _isFillButtonClicked = true;
        Debug.Log("[表单] 填写 — 已解锁编辑");
    }

    private void OnSaveClick()
    {
        _isSaved = true;
        _isModified = false;
        Debug.Log("[表单] 保存 — 数据已暂存");
    }

    private void OnSubmitClick()
    {
        _isSubmitButtonClicked = true;
        _isCompleted = true;
        Debug.Log("[表单] 提交");
    }

    private void OnApproveClick()
    {
        _isApproveButtonClicked = true;
        _isCompleted = true;
        Debug.Log("[表单] 审核");
    }

    private void OnPushClick()
    {
        _isPushButtonClicked = true;
        _isCompleted = true;
        Debug.Log("[表单] 下推");
    }

    private void OnExitClick()
    {
        // 修改过但未保存 → 还原数据
        if (_isModified && !_isSaved)
        {
            Debug.Log("[表单] 退出 — 未保存，数据已还原");
            RevertData();
        }
        else
        {
            Debug.Log("[表单] 退出");
        }
        _isCompleted = true;
    }

    private void OnShipClick()
    {
        _isCompleted = true;
        Debug.Log("[表单] 发货");
    }

    private void OnSignClick()
    {
        _isCompleted = true;
        Debug.Log("[表单] 签名");
    }
    #endregion

    #region 数据追踪
    private bool _isModified = false;
    private bool _isSaved = false;
    private string[] _originalData1;
    private string[] _originalData2;
    private System.Collections.Generic.List<string[]> _originalTableData;

    private void CaptureOriginalData()
    {
        // 从 InputBoxGenerator 中读取当前内容作为原始数据
        _originalData1 = inputBoxGenerator?.GetAllContents();
        _originalData2 = inputBoxGenerator2?.GetAllContents();
        Debug.Log("[表单] 已捕获原始数据快照");
    }

    private void RevertData()
    {
        // 还原输入框
        if (inputBoxGenerator != null && _originalData1 != null)
            inputBoxGenerator.SetAllInputBoxContents(_originalData1);
        if (inputBoxGenerator2 != null && _originalData2 != null)
            inputBoxGenerator2.SetAllInputBoxContents(_originalData2);
        Debug.Log("[表单] 数据已还原到修改前状态");
    }
    #endregion

    /// <summary>
    /// 按角色配置显示/隐藏按钮组
    /// </summary>
    public void ConfigureButtons(List<Interactables.ActionType> visibleButtons)
    {
        SetBtn(saveButton,    visibleButtons.Contains(Interactables.ActionType.Save));
        SetBtn(submitButton,  visibleButtons.Contains(Interactables.ActionType.Submit));
        SetBtn(fillButton,    visibleButtons.Contains(Interactables.ActionType.Fill));
        SetBtn(exitButton,    visibleButtons.Contains(Interactables.ActionType.Exit));
        SetBtn(shipButton,    visibleButtons.Contains(Interactables.ActionType.Ship));
        SetBtn(approveButton, visibleButtons.Contains(Interactables.ActionType.Approve));
        SetBtn(signButton,    visibleButtons.Contains(Interactables.ActionType.Sign));
        SetBtn(pushButton,    false); // 下推按钮默认隐藏
        _isCompleted = false;
    }

    private void SetBtn(Button btn, bool show)
    {
        if (btn != null) btn.gameObject.SetActive(show);
    }

    public void SetQuoteFormData(string[] data1, string[] data2, string[][] tableData)
    {
        if (inputBoxGenerator == null || inputBoxGenerator2 == null || settingTableGenerator == null)
        {
            Debug.LogError("表单组件引用未设置");
            return;
        }

        if (data1 != null && data1.Length > 0)
            inputBoxGenerator.SetAllInputBoxContents(data1);

        if (data2 != null && data2.Length > 0)
            inputBoxGenerator2.SetAllInputBoxContents(data2);

        if (tableData != null && tableData.Length > 0)
        {
            settingTableGenerator.ClearTable();
            settingTableGenerator.CreateHeaderRow();
            foreach (string[] rowData in tableData)
                settingTableGenerator.AddRow(rowData);
        }
    }

    public bool IsFillButtonClicked() => _isFillButtonClicked;
    public bool IsSubmitButtonClicked() => _isSubmitButtonClicked;
    public bool IsApproveButtonClicked() => _isApproveButtonClicked;
    public bool IsPushButtonClicked() => _isPushButtonClicked;

    public void ResetFillButtonClicked() => _isFillButtonClicked = false;
    public void ResetSubmitButtonClicked() => _isSubmitButtonClicked = false;
    public void ResetApproveButtonClicked() => _isApproveButtonClicked = false;
    public void ResetPushButtonClicked() => _isPushButtonClicked = false;

    public IEnumerator WaitForFillButtonClick()   { ResetFillButtonClicked(); while (!_isFillButtonClicked) yield return null; }
    public IEnumerator WaitForSubmitButtonClick() { ResetSubmitButtonClicked(); while (!_isSubmitButtonClicked) yield return null; }
    public IEnumerator WaitForApproveButtonClick(){ ResetApproveButtonClicked(); while (!_isApproveButtonClicked) yield return null; }
    public IEnumerator WaitForPushButtonClick()   { ResetPushButtonClicked(); while (!_isPushButtonClicked) yield return null; }
}
