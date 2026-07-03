using UnityEngine;

/// <summary>
/// 质检触发器：挂载在质检相关NPC上（仓管员B等）
/// 当前流程步骤匹配时，锁定玩家、切换相机到质检机前、播放质检动画
/// 不匹配时透传给正常交互
/// </summary>
public class QualityInspectionTrigger : MonoBehaviour
{
    [Header("=== 质检类型 ===")]
    [Tooltip("成品质检: compressor / 原料质检: rawMaterial")]
    public string inspectionType = "compressor";

    [Header("=== 触发条件（步骤匹配） ===")]
    [Tooltip("触发质检的步骤名称关键词（当前步骤stepName包含此关键词即触发）")]
    public string stepNameKeyword = "质检";

    [Header("=== 引用 ===")]
    [Tooltip("质检管理器（场景中的 QualityInspectionManager 组件）")]
    public QualityInspectionManager inspectionManager;

    private Interactables _interactable;

    private void Awake()
    {
        _interactable = GetComponent<Interactables>();
    }

    /// <summary>
    /// 判断当前流程步骤是否应该触发质检动画，并自动判断质检类型
    /// </summary>
    public bool ShouldTriggerInspection()
    {
        var flow = GetEffectiveFlow();
        if (flow == null) return false;

        string stepName = GetCurrentStepName(flow);
        if (string.IsNullOrEmpty(stepName)) return false;

        bool match = stepName.Contains(stepNameKeyword);
        if (match)
        {
            // 根据步骤名自动判断质检类型
            if (stepName.Contains("退料") || stepName.Contains("原料"))
                inspectionType = "rawMaterial";
            else if (stepName.Contains("成品") || stepName.Contains("完工"))
                inspectionType = "compressor";

            Debug.Log($"[QualityInspectionTrigger] 步骤匹配: {stepName} → 触发质检, type={inspectionType}");
        }

        return match;
    }

    /// <summary>
    /// 触发质检动画
    /// </summary>
    public void TriggerInspection(System.Action onComplete)
    {
        if (inspectionManager == null)
        {
            Debug.LogError("[QualityInspectionTrigger] inspectionManager 未绑定！");
            onComplete?.Invoke();
            return;
        }

        if (AnimationService.Instance == null)
        {
            Debug.LogError("[QualityInspectionTrigger] AnimationService 实例不存在！");
            onComplete?.Invoke();
            return;
        }

        AnimationService.Instance.PlayInspectionAnimation(inspectionType, inspectionManager, onComplete);
    }

    #region 从当前流程获取步骤名

    private FlowBase GetEffectiveFlow()
    {
        var flow = FlowTaskIntegration.Instance?.GetCurrentFlow();
        if (flow == null) return null;

        if (flow is StandardSalesFlow || flow is CustomSalesFlow)
        {
            var field = flow.GetType().GetField("_currentBranchFlow",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var branch = field.GetValue(flow) as FlowBase;
                if (branch != null && branch.IsRunning) return branch;
            }
        }

        return flow;
    }

    private string GetCurrentStepName(FlowBase flow)
    {
        if (flow is CustomProductionFlow customProdFlow)
            return customProdFlow.GetCurrentStep()?.stepName;
        else if (flow is StandardProductionFlow standardProdFlow)
            return standardProdFlow.GetCurrentStep()?.stepName;
        else if (flow is CustomSalesFlow customSalesFlow)
            return customSalesFlow.GetCurrentStep()?.stepName;
        else if (flow is StandardSalesFlow standardSalesFlow)
            return standardSalesFlow.GetCurrentStep()?.stepName;
        else if (flow is StandardDeliveryFlow deliveryFlow)
            return deliveryFlow.GetCurrentStep()?.stepName;
        else if (flow is StandardSalesBranchFlow salesBranchFlow)
            return salesBranchFlow.GetCurrentStep()?.stepName;
        else if (flow is CustomPurchaseFlow customPurchaseFlow)
            return customPurchaseFlow.GetCurrentStep()?.stepName;
        else if (flow is StandardPurchaseFlow purchaseFlow)
            return purchaseFlow.GetCurrentStep()?.stepName;
        else if (flow is ProductionDeptPurchaseFlow prodDeptPurchaseFlow)
            return prodDeptPurchaseFlow.GetCurrentStep()?.stepName;
        return null;
    }

    #endregion
}
