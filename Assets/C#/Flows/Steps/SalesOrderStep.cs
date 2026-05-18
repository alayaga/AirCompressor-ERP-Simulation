using UnityEngine;
using System.Collections;

/// <summary>
/// 销售订单处理步骤：封装原 ProcessSalesOrder 逻辑
/// </summary>
public class SalesOrderStep : FlowStepBase
{
    public SalesOrderStep()
        : base("销售订单流程") { }

    public override IEnumerator Execute(FlowContext context)
    {
        OnEnter(context);

        // 复用原 ProcessSalesOrder 逻辑
        var currentUI = UIManager.Instance.GetUIObject(UIManager.UIType.销售订单);
        var manager = currentUI.GetComponent<QuoteFormManager>();
        UIManager.Instance.ShowUI(UIManager.UIType.销售订单);

        string billNo = "XSDD-251120-002 ";

        string[] inputBoxContents = new string[] {
            billNo,  "2025-11-20 ",  "客户 B ",  "美元 ",  "价目表 B ",  "折扣表 C ",
             "销售组 A ",  "销售员 ",  "暂存 ",  "无 ",  "销售员 ",  " "
        };

        float[] quantities = new float[] { 15, 8 };
        float[] unitPrices = new float[] { 80.00f, 120.00f };
        float[] discountRates = new float[] { 0.05f, 0.08f };

        string[][] tableRowData = new string[][] {
            new string[] {
                 "MAT003 ",  "产品 C ",  "型号 C ",  "箱 ", quantities[0].ToString(), unitPrices[0].ToString( "F2 "),
                (quantities[0] * unitPrices[0]).ToString( "F2 "), discountRates[0].ToString( "P0 "),
                (quantities[0] * unitPrices[0] * (1 - discountRates[0])).ToString( "F2 "),
                 "2025-11-25 ",  "0 ",  "0 ",  "0 ",  "否 ",  "备注 C "
            },
            new string[] {
                 "MAT004 ",  "产品 D ",  "型号 D ",  "套 ", quantities[1].ToString(), unitPrices[1].ToString( "F2 "),
                (quantities[1] * unitPrices[1]).ToString( "F2 "), discountRates[1].ToString( "P0 "),
                (quantities[1] * unitPrices[1] * (1 - discountRates[1])).ToString( "F2 "),
                 "2025-11-30 ",  "0 ",  "0 ",  "0 ",  "否 ",  "备注 D "
            }
        };

        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.销售员);
        yield return manager.WaitForFillButtonClick();
        manager.SetQuoteFormData(inputBoxContents, tableRowData);

        inputBoxContents[8] = "已填写 ";
        inputBoxContents[11] = "销售主管 ";
        manager.SetQuoteFormData(inputBoxContents, tableRowData);
        yield return manager.WaitForSubmitButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.销售订单);
        FlowStepTracker.CompleteStep();

        // 审核阶段
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.销售主管));
        UIManager.Instance.ShowUI(UIManager.UIType.销售订单);
        manager.SetQuoteFormData(inputBoxContents, tableRowData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.销售主管);
        yield return manager.WaitForApproveButtonClick();
        inputBoxContents[8] = "已审核 ";
        manager.SetQuoteFormData(inputBoxContents, tableRowData);
        UIManager.Instance.HideUI(UIManager.UIType.销售订单);
        FlowStepTracker.CompleteStep();

        // 下推阶段
        yield return WaitForPlayerReachPosition(PositionManager.Instance.GetPosition(PositionManager.PositionType.销售员));
        UIManager.Instance.ShowUI(UIManager.UIType.销售订单);
        manager.SetQuoteFormData(inputBoxContents, tableRowData);
        PositionManager.Instance.SetObjectToPosition(ObjectManager.Instance.GetObject(ObjectManager.ObjectType.Player), PositionManager.PositionType.销售员);
        yield return manager.WaitForPushButtonClick();
        UIManager.Instance.HideUI(UIManager.UIType.销售订单);
        FlowStepTracker.CompleteStep();

        OnExit(context);
    }
}