using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QuoteFormManager : MonoBehaviour
{
    #region 成员变量
    [Header("UI组件引用")]
    [SerializeField]
    private InputBoxGenerator inputBoxGenerator; // 输入框生成器引用
    
    [SerializeField]
    private SettingTableGenerator settingTableGenerator; // 设置表格生成器引用
    
    [Header("按钮引用")]
    [SerializeField]
    private Button fillButton; // 填写按钮
    
    [SerializeField]
    private Button submitButton; // 提交按钮
    
    [SerializeField]
    private Button approveButton; // 审核按钮
    
    [SerializeField]
    private Button pushButton; // 下推按钮
    
    #region 按钮点击状态标志
    private bool _isFillButtonClicked = false;
    private bool _isSubmitButtonClicked = false;
    private bool _isApproveButtonClicked = false;
    private bool _isPushButtonClicked = false;
    #endregion
    #endregion
    
    #region 生命周期方法
    private void Awake()
    {
        // 添加按钮点击事件监听器
        if (fillButton != null)
        {
            fillButton.onClick.AddListener(OnFillButtonClicked);
        }
        
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitButtonClicked);
        }
        
        if (approveButton != null)
        {
            approveButton.onClick.AddListener(OnApproveButtonClicked);
        }
        
        if (pushButton != null)
        {
            pushButton.onClick.AddListener(OnPushButtonClicked);
        }
    }
    
    private void OnDestroy()
    {
        // 移除按钮点击事件监听器，避免内存泄漏
        if (fillButton != null)
        {
            fillButton.onClick.RemoveListener(OnFillButtonClicked);
        }
        
        if (submitButton != null)
        {
            submitButton.onClick.RemoveListener(OnSubmitButtonClicked);
        }
        
        if (approveButton != null)
        {
            approveButton.onClick.RemoveListener(OnApproveButtonClicked);
        }
        
        if (pushButton != null)
        {
            pushButton.onClick.RemoveListener(OnPushButtonClicked);
        }
    }
    #endregion
    
    #region 公共方法
    public void SetQuoteFormData(string[] inputBoxContents, string[][] tableRowData)
    {
        // 检查引用是否有效
        if (inputBoxGenerator == null)
        {
            Debug.LogError("InputBoxGenerator引用未设置");
            return;
        }
        
        if (settingTableGenerator == null)
        {
            Debug.LogError("SettingTableGenerator引用未设置");
            return;
        }
        
        // 设置输入框内容
        if (inputBoxContents != null && inputBoxContents.Length > 0)
        {
            inputBoxGenerator.SetAllInputBoxContents(inputBoxContents);
        }
        
        // 设置表格数据
        if (tableRowData != null && tableRowData.Length > 0)
        {
            // 首先清空表格中的默认行（除了表头）
            settingTableGenerator.ClearTable();
            settingTableGenerator.CreateHeaderRow();
            
            // 添加新的行数据
            foreach (string[] rowData in tableRowData)
            {
                settingTableGenerator.AddRow(rowData);
            }
        }
    }
    
    #region 按钮状态检查方法
    /// <summary>
    /// 获取填写按钮的点击状态
    /// </summary>
    public bool IsFillButtonClicked() { return _isFillButtonClicked; }
    
    /// <summary>
    /// 获取提交按钮的点击状态
    /// </summary>
    public bool IsSubmitButtonClicked() { return _isSubmitButtonClicked; }
    
    /// <summary>
    /// 获取审核按钮的点击状态
    /// </summary>
    public bool IsApproveButtonClicked() { return _isApproveButtonClicked; }
    
    /// <summary>
    /// 获取下推按钮的点击状态
    /// </summary>
    public bool IsPushButtonClicked() { return _isPushButtonClicked; }
    
    /// <summary>
    /// 重置填写按钮点击状态
    /// </summary>
    public void ResetFillButtonClicked() { _isFillButtonClicked = false; }
    
    /// <summary>
    /// 重置提交按钮点击状态
    /// </summary>
    public void ResetSubmitButtonClicked() { _isSubmitButtonClicked = false; }
    
    /// <summary>
    /// 重置审核按钮点击状态
    /// </summary>
    public void ResetApproveButtonClicked() { _isApproveButtonClicked = false; }
    
    /// <summary>
    /// 重置下推按钮点击状态
    /// </summary>
    public void ResetPushButtonClicked() { _isPushButtonClicked = false; }
    #endregion
    
    #region 按钮协程方法
    /// <summary>
    /// 等待填写按钮点击 - 供协程调用
    /// </summary>
    public IEnumerator WaitForFillButtonClick()
    {
        Debug.Log("等待填写按钮点击...");
        ResetFillButtonClicked();
        
        while (!_isFillButtonClicked)
        {
            yield return null;
        }
        
        Debug.Log("检测到填写按钮点击，继续执行");
    }
    
    /// <summary>
    /// 等待提交按钮点击 - 供协程调用
    /// </summary>
    public IEnumerator WaitForSubmitButtonClick()
    {
        Debug.Log("等待提交按钮点击...");
        ResetSubmitButtonClicked();
        
        while (!_isSubmitButtonClicked)
        {
            yield return null;
        }
        
        Debug.Log("检测到提交按钮点击，继续执行");
    }
    
    /// <summary>
    /// 等待审核按钮点击 - 供协程调用
    /// </summary>
    public IEnumerator WaitForApproveButtonClick()
    {
        Debug.Log("等待审核按钮点击...");
        ResetApproveButtonClicked();
        
        while (!_isApproveButtonClicked)
        {
            yield return null;
        }
        
        Debug.Log("检测到审核按钮点击，继续执行");
    }
    
    /// <summary>
    /// 等待下推按钮点击 - 供协程调用
    /// </summary>
    public IEnumerator WaitForPushButtonClick()
    {
        Debug.Log("等待下推按钮点击...");
        ResetPushButtonClicked();
        
        while (!_isPushButtonClicked)
        {
            yield return null;
        }
        
        Debug.Log("检测到下推按钮点击，继续执行");
    }
    #endregion
    #endregion
    
    #region 按钮事件处理
    private void OnFillButtonClicked()
    {
        _isFillButtonClicked = true;
        Debug.Log("填写按钮被点击");
    }
    
    private void OnSubmitButtonClicked()
    {
        _isSubmitButtonClicked = true;
        Debug.Log("提交按钮被点击");
    }
    
    private void OnApproveButtonClicked()
    {
        _isApproveButtonClicked = true;
        Debug.Log("审核按钮被点击");
    }
    
    private void OnPushButtonClicked()
    {
        _isPushButtonClicked = true;
        Debug.Log("下推按钮被点击");
    }
    #endregion
}