using UnityEngine;
using System.Collections;

public class QualityInspectionManager : MonoBehaviour
{
    [Header("--- 被质检的物品模型 ---")]
    public GameObject airCompressor;
    public GameObject rawMaterialBox;

    [Header("--- UI 状态层 ---")]
    public GameObject state1_Start;
    public GameObject state2_Loading;
    public GameObject state3_Pass;

    [Header("--- 动画组件 ---")]
    public Transform spinnerIcon;

    [Header("--- 视觉特效 ---")]
    public GameObject passEffect;

    public event System.Action OnInspectionComplete;

    // 防抖锁：防止动画播放中途被再次触发
    private bool isInspecting = false;
    public bool IsInspecting => isInspecting;

    void Start()
    {
        // 游戏一开始，强制清场，确保是黑屏无物状态
        InitializeScreen();
    }

    // 核心清场函数：将现场完全恢复到动画开始前的样子
    private void InitializeScreen()
    {
        if (airCompressor != null) airCompressor.SetActive(false);
        if (rawMaterialBox != null) rawMaterialBox.SetActive(false);

        if (state1_Start != null) state1_Start.SetActive(false);
        if (state2_Loading != null) state2_Loading.SetActive(false);
        if (state3_Pass != null) state3_Pass.SetActive(false);
        if (passEffect != null) passEffect.SetActive(false);
    }

    // ==========================================
    // 开放给外部调用的两个独立接口
    // ==========================================

    public void CallInspectCompressor(System.Action onComplete = null)
    {
        if (isInspecting) return;
        StartCoroutine(InspectionRoutine(airCompressor, onComplete));
    }

    public void CallInspectRawMaterial(System.Action onComplete = null)
    {
        if (isInspecting) return;
        StartCoroutine(InspectionRoutine(rawMaterialBox, onComplete));
    }

    // ==========================================
    // 底层通用动画流水线
    // ==========================================

    private IEnumerator InspectionRoutine(GameObject itemTarget, System.Action onComplete = null)
    {
        isInspecting = true;
        InitializeScreen(); // 再次确保场地干净

        // 阶段 1：物品上台，显示”开始检测”
        itemTarget.SetActive(true);
        state1_Start.SetActive(true);
        yield return new WaitForSeconds(1.0f);

        // 阶段 2：切换到加载层，图标旋转
        state1_Start.SetActive(false);
        state2_Loading.SetActive(true);

        float timer = 0f;
        while (timer < 2.5f)
        {
            // 每秒旋转 360 度
            spinnerIcon.Rotate(0, 0, -360 * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // 阶段 3：切换到通过层，显示绿勾
        state2_Loading.SetActive(false);
        state3_Pass.SetActive(true);
        if (passEffect != null) passEffect.SetActive(true);
        yield return new WaitForSeconds(2.0f); // 停留 2 秒展示结果

        // 动画结束：现场归零，解锁防抖
        InitializeScreen();
        isInspecting = false;

        onComplete?.Invoke();
    }
}