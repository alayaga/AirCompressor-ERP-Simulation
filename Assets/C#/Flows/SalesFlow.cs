using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 销售流程（重构版）
/// 保留原有变量名、协程安全调度、步骤抽象、DTO数据模型
/// </summary>
public class SalesFlow : FlowBase
{
    private FlowRunner _flowRunner;
    private FlowContext _currentContext;

    protected override IEnumerator FlowCoroutine()
    {
        Debug.Log("销售流程开始");

        _flowRunner = GetSafeRunner();
        _allowSafeStop = true;

        var demand = DemandManager.Instance?.GetCurrentDemand();
        if (demand == null)
        {
            Debug.LogError("无法获取客户需求数据，流程终止");
            yield break;
        }

        _currentContext = new FlowContext
        {
            Demand = demand
        };

        var steps = new List<FlowStepBase>
        {
            new QuoteFormStep(demand),
            new ContractStep(demand),
            new SalesOrderStep()
        };

        foreach (var step in steps)
        {
            if (!_flowRunner.IsRunning) yield break;

            yield return _flowRunner.RunSafe(
                step.Execute(_currentContext)
            );
        }

        yield return new WaitForSeconds(1f);
        FinishFlow();
    }

    public override void StartFlow()
    {
        Debug.Log("销售流程开始");
        base.StartFlow();
        this.InitTaskUI();
        _allowSafeStop = true;
    }

    public override void StopFlow()
    {
        Debug.Log("销售流程停止");
        base.StopFlow();
    }

    protected override void FinishFlow()
    {
        Debug.Log("销售流程结束");
        base.FinishFlow();
    }
}