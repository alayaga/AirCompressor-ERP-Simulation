using UnityEngine;
using System.Collections;

/// <summary>
/// 流程步骤基类：统一生命周期，减少重复代码
/// </summary>
public abstract class FlowStepBase
{
    public string StepName { get; }

    protected FlowStepBase(string name) => StepName = name;

    public abstract IEnumerator Execute(FlowContext context);

    protected virtual void OnEnter(FlowContext ctx) { }
    protected virtual void OnExit(FlowContext ctx) { }

    protected IEnumerator WaitForPlayerReachPosition(Vector3 targetPos)
    {
        var player = ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player);
        while (Vector3.Distance(player.transform.position, targetPos) > 0.5f)
        {
            yield return null;
        }
    }
}