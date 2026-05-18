using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 报价单处理步骤：封装原 ProcessQuoteForm 逻辑
/// </summary>
public class QuoteFormStep : FlowStepBase
{
    private DemandManager.CustomerDemand _demand;

    public QuoteFormStep(DemandManager.CustomerDemand demand)
        : base("报价单流程") => _demand = demand;

    public override IEnumerator Execute(FlowContext context)
    {
        OnEnter(context);

        var formData = BuildQuoteFormData(_demand);

        yield return ExecuteApprovalFlow(
            context,
            formData,
            UIManager.UIType.销售报价单,
            PositionManager.PositionType.销售员,
            PositionManager.PositionType.销售主管
        );

        OnExit(context);
        FlowStepTracker.CompleteStep();
    }

    private QuoteFormDTO BuildQuoteFormData(DemandManager.CustomerDemand demand)
    {
        var now = System.DateTime.Now;
        var total = demand.airCompressorCount * demand.unitPrice;

        return new QuoteFormDTO
        {
            BillNo = $"XSBJ-{now:yyMMdd}-{Random.Range(1, 1000):000}",
            Date = now.ToString("yyyy-MM-dd"),
            CustomerName = demand.customerName,
            Currency = "人民币",
            PriceList = "价目表 A",
            DiscountList = "折扣表 B",
            SalesGroup = "销售组 C",
            Salesperson = "销售员",
            Status = "待审核",
            ValidUntil = now.AddDays(30).ToString("yyyy-MM-dd"),
            Products = new List<ProductLineDTO> { new ProductLineDTO
            {
                ProductCode = "AC001",
                ProductName = "空压机",
                Model = "AC-2023",
                Unit = "台",
                Quantity = demand.airCompressorCount,
                UnitPrice = demand.unitPrice,
                TotalAmount = total,
                TaxRate = 0.13f,
                DiscountRate = 0f,
                DeliveryDate = "",
                Remark = "无"
            }}
        };
    }

    private IEnumerator ExecuteApprovalFlow(
        FlowContext ctx,
        QuoteFormDTO data,
        UIManager.UIType uiType,
        PositionManager.PositionType filler,
        PositionManager.PositionType approver)
    {
        var uiObj = UIManager.Instance.GetUIObject(uiType);
        var manager = uiObj.GetComponent<QuoteFormManager>();

        yield return WaitForPlayerReachPosition(
            PositionManager.Instance.GetPosition(filler));
        UIManager.Instance.ShowUI(uiType);
        manager.SetQuoteFormData(data.ToInputBoxArray(), ConvertProductsToTable(data.Products));
        yield return manager.WaitForFillButtonClick();
        UIManager.Instance.HideUI(uiType);
        FlowStepTracker.CompleteStep();

        data.Status = "已审核";
        data.Approver = "销售主管";
        yield return WaitForPlayerReachPosition(
            PositionManager.Instance.GetPosition(approver));
        UIManager.Instance.ShowUI(uiType);
        manager.SetQuoteFormData(data.ToInputBoxArray(), ConvertProductsToTable(data.Products));
        yield return manager.WaitForApproveButtonClick();
        UIManager.Instance.HideUI(uiType);
        FlowStepTracker.CompleteStep();

        yield return WaitForPlayerReachPosition(
            PositionManager.Instance.GetPosition(filler));
        UIManager.Instance.ShowUI(uiType);
        manager.SetQuoteFormData(data.ToInputBoxArray(), ConvertProductsToTable(data.Products));
        yield return manager.WaitForPushButtonClick();
        UIManager.Instance.HideUI(uiType);
        FlowStepTracker.CompleteStep();
    }

    private string[][] ConvertProductsToTable(List<ProductLineDTO> products)
    {
        var result = new string[products.Count][];
        for (int i = 0; i < products.Count; i++)
        {
            var p = products[i];
            result[i] = new string[]
            {
                p.ProductCode, p.ProductName, p.Model, p.Unit,
                p.Quantity.ToString(), p.UnitPrice.ToString("F2"),
                p.TotalAmount.ToString("F2"), (p.TaxRate*100).ToString("F0")+"%",
                p.DiscountRate==0 ? "无" : (p.DiscountRate*100).ToString("F0")+"%"
            };
        }
        return result;
    }
}