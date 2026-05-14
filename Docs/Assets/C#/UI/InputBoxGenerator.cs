using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 输入框生成脚本
/// 负责管理和生成多个输入框
/// </summary>
[ExecuteInEditMode] // 编辑模式执行特性（用完可注释）
public class InputBoxGenerator : MonoBehaviour
{
    #region 公共变量
    [Header("生成配置")]
    [SerializeField]
    private Transform spawnPosition;  // 生成位置

    [SerializeField]
    private GameObject inputBoxPrefab; // 输入框预制体

    [Header("标题配置")]
    [SerializeField]
    private string[] titleConfigurations; // 标题配置数组
    #endregion

    #region 私有变量
    private List<InputBoxScript> generatedInputBoxes = new List<InputBoxScript>(); // 生成的输入框列表
    #endregion

    #region 生命周期方法
    private void OnEnable()
    {
        // 仅运行模式执行初始化
        if (Application.isPlaying)
        {
            // 运行前先强制清理所有残留UI（包括编辑模式生成的）
            CleanAllResidualUI();
            InitializeInputBoxes();
        }
    }

    // 可选：运行开始时再清理一次，双重保险
    //private void Start()
    //{
    //    if (Application.isPlaying)
    //    {
    //        CleanAllResidualUI();
    //    }
    //}
    #endregion

    #region 公共方法
    public void InitializeInputBoxes()
    {
        // 清除已存在的输入框
        ClearInputBoxes();

        // 检查必要组件
        if (inputBoxPrefab == null || spawnPosition == null)
        {
            Debug.LogWarning("输入框预制体或生成位置未设置");
            return;
        }

        // 根据标题配置生成输入框
        foreach (string title in titleConfigurations)
        {
            GameObject newInputBox = Instantiate(inputBoxPrefab, spawnPosition);
            InputBoxScript inputBoxScript = newInputBox.GetComponent<InputBoxScript>();

            if (inputBoxScript != null)
            {
                inputBoxScript.SetInputBoxInfo(title, string.Empty);
                generatedInputBoxes.Add(inputBoxScript);
            }
            else
            {
                Debug.LogWarning("生成的输入框缺少InputBoxScript组件");
                if (Application.isPlaying)
                {
                    Destroy(newInputBox);
                }
                else
                {
                    DestroyImmediate(newInputBox);
                }
            }
        }
    }

    /// <param name="contents">内容数组</param>
    public void SetAllInputBoxContents(string[] contents)
    {
        if (contents == null)
        {
            Debug.LogWarning("内容数组为空");
            return;
        }

        for (int i = 0; i < generatedInputBoxes.Count && i < contents.Length; i++)
        {
            InputBoxScript inputBox = generatedInputBoxes[i];
            if (inputBox != null)
            {
                string title = inputBox.GetTitle();
                inputBox.SetInputBoxInfo(title, contents[i]);
            }
        }
    }

    public void SetInputBoxContentByTitle(string title, string content)
    {
        foreach (InputBoxScript inputBox in generatedInputBoxes)
        {
            if (inputBox != null && inputBox.GetTitle() == title)
            {
                inputBox.SetInputBoxInfo(title, content);
                break;
            }
        }
    }

    // 右键快捷生成
    [ContextMenu("编辑模式-生成完整单据面板")]
    public void GenerateInEditMode()
    {
        InitializeInputBoxes();
    }

    // 右键快捷清空
    [ContextMenu("编辑模式-清空单据面板UI")]
    public void ClearInEditMode()
    {
        ClearInputBoxes();
        CleanAllResidualUI(); // 清理所有残留
    }
    #endregion

    #region 私有方法
    // 清理列表内的输入框（原有逻辑+模式兼容）
    private void ClearInputBoxes()
    {
        foreach (InputBoxScript inputBox in generatedInputBoxes)
        {
            if (inputBox != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(inputBox.gameObject);
                }
                else
                {
                    DestroyImmediate(inputBox.gameObject);
                }
            }
        }
        generatedInputBoxes.Clear();
    }

    //强制清理spawnPosition下所有输入框（包括编辑模式残留的）
    private void CleanAllResidualUI()
    {
        if (spawnPosition == null || inputBoxPrefab == null) return;

        // 倒序遍历删除，避免索引错乱
        for (int i = spawnPosition.childCount - 1; i >= 0; i--)
        {
            Transform child = spawnPosition.GetChild(i);
            // 只删除输入框预制体生成的节点（避免误删其他UI）
            if (child.name.Contains(inputBoxPrefab.name))
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }
    #endregion
}