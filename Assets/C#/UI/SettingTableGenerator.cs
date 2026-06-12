using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingTableGenerator : MonoBehaviour
{
    #region 公共变量
    [Header("表格配置")]
    [SerializeField]
    private Transform tableParent; // 表格生成点
    
    [SerializeField]
    private GameObject headerCellPrefab; // 表头单元格预制体（支持旧版Text或TMP_Text）

    [SerializeField]
    private GameObject dataCellPrefab; // 数据单元格预制体（支持旧版Text或TMP_Text）
    
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
    private Dictionary<string, List<GameObject>> dataCells = new Dictionary<string, List<GameObject>>();
    #endregion
    
    #region 生命周期方法
    private void Awake()
    {
        EnsurePrefabs();
    }

    private void OnEnable()
    {
        indexColumnWidth = cellWidth * 0.5f;
        // 编辑模式下自动预览，运行时不自动建表（由BillView.FillData控制）
        if (!Application.isPlaying)
            InitializeTable();
    }
    #endregion

    /// <summary>确保必要预制体已配置，否则运行时自动创建默认的</summary>
    private void EnsurePrefabs()
    {
        if (rowPrefab == null)
        {
            var go = new GameObject("DefaultRow", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            rowPrefab = go.GetComponent<RectTransform>();
            rowPrefab.gameObject.SetActive(false); // 用作模板，默认隐藏
        }

        if (headerCellPrefab == null)
        {
            headerCellPrefab = CreateDefaultCell("DefaultHeaderCell", true);
        }

        if (dataCellPrefab == null)
        {
            dataCellPrefab = CreateDefaultCell("DefaultDataCell", false);
        }

        if (tableParent == null)
        {
            var go = new GameObject("TableContent", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            tableParent = go.transform;
        }
    }

    private GameObject CreateDefaultCell(string name, bool bold)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(transform, false);
        go.SetActive(false); // 模板隐藏

        // RectTransform 默认锚点居中
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(cellWidth, cellHeight);

        var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.fontSize = 12;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        if (bold) tmp.fontStyle = FontStyles.Bold;

        return go;
    }

    /// <summary>由 BillView 在 FillData 时调用，覆盖 prefab 上的列头配置</summary>
    public void SetColumnHeaders(string[] headers)
    {
        if (headers != null && headers.Length > 0)
            columnHeaders = headers;
    }
    
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
        if (tableParent == null || rowPrefab == null || dataCellPrefab == null)
        {
            Debug.LogError($"[SettingTableGenerator] AddRow失败: 未配置必要预制体! tableParent={tableParent}, rowPrefab={rowPrefab}, dataCellPrefab={dataCellPrefab}");
            return null;
        }

        RectTransform newRow = Instantiate(rowPrefab, tableParent);
        newRow.gameObject.SetActive(true);
        newRow.sizeDelta = new Vector2(indexColumnWidth + columnHeaders.Length * cellWidth, cellHeight);

        string rowId = "row_" + rows.Count;
        rows.Add(newRow);

        List<GameObject> rowCells = new List<GameObject>();

        // 序号列
        GameObject indexCellGO = Instantiate(dataCellPrefab, newRow);
        indexCellGO.SetActive(true);
        RectTransform indexCellRect = indexCellGO.GetComponent<RectTransform>();
        indexCellRect.anchoredPosition = new Vector2(0, 0);
        indexCellRect.sizeDelta = new Vector2(indexColumnWidth, cellHeight);
        SetCellText(indexCellGO, rows.Count.ToString());
        rowCells.Add(indexCellGO);

        // 数据列
        for (int i = 0; i < columnHeaders.Length; i++)
        {
            GameObject cellGO = Instantiate(dataCellPrefab, newRow);
            cellGO.SetActive(true);
            RectTransform cellRect = cellGO.GetComponent<RectTransform>();
            cellRect.anchoredPosition = new Vector2(indexColumnWidth + i * cellWidth, 0);
            cellRect.sizeDelta = new Vector2(cellWidth, cellHeight);
            SetCellText(cellGO, i < rowData.Length ? rowData[i] : "");
            rowCells.Add(cellGO);
        }

        dataCells[rowId] = rowCells;
        return rowId;
    }

    public void UpdateRow(string rowId, string[] rowData)
    {
        if (!dataCells.TryGetValue(rowId, out var rowCells))
        {
            Debug.LogWarning("未找到ID为" + rowId + "的行");
            return;
        }

        // 跳过第0个（序号），从第1个开始更新
        for (int i = 0; i < rowData.Length && i + 1 < rowCells.Count; i++)
            SetCellText(rowCells[i + 1], rowData[i]);
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
    /// <summary>读取所有行的数据，返回 string[][] 数组</summary>
    public string[][] GetAllRowData()
    {
        var result = new string[rows.Count][];
        for (int i = 0; i < rows.Count; i++)
        {
            string rowId = "row_" + i;
            if (dataCells.TryGetValue(rowId, out var cells))
            {
                // cells[0] 是序号，从 cells[1] 开始是数据
                result[i] = new string[columnHeaders.Length];
                for (int j = 0; j < columnHeaders.Length && j + 1 < cells.Count; j++)
                {
                    result[i][j] = GetCellText(cells[j + 1]);
                }
            }
            else
            {
                result[i] = new string[columnHeaders.Length];
            }
        }
        return result;
    }

    /// <summary>获取单个单元格的文本</summary>
    private string GetCellText(GameObject go)
    {
        var tmp = go.GetComponent<TMP_Text>();
        if (tmp != null) return tmp.text;
        var legacy = go.GetComponent<Text>();
        if (legacy != null) return legacy.text;
        return "";
    }

    /// <summary>是否有已生成的行（表格非空）</summary>
    public bool HasRows => rows.Count > 0;

    /// <summary>
    /// 创建表头行
    /// </summary>
    public void CreateHeaderRow()
    {
        if (tableParent == null || headerCellPrefab == null || rowPrefab == null)
        {
            Debug.LogError($"[SettingTableGenerator] CreateHeaderRow失败: 未配置必要预制体! tableParent={tableParent}, headerCellPrefab={headerCellPrefab}, rowPrefab={rowPrefab}");
            return;
        }

        RectTransform headerRow = Instantiate(rowPrefab, tableParent);
        headerRow.gameObject.SetActive(true);
        headerRow.sizeDelta = new Vector2(indexColumnWidth + columnHeaders.Length * cellWidth, cellHeight);
        headerRow.name = "HeaderRow";

        // 序号列表头
        GameObject indexHeaderGO = Instantiate(headerCellPrefab, headerRow);
        indexHeaderGO.SetActive(true);
        RectTransform indexCellRect = indexHeaderGO.GetComponent<RectTransform>();
        indexCellRect.anchoredPosition = new Vector2(0, 0);
        indexCellRect.sizeDelta = new Vector2(indexColumnWidth, cellHeight);
        SetCellText(indexHeaderGO, "#", true);

        // 数据列表头
        for (int i = 0; i < columnHeaders.Length; i++)
        {
            GameObject headerGO = Instantiate(headerCellPrefab, headerRow);
            headerGO.SetActive(true);
            RectTransform cellRect = headerGO.GetComponent<RectTransform>();
            cellRect.anchoredPosition = new Vector2(indexColumnWidth + i * cellWidth, 0);
            cellRect.sizeDelta = new Vector2(cellWidth, cellHeight);
            SetCellText(headerGO, columnHeaders[i], true);
        }
    }
    
    public void ClearTable()
    {
        if (tableParent == null) return;

        foreach (Transform child in tableParent)
            Destroy(child.gameObject);

        rows.Clear();
        dataCells.Clear();
    }
    #endregion

    #region 兼容工具

    /// <summary>设置单元格文字，同时兼容 TMP_Text 和旧版 Text</summary>
    private void SetCellText(GameObject go, string text, bool bold = false)
    {
        var tmp = go.GetComponent<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = text;
            if (bold) tmp.fontStyle = FontStyles.Bold;
            return;
        }
        var legacy = go.GetComponent<Text>();
        if (legacy != null)
        {
            legacy.text = text;
            if (bold) legacy.fontStyle = FontStyle.Bold;
        }
    }
    #endregion
}