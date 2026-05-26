using UnityEngine;
using TMPro;

/// <summary>
/// 任务引导面板（新版）- 支持 TextMeshPro
/// </summary>
public class TaskGuidePanelNew : MonoBehaviour
{
    #region 单例
    private static TaskGuidePanelNew _instance;
    public static TaskGuidePanelNew Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<TaskGuidePanelNew>();
            return _instance;
        }
    }
    #endregion

    [Header("文本组件")]
    [SerializeField] private TMP_Text flowNameText;
    [SerializeField] private TMP_Text taskTitleText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text currentStepText;
    [SerializeField] private TMP_Text stepDescriptionText;
    [SerializeField] private TMP_Text targetInfoText;
    [SerializeField] private TMP_Text hintText;

    [Header("面板尺寸设置")]
    [Tooltip("默认面板高度")]
    [SerializeField] private float defaultHeight = 400f;
    [Tooltip("扩展面板高度（显示更多内容时）")]
    [SerializeField] private float expandedHeight = 600f;
    
    private RectTransform _rectTransform;

    private int _currentStep = 0;
    private int _totalSteps = 0;

    private void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) Destroy(gameObject);
        
        // 获取 RectTransform 组件
        _rectTransform = GetComponent<RectTransform>();
        
    }
    

    private void Start()
    {
        // 初始化显示欢迎信息
        ShowWelcomeMessage();
    }

    /// <summary>
    /// 更新任务信息
    /// </summary>
    public void UpdateTaskInfo(string flowName, string taskTitle, string description, int currentStep, int totalSteps)
    {
        _currentStep = currentStep;
        _totalSteps = totalSteps;

        if (flowNameText != null)
            flowNameText.text = $"【{flowName}】";

        if (taskTitleText != null)
            taskTitleText.text = taskTitle;

        UpdateProgress(currentStep, totalSteps);
    }

    /// <summary>
    /// 更新当前步骤
    /// </summary>
    public void UpdateCurrentStep(string stepName, string description, string targetNPC, string targetLocation, string actionType)
    {
        if (currentStepText != null)
            currentStepText.text = $"当前任务：{stepName}";

        if (stepDescriptionText != null)
            stepDescriptionText.text = description;

        if (targetInfoText != null)
            targetInfoText.text = $"目标NPC：{targetNPC}\n位置：{targetLocation}\n操作：{actionType}";

        // 清空初始化时的提示信息
        if (hintText != null)
            hintText.text = "";
    }

    /// <summary>
    /// 更新进度
    /// </summary>
    public void UpdateProgress(int currentStep, int totalSteps)
    {
        _currentStep = currentStep;
        _totalSteps = totalSteps;

        if (progressText != null)
            progressText.text = $"进度: {currentStep}/{totalSteps}";
    }

    /// <summary>
    /// 完成当前步骤
    /// </summary>
    public void CompleteCurrentStep()
    {
        // 可以添加完成动画效果
        if (currentStepText != null)
            currentStepText.color = Color.green;
    }

    /// <summary>
    /// 完成任务
    /// </summary>
    public void CompleteTask()
    {
        if (currentStepText != null)
            currentStepText.text = "🎉 流程完成！";

        if (hintText != null)
            hintText.text = "恭喜完成所有步骤！";
    }

    /// <summary>
    /// 更新提示文本
    /// </summary>
    /// <param name="text">提示内容</param>
    public void UpdateHintText(string text)
    {
        if (hintText != null)
            hintText.text = text;
    }

    #region 面板尺寸调整方法

    /// <summary>
    /// 设置面板为默认高度
    /// </summary>
    public void SetDefaultHeight()
    {
        SetPanelHeight(defaultHeight);
    }

    /// <summary>
    /// 设置面板为扩展高度（更长）
    /// </summary>
    public void SetExpandedHeight()
    {
        SetPanelHeight(expandedHeight);
    }

    /// <summary>
    /// 设置面板高度
    /// </summary>
    /// <param name="height">目标高度</param>
    public void SetPanelHeight(float height)
    {
        if (_rectTransform != null)
        {
            _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, height);
            Debug.Log($"[TaskGuidePanelNew] 面板高度已设置为: {height}");
        }
        else
        {
            Debug.LogWarning("[TaskGuidePanelNew] RectTransform 组件未找到！");
        }
    }

    /// <summary>
    /// 获取当前面板高度
    /// </summary>
    /// <returns>当前高度</returns>
    public float GetCurrentHeight()
    {
        if (_rectTransform != null)
        {
            return _rectTransform.sizeDelta.y;
        }
        return 0f;
    }

    /// <summary>
    /// 动态调整面板高度以适应内容
    /// </summary>
    public void AutoAdjustHeight()
    {
        // 获取所有文本组件的总高度
        float totalTextHeight = 0f;
        
        if (flowNameText != null) totalTextHeight += flowNameText.preferredHeight;
        if (taskTitleText != null) totalTextHeight += taskTitleText.preferredHeight;
        if (progressText != null) totalTextHeight += progressText.preferredHeight;
        if (currentStepText != null) totalTextHeight += currentStepText.preferredHeight;
        if (stepDescriptionText != null) totalTextHeight += stepDescriptionText.preferredHeight;
        if (targetInfoText != null) totalTextHeight += targetInfoText.preferredHeight;
        if (hintText != null) totalTextHeight += hintText.preferredHeight;
        
        // 添加间距
        float targetHeight = totalTextHeight + 60f; // 60f 是边距
        
        // 限制最小和最大高度
        targetHeight = Mathf.Clamp(targetHeight, defaultHeight, expandedHeight);
        
        SetPanelHeight(targetHeight);
    }

    #endregion

    /// <summary>
    /// 显示欢迎信息
    /// </summary>
    private void ShowWelcomeMessage()
    {
        if (flowNameText != null)
            flowNameText.text = "【任务引导】";

        if (taskTitleText != null)
            taskTitleText.text = "欢迎来到空压机制造模拟";

        if (currentStepText != null)
            currentStepText.text = "等待任务开始...";

        if (hintText != null)
            hintText.text = "游戏加载中...";
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    public void ShowPanel()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    public void HidePanel()
    {
        gameObject.SetActive(false);
    }
}
