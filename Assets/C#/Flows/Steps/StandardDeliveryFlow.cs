using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 标准产品发货流程
/// 从销售订单到发货完成的完整流程
/// </summary>
public class StandardDeliveryFlow : FlowBase
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
        public bool allowShip;
        public StepData(string name, string desc, string npc, string location, Interactables.ActionType action, UIManager.UIType? billType = null, bool allowShip = false)
        {
            stepName = name;
            description = desc;
            targetNPC = npc;
            targetLocation = location;
            actionType = action;
            this.billType = billType;
            this.allowShip = allowShip;
        }
    }
    
    // 流程信息
    private const string FLOW_NAME = "标准产品发货流程";
    private const string TASK_TITLE = "销售发货流程";
    private const string TASK_DESCRIPTION = "完成从销售订单到发货的完整流程";
    
    // 步骤队列
    private Queue<StepData> _steps = new Queue<StepData>();
    private StepData _currentStep;
    private bool _isStepCompleted = false;
    private int _totalSteps;
    private int _currentStepIndex = 0;
    
    // 库存检查相关
    private bool _hasStock = false;  // 是否有现货

    protected override IEnumerator FlowCoroutine()
    {
        Debug.Log("[StandardDeliveryFlow] 开始标准产品发货流程");

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
            bool isAutoStep = _currentStep.targetNPC == "客户" || _currentStep.targetNPC == "系统";
            if (_currentStep.billType != null)
            {
                // 单据步骤：尝试打开UI，未配置则回退到else分支
                yield return WaitForBillComplete(_currentStep.billType.Value, _currentStep.targetNPC, _currentStep.actionType, _currentStep.allowShip);
                if (!_isStepCompleted) yield return new WaitUntil(() => _isStepCompleted);
            }
            else if (isAutoStep)
            {
                Debug.Log($"[StandardDeliveryFlow] 自动步骤：{_currentStep.stepName}，等待5秒后自动完成");
                yield return new WaitForSeconds(5f);
                _isStepCompleted = true;
            }
            else
            {
                yield return new WaitUntil(() => _isStepCompleted);
            }

            Debug.Log($"[完成] {_currentStep.stepName}");
            UpdateProgressToUI();

            if (_currentStep.stepName == "检查库存")
            {
                _hasStock = CheckStock();
                if (!_hasStock)
                {
                    Debug.Log("[StandardDeliveryFlow] 库存不足，启动标准产品生产流程");
                    yield return StartProductionSubFlow();
                    Debug.Log("[StandardDeliveryFlow] 生产完成，继续发货流程");
                }
                else
                {
                    Debug.Log("[StandardDeliveryFlow] 库存充足，直接进入发货流程");
                }
            }

            yield return new WaitForSeconds(0.5f);
        }

        ShowTaskCompleteToUI();
        Debug.Log("[StandardDeliveryFlow] 标准产品发货流程完成！");
    }

    private void InitializeSteps()
    {
        _steps.Clear();

        // ===== 标准产品销售-发货流程 v1.3 =====

        // 阶段1：订单接收与审核
        _steps.Enqueue(new StepData("客户下单", "客户通过官网选择机型、数量，点击下单，自动生成销售订单", "客户", "官网", Interactables.ActionType.View));
        _steps.Enqueue(new StepData("审核销售订单", "销售总监审核销售订单；点：审核；仓库可查看销售订单", "销售总监", "销售部", Interactables.ActionType.Approve, UIManager.UIType.SalesOrder));
        _steps.Enqueue(new StepData("查看销售订单", "仓管员查看销售订单", "仓管员", "质检区", Interactables.ActionType.View, UIManager.UIType.SalesOrder));
        _steps.Enqueue(new StepData("检查库存", "仓管员检查现有库存", "仓管员", "仓库", Interactables.ActionType.View));
        // 注意：检查库存后，无现货时会自动启动"标准产品生产流程"子流程

        // 阶段2：确认发货（有现货时执行）
        _steps.Enqueue(new StepData("联系客户确认发货", "销售员联系客户确认是否可以发货", "销售员", "销售办公室", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("客户确认可发货", "客户确认可以发货", "客户", "客户处", Interactables.ActionType.View));
        _steps.Enqueue(new StepData("销售订单点发货", "销售员在销售订单点发货；自动下推发货通知单", "销售员", "销售办公室", Interactables.ActionType.Fill, UIManager.UIType.SalesOrder, allowShip: true));

        // 阶段3：发货出库
        _steps.Enqueue(new StepData("填写发货通知单", "仓管员填写发货通知单（由销售订单下推）", "仓管员", "质检区", Interactables.ActionType.Fill, UIManager.UIType.DeliveryNotice));
        _steps.Enqueue(new StepData("审核发货通知单", "仓库主管审核发货通知单", "仓库主管", "仓库部", Interactables.ActionType.Approve, UIManager.UIType.DeliveryNotice));
        _steps.Enqueue(new StepData("包装出库", "仓库包装出库，发货完成", "仓管员", "质检区", Interactables.ActionType.Fill));
        _steps.Enqueue(new StepData("填写销售出库单", "仓管员填写销售出库单（由发货通知单下推）", "仓管员", "质检区", Interactables.ActionType.Fill, UIManager.UIType.SalesOutbound));
        _steps.Enqueue(new StepData("审核销售出库单", "仓库主管审核销售出库单", "仓库主管", "仓库部", Interactables.ActionType.Approve, UIManager.UIType.SalesOutbound));
        _steps.Enqueue(new StepData("客户签收", "客户收货，在发货通知单上签字", "客户", "客户处", Interactables.ActionType.View, UIManager.UIType.DeliveryNotice));
    }

    /// <summary>
    /// 启动标准产品生产流程作为子流程
    /// </summary>
    private IEnumerator StartProductionSubFlow()
    {
        if (TaskGuidePanelNew.Instance != null)
        {
            TaskGuidePanelNew.Instance.UpdateHintText("库存不足，正在启动生产流程...");
        }

        var productionFlow = new StandardProductionFlow();

        if (FlowManager.Instance != null)
        {
            FlowManager.Instance.RegisterFlow(productionFlow);
        }

        productionFlow.StartFlow();

        yield return new WaitUntil(() => !productionFlow.IsRunning);

        _hasStock = true;

        if (TaskGuidePanelNew.Instance != null)
        {
            TaskGuidePanelNew.Instance.UpdateHintText("生产完成，库存已补充，继续发货流程");
        }
    }

    /// <summary>
    /// 检查库存（模拟）
    /// 实际项目中应该从数据库或库存系统获取真实库存数据
    /// </summary>
    /// <returns>true=有现货，false=库存不足</returns>
    private bool CheckStock()
    {
        // 模拟库存检查（70%概率有货）
        bool hasStock = Random.value > 0.3f;
        
        // 更新UI显示库存状态
        if (TaskGuidePanelNew.Instance != null)
        {
            string hintText = hasStock 
                ? "库存充足，可以直接发货\n(跳过生产流程)" 
                : "库存不足，需要生产";
            TaskGuidePanelNew.Instance.UpdateHintText(hintText);
        }
        
        return hasStock;
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
        Debug.Log($"[StandardDeliveryFlow] MarkStepComplete 被调用！_isStepCompleted 设置为 true");
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