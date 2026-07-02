using UnityEngine;
using System.Collections;

public class ProductionPipelineManager : MonoBehaviour
{
    [Header("--- 1. 弯管工序资产 ---")]
    public Animator worker1_Bender;
    public Animator pipeBenderMachine;
    public GameObject pipe_Straight;
    public GameObject pipe_Bent;
    public ParticleSystem benderSmoke;

    [Header("--- 2. 焊接工序资产 ---")]
    public Animator worker2_Welder;
    public Animator roboticArm;
    public GameObject tank_Gold_OnTable;
    public GameObject combo_Welded;
    public ParticleSystem weldSparks;
    public Transform hoverPoint;
    public Transform conveyorPoint1;
    public Transform weldStationPoint;

    [Header("--- 3. 配电工序资产 ---")]
    public Animator worker3_Electric;
    public GameObject motor_Black_OnTable;
    public GameObject combo_Electric;
    public ParticleSystem electricSmoke;
    public Transform electricStationPoint;

    [Header("--- 4. 总装工序资产 ---")]
    public Animator worker4_Final;
    public GameObject compressor_Final;
    public ParticleSystem finalSmoke;
    public Transform cornerPoint;
    public Transform finalStationBackPoint;
    public Transform finalStationFrontPoint;

    [Header("--- 全局设置 ---")]
    public float moveSpeed = 2f;

    private bool isRunning = false;

    // 空间锚点：记录零件的初始坐标
    private Vector3 startPos_PipeBent;
    private Vector3 startPos_ComboWelded;
    private Vector3 startPos_ComboElectric;

    // 空间锚点：记录工人的初始坐标与朝向，防止走位后发生偏移
    private Vector3 startPos_Worker1;
    private Quaternion startRot_Worker1;
    private Vector3 startPos_Worker2;
    private Quaternion startRot_Worker2;
    private Vector3 startPos_Worker3;
    private Quaternion startRot_Worker3;
    private Vector3 startPos_Worker4;
    private Quaternion startRot_Worker4;

    void Start()
    {
        // 记录零件初始坐标
        if (pipe_Bent != null) startPos_PipeBent = pipe_Bent.transform.position;
        if (combo_Welded != null) startPos_ComboWelded = combo_Welded.transform.position;
        if (combo_Electric != null) startPos_ComboElectric = combo_Electric.transform.position;

        // 记录工人初始坐标和旋转角度
        if (worker1_Bender != null)
        {
            startPos_Worker1 = worker1_Bender.transform.position;
            startRot_Worker1 = worker1_Bender.transform.rotation;
        }
        if (worker2_Welder != null)
        {
            startPos_Worker2 = worker2_Welder.transform.position;
            startRot_Worker2 = worker2_Welder.transform.rotation;
        }
        if (worker3_Electric != null)
        {
            startPos_Worker3 = worker3_Electric.transform.position;
            startRot_Worker3 = worker3_Electric.transform.rotation;
        }
        if (worker4_Final != null)
        {
            startPos_Worker4 = worker4_Final.transform.position;
            startRot_Worker4 = worker4_Final.transform.rotation;
        }

        InitializeSystem();
    }

