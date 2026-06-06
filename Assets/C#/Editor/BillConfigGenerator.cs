using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BillConfigGenerator : EditorWindow
{
    [MenuItem("Tools/AirCompressor/生成按钮权限配置")]
    public static void ShowWindow()
    {
        GetWindow<BillConfigGenerator>("按钮权限配置生成器");
    }

    private void OnGUI()
    {
        GUILayout.Label("单据按钮权限配置生成器", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("生成所有单据配置"))
        {
            GenerateAllBillConfigs();
            EditorUtility.DisplayDialog("成功", "所有单据配置已生成！", "确定");
        }

        GUILayout.Space(20);
        GUILayout.Label("配置文件输出路径: Assets/Resources/BillConfigs/", EditorStyles.helpBox);
    }

    private static void GenerateAllBillConfigs()
    {
        // 销售订单配置
        GenerateBillConfig(UIManager.UIType.SalesOrder, "销售订单", new Dictionary<string, List<Interactables.ActionType>>
        {
            { "销售员", new List<Interactables.ActionType> { Interactables.ActionType.Save, Interactables.ActionType.Submit, Interactables.ActionType.Fill, Interactables.ActionType.Ship, Interactables.ActionType.Exit } },
            { "销售主管", new List<Interactables.ActionType> { Interactables.ActionType.Approve, Interactables.ActionType.Exit } },
            { "仓管员", new List<Interactables.ActionType> { Interactables.ActionType.Ship, Interactables.ActionType.Exit } },
            { "PMC", new List<Interactables.ActionType> { Interactables.ActionType.Exit } }
        });

        // BOM单配置
        GenerateBillConfig(UIManager.UIType.ProductionBOM, "BOM单", new Dictionary<string, List<Interactables.ActionType>>
        {
            { "技术员", new List<Interactables.ActionType> { Interactables.ActionType.Save, Interactables.ActionType.Submit, Interactables.ActionType.Fill, Interactables.ActionType.Exit } },
            { "财务", new List<Interactables.ActionType> { Interactables.ActionType.Exit } }
        });

        // 销售报价单配置
        GenerateBillConfig(UIManager.UIType.SalesQuotation, "销售报价单", new Dictionary<string, List<Interactables.ActionType>>
        {
            { "财务", new List<Interactables.ActionType> { Interactables.ActionType.Save, Interactables.ActionType.Submit, Interactables.ActionType.Fill, Interactables.ActionType.Exit } },
            { "销售员", new List<Interactables.ActionType> { Interactables.ActionType.Exit } }
        });

        // 一周生产计划配置
        GenerateBillConfig(UIManager.UIType.WeeklyProductionPlan, "一周生产计划", new Dictionary<string, List<Interactables.ActionType>>
        {
            { "PMC", new List<Interactables.ActionType> { Interactables.ActionType.Save, Interactables.ActionType.Submit, Interactables.ActionType.Fill, Interactables.ActionType.Exit } }
        });

        // 排产单配置
        GenerateBillConfig(UIManager.UIType.ProductionSchedule, "排产单", new Dictionary<string, List<Interactables.ActionType>>
        {
            { "PMC", new List<Interactables.ActionType> { Interactables.ActionType.Save, Interactables.ActionType.Submit, Interactables.ActionType.Fill, Interactables.ActionType.Exit } }
        });

        // 生产用料清单配置
        GenerateBillConfig(UIManager.UIType.ProductionMaterialList, "生产用料清单", new Dictionary<string, List<Interactables.ActionType>>
        {
            { "PMC", new List<Interactables.ActionType> { Interactables.ActionType.Save, Interactables.ActionType.Submit, Interactables.ActionType.Fill, Interactables.ActionType.Exit } },
            { "仓管员", new List<Interactables.ActionType> { Interactables.ActionType.Exit } }
        });

        // 生产工单配置
        GenerateBillConfig(UIManager.UIType.ProductionWorkOrder, "生产工单", new Dictionary<string, List<Interactables.ActionType>>
        {
            { "生产主管", new List<Interactables.ActionType> { Interactables.ActionType.Save, Interactables.ActionType.Submit, Interactables.ActionType.Fill, Interactables.ActionType.Exit } },
            { "车间组长", new List<Interactables.ActionType> { Interactables.ActionType.Exit } }
        });

        // 派工单配置
        GenerateBillConfig(UIManager.UIType.DispatchOrder, "派工单", new Dictionary<string, List<Interactables.ActionType>>
        {
            { "班组长", new List<Interactables.ActionType> { Interactables.ActionType.Save, Interactables.ActionType.Submit, Interactables.ActionType.Fill, Interactables.ActionType.Exit } },
            { "工人", new List<Interactables.ActionType> { Interactables.ActionType.Exit } }
        });

        // 领料单配置
        GenerateBillConfig(UIManager.UIType.PickList, "领料单", new Dictionary<string, List<Interactables.ActionType>>
        {
            { "工人", new List<Interactables.ActionType> { Interactables.ActionType.Save, Interactables.ActionType.Submit, Interactables.ActionType.Fill, Interactables.ActionType.Exit } }
        });

        // 工序汇报单配置
        GenerateBillConfig(UIManager.UIType.ProcessReport, "工序汇报单", new Dictionary<string, List<Interactables.ActionType>>
        {
            { "班组长", new List<Interactables.ActionType> { Interactables.ActionType.Save, Interactables.ActionType.Submit, Interactables.ActionType.Fill, Interactables.ActionType.Exit } }
        });

        // 完工入库单配置
        GenerateBillConfig(UIManager.UIType.FinishedInbound, "完工入库单", new Dictionary<string, List<Interactables.ActionType>>
        {
            { "仓管员", new List<Interactables.ActionType> { Interactables.ActionType.Save, Interactables.ActionType.Submit, Interactables.ActionType.Fill, Interactables.ActionType.Exit } },
            { "工人", new List<Interactables.ActionType> { Interactables.ActionType.Save, Interactables.ActionType.Sign, Interactables.ActionType.Submit, Interactables.ActionType.Exit } }
        });

        // 生产退料入库单配置
        GenerateBillConfig(UIManager.UIType.ProductionReturn, "生产退料入库单", new Dictionary<string, List<Interactables.ActionType>>
        {
            { "仓管员", new List<Interactables.ActionType> { Interactables.ActionType.Submit, Interactables.ActionType.Fill, Interactables.ActionType.Exit } },
            { "工人", new List<Interactables.ActionType> { Interactables.ActionType.Sign, Interactables.ActionType.Submit, Interactables.ActionType.Exit } }
        });

        // 发货通知单配置
        GenerateBillConfig(UIManager.UIType.DeliveryNotice, "发货通知单", new Dictionary<string, List<Interactables.ActionType>>
        {
            { "仓管员", new List<Interactables.ActionType> { Interactables.ActionType.Submit, Interactables.ActionType.Fill, Interactables.ActionType.Exit } },
            { "仓库主管", new List<Interactables.ActionType> { Interactables.ActionType.Approve, Interactables.ActionType.Exit } }
        });

        // 销售出库单配置
        GenerateBillConfig(UIManager.UIType.SalesOutbound, "销售出库单", new Dictionary<string, List<Interactables.ActionType>>
        {
            { "仓管员", new List<Interactables.ActionType> { Interactables.ActionType.Save, Interactables.ActionType.Submit, Interactables.ActionType.Fill, Interactables.ActionType.Exit } },
            { "仓库主管", new List<Interactables.ActionType> { Interactables.ActionType.Approve, Interactables.ActionType.Exit } }
        });

        AssetDatabase.Refresh();
    }

    private static void GenerateBillConfig(UIManager.UIType billType, string billName, Dictionary<string, List<Interactables.ActionType>> roleButtons)
    {
        string configName = billType.ToString();
        string path = $"Assets/Resources/BillConfigs/{configName}.asset";

        BillButtonConfig config = AssetDatabase.LoadAssetAtPath<BillButtonConfig>(path);
        
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<BillButtonConfig>();
            AssetDatabase.CreateAsset(config, path);
        }

        config.billType = billType;
        config.billName = billName;
        config.roleSettings = new List<BillButtonConfig.RoleButtonSetting>();

        foreach (var kvp in roleButtons)
        {
            config.roleSettings.Add(new BillButtonConfig.RoleButtonSetting
            {
                roleName = kvp.Key,
                visibleButtons = kvp.Value
            });
        }

        EditorUtility.SetDirty(config);
        Debug.Log($"生成配置: {path}");
    }
}