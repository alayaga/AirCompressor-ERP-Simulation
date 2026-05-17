using UnityEngine;
using System.Collections;

/// <summary>
/// 采购流程控制器
/// 🔹 支持标准/定制产品采购路由判断
/// 🔹 结构：专属前置步骤(分支) + 公共中后端步骤(复用)
/// </summary>
public class PurchaseFlow : FlowBase
{
    #region  流程路由配置
    [Header(" 采购流程路由")]
    [Tooltip("是否为标准产品采购流程（由 SalesFlow 或 FlowManager 传入）")]
    [SerializeField]
    public bool isStandard = true;
    #endregion

    #region 生命周期与重写方法
    protected override IEnumerator FlowCoroutine()
    {
        Debug.Log($"[采购] 流程启动 | 类型: {(isStandard ? "标准产品" : "定制产品")}");

        // 1. 根据流程类型执行差异化的前置单据流转
        if (isStandard)
        {
            yield return RunStandardInitSteps();
        }
        else
        {
            yield return RunCustomInitSteps();
        }

        // 2. 执行标准/定制完全重合的中后端采购流程（从采购订单开始至入库）
        yield return RunCommonProcurementSteps();

        Debug.Log("[采购]  采购流程全部执行完毕");
        FinishFlow();
    }

    public override void StartFlow()
    {
        Debug.Log("采购流程开始");
        base.StartFlow();
        this.InitTaskUI();
    }

    public override void StopFlow()
    {
        Debug.Log("采购流程停止");
        base.StopFlow();
    }

    protected override void FinishFlow()
    {
        Debug.Log("采购流程完整结束");
        base.FinishFlow();
    }

    /// <summary>
    /// 供外部调用，动态设置采购流程类型
    /// </summary>
    public void SetProcurementType(bool isStandardMode)
    {
        isStandard = isStandardMode;
        Debug.Log($"[采购] 流程类型已注入: {(isStandard ? "标准" : "定制")}");
    }
    #endregion

