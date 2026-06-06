using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 销售分支流程（销售计划部分）
/// 从销售计划到PMC计划的流程
/// </summary>
public class StandardSalesBranchFlow : FlowBase
{
    [System.Serializable]
    public class StepData
    {
        public string stepName;
        public string description;
        public string targetNPC;
        public string targetLocation;
        public Interactables.ActionType actionType;
        public UIManager.UIType? billType;  // 对应单据类型，null=无单据
        public StepData(string name, string desc, string npc, string location, Interactables.ActionType action, UIManager.UIType? billType = null)
        {
            stepName = name;
            description = desc;
            targetNPC = npc;
            targetLocation = location;
            actionType = action;
            this.billType = billType;
        }
    }
    
    private const string FLOW_NAME = "销售流程";
    private const string TASK_TITLE = "销售-PMC流程";
    private const string TASK_DESCRIPTION = "完成销售计划到PMC计划的流程";
    
    private Queue<StepData> _steps = new Queue<StepData>();
    private StepData _currentStep;
    private bool _isStepCompleted = false;
    private int _totalSteps;
    private int _currentStepIndex = 0;

    protected override IEnumerator FlowCoroutine()
    {
        Debug.Log("[StandardSalesBranchFlow] 开始销售分支流程");

        InitializeSteps();
        _totalSteps = _steps.Count;

        ShowTaskInfoToUI();

        while (_steps.Count > 0)
        {
            _currentStep = _steps.Dequeue();
            _isStepCompleted = false;
            _currentStepIndex++;

            Debug.Log($"[步骤 {_currentStepIndex}/{_totalSteps}] {_currentStep.stepName}");

            ShowCurrentStepToUI();

            Debug.Log($"[FLOW STEP] {_currentStep.stepName} | billType={_currentStep.billType} | NPC={_currentStep.targetNPC}");
            // 判断是否有单据交互
            if (_currentStep.billType != null)
            {
                yield return WaitForBillComplete(_currentStep.billType.Value, _currentStep.targetNPC);

                if (!_isStepCompleted) yield return new WaitUntil(() => _isStepCompleted);
            }
            else
            {
                // 手动步骤：等待玩家完成（通过按E触发 CompleteStep）
                yield return new WaitUntil(() => _isStepCompleted);
            }

            Debug.Log($"[完成] {_currentStep.stepName}");

            UpdateProgressToUI();

            yield return new WaitForSeconds(0.5f);
        }

        ShowTaskCompleteToUI();
        Debug.Log("[StandardSalesBranchFlow] 销售分支流程完成！");
    }

    private void InitializeSteps()
    {
        _steps.Clear();

        _steps.Enqueue(new StepData("填写销售计划", "销售总监填写下月月度销售计划；点：提交", "销售总监", "销售办公室", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("查看销售计划", "PMC查看订单、库存、在制、在途等情况", "PMC主管", "计划物控中心", Interactables.ActionType.View));
        _steps.Enqueue(new StepData("回复交期", "PMC回复销售产品交期", "PMC主管", "计划物控中心", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("制作生产计划", "PMC制作一周生产计划；点：提交；生产主管可查看", "PMC主管", "计划物控中心", Interactables.ActionType.Fill, UIManager.UIType.WeeklyProductionPlan));
        _steps.Enqueue(new StepData("制作采购计划", "PMC制作两周采购计划；点：提交；采购主管可查看", "PMC主管", "计划物控中心", Interactables.ActionType.Fill));
    }

    #region UI更新方法

    private void ShowTaskInfoToUI()
    {
        if (TaskGuidePanelNew.Instance != null)
        {
            TaskGuidePanelNew.Instance.UpdateTaskInfo(FLOW_NAME, TASK_TITLE, TASK_DESCRIPTION, _currentStepIndex, _totalSteps);
        }
    }

    private void ShowCurrentStepToUI()
    {
        if (TaskGuidePanelNew.Instance != null && _currentStep != null)
        {
            string actionText = GetActionText(_currentStep.actionType);
            TaskGuidePanelNew.Instance.UpdateCurrentStep(
                _currentStep.stepName,
                _currentStep.description,
                _currentStep.targetNPC,
                _currentStep.targetLocation,
                actionText
            );
        }
    }

    private void UpdateProgressToUI()
    {
        if (TaskGuidePanelNew.Instance != null)
        {
            TaskGuidePanelNew.Instance.UpdateProgress(_currentStepIndex, _totalSteps);
        }
    }

    private void ShowTaskCompleteToUI()
    {
        if (TaskGuidePanelNew.Instance != null)
        {
            TaskGuidePanelNew.Instance.CompleteTask();
        }
    }

    private string GetActionText(Interactables.ActionType action)
    {
        switch (action)
        {
            case Interactables.ActionType.Fill: return "填写";
            case Interactables.ActionType.Approve: return "审核";
            case Interactables.ActionType.PushDown: return "下推";
            case Interactables.ActionType.View: return "查看";
            case Interactables.ActionType.Pick: return "领取";
            case Interactables.ActionType.Deliver: return "交付";
            default: return "操作";
        }
    }

    #endregion

    #region 公共方法

    public override void MarkStepComplete()
    {
        Debug.Log($"[StandardSalesBranchFlow] MarkStepComplete 被调用！");
        _isStepCompleted = true;
    }

    public StepData GetCurrentStep()
    {
        return _currentStep;
    }

    public int GetTotalSteps()
    {
        return _totalSteps;
    }

    public int GetCurrentStepIndex()
    {
        return _currentStepIndex;
    }

    #endregion
}
