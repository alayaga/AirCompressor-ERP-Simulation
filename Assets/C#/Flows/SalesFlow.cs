using UnityEngine;
using System.Collections;

/// <summary>
/// 销售流程
/// </summary>
public class SalesFlow : FlowBase
{
    protected override IEnumerator FlowCoroutine()
    {
        Debug.Log("销售流程开始");

        var demand = DemandManager.Instance.GetCurrentDemand();
        
        // 确保需求数据有效
        if (demand == null)
        {
            Debug.LogError("无法获取客户需求数据，流程终止");
            yield break;
        }
        
        // 销售报价单流程
        yield return ProcessQuoteForm(demand);
        
        // 销售合同流程
        yield return ProcessContract(demand);
        
        // 销售订单流程
        yield return ProcessSalesOrder();
        
        yield return new WaitForSeconds(1f);
        FinishFlow();
    }

    private IEnumerator ProcessQuoteForm(DemandManager.CustomerDemand demand)
    {
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.销售员));

        var currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.销售报价单);
        var manager = currentUI.GetComponent<QuoteFormManager>();
        UIManager.Instance.ShowUI(UIManager.UIType.销售报价单);
        
        System.DateTime currentDate = System.DateTime.Now;
        string dateStr = currentDate.ToString("yyyy-MM-dd");
        string billNo = $"XSBJ-{currentDate:yyMMdd}-{Random.Range(1, 1000):000}";
        float totalAmount = demand.airCompressorCount * demand.unitPrice;
        
        string[] inputBoxContents = new string[] {
            billNo, dateStr, demand.customerName, "人民币", "价目表 A", "折扣表 B",
            "销售组 C", "销售员", dateStr, currentDate.AddDays(30).ToString("yyyy-MM-dd"),
            "待审核", "销售员", ""
        };
        
        string[][] tableRowData = new string[][] {
            new string[] { 
                "AC001", "空压机", "AC-2023", "台", demand.airCompressorCount.ToString(),
                demand.unitPrice.ToString("F2"), totalAmount.ToString("F2"), "13%", "无"
            }
        };
        
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.销售员);
        yield return manager.WaitForFillButtonClick();
        manager.SetQuoteFormData(inputBoxContents, tableRowData);
        yield return manager.WaitForSubmitButtonClick();

        inputBoxContents[10] = "已审核";
        inputBoxContents[12] = "销售主管";
        UIManager.Instance.HideUI(UIManager.UIType.销售报价单);
        FlowStepTracker.CompleteStep();

        // 审核
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.销售主管));
        UIManager.Instance.ShowUI(UIManager.UIType.销售报价单);
        manager.SetQuoteFormData(inputBoxContents, tableRowData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.销售主管);
        yield return manager.WaitForApproveButtonClick();
        manager.SetQuoteFormData(inputBoxContents, tableRowData);
        UIManager.Instance.HideUI(UIManager.UIType.销售报价单);
        FlowStepTracker.CompleteStep();

        // 下推
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.销售员));
        UIManager.Instance.ShowUI(UIManager.UIType.销售报价单);
        manager.SetQuoteFormData(inputBoxContents, tableRowData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.销售员);
        yield return manager.WaitForPushButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.销售报价单);
        FlowStepTracker.CompleteStep();
    }

    private IEnumerator ProcessContract(DemandManager.CustomerDemand demand)
    {
        var currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.通用UI);
        UIManager.Instance.ShowUI(UIManager.UIType.通用UI);
        
        string contractContent = "客户名称：    \n\n产品信息：         \n空压机数量：     台\n" +
                                "单价：¥          /台\n总价：¥          \n\n" +
                                "交货条款：        \n付款方式：        \n质量保证：        \n违约责任：        ";
        
        currentUI.GetComponent<GeneralUIScript>().SetAllTexts("销售合同", contractContent);
        currentUI.GetComponent<GeneralUIScript>().ShowUIMode(UIFormMode.Fill);
        yield return currentUI.GetComponent<GeneralUIScript>().WaitForFillButtonClick();

        contractContent = $"客户名称：{demand.customerName}\n\n产品信息：\n空压机数量：{demand.airCompressorCount}台\n" +
                         $"单价：¥{demand.unitPrice:N2}/台\n总价：¥{demand.airCompressorCount * demand.unitPrice:N2}\n\n" +
                         "交货条款：订单确认后15个工作日内送达指定地点\n付款方式：30%预付款，70%到货验收后7日内付清\n" +
                         "质量保证：整机保修一年，核心部件保修两年\n违约责任：按《合同法》相关规定执行";
        
        currentUI.GetComponent<GeneralUIScript>().SetContentText(contractContent);
        currentUI.GetComponent<GeneralUIScript>().SetAllUIVisibility(false);
        yield return WaitForSeconds(1f);
        UIManager.Instance.HideUI(UIManager.UIType.通用UI);
        FlowStepTracker.CompleteStep();

        // 审核
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.销售主管));
        UIManager.Instance.ShowUI(UIManager.UIType.通用UI);
        currentUI.GetComponent<GeneralUIScript>().ShowUIMode(UIFormMode.Approve);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.销售主管);
        yield return currentUI.GetComponent<GeneralUIScript>().WaitForApproveButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.通用UI);
        FlowStepTracker.CompleteStep();

        // 下推
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.销售员));
        UIManager.Instance.ShowUI(UIManager.UIType.通用UI);
        currentUI.GetComponent<GeneralUIScript>().ShowUIMode(UIFormMode.PushDown);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.销售员);
        yield return currentUI.GetComponent<GeneralUIScript>().WaitForForwardButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.通用UI);
        FlowStepTracker.CompleteStep();
    }

    private IEnumerator ProcessSalesOrder()
    {
        var currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.销售订单);
        var manager = currentUI.GetComponent<QuoteFormManager>();
        UIManager.Instance.ShowUI(UIManager.UIType.销售订单);
        
        string billNo = "XSDD-251120-002";
        
        string[] inputBoxContents = new string[] { 
            billNo, "2025-11-20", "客户 B", "美元", "价目表 B", "折扣表 C",
            "销售组 A", "销售员", "暂存", "无", "销售员", ""
        };
        
        float[] quantities = new float[] { 15, 8 };
        float[] unitPrices = new float[] { 80.00f, 120.00f };
        float[] discountRates = new float[] { 0.05f, 0.08f };
        
        string[][] tableRowData = new string[][] {
            new string[] {
                "MAT003", "产品 C", "型号 C", "箱", quantities[0].ToString(), unitPrices[0].ToString("F2"),
                (quantities[0] * unitPrices[0]).ToString("F2"), discountRates[0].ToString("P0"),
                (quantities[0] * unitPrices[0] * (1 - discountRates[0])).ToString("F2"),
                "2025-11-25", "0", "0", "0", "否", "备注 C"
            },
            new string[] {
                "MAT004", "产品 D", "型号 D", "套", quantities[1].ToString(), unitPrices[1].ToString("F2"),
                (quantities[1] * unitPrices[1]).ToString("F2"), discountRates[1].ToString("P0"),
                (quantities[1] * unitPrices[1] * (1 - discountRates[1])).ToString("F2"),
                "2025-11-30", "0", "0", "0", "否", "备注 D"
            }
        };

        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.销售员);
        yield return manager.WaitForFillButtonClick();
        manager.SetQuoteFormData(inputBoxContents, tableRowData);

        inputBoxContents[8] = "已填写";
        inputBoxContents[11] = "销售主管";
        manager.SetQuoteFormData(inputBoxContents, tableRowData);
        yield return manager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.销售订单);
        FlowStepTracker.CompleteStep();

        // 审核
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.销售主管));
        UIManager.Instance.ShowUI(UIManager.UIType.销售订单);
        manager.SetQuoteFormData(inputBoxContents, tableRowData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.销售主管);
        yield return manager.WaitForApproveButtonClick();
        inputBoxContents[8] = "已审核";
        manager.SetQuoteFormData(inputBoxContents, tableRowData);
        UIManager.Instance.HideUI(UIManager.UIType.销售订单);
        FlowStepTracker.CompleteStep();

        // 下推
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.销售员));
        UIManager.Instance.ShowUI(UIManager.UIType.销售订单);
        manager.SetQuoteFormData(inputBoxContents, tableRowData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.销售员);
        yield return manager.WaitForPushButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.销售订单);
        FlowStepTracker.CompleteStep();
    }
    
    public override void StartFlow()
    {
        Debug.Log("销售流程开始");
        base.StartFlow();
        this.InitTaskUI();
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
