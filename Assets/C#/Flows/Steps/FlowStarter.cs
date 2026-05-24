using UnityEngine;
using System.Collections;

/// <summary>
/// 流程启动器
/// 在场景加载完成后自动启动定制产品销售流程
/// 挂在场景中的 FlowStarter 空物体上
/// </summary>
public class FlowStarter : MonoBehaviour
{
    [Header("流程配置")]
    [Tooltip("是否在启动时自动开始流程")]
    public bool autoStartFlow = true;

    [Tooltip("要启动的流程类型")]
    public System.Type flowType = typeof(CustomSalesFlow);

    [Tooltip("延迟启动时间（秒）")]
    public float startDelay = 2f;

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
        flowType = type;
        StartFlow();
    }
}
