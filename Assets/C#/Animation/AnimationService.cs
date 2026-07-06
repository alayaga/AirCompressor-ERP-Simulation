using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 动画服务单例：管理动画播放期间的全部状态切换（锁定玩家、隐藏UI、切换相机、执行序列）
/// </summary>
public class AnimationService : MonoBehaviour
{
    #region Singleton

    private static AnimationService _instance;
    public static AnimationService Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<AnimationService>();
            return _instance;
        }
    }

    #endregion

    #region Inspector - 相机

    [Header("=== 相机 ===")]
    public CameraController cameraController;

    [Header("=== 观看位置锚点 ===")]
    public Transform view_Workshop1;           // 弯管车间
    public Transform view_Workshop2;           // 焊接车间
    public Transform view_Workshop3;           // 配电车间
    public Transform view_Workshop4;           // 总装车间
    public Transform view_Inspection_Compressor;  // 成品质检
    public Transform view_Inspection_RawMaterial; // 原料质检

    #endregion

    #region Inspector - 车间1 弯管

    [Header("=== 车间1 弯管 ===")]
    public Animator worker1_Bender;
    public Animator pipeBenderMachine;
    public GameObject pipe_Straight;
    public GameObject pipe_Bent;
    public ParticleSystem benderSmoke;

    #endregion

    #region Inspector - 车间2 焊接

    [Header("=== 车间2 焊接 ===")]
    public Animator worker2_Welder;
    public Animator roboticArm;
    public GameObject tank_Gold_OnTable;
    public GameObject combo_Welded;
    public ParticleSystem weldSparks;
    public Transform hoverPoint;
    public Transform conveyorPoint1;
    public Transform weldStationPoint;

    #endregion

    #region Inspector - 车间3 配电

    [Header("=== 车间3 配电 ===")]
    public Animator worker3_Electric;
    public GameObject motor_Black_OnTable;
    public GameObject combo_Electric;
    public ParticleSystem electricSmoke;
    public Transform electricStationPoint;

    #endregion

    #region Inspector - 车间4 总装

    [Header("=== 车间4 总装 ===")]
    public Animator worker4_Final;
    public GameObject compressor_Final;
    public ParticleSystem finalSmoke;
    public Transform cornerPoint;
    public Transform finalStationBackPoint;
    public Transform finalStationFrontPoint;

    #endregion

    #region Inspector - 设置

    [Header("=== 全局设置 ===")]
    public float moveSpeed = 2f;

    #endregion

    #region 私有状态

    public bool IsPlayingAnimation { get; private set; }

    private SimpleFirstPersonController _playerController;
    private CrosshairUI _crosshairUI;
    private InteractionManager _interactionManager;
    private GameObject _playerModel;  // 玩家模型（用于动画期间隐藏）

    // 初始坐标记录（用于重置）
    private Vector3 _startPos_PipeBent;
    private Vector3 _startPos_ComboWelded;
    private Vector3 _startPos_ComboElectric;
    private Vector3 _startPos_Worker1, _startPos_Worker2, _startPos_Worker3, _startPos_Worker4;
    private Quaternion _startRot_Worker1, _startRot_Worker2, _startRot_Worker3, _startRot_Worker4;

    #endregion

    #region Unity 生命周期

    private void Awake()
    {
        // 场景重载时，旧实例持有的 Inspector 引用（viewpoint/animator等）全部失效
        // 新实例有正确的引用 → 销毁旧实例，新实例接管单例
        if (_instance != null && _instance != this)
        {
            Destroy(_instance.gameObject);
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        RecordInitialPositions();
        InitializeSystem();
    }

    /// <summary>
    /// 每次播放前刷新运行时引用（解决 Start 执行顺序问题 + 场景重载后的失效引用）
    /// </summary>
    private void RefreshRuntimeReferences()
    {
        // CameraController 在同一物体上
        if (cameraController == null)
            cameraController = GetComponent<CameraController>();

        // 重新查找玩家相关引用
        var playerObj = ObjectManager.Instance?.GetObject(ObjectManager.ObjectType.Player);
        if (playerObj != null)
        {
            _playerController = playerObj.GetComponent<SimpleFirstPersonController>();
            _playerModel = playerObj;
        }

        // 重新查找 CrosshairUI
        _crosshairUI = FindObjectOfType<CrosshairUI>();
        if (_crosshairUI == null)
        {
            var canvases = FindObjectsOfType<Canvas>();
            foreach (var c in canvases)
            {
                _crosshairUI = c.GetComponentInChildren<CrosshairUI>(true);
                if (_crosshairUI != null) break;
            }
        }

        _interactionManager = InteractionManager.Instance;
    }

    #endregion

    #region 公开API — 外部调用入口

    /// <summary>
    /// 播放指定车间的生产动画
    /// </summary>
    /// <param name="workshopId">"1-弯管" / "2-焊接" / "3-配电" / "4-总装"</param>
    /// <param name="onComplete">动画全部完成后的回调（用于 MarkStepComplete）</param>
    public void PlayWorkshopAnimation(string workshopId, Action onComplete)
    {
        RefreshRuntimeReferences();
        Debug.Log($"[AnimationService] PlayWorkshopAnimation 开始: {workshopId}");
        if (IsPlayingAnimation)
        {
            Debug.LogWarning($"[AnimationService] 动画正在播放中，忽略请求: {workshopId}");
            return;
        }

        Transform viewpoint = GetViewpoint(workshopId);
        if (viewpoint == null)
        {
            Debug.LogError($"[AnimationService] 未找到观看位置: {workshopId}，直接回调");
            onComplete?.Invoke();
            return;
        }

        IsPlayingAnimation = true;

        LockPlayer();
        HideUI();

        cameraController.SwitchToViewpoint(viewpoint, () =>
        {
            Debug.Log("[AnimationService] 相机就位，开始序列");
            StartCoroutine(RunWorkshopSequence(workshopId, () =>
            {
                Debug.Log("[AnimationService] 序列完成，恢复相机");
                cameraController.RestoreToPlayer(() =>
                {
                    Debug.Log("[AnimationService] 相机已恢复，显示UI+解锁");
                    ShowUI();
                    UnlockPlayer();
                    IsPlayingAnimation = false;
                    Debug.Log("[AnimationService] 播放完毕，调用onComplete");
                    onComplete?.Invoke();
                });
            }));
        });
    }

    /// <summary>
    /// 播放质检动画（由 QualityInspectionTrigger 调用）
    /// </summary>
    public void PlayInspectionAnimation(string inspectionType, QualityInspectionManager manager, System.Action onComplete)
    {
        RefreshRuntimeReferences();
        if (IsPlayingAnimation)
        {
            Debug.LogWarning($"[AnimationService] 动画正在播放中，忽略质检请求: {inspectionType}");
            return;
        }

        string viewpointKey = inspectionType == "compressor" ? "inspection-compressor" : "inspection-rawMaterial";
        Transform viewpoint = GetViewpoint(viewpointKey);

        IsPlayingAnimation = true;

        LockPlayer();
        HideUI();

        System.Action doInspection = () =>
        {
            System.Action onInspectionDone = () =>
            {
                cameraController.RestoreToPlayer(() =>
                {
                    ShowUI();
                    UnlockPlayer();
                    IsPlayingAnimation = false;
                    onComplete?.Invoke();
                });
            };

            if (inspectionType == "compressor")
                manager.CallInspectCompressor(onInspectionDone);
            else
                manager.CallInspectRawMaterial(onInspectionDone);
        };

        if (viewpoint != null)
        {
            cameraController.SwitchToViewpoint(viewpoint, doInspection);
        }
        else
        {
            Debug.LogWarning($"[AnimationService] 未找到质检观看位置: {viewpointKey}，在原地播放");
            doInspection();
        }
    }

    #endregion

    #region 观看位置查找

    private Transform GetViewpoint(string id)
    {
        switch (id)
        {
            case "1-弯管":                  return view_Workshop1;
            case "2-焊接":                  return view_Workshop2;
            case "3-配电":                  return view_Workshop3;
            case "4-总装":                  return view_Workshop4;
            case "inspection-compressor":   return view_Inspection_Compressor;
            case "inspection-rawMaterial":  return view_Inspection_RawMaterial;
            default:
                Debug.LogWarning($"[AnimationService] 未知的观看位置ID: {id}");
                return null;
        }
    }

    #endregion

    #region 序列调度

    private IEnumerator RunWorkshopSequence(string workshopId, Action onComplete)
    {
        // 注意：不调用 InitializeSystem()！每个车间独立运行，产物在车间之间传递
        // 仅车间1需要重置全局状态（由 AnimationService.Start() 完成初始重置）
        yield return null;

        Debug.Log($"[AnimationService] RunWorkshopSequence 开始: {workshopId}");
        switch (workshopId)
        {
            case "1-弯管":
                yield return StartCoroutine(Sequence_Workshop1_Bending());
                break;
            case "2-焊接":
                yield return StartCoroutine(Sequence_Workshop2_Welding());
                break;
            case "3-配电":
                yield return StartCoroutine(Sequence_Workshop3_Electric());
                break;
            case "4-总装":
                yield return StartCoroutine(Sequence_Workshop4_FinalAssembly());
                break;
            default:
                Debug.LogWarning($"[AnimationService] 未知车间: {workshopId}");
                yield return new WaitForSeconds(1f);
                break;
        }

        Debug.Log($"[AnimationService] RunWorkshopSequence 完成: {workshopId}");
        onComplete?.Invoke();
    }

    #endregion

    #region 玩家锁定 / UI 控制

    private Renderer[] _playerRenderers;  // 玩家身上的渲染器（动画期间隐藏）

    private void LockPlayer()
    {
        if (_playerController != null)
            _playerController.SetPlayerInputEnabled(false);
        if (_interactionManager != null)
            _interactionManager.SetInteractionEnabled(false);

        // 隐藏玩家模型渲染器（避免动画中出现玩家自己的模型）
        if (_playerModel != null)
        {
            _playerRenderers = _playerModel.GetComponentsInChildren<Renderer>();
            foreach (var r in _playerRenderers)
                r.enabled = false;
        }
    }

    private void UnlockPlayer()
    {
        if (_playerController != null)
            _playerController.SetPlayerInputEnabled(true);
        if (_interactionManager != null)
            _interactionManager.SetInteractionEnabled(true);

        // 恢复玩家模型渲染器
        if (_playerRenderers != null)
        {
            foreach (var r in _playerRenderers)
                if (r != null) r.enabled = true;
            _playerRenderers = null;
        }
    }

    private void HideUI()
    {
        if (_crosshairUI != null)
        {
            _crosshairUI.gameObject.SetActive(false);
            Debug.Log("[AnimationService] 准星物体已隐藏");
        }

        if (TaskGuidePanelNew.Instance != null)
        {
            TaskGuidePanelNew.Instance.gameObject.SetActive(false);
            Debug.Log("[AnimationService] TaskGuidePanel 已隐藏");
        }

        var promptUI = FindObjectOfType<InteractionPromptUI>();
        if (promptUI != null)
        {
            promptUI.gameObject.SetActive(false);
            Debug.Log("[AnimationService] InteractionPromptUI 已隐藏");
        }
    }

    private void ShowUI()
    {
        if (_crosshairUI != null)
        {
            _crosshairUI.gameObject.SetActive(true);
            Debug.Log("[AnimationService] 准星物体已恢复");
        }

        if (TaskGuidePanelNew.Instance != null)
        {
            TaskGuidePanelNew.Instance.gameObject.SetActive(true);
            Debug.Log("[AnimationService] TaskGuidePanel 已恢复");
        }
    }

    #endregion

    #region 动画工具方法

    private void RecordInitialPositions()
    {
        if (pipe_Bent != null) _startPos_PipeBent = pipe_Bent.transform.position;
        if (combo_Welded != null) _startPos_ComboWelded = combo_Welded.transform.position;
        if (combo_Electric != null) _startPos_ComboElectric = combo_Electric.transform.position;

        if (worker1_Bender != null)
        { _startPos_Worker1 = worker1_Bender.transform.position; _startRot_Worker1 = worker1_Bender.transform.rotation; }
        if (worker2_Welder != null)
        { _startPos_Worker2 = worker2_Welder.transform.position; _startRot_Worker2 = worker2_Welder.transform.rotation; }
        if (worker3_Electric != null)
        { _startPos_Worker3 = worker3_Electric.transform.position; _startRot_Worker3 = worker3_Electric.transform.rotation; }
        if (worker4_Final != null)
        { _startPos_Worker4 = worker4_Final.transform.position; _startRot_Worker4 = worker4_Final.transform.rotation; }
    }

    private void InitializeSystem()
    {
        // 零件归位
        if (pipe_Bent != null) pipe_Bent.transform.position = _startPos_PipeBent;
        if (combo_Welded != null) combo_Welded.transform.position = _startPos_ComboWelded;
        if (combo_Electric != null) combo_Electric.transform.position = _startPos_ComboElectric;

        // 工人归位
        if (worker1_Bender != null) worker1_Bender.transform.SetPositionAndRotation(_startPos_Worker1, _startRot_Worker1);
        if (worker2_Welder != null) worker2_Welder.transform.SetPositionAndRotation(_startPos_Worker2, _startRot_Worker2);
        if (worker3_Electric != null) worker3_Electric.transform.SetPositionAndRotation(_startPos_Worker3, _startRot_Worker3);
        if (worker4_Final != null) worker4_Final.transform.SetPositionAndRotation(_startPos_Worker4, _startRot_Worker4);

        // 模型显隐重置
        if (pipe_Straight != null) pipe_Straight.SetActive(true);
        if (pipe_Bent != null) pipe_Bent.SetActive(false);
        if (tank_Gold_OnTable != null) tank_Gold_OnTable.SetActive(true);
        if (combo_Welded != null) combo_Welded.SetActive(false);
        if (motor_Black_OnTable != null) motor_Black_OnTable.SetActive(true);
        if (combo_Electric != null) combo_Electric.SetActive(false);
        if (compressor_Final != null) compressor_Final.SetActive(false);

        // 特效清空
        StopAndHideEffect(benderSmoke);
        StopAndHideEffect(weldSparks);
        StopAndHideEffect(electricSmoke);
        StopAndHideEffect(finalSmoke);

        // Animator归零
        ResetAnimator(worker1_Bender);
        ResetAnimator(pipeBenderMachine);
        ResetAnimator(worker2_Welder);
        ResetAnimator(roboticArm);
        ResetAnimator(worker3_Electric);
        ResetAnimator(worker4_Final);
    }

    private void ResetAnimator(Animator anim)
    {
        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }
    }

    private void PlayEffect(ParticleSystem effect)
    {
        if (effect != null)
        {
            effect.gameObject.SetActive(false);
            effect.gameObject.SetActive(true);
            effect.time = 0f;
            effect.Play(true);
        }
    }

    private void StopAndHideEffect(ParticleSystem effect)
    {
        if (effect != null)
        {
            effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            effect.gameObject.SetActive(false);
        }
    }

    private IEnumerator MoveObject(Transform obj, Vector3 targetPos)
    {
        if (obj == null) yield break;
        while (Vector3.Distance(obj.position, targetPos) > 0.01f)
        {
            obj.position = Vector3.MoveTowards(obj.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        obj.position = targetPos;
    }

    #endregion

    #region 车间动画序列（移植自 ProductionPipelineManager）

    // ==================== 车间1：弯管 ====================
    private IEnumerator Sequence_Workshop1_Bending()
    {
        // 车间1是起点：重置全部状态确保场地干净
        InitializeSystem();
        yield return null;

        ResetAnimator(worker1_Bender);
        ResetAnimator(pipeBenderMachine);
        yield return null;

        // 工人走到机器前
        worker1_Bender.SetTrigger("little_walk");
        yield return new WaitForSeconds(2.2f);

        // 工人按下开关
        worker1_Bender.SetTrigger("click_switch");
        yield return new WaitForSeconds(0.5f);

        // 弯管机合盖
        pipeBenderMachine.SetTrigger("machine_close");
        yield return new WaitForSeconds(2.0f);

        // 冒烟 → 直管变弯管
        PlayEffect(benderSmoke);
        yield return new WaitForSeconds(0.1f);

        pipe_Straight.SetActive(false);
        pipe_Bent.SetActive(true);

        // 弯管机开盖
        pipeBenderMachine.SetTrigger("machine_open");
        yield return new WaitForSeconds(1.5f);

        StopAndHideEffect(benderSmoke);
    }

    // ==================== 车间2：焊接 ====================
    private IEnumerator Sequence_Workshop2_Welding()
    {
        ResetAnimator(worker2_Welder);
        yield return null;

        // 半成品传送：hover → conveyor → weldStation
        yield return StartCoroutine(MoveObject(pipe_Bent.transform, hoverPoint.position));
        yield return StartCoroutine(MoveObject(pipe_Bent.transform, conveyorPoint1.position));
        yield return StartCoroutine(MoveObject(pipe_Bent.transform, weldStationPoint.position));

        // 工人走 → 按开关
        worker2_Welder.SetTrigger("little_walk");
        yield return new WaitForSeconds(2.2f);

        worker2_Welder.SetTrigger("click_switch");
        yield return new WaitForSeconds(1.0f);

        // 机械臂工作
        if (roboticArm != null) roboticArm.Play("Work");
        yield return new WaitForSeconds(0.8f);

        // 焊接火花
        PlayEffect(weldSparks);
        yield return new WaitForSeconds(1.7f);

        // 替换为焊接成品
        pipe_Bent.SetActive(false);
        tank_Gold_OnTable.SetActive(false);
        combo_Welded.SetActive(true);

        yield return new WaitForSeconds(1.0f);
        StopAndHideEffect(weldSparks);
    }

    // ==================== 车间3：配电 ====================
    private IEnumerator Sequence_Workshop3_Electric()
    {
        ResetAnimator(worker3_Electric);
        yield return null;

        // 半成品传送到配电工位
        yield return StartCoroutine(MoveObject(combo_Welded.transform, electricStationPoint.position));

        // 工人走 → 双手操作
        worker3_Electric.SetTrigger("little_walk");
        yield return new WaitForSeconds(2.2f);

        worker3_Electric.SetTrigger("two_hand_operate");
        yield return new WaitForSeconds(1.5f);

        // 电火花
        PlayEffect(electricSmoke);
        yield return new WaitForSeconds(0.1f);

        // 替换为配电成品
        combo_Welded.SetActive(false);
        motor_Black_OnTable.SetActive(false);
        combo_Electric.SetActive(true);

        yield return new WaitForSeconds(1.5f);
        StopAndHideEffect(electricSmoke);
    }

    // ==================== 车间4：总装 ====================
    private IEnumerator Sequence_Workshop4_FinalAssembly()
    {
        ResetAnimator(worker4_Final);
        yield return null;

        // 半成品传送：corner → back → front
        yield return StartCoroutine(MoveObject(combo_Electric.transform, cornerPoint.position));
        yield return StartCoroutine(MoveObject(combo_Electric.transform, finalStationBackPoint.position));
        yield return StartCoroutine(MoveObject(combo_Electric.transform, finalStationFrontPoint.position));

        // 工人走 → 双手操作
        worker4_Final.SetTrigger("little_walk");
        yield return new WaitForSeconds(2.2f);

        worker4_Final.SetTrigger("two_hand_operate");
        yield return new WaitForSeconds(2.0f);

        // 烟雾
        PlayEffect(finalSmoke);
        yield return new WaitForSeconds(0.1f);

        // 最终成品
        combo_Electric.SetActive(false);
        compressor_Final.SetActive(true);

        yield return new WaitForSeconds(2.0f);
        StopAndHideEffect(finalSmoke);
    }

    #endregion
}
