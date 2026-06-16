using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 标准产品生产流程
/// 从PMC排产到成品入库的完整流程
/// </summary>
public class StandardProductionFlow : FlowBase
{
    /// <summary>
    /// 步骤信息结构
    /// </summary>
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

    // 流程信息
    private const string FLOW_NAME = "标准产品生产流程";
    private const string TASK_TITLE = "生产流程";
    private const string TASK_DESCRIPTION = "完成从PMC排产到成品入库的完整流程";

    // 步骤队列
    private Queue<StepData> _steps = new Queue<StepData>();
    private StepData _currentStep;
    private bool _isStepCompleted = false;
    private int _totalSteps;
    private int _currentStepIndex = 0;

        protected override IEnumerator FlowCoroutine()
    {
        Debug.Log("[StandardProductionFlow] 开始标准产品生产流程");

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

            if (_currentStep.targetNPC == "系统")
            {
                Debug.Log($"[StandardProductionFlow] 系统步骤自动完成: {_currentStep.stepName}");
                yield return new WaitForSeconds(3f);
                _isStepCompleted = true;
            }
            else
            {
                yield return new WaitUntil(() => _isStepCompleted);
            }

            if (_currentStep.billType != null)
            {
                bool stepDone = false;
                while (!stepDone)
                {
                    _isStepCompleted = false;
                    yield return WaitForBillComplete(_currentStep.billType.Value, _currentStep.targetNPC);
                    if (_isStepCompleted)
                        stepDone = true;
                    else
                        yield return new WaitUntil(() => _isStepCompleted);
                }
            }

            Debug.Log($"[完成] {_currentStep.stepName}");
            UpdateProgressToUI();
            yield return new WaitForSeconds(0.5f);
        }

