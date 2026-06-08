using UnityEngine;
using System.Collections;

/// <summary>
/// 流程测试类型枚举（仅在 Enable Test Mode 时生效）
/// </summary>
public enum TestFlowType
{
    CustomSales,
    CustomProduction,
    CustomPurchase,
    StandardSales,
    StandardSalesBranch,
    StandardProduction,
    StandardPurchase,
    StandardDelivery
}

/// <summary>
/// 流程启动器
/// 在场景加载完成后自动启动流程（根据 DemandManager 的流程类型动态选择）
/// 挂在场景中的 FlowStarter 空物体上
/// </summary>
public class FlowStarter : MonoBehaviour
{
    [Header("流程配置")]
    [Tooltip("是否在启动时自动开始流程")]
    public bool autoStartFlow = true;

    [Tooltip("延迟启动时间（秒）")]
    public float startDelay = 2f;

    [Header("测试模式")]
    [Tooltip("开启后绕过 DemandManager，直接启动下方选择的流程")]
    public bool enableTestMode = false;

    [Tooltip("选择要测试的流程")]
    public TestFlowType testFlowType = TestFlowType.CustomSales;

    private void Start()
    {
        if (autoStartFlow)
        {
            StartCoroutine(StartFlowAfterDelay());
        }
    }

    private IEnumerator StartFlowAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
        
        // 根据测试模式或 DemandManager 决定要启动的流程
        System.Type flowType = GetFlowType();

        Debug.Log($"[FlowStarter] 当前流程类型: {flowType.Name} (测试模式: {enableTestMode})");
        
        // 创建流程实例
        FlowBase flow = System.Activator.CreateInstance(flowType) as FlowBase;
        
        if (flow != null)
        {
            // 注册流程到 FlowManager
            if (FlowManager.Instance != null)
            {
                FlowManager.Instance.RegisterFlow(flow);
            }
            
            // 通过 FlowTaskIntegration 启动（会设置 currentFlow，让 CompleteStep() 能工作）
            if (FlowTaskIntegration.Instance != null)
            {
                FlowTaskIntegration.Instance.StartFlowWithUI(flow);
            }
            else
            {
                // 如果 FlowTaskIntegration 不存在，直接启动流程
                flow.StartFlow();
            }
            
            Debug.Log($"[FlowStarter] 已启动流程: {flowType.Name}");
        }
        else
        {
            Debug.LogError($"[FlowStarter] 无法创建流程实例: {flowType.Name}");
        }
    }

    /// <summary>
    /// 手动启动流程（供按钮等调用）
    /// </summary>
    public void StartFlow()
    {
        StartCoroutine(StartFlowAfterDelay());
    }

    /// <summary>
    /// 启动指定类型的流程
    /// </summary>
    public void StartFlow(System.Type type)
    {
        StartCoroutine(StartFlowWithType(type));
    }

    private IEnumerator StartFlowWithType(System.Type flowType)
    {
        yield return new WaitForSeconds(startDelay);
        
        FlowBase flow = System.Activator.CreateInstance(flowType) as FlowBase;
        if (flow != null)
        {
            if (FlowManager.Instance != null)
            {
                FlowManager.Instance.RegisterFlow(flow);
            }
            if (FlowTaskIntegration.Instance != null)
            {
                FlowTaskIntegration.Instance.StartFlowWithUI(flow);
            }
            else
            {
                flow.StartFlow();
            }
            Debug.Log($"[FlowStarter] 已启动流程: {flowType.Name}");
        }
    }

    /// <summary>
    /// 获取要启动的流程类型
    /// 测试模式下直接返回枚举对应的类型，否则走 DemandManager
    /// </summary>
    private System.Type GetFlowType()
    {
        // 测试模式：绕过 DemandManager，直接返回指定流程
        if (enableTestMode)
        {
            return GetTestFlowType();
        }

        // 正常模式：根据 DemandManager 的流程类型
        if (DemandManager.Instance != null)
        {
            var demand = DemandManager.Instance.GetCurrentDemand();
            if (demand != null)
            {
                switch (demand.workflowType)
                {
                    case DemandManager.WorkflowType.Standard:
                        return typeof(StandardSalesFlow);
                    case DemandManager.WorkflowType.Custom:
                        return typeof(CustomSalesFlow);
                    default:
                        Debug.LogWarning("[FlowStarter] 未知流程类型，默认使用定制流程");
                        return typeof(CustomSalesFlow);
                }
            }
        }

        // 默认返回定制流程（保持向后兼容）
        Debug.LogWarning("[FlowStarter] 未找到 DemandManager，默认使用定制流程");
        return typeof(CustomSalesFlow);
    }

    /// <summary>
    /// 根据 TestFlowType 枚举返回对应的 FlowBase 子类
    /// </summary>
    private System.Type GetTestFlowType()
    {
        switch (testFlowType)
        {
            case TestFlowType.CustomSales:         return typeof(CustomSalesFlow);
            case TestFlowType.CustomProduction:    return typeof(CustomProductionFlow);
            case TestFlowType.CustomPurchase:      return typeof(CustomPurchaseFlow);
            case TestFlowType.StandardSales:       return typeof(StandardSalesFlow);
            case TestFlowType.StandardSalesBranch: return typeof(StandardSalesBranchFlow);
            case TestFlowType.StandardProduction:  return typeof(StandardProductionFlow);
            case TestFlowType.StandardPurchase:    return typeof(StandardPurchaseFlow);
            case TestFlowType.StandardDelivery:    return typeof(StandardDeliveryFlow);
            default:
                Debug.LogWarning($"[FlowStarter] 未知测试流程类型: {testFlowType}，回退到 CustomSalesFlow");
                return typeof(CustomSalesFlow);
        }
    }
}
