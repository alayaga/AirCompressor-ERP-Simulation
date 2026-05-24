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

    private int _currentStep = 0;
    private int _totalSteps = 0;

    private void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) Destroy(gameObject);
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
