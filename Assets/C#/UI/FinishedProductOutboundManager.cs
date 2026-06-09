using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 成品出库管理器 — 继承 BillPanelBase
/// 支持双输入框生成器（inputBoxGenerator + inputBoxGenerator2）
/// </summary>
public class FinishedProductOutboundManager : BillPanelBase
{
    /// <summary>
    /// 填充成品出库表单数据
    /// </summary>
    public void SetFormData(string[] data1, string[] data2, string[][] tableData)
    {
        if (inputBoxGenerator != null && data1 != null && data1.Length > 0)
            inputBoxGenerator.SetAllInputBoxContents(data1);

        if (inputBoxGenerator2 != null && data2 != null && data2.Length > 0)
            inputBoxGenerator2.SetAllInputBoxContents(data2);

        if (settingTableGenerator != null && tableData != null && tableData.Length > 0)
        {
            settingTableGenerator.ClearTable();
            settingTableGenerator.CreateHeaderRow();
            foreach (string[] rowData in tableData)
                settingTableGenerator.AddRow(rowData);
        }
    }
}
