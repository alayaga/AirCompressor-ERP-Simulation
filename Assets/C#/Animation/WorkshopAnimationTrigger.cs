using UnityEngine;

/// <summary>
/// 车间动画触发器：挂载在车间工人NPC上（与 Interactables 组件并列）
/// 仅在当前流程步骤匹配时截获交互、播放动画；不匹配时透传给正常交互
/// </summary>
public class WorkshopAnimationTrigger : MonoBehaviour
{
    [Header("=== 动画标识 ===")]
    [Tooltip("车间ID，对应 AnimationService 中的序列：1-弯管 / 2-焊接 / 3-配电 / 4-总装")]
    public string workshopId = "1-弯管";

    [Header("=== 触发条件（步骤匹配） ===")]
    [Tooltip("触发动画的目标NPC名（与Flow步骤中的 targetNPC 精确匹配）")]
    public string expectedNPCName = "";
    [Tooltip("触发动画的目标位置（与Flow步骤中的 targetLocation 精确匹配）")]
    public string expectedLocation = "";

    private Interactables _interactable;

    private void Awake()
    {
        _interactable = GetComponent<Interactables>();
    }

    /// <summary>
    /// 判断当前流程步骤是否应该触发动画
    /// 条件：当前步骤的 targetNPC == expectedNPCName 且 targetLocation == expectedLocation
    /// </summary>
    public bool ShouldTriggerAnimation()
    {
        var flow = GetEffectiveFlow();
        Debug.Log($"[WAT-DEBUG] GetEffectiveFlow 结果: {(flow != null ? flow.GetType().Name : "null")}");
        if (flow == null) return false;

        string currentNPC = GetCurrentStepTargetNPC(flow);
        string currentLocation = GetCurrentStepTargetLocation(flow);
        Debug.Log($"[WAT-DEBUG] 当前步骤: NPC='{currentNPC}', Location='{currentLocation}'");
        Debug.Log($"[WAT-DEBUG] 期望匹配: NPC='{expectedNPCName}', Location='{expectedLocation}'");

        bool match = !string.IsNullOrEmpty(currentNPC)
                  && !string.IsNullOrEmpty(currentLocation)
                  && currentNPC == expectedNPCName
                  && currentLocation == expectedLocation;

        Debug.Log($"[WAT-DEBUG] 匹配结果: {match}");

        if (match)
            Debug.Log($"[WorkshopAnimationTrigger] 步骤匹配成功: NPC={currentNPC}, Location={currentLocation} → 触发动画 {workshopId}");

        return match;
    }

    /// <summary>
    /// 触发动画
    /// </summary>
    /// <param name="onComplete">动画完成回调（调用 CompleteStep）</param>
    public void TriggerAnimation(System.Action onComplete)
    {
        if (AnimationService.Instance == null)
        {
            Debug.LogError("[WorkshopAnimationTrigger] AnimationService 实例不存在！");
            onComplete?.Invoke();
            return;
        }

        AnimationService.Instance.PlayWorkshopAnimation(workshopId, onComplete);
    }

    #region 从当前流程获取步骤信息

    /// <summary>
    /// 获取"实际活跃"的流程：主流程有活跃分支时返回分支，否则返回主流程
    /// </summary>
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

    /// <summary>
    /// 获取当前步骤的目标NPC
    /// </summary>
    private string GetCurrentStepTargetNPC(FlowBase flow)
    {
        if (flow is CustomProductionFlow customProdFlow)
        {
            var step = customProdFlow.GetCurrentStep();
            return step?.targetNPC;
        }
        else if (flow is StandardProductionFlow standardProdFlow)
        {
            var step = standardProdFlow.GetCurrentStep();
            return step?.targetNPC;
        }
        else if (flow is CustomSalesFlow customSalesFlow)
        {
            var step = customSalesFlow.GetCurrentStep();
            return step?.targetNPC;
        }
        else if (flow is StandardSalesFlow standardSalesFlow)
        {
            var step = standardSalesFlow.GetCurrentStep();
            return step?.targetNPC;
        }
        else if (flow is StandardDeliveryFlow deliveryFlow)
        {
            var step = deliveryFlow.GetCurrentStep();
            return step?.targetNPC;
        }
        else if (flow is StandardSalesBranchFlow salesBranchFlow)
        {
            var step = salesBranchFlow.GetCurrentStep();
            return step?.targetNPC;
        }
        else if (flow is CustomPurchaseFlow customPurchaseFlow)
        {
            var step = customPurchaseFlow.GetCurrentStep();
            return step?.targetNPC;
        }
        else if (flow is StandardPurchaseFlow purchaseFlow)
        {
            var step = purchaseFlow.GetCurrentStep();
            return step?.targetNPC;
        }
        else if (flow is ProductionDeptPurchaseFlow prodDeptPurchaseFlow)
        {
            var step = prodDeptPurchaseFlow.GetCurrentStep();
            return step?.targetNPC;
        }
        return null;
    }

    /// <summary>
    /// 获取当前步骤的目标位置
    /// </summary>
    private string GetCurrentStepTargetLocation(FlowBase flow)
    {
        if (flow is CustomProductionFlow customProdFlow)
        {
            var step = customProdFlow.GetCurrentStep();
            return step?.targetLocation;
        }
        else if (flow is StandardProductionFlow standardProdFlow)
        {
            var step = standardProdFlow.GetCurrentStep();
            return step?.targetLocation;
        }
        else if (flow is CustomSalesFlow customSalesFlow)
        {
            var step = customSalesFlow.GetCurrentStep();
            return step?.targetLocation;
        }
        else if (flow is StandardSalesFlow standardSalesFlow)
        {
            var step = standardSalesFlow.GetCurrentStep();
            return step?.targetLocation;
        }
        else if (flow is StandardDeliveryFlow deliveryFlow)
        {
            var step = deliveryFlow.GetCurrentStep();
            return step?.targetLocation;
        }
        else if (flow is StandardSalesBranchFlow salesBranchFlow)
        {
            var step = salesBranchFlow.GetCurrentStep();
            return step?.targetLocation;
        }
        else if (flow is CustomPurchaseFlow customPurchaseFlow)
        {
            var step = customPurchaseFlow.GetCurrentStep();
            return step?.targetLocation;
        }
        else if (flow is StandardPurchaseFlow purchaseFlow)
        {
            var step = purchaseFlow.GetCurrentStep();
            return step?.targetLocation;
        }
        else if (flow is ProductionDeptPurchaseFlow prodDeptPurchaseFlow)
        {
            var step = prodDeptPurchaseFlow.GetCurrentStep();
            return step?.targetLocation;
        }
        return null;
    }

    #endregion
}
