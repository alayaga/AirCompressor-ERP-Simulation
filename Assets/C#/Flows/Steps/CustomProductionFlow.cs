using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 定制产品生产流程（共57步）
/// 从PMC查看销售订单到产品入库通知销售员的完整生产流程
/// 车间顺序：1弯管 → 2焊接 → 3配电 → 4总装（流水线依赖）
/// </summary>
public class CustomProductionFlow : FlowBase
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
        public UIManager.UIType? billType;

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
    private const string FLOW_NAME = "定制产品生产流程";
    private const string TASK_TITLE = "生产流程";
    private const string TASK_DESCRIPTION = "完成从PMC排产到成品入库的完整生产流程";

    // 步骤队列
    private Queue<StepData> _steps = new Queue<StepData>();
    private StepData _currentStep;
    private bool _isStepCompleted = false;
    private int _totalSteps;
    private int _currentStepIndex = 0;

    protected override IEnumerator FlowCoroutine()
    {
        Debug.Log("[CustomProductionFlow] 开始定制产品生产流程");

        // 初始化步骤队列
        InitializeSteps();
        _totalSteps = _steps.Count;

        // 显示流程信息到UI
        ShowTaskInfoToUI();

        // 执行所有步骤
        while (_steps.Count > 0)
        {
            _currentStep = _steps.Dequeue();
            _isStepCompleted = false;
            _currentStepIndex++;

            Debug.Log($"[步骤 {_currentStepIndex}/{_totalSteps}] {_currentStep.stepName}");

            // 显示当前步骤到UI
            ShowCurrentStepToUI();

            // 先等待玩家交互（系统步骤自动完成）
            if (_currentStep.targetNPC == "系统")
            {
                Debug.Log($"[CustomProductionFlow] 系统步骤自动完成: {_currentStep.stepName}");
                yield return new WaitForSeconds(3f);
                _isStepCompleted = true;
            }
            else
            {
                // 等待玩家走到NPC前按E
                yield return new WaitUntil(() => _isStepCompleted);
            }

            // 交互完成后，如果步骤关联了单据，再打开单据面板
            if (_currentStep.billType != null)
            {
                bool stepDone = false;
                while (!stepDone)
                {
                    _isStepCompleted = false;
                    yield return WaitForBillComplete(_currentStep.billType.Value, _currentStep.targetNPC, _currentStep.actionType);
                    if (_isStepCompleted)
                        stepDone = true;
                    else
                        yield return new WaitUntil(() => _isStepCompleted);
                }
            }

            Debug.Log($"[完成] {_currentStep.stepName}");

            // 更新进度到UI
            UpdateProgressToUI();

            yield return new WaitForSeconds(0.5f);
        }

        // 显示流程完成
        ShowTaskCompleteToUI();
        Debug.Log("[CustomProductionFlow] 定制产品生产流程完成！");
    }

    /// <summary>
    /// 初始化所有步骤（共57步）
    /// </summary>
    private void InitializeSteps()
    {
        _steps.Clear();

        // ========================================================
        // 阶段1: PMC计划阶段（步骤1~4）
        // ========================================================

        _steps.Enqueue(new StepData(
            "PMC查看销售订单",
            "PMC查看销售订单内容，了解定制产品需求",
            "PMC主管",
            "计划物控中心",
            Interactables.ActionType.View,
            UIManager.UIType.SalesOrder
        ));

        _steps.Enqueue(new StepData(
            "制定一周生产计划",
            "制定一周生产计划；填：一周生产计划；点：提交",
            "PMC主管",
            "计划物控中心",
            Interactables.ActionType.Fill,
            UIManager.UIType.WeeklyProductionPlan
        ));

        _steps.Enqueue(new StepData(
            "PMC填写每日排产单",
            "填写每日排产单；点：提交。自动下推给仓库仓管员（生产用料清单）和车间主管（生产工单）",
            "PMC主管",
            "计划物控中心",
            Interactables.ActionType.Fill,
            UIManager.UIType.ProductionSchedule
        ));

        _steps.Enqueue(new StepData(
            "仓管员发料到备料区",
            "仓管员按生产用料清单发料到备料区（位于生产区）",
            "仓管员A",
            "备料区",
            Interactables.ActionType.Deliver
        ));

        // ========================================================
        // 阶段2: 1车间 — 弯管车间（步骤5~15）
        // ========================================================

        _steps.Enqueue(new StepData(
            "1车间主管填写生产工单",
            "填写1车间（弯管车间）生产工单",
            "1车间主管",
            "1车间-弯管",
            Interactables.ActionType.Fill,
            UIManager.UIType.ProductionWorkOrder
        ));

        _steps.Enqueue(new StepData(
            "1车间班组长填写派工单",
            "查看1车间生产工单；填写工人个人的派工单",
            "1车间班组长",
            "1车间-弯管",
            Interactables.ActionType.Fill,
            UIManager.UIType.DispatchOrder
        ));

        _steps.Enqueue(new StepData(
            "1车间工人到备料区领料",
            "查看自己的派工单，根据派工单到备料区领料",
            "1车间工人",
            "备料区",
            Interactables.ActionType.Pick
        ));

        _steps.Enqueue(new StepData(
            "1车间工人填写领料单",
            "填写领料单；点：提交",
            "1车间工人",
            "备料区",
            Interactables.ActionType.Fill,
            UIManager.UIType.PickList
        ));

        // [步骤9] 工人生产 - 需按E触发，将来加入动画控制
        _steps.Enqueue(new StepData(
            "1车间工人生产",
            "工人进行弯管生产操作",
            "1车间工人",
            "1车间-弯管",
            Interactables.ActionType.View
        ));

        _steps.Enqueue(new StepData(
            "1车间工人退料实物送仓库",
            "将生产多余物料退回仓库",
            "1车间工人",
            "仓库",
            Interactables.ActionType.Deliver
        ));

        _steps.Enqueue(new StepData(
            "仓管员质检确认退料",
            "对1车间退料进行质检确认",
            "仓管员B",
            "仓库",
            Interactables.ActionType.Approve
        ));

        _steps.Enqueue(new StepData(
            "仓管员制生产退料入库单",
            "填写生产退料入库单",
            "仓管员B",
            "仓库",
            Interactables.ActionType.Fill,
            UIManager.UIType.ProductionReturn
        ));

        _steps.Enqueue(new StepData(
            "1车间工人签字确认退料",
            "在生产退料入库单上签字",
            "1车间工人",
            "仓库",
            Interactables.ActionType.Approve,
            UIManager.UIType.ProductionReturn
        ));

        _steps.Enqueue(new StepData(
            "1车间班组长检查并填写工序汇报单",
            "弯管生产完成后检查；填写工序汇报单",
            "1车间班组长",
            "1车间-弯管",
            Interactables.ActionType.Fill,
            UIManager.UIType.ProcessReport
        ));

        _steps.Enqueue(new StepData(
            "1车间工人将半成品送往2车间",
            "将弯管半成品送往2车间（焊接车间）",
            "1车间工人",
            "2车间-焊接",
            Interactables.ActionType.Deliver
        ));

        // ========================================================
        // 阶段3: 2车间 — 焊接车间（步骤16~26）
        // ========================================================

        _steps.Enqueue(new StepData(
            "2车间主管填写生产工单",
            "填写2车间（焊接车间）生产工单",
            "2车间主管",
            "2车间-焊接",
            Interactables.ActionType.Fill,
            UIManager.UIType.ProductionWorkOrder
        ));

        _steps.Enqueue(new StepData(
            "2车间班组长填写派工单",
            "查看2车间生产工单；填写工人个人的派工单",
            "2车间班组长",
            "2车间-焊接",
            Interactables.ActionType.Fill,
            UIManager.UIType.DispatchOrder
        ));

        _steps.Enqueue(new StepData(
            "2车间工人到备料区领料",
            "查看自己的派工单，根据派工单到备料区领料",
            "2车间工人",
            "备料区",
            Interactables.ActionType.Pick
        ));

        _steps.Enqueue(new StepData(
            "2车间工人填写领料单",
            "填写领料单",
            "2车间工人",
            "备料区",
            Interactables.ActionType.Fill,
            UIManager.UIType.PickList
        ));

        // [步骤20] 工人生产 - 需按E触发，将来加入动画控制
        _steps.Enqueue(new StepData(
            "2车间工人生产",
            "工人进行焊接生产操作",
            "2车间工人",
            "2车间-焊接",
            Interactables.ActionType.View
        ));

        _steps.Enqueue(new StepData(
            "2车间工人退料实物送仓库",
            "将生产多余物料退回仓库",
            "2车间工人",
            "仓库",
            Interactables.ActionType.Deliver
        ));

        _steps.Enqueue(new StepData(
            "仓管员质检确认退料",
            "对2车间退料进行质检确认",
            "仓管员B",
            "仓库",
            Interactables.ActionType.Approve
        ));

        _steps.Enqueue(new StepData(
            "仓管员制生产退料入库单",
            "填写生产退料入库单",
            "仓管员B",
            "仓库",
            Interactables.ActionType.Fill,
            UIManager.UIType.ProductionReturn
        ));

        _steps.Enqueue(new StepData(
            "2车间工人签字确认退料",
            "在生产退料入库单上签字",
            "2车间工人",
            "仓库",
            Interactables.ActionType.Approve,
            UIManager.UIType.ProductionReturn
        ));

        _steps.Enqueue(new StepData(
            "2车间班组长检查并填写工序汇报单",
            "焊接生产完成后检查；填写工序汇报单",
            "2车间班组长",
            "2车间-焊接",
            Interactables.ActionType.Fill,
            UIManager.UIType.ProcessReport
        ));

        _steps.Enqueue(new StepData(
            "2车间工人将半成品送往3车间",
            "将焊接半成品送往3车间（配电车间）",
            "2车间工人",
            "3车间-配电",
            Interactables.ActionType.Deliver
        ));

        // ========================================================
        // 阶段4: 3车间 — 配电车间（步骤27~37）
        // ========================================================

        _steps.Enqueue(new StepData(
            "3车间主管填写生产工单",
            "填写3车间（配电车间）生产工单",
            "3车间主管",
            "3车间-配电",
            Interactables.ActionType.Fill,
            UIManager.UIType.ProductionWorkOrder
        ));

        _steps.Enqueue(new StepData(
            "3车间班组长填写派工单",
            "查看3车间生产工单；填写工人个人的派工单",
            "3车间班组长",
            "3车间-配电",
            Interactables.ActionType.Fill,
            UIManager.UIType.DispatchOrder
        ));

        _steps.Enqueue(new StepData(
            "3车间工人到备料区领料",
            "查看自己的派工单，根据派工单到备料区领料",
            "3车间工人",
            "备料区",
            Interactables.ActionType.Pick
        ));

        _steps.Enqueue(new StepData(
            "3车间工人填写领料单",
            "填写领料单",
            "3车间工人",
            "备料区",
            Interactables.ActionType.Fill,
            UIManager.UIType.PickList
        ));

        // [步骤31] 工人生产 - 需按E触发，将来加入动画控制
        _steps.Enqueue(new StepData(
            "3车间工人生产",
            "工人进行配电生产操作",
            "3车间工人",
            "3车间-配电",
            Interactables.ActionType.View
        ));

        _steps.Enqueue(new StepData(
            "3车间工人退料实物送仓库",
            "将生产多余物料退回仓库",
            "3车间工人",
            "仓库",
            Interactables.ActionType.Deliver
        ));

        _steps.Enqueue(new StepData(
            "仓管员质检确认退料",
            "对3车间退料进行质检确认",
            "仓管员B",
            "仓库",
            Interactables.ActionType.Approve
        ));

        _steps.Enqueue(new StepData(
            "仓管员制生产退料入库单",
            "填写生产退料入库单",
            "仓管员B",
            "仓库",
            Interactables.ActionType.Fill,
            UIManager.UIType.ProductionReturn
        ));

        _steps.Enqueue(new StepData(
            "3车间工人签字确认退料",
            "在生产退料入库单上签字",
            "3车间工人",
            "仓库",
            Interactables.ActionType.Approve,
            UIManager.UIType.ProductionReturn
        ));

        _steps.Enqueue(new StepData(
            "3车间班组长检查并填写工序汇报单",
            "配电生产完成后检查；填写工序汇报单",
            "3车间班组长",
            "3车间-配电",
            Interactables.ActionType.Fill,
            UIManager.UIType.ProcessReport
        ));

        _steps.Enqueue(new StepData(
            "3车间工人将半成品送往4车间",
            "将配电半成品送往4车间（总装车间）",
            "3车间工人",
            "4车间-总装",
            Interactables.ActionType.Deliver
        ));

        // ========================================================
        // 阶段5: 4车间 — 总装车间及入库（步骤38~47）
        // ========================================================

        _steps.Enqueue(new StepData(
            "4车间主管填写生产工单",
            "填写4车间（总装车间）生产工单",
            "4车间主管",
            "4车间-总装",
            Interactables.ActionType.Fill,
            UIManager.UIType.ProductionWorkOrder
        ));

        _steps.Enqueue(new StepData(
            "4车间班组长填写派工单",
            "查看4车间生产工单；填写工人个人的派工单",
            "4车间班组长",
            "4车间-总装",
            Interactables.ActionType.Fill,
            UIManager.UIType.DispatchOrder
        ));

        _steps.Enqueue(new StepData(
            "4车间工人到备料区领料",
            "查看自己的派工单，根据派工单到备料区领料",
            "4车间工人",
            "备料区",
            Interactables.ActionType.Pick
        ));

        _steps.Enqueue(new StepData(
            "4车间工人填写领料单",
            "填写领料单；点：提交",
            "4车间工人",
            "备料区",
            Interactables.ActionType.Fill,
            UIManager.UIType.PickList
        ));

        // [步骤42] 总装 - 需按E触发，将来加入动画控制
        _steps.Enqueue(new StepData(
            "4车间工人总装",
            "4车间负责组装来自1/2/3车间的半成品",
            "4车间工人",
            "4车间-总装",
            Interactables.ActionType.View
        ));

        _steps.Enqueue(new StepData(
            "4车间工人将成品送往仓库",
            "组装完成后由4车间工人将成品送往仓库",
            "4车间工人",
            "仓库",
            Interactables.ActionType.Deliver
        ));

        _steps.Enqueue(new StepData(
            "仓管员质检确认成品",
            "对成品进行质检确认",
            "仓管员B",
            "仓库",
            Interactables.ActionType.Approve
        ));

        _steps.Enqueue(new StepData(
            "仓管员填写完工入库单",
            "填写完工入库单；工人可查看",
            "仓管员B",
            "仓库",
            Interactables.ActionType.Fill,
            UIManager.UIType.FinishedInbound
        ));

        _steps.Enqueue(new StepData(
            "工人在完工入库单签字",
            "工人在完工入库单上签字",
            "4车间工人",
            "仓库",
            Interactables.ActionType.Approve,
            UIManager.UIType.FinishedInbound
        ));

        _steps.Enqueue(new StepData(
            "自动通知销售员产品入库",
            "系统自动通知销售员产品已入库",
            "系统",
            "-",
            Interactables.ActionType.View
        ));

        // ========================================================
        // 阶段6: 发货流程（步骤48~57）
        // ========================================================

        _steps.Enqueue(new StepData(
            "销售员查看入库通知",
            "销售员查看产品入库通知，确认货已配齐",
            "销售员",
            "销售办公室",
            Interactables.ActionType.View
        ));

        _steps.Enqueue(new StepData(
            "联系客户确认发货",
            "销售员联系客户，确认是否可以发货",
            "销售员",
            "销售办公室",
            Interactables.ActionType.Fill
        ));

        _steps.Enqueue(new StepData(
            "客户确认可发货",
            "客户确认可以发货",
            "销售员",
            "销售办公室",
            Interactables.ActionType.View
        ));

        _steps.Enqueue(new StepData(
            "销售员点击发货",
            "销售员在销售订单点击发货，自动下推发货通知单",
            "销售员",
            "销售办公室",
            Interactables.ActionType.Fill,
            UIManager.UIType.SalesOrder
        ));

        _steps.Enqueue(new StepData(
            "仓管员填写发货通知单",
            "仓管员填写发货通知单（由销售订单下推）",
            "仓管员B",
            "仓库",
            Interactables.ActionType.Fill,
            UIManager.UIType.DeliveryNotice
        ));

        _steps.Enqueue(new StepData(
            "仓库主管审核发货通知单",
            "仓库主管审核发货通知单",
            "仓库主管",
            "仓库",
            Interactables.ActionType.Approve,
            UIManager.UIType.DeliveryNotice
        ));

        _steps.Enqueue(new StepData(
            "仓库包装出库",
            "仓库包装出库，发货完成",
            "仓管员B",
            "仓库",
            Interactables.ActionType.Fill
        ));

        _steps.Enqueue(new StepData(
            "仓管员填写销售出库单",
            "仓管员填写销售出库单（由发货通知单下推）",
            "仓管员B",
            "仓库",
            Interactables.ActionType.Fill,
            UIManager.UIType.SalesOutbound
        ));

        _steps.Enqueue(new StepData(
            "仓库主管审核销售出库单",
            "仓库主管审核销售出库单",
            "仓库主管",
            "仓库",
            Interactables.ActionType.Approve,
            UIManager.UIType.SalesOutbound
        ));

        _steps.Enqueue(new StepData(
            "客户收货签字",
            "客户收货，在发货通知单上签字",
            "销售员",
            "销售办公室",
            Interactables.ActionType.View
        ));
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
        Debug.Log($"[CustomProductionFlow] MarkStepComplete 被调用！_isStepCompleted 设置为 true");
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