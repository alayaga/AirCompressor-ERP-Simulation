using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 表单管理器
/// </summary>
public class FinishedProductOutboundManager : MonoBehaviour
{
    [Header("输入框生成器")]
    [SerializeField] private InputBoxGenerator inputBoxGenerator;
    [SerializeField] private InputBoxGenerator inputBoxGenerator2;
    
    [Header("表格生成器")]
    [SerializeField] private SettingTableGenerator settingTableGenerator;
    
    [Header("按钮")]
    [SerializeField] private Button fillButton;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button approveButton;
    [SerializeField] private Button pushButton;
    
    private bool _isFillButtonClicked = false;
    private bool _isSubmitButtonClicked = false;
    private bool _isApproveButtonClicked = false;
    private bool _isPushButtonClicked = false;
    
    private void Awake()
    {
        if (fillButton != null) fillButton.onClick.AddListener(() => _isFillButtonClicked = true);
        if (submitButton != null) submitButton.onClick.AddListener(() => _isSubmitButtonClicked = true);
        if (approveButton != null) approveButton.onClick.AddListener(() => _isApproveButtonClicked = true);
        if (pushButton != null) pushButton.onClick.AddListener(() => _isPushButtonClicked = true);
    }
    
    private void OnDestroy()
    {
        if (fillButton != null) fillButton.onClick.RemoveAllListeners();
        if (submitButton != null) submitButton.onClick.RemoveAllListeners();
        if (approveButton != null) approveButton.onClick.RemoveAllListeners();
        if (pushButton != null) pushButton.onClick.RemoveAllListeners();
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
    
    public IEnumerator WaitForFillButtonClick()
    {
        ResetFillButtonClicked();
        while (!_isFillButtonClicked) yield return null;
    }
    
    public IEnumerator WaitForSubmitButtonClick()
    {
        ResetSubmitButtonClicked();
        while (!_isSubmitButtonClicked) yield return null;
    }
    
    public IEnumerator WaitForApproveButtonClick()
    {
        ResetApproveButtonClicked();
        while (!_isApproveButtonClicked) yield return null;
    }
    
    public IEnumerator WaitForPushButtonClick()
    {
        ResetPushButtonClicked();
        while (!_isPushButtonClicked) yield return null;
    }
}