        ShowTaskCompleteToUI();
        Debug.Log("[StandardProductionFlow] 标准产品生产流程完成！");
    }    private void InitializeSteps()
    {
        _steps.Clear();

        // ===== 标准产品生产流程 v1.3 =====
        // 从PMC制作一周生产计划开始，到完工入库签字结束

        // 阶段1：计划与排产
        _steps.Enqueue(new StepData("制作一周生产计划", "PMC制作一周生产计划；点：提交；生产主管可查看", "PMC主管", "PMC部", Interactables.ActionType.Fill, UIManager.UIType.WeeklyProductionPlan));
        _steps.Enqueue(new StepData("PMC填写每日排产单", "PMC填写每日排产单给仓库仓管员和车间主管；点：提交；自动下推给仓管员（生产用料清单）和车间主管（生产工单）", "PMC主管", "PMC部", Interactables.ActionType.Fill, UIManager.UIType.ProductionSchedule));

        // 阶段2：发料与派工
        _steps.Enqueue(new StepData("仓管员发料到备料区", "仓管员按生产用料清单发料到备料区（位于生产区）", "仓管员", "备料区", Interactables.ActionType.Deliver));
        _steps.Enqueue(new StepData("车间主管派工", "车间主管按PMC下推的生产工单（已含工序信息）直接派工给班组长；1/2/3/4车间班组长可查看自己车间的生产工单", "车间主管", "生产部", Interactables.ActionType.Fill, UIManager.UIType.ProductionWorkOrder));
        _steps.Enqueue(new StepData("班组长填写派工单", "1/2/3/4车间班组长查看自己车间的生产工单；填写工人个人的派工单；点：提交", "车间班组长", "生产区", Interactables.ActionType.Fill, UIManager.UIType.DispatchOrder));

        // 阶段3：领料与生产
        _steps.Enqueue(new StepData("工人到备料区领料", "工人查看自己的派工单，根据派工单到备料区领料", "工人", "备料区", Interactables.ActionType.Pick));
        _steps.Enqueue(new StepData("工人填写领料单", "工人领料；填写领料单；点：提交", "工人", "备料区", Interactables.ActionType.Fill, UIManager.UIType.PickList));
        _steps.Enqueue(new StepData("工人生产", "工人进行生产加工", "工人", "生产区", Interactables.ActionType.View));

        // 阶段4：退料分支（与生产并行）
        _steps.Enqueue(new StepData("退料送仓库", "工人将生产多余物料退回仓库", "工人", "仓库", Interactables.ActionType.Deliver));
        _steps.Enqueue(new StepData("仓管员质检确认退料", "仓管员对退料进行质检确认", "仓管员", "仓库", Interactables.ActionType.Approve));
        _steps.Enqueue(new StepData("仓管员制退料入库单", "仓管员制作生产退料入库单；点：提交", "仓管员", "仓库", Interactables.ActionType.Fill, UIManager.UIType.ProductionReturn));
        _steps.Enqueue(new StepData("工人签字确认退料", "工人在生产退料入库单上签字", "工人", "仓库", Interactables.ActionType.Approve, UIManager.UIType.ProductionReturn));

        // 阶段5：工序汇报与转运
        _steps.Enqueue(new StepData("工序汇报", "生产完成后1/2/3车间班组长检查；填写工序汇报单；点：提交", "车间班组长", "生产区", Interactables.ActionType.Fill, UIManager.UIType.ProcessReport));
        _steps.Enqueue(new StepData("半成品转运", "1/2/3车间员工将半成品送往4车间", "工人", "生产区", Interactables.ActionType.View));
        _steps.Enqueue(new StepData("4车间组装", "4车间负责组装，组装完成后由4车间工人将成品送往仓库", "工人", "生产区", Interactables.ActionType.View));

        // 阶段6：质检与入库
        _steps.Enqueue(new StepData("仓管员质检确认成品", "仓管员对成品进行质检确认", "仓管员", "质检区", Interactables.ActionType.Approve));
        _steps.Enqueue(new StepData("填写完工入库单", "仓管员填写完工入库单；点：提交；工人可查看", "仓管员", "质检区", Interactables.ActionType.Fill, UIManager.UIType.FinishedInbound));
        _steps.Enqueue(new StepData("工人签字确认入库", "工人在完工入库单上签字", "工人", "仓库", Interactables.ActionType.Approve, UIManager.UIType.FinishedInbound));
    }

    #region UI更新方法

    /// <summary>
    /// 显示流程信息到UI
    /// </summary>
    private void ShowTaskInfoToUI()
    {
        if (TaskGuidePanelNew.Instance != null)
        {
            TaskGuidePanelNew.Instance.UpdateTaskInfo(FLOW_NAME, TASK_TITLE, TASK_DESCRIPTION, _currentStepIndex, _totalSteps);
        }
    }

    /// <summary>
    /// 显示当前步骤到UI
    /// </summary>
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

    /// <summary>
    /// 更新进度到UI
    /// </summary>
    private void UpdateProgressToUI()
    {
        if (TaskGuidePanelNew.Instance != null)
        {
            TaskGuidePanelNew.Instance.UpdateProgress(_currentStepIndex, _totalSteps);
        }
    }

    /// <summary>
    /// 显示任务完成到UI
    /// </summary>
    private void ShowTaskCompleteToUI()
    {
        if (TaskGuidePanelNew.Instance != null)
        {
            TaskGuidePanelNew.Instance.CompleteTask();
        }
    }

    /// <summary>
    /// 获取操作类型文字
    /// </summary>
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

    /// <summary>
    /// 完成当前步骤（重写基类方法，由 InteractionManager 调用）
    /// </summary>
    public override void MarkStepComplete()
    {
        Debug.Log($"[StandardProductionFlow] MarkStepComplete 被调用！_isStepCompleted 设置为 true");
        _isStepCompleted = true;
    }

    /// <summary>
    /// 获取当前步骤信息
    /// </summary>
    public StepData GetCurrentStep()
    {
        return _currentStep;
    }

    /// <summary>
    /// 获取总步骤数
    /// </summary>
    public int GetTotalSteps()
    {
        return _totalSteps;
    }

    /// <summary>
    /// 获取当前步骤索引（从1开始）
    /// </summary>
    public int GetCurrentStepIndex()
    {
        return _currentStepIndex;
    }

    #endregion
}