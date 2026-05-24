using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 任务引导面板
/// </summary>
public class TaskGuidePanel : MonoBehaviour
{
    #region 单例
    private static TaskGuidePanel _instance;
    public static TaskGuidePanel Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<TaskGuidePanel>();
            return _instance;
        }
    }
    #endregion

    [Header("面板")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("任务信息")]
    [SerializeField] private Text taskTitleText;
    [SerializeField] private Text taskDescriptionText;
    [SerializeField] private Text currentStepText;
    [SerializeField] private Text progressText;
    
    [Header("目标信息")]
    [SerializeField] private Text targetNPCText;
    [SerializeField] private Text targetLocationText;
    [SerializeField] private Text actionTypeText;
    
    [Header("提示")]
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private Text hintText;
    
    [Header("时间")]
    [SerializeField] private Text localTimeText;
    
    [Header("折叠按钮")]
    [SerializeField] private Button toggleButton;
    [SerializeField] private Text toggleButtonText;
    
    [Header("样式")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.9f);
    [SerializeField] private Color completedColor = new Color(0.3f, 1f, 0.3f, 0.9f);
    [SerializeField] private Color highlightColor = new Color(1f, 0.8f, 0.2f, 0.9f);
    
    [Header("动画")]
    [SerializeField] private float fadeDuration = 0.3f;

    private bool isPanelExpanded = true;
    private Vector2 expandedSize = new Vector2(350, 400);
    private Vector2 collapsedSize = new Vector2(350, 60);
    private RectTransform panelRectTransform;
    private bool isAnimating = false;
    private string currentFlowName = "";
    private int currentStep = 0;
    private int totalSteps = 0;

    private void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) { Destroy(gameObject); return; }
        
        if (panelRoot == null) panelRoot = gameObject;
        panelRectTransform = panelRoot?.GetComponent<RectTransform>();
        
        if (canvasGroup == null && panelRoot != null)
            canvasGroup = panelRoot.GetComponent<CanvasGroup>() ?? panelRoot.AddComponent<CanvasGroup>();
        
        if (toggleButton != null) toggleButton.onClick.AddListener(TogglePanel);
        if (hintPanel != null) hintPanel.SetActive(false);
    }

    private void Start()
    {
        // FlowTaskIntegration.Instance?.SetTaskGuidePanel(this);
        UpdateToggleButtonText();
        ShowWelcomeMessage();
        if (panelRoot != null && !panelRoot.activeSelf) panelRoot.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    private void Update()
    {
        UpdateLocalTime();
    }

    private void OnDestroy()
    {
        if (toggleButton != null) toggleButton.onClick.RemoveListener(TogglePanel);
    }

    public void UpdateTaskInfo(string flowName, string taskTitle, string taskDescription, int currentStep, int totalSteps)
    {
        this.currentFlowName = flowName;
        this.currentStep = currentStep;
        this.totalSteps = totalSteps;
        
        if (taskTitleText != null)
        {
            taskTitleText.text = $"【{flowName}】{taskTitle}";
            taskTitleText.color = highlightColor;
        }
        
        if (taskDescriptionText != null) taskDescriptionText.text = taskDescription;
        
        UpdateProgress(currentStep, totalSteps);
        ShowPanel();
    }

    public void UpdateCurrentStep(string stepTitle, string stepDescription, 
                                  string targetNPC = "", string targetLocation = "", 
                                  string actionType = "", string hint = "")
    {
        if (currentStepText != null)
        {
            currentStepText.text = $"➤ {stepTitle}";
            currentStepText.color = highlightColor;
        }
        
        if (targetNPCText != null)
        {
            if (!string.IsNullOrEmpty(targetNPC))
            {
                targetNPCText.text = $"🎯 前往找：{targetNPC}";
                targetNPCText.color = highlightColor;
                targetNPCText.gameObject.SetActive(true);
            }
            else targetNPCText.gameObject.SetActive(false);
        }
        
        if (targetLocationText != null)
        {
            if (!string.IsNullOrEmpty(targetLocation))
            {
                targetLocationText.text = $"📍 位置：{targetLocation}";
                targetLocationText.color = normalColor;
                targetLocationText.gameObject.SetActive(true);
            }
            else targetLocationText.gameObject.SetActive(false);
        }
        
        if (actionTypeText != null)
        {
            if (!string.IsNullOrEmpty(actionType))
            {
                actionTypeText.text = $"✋ 操作：{actionType}";
                actionTypeText.color = normalColor;
                actionTypeText.gameObject.SetActive(true);
            }
            else actionTypeText.gameObject.SetActive(false);
        }
        
        if (!string.IsNullOrEmpty(hint)) ShowHint(hint);
        else HideHint();
    }

    public void UpdateProgress(int current, int total)
    {
        this.currentStep = current;
        this.totalSteps = total;
        
        if (progressText != null)
        {
            progressText.text = $"进度: {current}/{total}";
            progressText.color = current >= total ? completedColor : normalColor;
        }
    }

    public void CompleteCurrentStep()
    {
        if (currentStepText != null)
        {
            string originalText = currentStepText.text.Replace("➤ ", "");
            currentStepText.text = "✓ " + originalText;
            currentStepText.color = completedColor;
        }
        StartCoroutine(PlayStepCompleteAnimation());
    }

    public void CompleteTask()
    {
        if (taskTitleText != null)
        {
            taskTitleText.color = completedColor;
            taskTitleText.text = "✓ " + taskTitleText.text;
        }
        
        if (currentStepText != null)
        {
            currentStepText.text = "🎉 任务完成！";
            currentStepText.color = completedColor;
        }
        
        StartCoroutine(PlayTaskCompleteAnimation());
    }

    public void ShowPanel()
    {
        if (panelRoot != null && !panelRoot.activeSelf) panelRoot.SetActive(true);
        if (canvasGroup != null && !isAnimating) StartCoroutine(FadeIn());
        else if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    public void HidePanel()
    {
        if (!isAnimating) StartCoroutine(FadeOut());
    }

    public void TogglePanel()
    {
        if (isAnimating) return;
        isPanelExpanded = !isPanelExpanded;
        UpdateToggleButtonText();
        StartCoroutine(AnimatePanelSize());
    }

    private void UpdateToggleButtonText()
    {
        if (toggleButtonText != null) toggleButtonText.text = isPanelExpanded ? "▼" : "▲";
    }

    public void ShowHint(string hint)
    {
        if (hintPanel != null && hintText != null)
        {
            hintText.text = $"💡 {hint}";
            hintPanel.SetActive(true);
        }
    }

    public void HideHint()
    {
        if (hintPanel != null) hintPanel.SetActive(false);
    }

    public void ShowTemporaryHint(string hint, float duration = 3f)
    {
        StartCoroutine(ShowTemporaryHintCoroutine(hint, duration));
    }

    private IEnumerator ShowTemporaryHintCoroutine(string hint, float duration)
    {
        ShowHint(hint);
        yield return new WaitForSeconds(duration);
        HideHint();
    }

    private IEnumerator FadeIn()
    {
        isAnimating = true;
        if (canvasGroup != null)
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
        isAnimating = false;
    }

    private IEnumerator FadeOut()
    {
        isAnimating = true;
        if (canvasGroup != null)
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
        if (panelRoot != null) panelRoot.SetActive(false);
        isAnimating = false;
    }

    private IEnumerator AnimatePanelSize()
    {
        isAnimating = true;
        if (panelRectTransform != null)
        {
            Vector2 startSize = panelRectTransform.sizeDelta;
            Vector2 targetSize = isPanelExpanded ? expandedSize : collapsedSize;
            float elapsed = 0f;
            float duration = 0.3f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                panelRectTransform.sizeDelta = Vector2.Lerp(startSize, targetSize, elapsed / duration);
                yield return null;
            }
            panelRectTransform.sizeDelta = targetSize;
        }
        isAnimating = false;
    }

    private IEnumerator PlayStepCompleteAnimation()
    {
        if (currentStepText != null)
        {
            for (int i = 0; i < 2; i++)
            {
                currentStepText.color = completedColor;
                yield return new WaitForSeconds(0.1f);
                currentStepText.color = Color.white;
                yield return new WaitForSeconds(0.1f);
            }
            currentStepText.color = completedColor;
        }
    }

    private IEnumerator PlayTaskCompleteAnimation()
    {
        if (panelRectTransform != null)
        {
            Vector3 originalScale = panelRectTransform.localScale;
            float elapsed = 0f;
            float duration = 0.2f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float scale = Mathf.Lerp(1f, 1.1f, elapsed / duration);
                panelRectTransform.localScale = originalScale * scale;
                yield return null;
            }
            
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float scale = Mathf.Lerp(1.1f, 1f, elapsed / duration);
                panelRectTransform.localScale = originalScale * scale;
                yield return null;
            }
            panelRectTransform.localScale = originalScale;
        }
        ShowTemporaryHint("任务完成！", 2f);
    }

    private void UpdateLocalTime()
    {
        if (localTimeText != null)
            localTimeText.text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    private void ShowWelcomeMessage()
    {
        if (taskTitleText != null) { taskTitleText.text = "【系统】任务引导"; taskTitleText.color = normalColor; }
        if (taskDescriptionText != null) taskDescriptionText.text = "等待流程开始...";
        if (currentStepText != null) { currentStepText.text = "➤ 等待任务分配"; currentStepText.color = normalColor; }
        if (progressText != null) progressText.text = "进度：--";
        if (targetNPCText != null) targetNPCText.gameObject.SetActive(false);
        if (targetLocationText != null) targetLocationText.gameObject.SetActive(false);
        if (actionTypeText != null) actionTypeText.gameObject.SetActive(false);
        UpdateProgress(0, 0);
        HideHint();
    }

    public void ClearTaskInfo() { ShowWelcomeMessage(); }
}