    public void InitializeSystem()
    {
        // 1. 零件物理坐标归位
        if (pipe_Bent != null) pipe_Bent.transform.position = startPos_PipeBent;
        if (combo_Welded != null) combo_Welded.transform.position = startPos_ComboWelded;
        if (combo_Electric != null) combo_Electric.transform.position = startPos_ComboElectric;

        // 2. 工人物理坐标与朝向归位
        if (worker1_Bender != null) worker1_Bender.transform.SetPositionAndRotation(startPos_Worker1, startRot_Worker1);
        if (worker2_Welder != null) worker2_Welder.transform.SetPositionAndRotation(startPos_Worker2, startRot_Worker2);
        if (worker3_Electric != null) worker3_Electric.transform.SetPositionAndRotation(startPos_Worker3, startRot_Worker3);
        if (worker4_Final != null) worker4_Final.transform.SetPositionAndRotation(startPos_Worker4, startRot_Worker4);

        // 3. 模型显隐状态重置
        if (pipe_Straight != null) pipe_Straight.SetActive(true);
        if (pipe_Bent != null) pipe_Bent.SetActive(false);
        if (tank_Gold_OnTable != null) tank_Gold_OnTable.SetActive(true);
        if (combo_Welded != null) combo_Welded.SetActive(false);
        if (motor_Black_OnTable != null) motor_Black_OnTable.SetActive(true);
        if (combo_Electric != null) combo_Electric.SetActive(false);
        if (compressor_Final != null) compressor_Final.SetActive(false);

        // 4. 特效彻底清空并隐藏
        StopAndHideEffect(benderSmoke);
        StopAndHideEffect(weldSparks);
        StopAndHideEffect(electricSmoke);
        StopAndHideEffect(finalSmoke);

        // 5. 动画机内部状态归零 (恢复 Idle 待机动作)
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
            anim.Update(0f); // 强行推入第 0 帧
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

    public void CallFullProduction()
    {
        if (isRunning)
        {
            Debug.LogWarning("系统正在运行中，请勿重复点击！");
            return;
        }
        StartCoroutine(FullProductionSequence());
    }

    private IEnumerator FullProductionSequence()
    {
        isRunning = true;

        InitializeSystem();
        yield return null;

        yield return StartCoroutine(Step1_Bending_Internal());
        yield return StartCoroutine(Step2_Welding_Internal());
        yield return StartCoroutine(Step3_Electric_Internal());
        yield return StartCoroutine(Step4_FinalAssembly_Internal());

        Debug.Log("生产完毕！等待 5 秒后场地自动重置...");

        yield return new WaitForSeconds(5.0f);

        InitializeSystem();
        isRunning = false;

        Debug.Log("🔄 场地已重置，等待下一次触发！");
    }

    // ================= 生产工序协程 =================

    public IEnumerator Step1_Bending_Internal()
    {
        ResetAnimator(worker1_Bender);
        ResetAnimator(pipeBenderMachine);
        yield return null;

        worker1_Bender.SetTrigger("little_walk");
        yield return new WaitForSeconds(2.2f);

        worker1_Bender.SetTrigger("click_switch");
        yield return new WaitForSeconds(0.5f);

        pipeBenderMachine.SetTrigger("machine_close");
        yield return new WaitForSeconds(2.0f);

        PlayEffect(benderSmoke);
        yield return new WaitForSeconds(0.1f);

        pipe_Straight.SetActive(false);
        pipe_Bent.SetActive(true);

        pipeBenderMachine.SetTrigger("machine_open");
        yield return new WaitForSeconds(1.5f);

        StopAndHideEffect(benderSmoke);
    }

    public IEnumerator Step2_Welding_Internal()
    {
        ResetAnimator(worker2_Welder);
        yield return null;

        yield return StartCoroutine(MoveObject(pipe_Bent.transform, hoverPoint.position));
        yield return StartCoroutine(MoveObject(pipe_Bent.transform, conveyorPoint1.position));
        yield return StartCoroutine(MoveObject(pipe_Bent.transform, weldStationPoint.position));

        worker2_Welder.SetTrigger("little_walk");
        yield return new WaitForSeconds(2.2f);

        worker2_Welder.SetTrigger("click_switch");
        yield return new WaitForSeconds(1.0f);

        if (roboticArm != null) roboticArm.Play("Work");

        yield return new WaitForSeconds(0.8f);
        PlayEffect(weldSparks);

        yield return new WaitForSeconds(1.7f);

        pipe_Bent.SetActive(false);
        tank_Gold_OnTable.SetActive(false);
        combo_Welded.SetActive(true);

        yield return new WaitForSeconds(1.0f);
        StopAndHideEffect(weldSparks);
    }

    public IEnumerator Step3_Electric_Internal()
    {
        ResetAnimator(worker3_Electric);
        yield return null;

        yield return StartCoroutine(MoveObject(combo_Welded.transform, electricStationPoint.position));

        worker3_Electric.SetTrigger("little_walk");
        yield return new WaitForSeconds(2.2f);

        worker3_Electric.SetTrigger("two_hand_operate");
        yield return new WaitForSeconds(1.5f);

        PlayEffect(electricSmoke);
        yield return new WaitForSeconds(0.1f);

        combo_Welded.SetActive(false);
        motor_Black_OnTable.SetActive(false);
        combo_Electric.SetActive(true);

        yield return new WaitForSeconds(1.5f);
        StopAndHideEffect(electricSmoke);
    }

    public IEnumerator Step4_FinalAssembly_Internal()
    {
        ResetAnimator(worker4_Final);
        yield return null;

        yield return StartCoroutine(MoveObject(combo_Electric.transform, cornerPoint.position));
        yield return StartCoroutine(MoveObject(combo_Electric.transform, finalStationBackPoint.position));
        yield return StartCoroutine(MoveObject(combo_Electric.transform, finalStationFrontPoint.position));

        worker4_Final.SetTrigger("little_walk");
        yield return new WaitForSeconds(2.2f);

        worker4_Final.SetTrigger("two_hand_operate");
        yield return new WaitForSeconds(2.0f);

        PlayEffect(finalSmoke);
        yield return new WaitForSeconds(0.1f);

        combo_Electric.SetActive(false);
        compressor_Final.SetActive(true);

        yield return new WaitForSeconds(2.0f);
        StopAndHideEffect(finalSmoke);
    }

    private IEnumerator MoveObject(Transform obj, Vector3 targetPosition)
    {
        if (obj == null || targetPosition == null) yield break;

        while (Vector3.Distance(obj.position, targetPosition) > 0.01f)
        {
            obj.position = Vector3.MoveTowards(obj.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        obj.position = targetPosition;
    }
}