using UnityEngine;
using TMPro;

/// <summary>
/// 分支选择面板控制器
/// 显示分支选择选项并处理玩家输入
/// </summary>
public class BranchChoicePanel : MonoBehaviour
{
    #region 单例
    private static BranchChoicePanel _instance;
    public static BranchChoicePanel Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<BranchChoicePanel>();
            return _instance;
        }
    }
    #endregion

    [Header("UI组件")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text salesOptionText;
    [SerializeField] private TMP_Text deliveryOptionText;
    [SerializeField] private GameObject panel;

    private StandardSalesFlow _currentMainFlow;

    private void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) Destroy(gameObject);
        
        HidePanel();
    }

    private void Update()
    {
        Debug.Log($"panel={panel}, active={panel?.activeSelf}, flow={_currentMainFlow != null}");
        Debug.Log($"[BranchChoicePanel] panel.activeSelf = {panel?.activeSelf ?? false}, _currentMainFlow = {_currentMainFlow != null}");
    
        if (panel.activeSelf && _currentMainFlow != null)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("[BranchChoicePanel] 检测到按键1");
                SelectSalesBranch();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("[BranchChoicePanel] 检测到按键2");
                SelectDeliveryBranch();
            }
        }
    }

    /// <summary>
    /// 显示分支选择面板
    /// </summary>
    public void ShowPanel(StandardSalesFlow mainFlow)
    {
        _currentMainFlow = mainFlow;
        
        if (panel != null) panel.SetActive(true);
        
        // 更新选项状态
        bool salesCompleted = mainFlow.IsSalesBranchCompleted();
        bool deliveryCompleted = mainFlow.IsDeliveryBranchCompleted();
        
        if (titleText != null)
            titleText.text = "选择流程分支";
        
        if (descriptionText != null)
            descriptionText.text = "请选择要完成的流程分支";
        
        if (salesOptionText != null)
        {
            salesOptionText.text = salesCompleted 
                ? "[1] 销售流程 ✓ 已完成" 
                : "[1] 销售流程 - 销售计划与PMC确认";
            salesOptionText.color = salesCompleted ? Color.green : Color.white;
        }
        
        if (deliveryOptionText != null)
        {
            deliveryOptionText.text = deliveryCompleted 
                ? "[2] 发货流程 ✓ 已完成" 
                : "[2] 发货流程 - 订单处理与发货";
            deliveryOptionText.color = deliveryCompleted ? Color.green : Color.white;
        }
        
        Debug.Log("[BranchChoicePanel] 显示分支选择面板");
    }

    /// <summary>
    /// 隐藏分支选择面板
    /// </summary>
    public void HidePanel()
    {
        if (panel != null) panel.SetActive(false);
        _currentMainFlow = null;
    }

    /// <summary>
    /// 选择销售流程分支
    /// </summary>
    public void SelectSalesBranch()
    {
        if (_currentMainFlow != null && !_currentMainFlow.IsSalesBranchCompleted())
        {
            Debug.Log("[BranchChoicePanel] 选择销售流程分支");
            _currentMainFlow.SelectBranch(1);
            HidePanel();
        }
        else if (_currentMainFlow != null && _currentMainFlow.IsSalesBranchCompleted())
        {
            Debug.LogWarning("[BranchChoicePanel] 销售流程已完成");
        }
    }

    /// <summary>
    /// 选择发货流程分支
    /// </summary>
    public void SelectDeliveryBranch()
    {
        if (_currentMainFlow != null && !_currentMainFlow.IsDeliveryBranchCompleted())
        {
            Debug.Log("[BranchChoicePanel] 选择发货流程分支");
            _currentMainFlow.SelectBranch(2);
            HidePanel();
        }
        else if (_currentMainFlow != null && _currentMainFlow.IsDeliveryBranchCompleted())
        {
            Debug.LogWarning("[BranchChoicePanel] 发货流程已完成");
        }
    }

    /// <summary>
    /// 检查是否需要显示分支选择
    /// </summary>
    public bool NeedsBranchChoice()
    {
        if (_currentMainFlow == null) return false;
        
        bool salesDone = _currentMainFlow.IsSalesBranchCompleted();
        bool deliveryDone = _currentMainFlow.IsDeliveryBranchCompleted();
        
        // 如果两个分支都未完成或只完成了一个，需要选择
        return !salesDone || !deliveryDone;
    }
}
