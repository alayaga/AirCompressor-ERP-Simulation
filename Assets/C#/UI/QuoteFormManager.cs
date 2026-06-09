using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 报价单/销售订单管理器 — 继承 BillPanelBase
/// 仅保留销售订单特有的业务逻辑（SetQuoteFormData、发货条件检查等）
/// </summary>
public class QuoteFormManager : BillPanelBase
{
    /// <summary>
    /// 填充报价单/销售订单数据（由外部流程调用）
    /// </summary>
    public void SetQuoteFormData(string[] inputBoxContents, string[][] tableRowData)
    {
        if (inputBoxGenerator != null && inputBoxContents != null && inputBoxContents.Length > 0)
            inputBoxGenerator.SetAllInputBoxContents(inputBoxContents);

        if (settingTableGenerator != null && tableRowData != null && tableRowData.Length > 0)
        {
            settingTableGenerator.ClearTable();
            settingTableGenerator.CreateHeaderRow();
            foreach (string[] rowData in tableRowData)
                settingTableGenerator.AddRow(rowData);
        }
    }

    /// <summary>
    /// 发货条件检查（销售订单专用）
    /// </summary>
    protected override bool CheckShipConditions(out string failReason)
    {
        // TODO: 后续接入仓库通知系统、客户确认状态、订单审核状态
        // 暂时默认通过
        failReason = null;
        return true;
    }
}
