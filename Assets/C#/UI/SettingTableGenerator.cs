using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingTableGenerator : MonoBehaviour
{
    #region 公共变量
    [Header("表格配置")]
    [SerializeField]
    private Transform tableParent; // 表格生成点
    
    [SerializeField]
    private Text headerCellPrefab; // 表头单元格预制体
    
    [SerializeField]
    private Text dataCellPrefab; // 数据单元格预制体
    
    [SerializeField]
    private RectTransform rowPrefab; // 行预制体
    
    [Header("列配置")]
    [SerializeField]
    private string[] columnHeaders; // 列标题数组
    
    [Header("表格样式")]
    [SerializeField]
    private float cellWidth = 100f; // 单元格宽度
    [SerializeField]
    private float indexColumnWidth; // 序号列宽度（其他列的一半）
    [SerializeField]
    private float cellHeight = 30f; // 单元格高度
    #endregion
    
    #region 私有变量
    private List<RectTransform> rows = new List<RectTransform>(); // 存储生成的行
    private Dictionary<string, List<Text>> dataCells = new Dictionary<string, List<Text>>(); // 存储数据单元格，便于更新
    #endregion
    
    #region 生命周期方法
    private void OnEnable()
    {
        // 设置序号列宽度为其他列的一半
        indexColumnWidth = cellWidth * 0.5f;
        // 初始化表格
        InitializeTable();
    }
    #endregion
    
    #region 公共方法
    public void InitializeTable()
    {        
        // 检查必要组件
        if (tableParent == null || headerCellPrefab == null || rowPrefab == null || dataCellPrefab == null)
        {            
            return;
        }
        
        // 清除现有表格内容
        ClearTable();
        
        // 创建表头行
        CreateHeaderRow();
        
        // 默认创建两行空数据
        string[] emptyRow1 = new string[columnHeaders.Length];
        string[] emptyRow2 = new string[columnHeaders.Length];
        for (int i = 0; i < columnHeaders.Length; i++)
        {
            emptyRow1[i] = "";
            emptyRow2[i] = "";
        }
        AddRow(emptyRow1);
        AddRow(emptyRow2);
    }
    
    public string AddRow(string[] rowData)
    {        
        // 创建新行
        RectTransform newRow = Instantiate(rowPrefab, tableParent);
        // 计算行宽（序号列+其他列）
        newRow.sizeDelta = new Vector2(indexColumnWidth + columnHeaders.Length * cellWidth, cellHeight);
        
        // 生成行ID
        string rowId = "row_" + rows.Count;
        rows.Add(newRow);
        
        // 存储该行的单元格
        List<Text> rowCells = new List<Text>();
        
        // 创建序号列单元格
        Text indexCell = Instantiate(dataCellPrefab, newRow);
        RectTransform indexCellRect = indexCell.GetComponent<RectTransform>();
        indexCellRect.anchoredPosition = new Vector2(0, 0);
        indexCellRect.sizeDelta = new Vector2(indexColumnWidth, cellHeight);
        // 设置序号文本（从1开始计数）
        indexCell.text = (rows.Count).ToString();
        rowCells.Add(indexCell);
        
        // 创建并设置数据单元格
        for (int i = 0; i < columnHeaders.Length; i++)
        {            
            Text cell = Instantiate(dataCellPrefab, newRow);
            RectTransform cellRect = cell.GetComponent<RectTransform>();
            // 位置计算需要考虑序号列
            cellRect.anchoredPosition = new Vector2(indexColumnWidth + i * cellWidth, 0);
            cellRect.sizeDelta = new Vector2(cellWidth, cellHeight);
            
            // 设置单元格内容
            if (i < rowData.Length)
            {
                cell.text = rowData[i];
            }
            else
            {
                cell.text = string.Empty;
            }
            
            rowCells.Add(cell);
        }
        
        // 存储该行的单元格引用
        dataCells[rowId] = rowCells;
        
        return rowId;
    }
    
    public void UpdateRow(string rowId, string[] rowData)
    {        
        if (dataCells.ContainsKey(rowId))
        {            
            List<Text> rowCells = dataCells[rowId];
            
            // 更新单元格内容（跳过第0个，它是序号）
            for (int i = 0; i < rowData.Length && i + 1 < rowCells.Count; i++)
            {                
                rowCells[i + 1].text = rowData[i];
            }
        }
        else
        {            
            Debug.LogWarning("未找到ID为" + rowId + "的行");
        }
    }
    
    /// <summary>
    /// 根据索引更新行数据
    /// </summary>
    /// <param name="rowIndex">行索引</param>
    /// <param name="rowData">新的行数据</param>
    public void UpdateRowByIndex(int rowIndex, string[] rowData)
    {        
        if (rowIndex >= 0 && rowIndex < rows.Count)
        {            
            string rowId = "row_" + rowIndex;
            UpdateRow(rowId, rowData);
        }
        else
        {            
            Debug.LogWarning("行索引超出范围");
        }
    }
    /// <summary>
    /// 创建表头行
    /// </summary>
    public void CreateHeaderRow()
    {        
        // 创建表头行
        RectTransform headerRow = Instantiate(rowPrefab, tableParent);
        // 计算行宽（序号列+其他列）
        headerRow.sizeDelta = new Vector2(indexColumnWidth + columnHeaders.Length * cellWidth, cellHeight);
        
        // 标记为表头行
        headerRow.name = "HeaderRow";
        
        // 创建序号列表头
        Text indexHeaderCell = Instantiate(headerCellPrefab, headerRow);
        RectTransform indexCellRect = indexHeaderCell.GetComponent<RectTransform>();
        indexCellRect.anchoredPosition = new Vector2(0, 0);
        indexCellRect.sizeDelta = new Vector2(indexColumnWidth, cellHeight);
        indexHeaderCell.text = "#";
        indexHeaderCell.fontStyle = FontStyle.Bold;
        
        // 创建数据列表头单元格
        for (int i = 0; i < columnHeaders.Length; i++)
        {            
            Text headerCell = Instantiate(headerCellPrefab, headerRow);
            RectTransform cellRect = headerCell.GetComponent<RectTransform>();
            // 位置计算需要考虑序号列
            cellRect.anchoredPosition = new Vector2(indexColumnWidth + i * cellWidth, 0);
            cellRect.sizeDelta = new Vector2(cellWidth, cellHeight);
            
            // 设置表头文本
            headerCell.text = columnHeaders[i];
            
            // 可以在这里设置表头样式（如粗体等）
            headerCell.fontStyle = FontStyle.Bold;
        }
    }
    
    /// <summary>
    /// 清空整个表格（包括表头）
    /// </summary>
    public void ClearTable()
    {        
        // 检查 tableParent 是否为空
        if (tableParent == null)
        {            
            return;
        }
        
        // 销毁所有子对象
        foreach (Transform child in tableParent)
        {            
            Destroy(child.gameObject);
        }
        
        rows.Clear();
        dataCells.Clear();
    }
    #endregion
}