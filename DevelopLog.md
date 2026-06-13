# DevelopLog — 2026-06-13

## 1. BillView 签字区域文字功能

- 新增 `signHintText`（签字时显示工人姓名）、`fillSignNameText`（填写时显示仓管员姓名）、`fillSignFixedText`（固定声明文字）
- 填写和签字分离：`showSignHintOnFill=true` 时填写即显示，否则仅签字显示
- 仓管员名跨步骤保存（`_savedFillSignName`），提交不清除，工人打开时恢复
- 关闭面板全部隐藏

## 2. BillData 扩展

- 新增 `signHintText`、`fillSignFixedContent`（多行）、`showSignHintOnFill`、`shipAllowed`
- 新增 `previewTableData`：打开单据时预显示部分表格行，点填写后替换为完整数据
- `GetPreviewTableForRole()`、`GetSignHintForRole()` 角色覆盖取值

## 3. 采购流程完整搭建

### 新增 UIType（7个）
`IncomingNotification`(收料通知单)、`IncomingInspection`(来料检验单)、`BiweeklyPurchasePlan`(两周采购计划)、`MonthlySalesPlan`(月度销售计划)

### 新增 BillData 资产（7个）
采购申请单、两周采购计划、采购订单、送货通知单、收料通知单、来料检验单、采购入库单、月度销售计划

### 修复采购流程交互
- `CustomPurchaseFlow` / `StandardPurchaseFlow`：先等玩家按E → 再开单据面板；取消后需重新按E
- `InteractionManager` 补充 `CustomPurchaseFlow` 的 NPC 校验
- 送货通知单改为「货和单一起到，跟单员查看」模式
- 统一 PMC→PMC主管 角色名

### 修复 billType 绑定
- 采购申请单、两周采购计划、来料检验单、收料通知单 → 补绑 billType
- `CustomPurchaseFlow` 删除不存在的「跟单员查看采购计划」步骤

## 4. 发货条件修复

- `CheckShipConditions` 改为由 `AllowShip`（运行时）+ `BillData.shipAllowed`（配置）双重控制
- `FlowBase.WaitForBillComplete` 新增 `allowShip` 参数
- 生产流程结束后的发货步骤传入 `allowShip: true`

## 5. 流程引导文案修复

- 修正位置：客户下单→官网、仓管员发料→备料区、检查库存→仓库
- 修正描述：退料送仓库删「与生产并行分支」误导文案
- 客户签收到 DeliveryNotice

## 6. 代码清理

- 删除所有 emoji（→ ✅ 🔧 ★ 等）
- 修复 `FlowManager._isInitialized` CS0414 警告

## 变更文件（37 files, +120k / -134k）

### 新增（7 个 BillData）
BiweeklyPurchasePlan, IncomingInspection, IncomingNotification, MonthlySalesPlan, PurchaseInbound, PurchaseOrder, PurchaseRequest, ReceiptNotice

### 修改（22 个 C#）
BillView, BillData, FlowBase, FlowManager, UIManager, InteractionManager
BillConfigGenerator, CustomProductionFlow, CustomPurchaseFlow, CustomSalesFlow
StandardDeliveryFlow, StandardProductionFlow, StandardPurchaseFlow
StandardSalesBranchFlow, StandardSalesFlow, DemandManager, SceneManage

### 场景/Prefab/BillConfig
新主场景、填写框prefab、表格单元prefab、所有现有 BillData 资产刷新
