using System;
using System.Collections.Generic;

/// <summary>
/// 流程上下文：传递步骤数据，避免全局单例依赖
/// 类型安全、易于测试、可扩展
/// </summary>
public class FlowContext
{
    public DemandManager.CustomerDemand Demand { get; set; }

    // 删除 Tracker 属性（FlowStepTracker 是静态类，不需要实例引用）
    // 直接使用 FlowStepTracker.CompleteStep() 调用即可

    public Dictionary<string, object> ExtraData { get; } = new Dictionary<string, object>();

    public T GetData<T>(string key, T defaultValue = default)
    {
        if (ExtraData.TryGetValue(key, out var value) && value is T typed)
            return typed;
        return defaultValue;
    }
}

/// <summary>
/// 步骤事件总线：解耦步骤完成通知
/// </summary>
public static class StepEventBus
{
    public static event Action<int, string> OnStepCompleted;

    public static void NotifyStepComplete(int stepIndex, string stepName)
    {
        OnStepCompleted?.Invoke(stepIndex, stepName);
        FlowTaskIntegration.Instance?.CompleteCurrentStep();
    }
}

/// <summary>
/// 扩展方法：带上下文的步骤完成
/// </summary>
public static class FlowStepTrackerExtensions
{
    public static void CompleteStep(this FlowContext ctx, string stepName = "")
    {
        // 直接调用静态方法
        FlowStepTracker.CompleteStep();
        StepEventBus.NotifyStepComplete(-1, stepName);
    }
}