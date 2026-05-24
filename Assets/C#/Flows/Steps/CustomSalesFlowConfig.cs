using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 定制产品销售流程配置
/// 定义每个步骤的详细信息（角色、位置、操作等）
/// </summary>
[CreateAssetMenu(fileName = "CustomSalesFlowConfig", menuName = "Flow/CustomSalesFlowConfig")]
public class CustomSalesFlowConfig : ScriptableObject
{
    [Header("流程基本信息")]
    public string flowName = "定制产品销售流程";
    public string taskTitle = "定制产品订单处理";
    public string taskDescription = "完成从客户询单到提交PMC的完整销售流程";

    [Header("步骤列表")]
    public List<StepInfo> steps = new List<StepInfo>();

    [System.Serializable]
    public class StepInfo
    {
        [Tooltip("步骤名称")]
        public string stepName;

        [Tooltip("步骤描述")]
        public string description;

        [Tooltip("目标NPC/角色名称")]
        public string targetNPC;

        [Tooltip("目标位置")]
        public string targetLocation;

        [Tooltip("操作类型")]
        public Interactables.ActionType actionType;

        [Tooltip("提示信息（可选）")]
        public string hint;
    }
}
