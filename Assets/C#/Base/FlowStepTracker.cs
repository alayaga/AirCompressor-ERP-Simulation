using UnityEngine;
using System.Collections;

/// <summary>
/// 流程步骤追踪器
/// </summary>
public static class FlowStepTracker
{
    public static void CompleteStep()
    {
        Debug.Log($"[FlowStepTracker] CompleteStep 被调用！");
        Debug.Log($"[FlowStepTracker] FlowTaskIntegration.Instance 是否为空: {FlowTaskIntegration.Instance == null}");
        
        FlowTaskIntegration.Instance?.CompleteCurrentStep();
    }

    public static void StartFlow(FlowBase flow)
    {
        FlowTaskIntegration.Instance?.StartFlowWithUI(flow);
    }
}

/// <summary>
/// FlowBase扩展方法
/// </summary>
public static class FlowBaseExtensions
{
    public static void InitTaskUI(this FlowBase flow)
    {
        FlowStepTracker.StartFlow(flow);
    }

    public static IEnumerator CompleteStepWithUI(this FlowBase flow, float delay = 0.3f)
    {
        FlowStepTracker.CompleteStep();
        yield return new WaitForSeconds(delay);
    }
}