    #region 🔹 标准产品专属前置步骤 (对应标准流程图: PMC计划 -> 主管审核 -> 跟单员查看 -> 填PO)
    private IEnumerator RunStandardInitSteps()
    {
        Debug.Log("[采购-标准] 进入标准采购前置流程");

        // 节点1: PMC制作两周采购计划 -> 提交
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.PMC));
        var planUI = UIManager.Instance.GetUIObject(UIManager.UIType.测试UI);
        UIManager.Instance.ShowUI(UIManager.UIType.测试UI);
        planUI.GetComponent<TestUIScript>().SetTexts("PMC 制作两周采购计划\n点：提交", "确认提交");
        yield return planUI.GetComponent<TestUIScript>().WaitForButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.测试UI);
        FlowStepTracker.CompleteStep();

        // 节点2: 采购主管审核采购计划 -> 自动下推
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.采购主管));
        UIManager.Instance.ShowUI(UIManager.UIType.测试UI);
        planUI.GetComponent<TestUIScript>().SetTexts("采购主管 审核两周采购计划\n点：审核（自动下推）", "确认审核");
        yield return planUI.GetComponent<TestUIScript>().WaitForButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.测试UI);
        FlowStepTracker.CompleteStep();

        // 节点3: 跟单员查看采购计划 -> 填采购订单(邮件供应商)
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.采购员));
        Debug.Log("[采购-标准] 前置完成，即将进入公共采购订单环节");
    }
    #endregion

    #region  定制产品专属前置步骤 (对应定制流程图: PMC看BOM -> 填采购申请单 -> 主管审核 -> 跟单员填PO)
    private IEnumerator RunCustomInitSteps()
    {
        Debug.Log("[采购-定制] 进入定制采购前置流程");

        // 节点1: PMC查看销售订单 & 下查BOM单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.PMC));
        var bomUI = UIManager.Instance.GetUIObject(UIManager.UIType.测试UI);
        UIManager.Instance.ShowUI(UIManager.UIType.测试UI);
        bomUI.GetComponent<TestUIScript>().SetTexts("PMC 查看销售订单\n点：下查 -> 查看对应BOM单", "确认查看");
        yield return bomUI.GetComponent<TestUIScript>().WaitForButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.测试UI);
        FlowStepTracker.CompleteStep();

        // 节点2: PMC填写采购申请单 -> 提交
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.PMC));
        var appUI = UIManager.Instance.GetUIObject(UIManager.UIType.采购申请单);
        UIManager.Instance.ShowUI(UIManager.UIType.采购申请单);
        var appManager = appUI.GetComponent<FinishedProductOutboundManager>();

        string[] appForm1 = { "PAP-" + System.DateTime.Now.ToString("yyyyMMdd") + "-CUS", "采购部", "采购员", System.DateTime.Now.ToString("yyyy-MM-dd"), "急", "关联BOM单" };
        string[] appForm2 = { "PMC", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "PMC", " ", " ", " " };
        string[][] appTable = { new string[] { "PM001", "定制螺杆", "SK-100C", "台", "10", "10", "0", "10", "无" } };

        yield return appManager.WaitForFillButtonClick();
        appManager.SetQuoteFormData(appForm1, appForm2, appTable);
        yield return appManager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.采购申请单);
        FlowStepTracker.CompleteStep();

        // 节点3: 采购主管审核采购申请单 -> 跟单员可查看
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.采购主管));
        UIManager.Instance.ShowUI(UIManager.UIType.采购申请单);
        appManager.SetQuoteFormData(appForm1, appForm2, appTable);
        yield return appManager.WaitForApproveButtonClick();
        appForm2[4] = "采购主管";
        appManager.SetQuoteFormData(appForm1, appForm2, appTable);
        UIManager.Instance.HideUI(UIManager.UIType.采购申请单);
        FlowStepTracker.CompleteStep();

        Debug.Log("[采购-定制] 前置完成，即将进入公共采购订单环节");
    }
    #endregion

    #region  标准/定制共用中后端步骤 (从采购订单提交开始 -> 供应商 -> 收料 -> 质检 -> 入库)
    private IEnumerator RunCommonProcurementSteps()
    {
        Debug.Log("[采购-公共] 开始执行通用采购流转");

        // 1. 采购员填采购订单(邮件供应商) -> 提交
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.采购员));
        var poUI = UIManager.Instance.GetUIObject(UIManager.UIType.采购订单);
        UIManager.Instance.ShowUI(UIManager.UIType.采购订单);
        var poManager = poUI.GetComponent<FinishedProductOutboundManager>();

        string currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string[] orderFormData1 = { "PO-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + Random.Range(100, 999), currentTime, "采购部", "采购员", "机械零部件供应商", "已填写", "无" };
        string[] orderFormData2 = { "采购员", currentTime, "采购员", currentTime, " ", " " };
        string[][] orderTableData = { new string[] { "PM001", "螺杆主机", "SK-100", "台", "10", "10", "0", "10", "无" } };

        yield return poManager.WaitForFillButtonClick();
        poManager.SetQuoteFormData(orderFormData1, orderFormData2, orderTableData);
        yield return poManager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.采购订单);
        FlowStepTracker.CompleteStep();

        // 2. 采购主管审核采购订单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.采购主管));
        UIManager.Instance.ShowUI(UIManager.UIType.采购订单);
        poManager.SetQuoteFormData(orderFormData1, orderFormData2, orderTableData);
        yield return poManager.WaitForApproveButtonClick();
        orderFormData2[4] = "采购主管";
        poManager.SetQuoteFormData(orderFormData1, orderFormData2, orderTableData);
        UIManager.Instance.HideUI(UIManager.UIType.采购订单);
        FlowStepTracker.CompleteStep();

        // 3. 供应商送货 & 电话联系跟单员 (3D动画/测试UI模拟)
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.采购员));
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.卡车视角);
        yield return ObjectManager.Instance.GetObject(ObjectManager.ObjectType.卡车).GetComponent<ObjectMovementController>()?.MoveToPositionsSequentially();
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.进货位置);
        yield return ObjectManager.Instance.GetObject(ObjectManager.ObjectType.原料仓储).GetComponent<GameObjectSequenceController>()?.ShowObjectsSequentially();
        ObjectManager.Instance.GetObject(ObjectManager.ObjectType.卡车).SetActive(false);
        FlowStepTracker.CompleteStep();

        // 4. 采购员下推采购订单 -> 填收料通知单 -> 提交
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.采购员));
        var noticeUI = UIManager.Instance.GetUIObject(UIManager.UIType.收料通知单);
        UIManager.Instance.ShowUI(UIManager.UIType.收料通知单);
        var noticeManager = noticeUI.GetComponent<FinishedProductOutboundManager>();

        string[] noticeFormData1 = { "RN-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + Random.Range(100, 999), "机械零部件供应商", System.DateTime.Now.AddDays(7).ToString("yyyy-MM-dd"), System.DateTime.Now.ToString("yyyy-MM-dd"), "采购员", orderFormData1[0] };
        string[] noticeFormData2 = { "采购员", currentTime, "采购员", currentTime, " ", " " };

        yield return noticeManager.WaitForFillButtonClick();
        noticeManager.SetQuoteFormData(noticeFormData1, noticeFormData2, orderTableData);
        yield return noticeManager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.收料通知单);
        FlowStepTracker.CompleteStep();

        // 5. 采购主管审核收料通知单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.采购主管));
        UIManager.Instance.ShowUI(UIManager.UIType.收料通知单);
        noticeManager.SetQuoteFormData(noticeFormData1, noticeFormData2, orderTableData);
        yield return noticeManager.WaitForApproveButtonClick();
        noticeFormData2[4] = "采购主管";
        noticeManager.SetQuoteFormData(noticeFormData1, noticeFormData2, orderTableData);
        UIManager.Instance.HideUI(UIManager.UIType.收料通知单);
        FlowStepTracker.CompleteStep();

        // 6. 仓管员质检 & 填来料检验单 (自动下推)
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.仓管员));
        var qcUI = UIManager.Instance.GetUIObject(UIManager.UIType.测试UI);
        UIManager.Instance.ShowUI(UIManager.UIType.测试UI);
        qcUI.GetComponent<TestUIScript>().SetTexts("仓管员 质检确认\n填：来料检验单（自动下推）", "确认质检");
        yield return qcUI.GetComponent<TestUIScript>().WaitForButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.测试UI);
        FlowStepTracker.CompleteStep();

        // 7. 仓管员填采购入库单 -> 提交 -> 审核 -> 更新库存
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.仓管员));
        var inboundUI = UIManager.Instance.GetUIObject(UIManager.UIType.采购入库单);
        UIManager.Instance.ShowUI(UIManager.UIType.采购入库单);
        var inboundManager = inboundUI.GetComponent<FinishedProductOutboundManager>();

        string[] inboundFormData1 = { "IN-" + System.DateTime.Now.ToString("yyyyMMdd") + "-" + Random.Range(100, 999), "机械零部件供应商", "主仓库", System.DateTime.Now.ToString("yyyy-MM-dd"), "仓管员", noticeFormData1[0] };
        string[] inboundFormData2 = { "仓管员", currentTime, "仓管员", currentTime, " ", " " };

        yield return inboundManager.WaitForFillButtonClick();
        inboundManager.SetQuoteFormData(inboundFormData1, inboundFormData2, orderTableData);
        yield return inboundManager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.采购入库单);
        FlowStepTracker.CompleteStep();

        // 仓库主管审核入库单
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.仓库主管));
        UIManager.Instance.ShowUI(UIManager.UIType.采购入库单);
        inboundManager.SetQuoteFormData(inboundFormData1, inboundFormData2, orderTableData);
        yield return inboundManager.WaitForApproveButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.采购入库单);
        FlowStepTracker.CompleteStep();

        // 更新库存系统
        Debug.Log("====== 采购完成：更新库存管理系统 ======");
        InventoryManager.Instance?.AddAllInventory(10);
    }
    #endregion
}