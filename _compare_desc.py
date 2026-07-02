# Compare old vs new descriptions character counts
comparisons = []

# === StandardSalesBranchFlow.cs ===
comparisons.append(('StandardSalesBranchFlow', '填写销售计划',
    '销售总监填写下月月度销售计划；点：提交',
    '填写下月月度销售计划，明确各机型销售目标与交付周期'))
comparisons.append(('StandardSalesBranchFlow', '查看销售计划',
    'PMC查看订单、库存、在制、在途等情况',
    '查看销售计划、订单、库存、在制、在途等情况'))
comparisons.append(('StandardSalesBranchFlow', '回复交期',
    'PMC回复销售产品交期',
    '根据生产计划与采购周期，向销售部回复产品预计交付时间'))
comparisons.append(('StandardSalesBranchFlow', '制作生产计划',
    'PMC制作一周生产计划；点：提交；生产主管可查看',
    '根据销售计划与库存情况，制作一周生产计划并提交'))
comparisons.append(('StandardSalesBranchFlow', '制作采购计划',
    'PMC制作两周采购计划；点：提交；采购主管可查看',
    '根据生产计划、BOM 清单与库存情况，制作两周采购计划并提交'))

# === StandardProductionFlow.cs ===
comparisons.append(('StandardProductionFlow', '制作一周生产计划',
    'PMC制作一周生产计划；点：提交；生产主管可查看',
    '根据销售计划与库存情况，制作一周生产计划并提交'))
comparisons.append(('StandardProductionFlow', 'PMC填写每日排产单',
    'PMC填写每日排产单给仓库仓管员和车间主管；点：提交；自动下推给仓管员（生产用料清单）和车间主管（生产工单）',
    '填写每日排产单，点击提交后生产主管可查看'))
comparisons.append(('StandardProductionFlow', '仓管员发料到备料区',
    '仓管员按生产用料清单发料到备料区（位于生产区）',
    '根据排产单与生产用料清单，将物料发放到备料区'))
comparisons.append(('StandardProductionFlow', '1车间工人生产',
    '工人进行1车间生产操作',
    '按工序要求，进行弯管生产操作'))
comparisons.append(('StandardProductionFlow', '2车间工人生产',
    '工人进行2车间生产操作',
    '按工序要求，进行焊接生产操作'))
comparisons.append(('StandardProductionFlow', '3车间工人生产',
    '工人进行3车间生产操作',
    '按工序要求，进行配电生产操作'))
comparisons.append(('StandardProductionFlow', '工序汇报(通用)',
    'X车间生产完成后检查；填写工序汇报单',
    '检查本工序生产完成情况，填写工序汇报单并提交'))
comparisons.append(('StandardProductionFlow', '半成品转运(通用)',
    '将X车间半成品送往Y车间',
    '将本工序生产的半成品转运至下一工序对应车间，完成交接'))
comparisons.append(('StandardProductionFlow', '4车间工人总装',
    '4车间负责组装来自1/2/3车间的半成品',
    '对来自各车间的半成品进行整机总装，完成后将成品送往仓库'))
comparisons.append(('StandardProductionFlow', '仓管员质检确认成品',
    '对成品进行质检确认',
    '总装完成的成品经质检合格后，完成入库'))

# === StandardDeliveryFlow.cs ===
comparisons.append(('StandardDeliveryFlow', '填写发货通知单',
    '仓管员填写发货通知单（由销售订单下推）',
    '填写发货通知单，明确发货产品、数量、收货地址，提交后仓库主管可审核'))
comparisons.append(('StandardDeliveryFlow', '审核发货通知单',
    '仓库主管审核发货通知单',
    '核对发货信息与订单匹配度，确认无误后完成审核'))
comparisons.append(('StandardDeliveryFlow', '包装出库',
    '仓库包装出库，发货完成',
    '对出库产品进行包装、贴标，完成后交给物流，发货完成'))
comparisons.append(('StandardDeliveryFlow', '填写销售出库单',
    '仓管员填写销售出库单（由发货通知单下推）',
    '根据发货通知单，填写销售出库单并提交'))
comparisons.append(('StandardDeliveryFlow', '审核销售出库单',
    '仓库主管审核销售出库单',
    '核对出库信息与库存数据，确认无误后完成审核'))
comparisons.append(('StandardDeliveryFlow', '客户签收',
    '客户收货，在发货通知单上签字（此步骤自动进行）',
    '客户收到货品，核对无误后在发货通知单上签字确认收货'))

# === CustomSalesFlow.cs ===
comparisons.append(('CustomSalesFlow', '客户询单',
    '客户咨询定制产品需求',
    '对接客户，记录定制空压机产品需求'))
comparisons.append(('CustomSalesFlow', '填写销售订单',
    '录入客户需求和产品规格',
    '录入客户需求、产品规格；填写销售订单并提交'))
comparisons.append(('CustomSalesFlow', '审核销售订单',
    '销售总监审核订单内容',
    '审核订单内容，审核通过后自动下推 BOM 单'))
comparisons.append(('CustomSalesFlow', '填写BOM单',
    '技术部根据订单生成物料清单',
    '根据订单生成物料清单；填写 BOM 单并提交'))
comparisons.append(('CustomSalesFlow', '查看BOM单',
    '财务部查看物料清单',
    '查看物料清单内容'))
comparisons.append(('CustomSalesFlow', '计算成本并确认价格',
    '填写销售报价单',
    '核算成本，填写销售报价单并提交'))
comparisons.append(('CustomSalesFlow', '查看报价单',
    '销售员查看财务核算的报价',
    '查看已生成的销售报价单'))
comparisons.append(('CustomSalesFlow', '报价给客户',
    '将报价单发送给客户',
    '向客户同步产品报价'))
comparisons.append(('CustomSalesFlow', '客户确认',
    '客户确认报价并签订合同',
    '客户确认报价并敲定订单'))
comparisons.append(('CustomSalesFlow', '提交销售订单给PMC',
    'PMC可查看销售订单',
    '提交订单，供 PMC 查看'))
comparisons.append(('CustomSalesFlow', '选择后续流程',
    '请选择要继续的流程分支\n[1]生产流程 [2]采购流程',
    '选择生产流程或采购流程分支'))

# === CustomProductionFlow.cs ===
comparisons.append(('CustomProductionFlow', 'PMC查看销售订单',
    'PMC查看销售订单内容，了解定制产品需求',
    'PMC查看销售订单内容，了解定制产品需求'))
comparisons.append(('CustomProductionFlow', '制定一周生产计划',
    '制定一周生产计划；填：一周生产计划；点：提交',
    '填写一周生产计划表并提交'))
comparisons.append(('CustomProductionFlow', 'PMC填写每日排产单',
    '填写每日排产单；点：提交。自动下推给仓库仓管员（生产用料清单）和车间主管（生产工单）',
    '填写每日排产单并提交，自动下发对应单据'))
comparisons.append(('CustomProductionFlow', '仓管员发料到备料区',
    '仓管员按生产用料清单发料到备料区（位于生产区）',
    '按照生产用料清单，将物料发放至生产区备料区'))
comparisons.append(('CustomProductionFlow', '1车间工人到备料区领料',
    '查看自己的派工单，根据派工单到备料区领料',
    '根据派工单，前往备料区领取生产物料'))
comparisons.append(('CustomProductionFlow', '1车间工人填写领料单',
    '填写领料单；点：提交',
    '填写领料单并提交'))
comparisons.append(('CustomProductionFlow', '1车间工人生产',
    '工人进行1车间生产操作',
    '开展生产作业'))
comparisons.append(('CustomProductionFlow', '1车间工人退料实物送仓库',
    '将生产多余物料退回仓库',
    '将生产多余物料、边角料退回仓库'))
comparisons.append(('CustomProductionFlow', '仓管员质检确认退料',
    '对1车间退料进行质检确认',
    '对退回物料开展质检工作'))
comparisons.append(('CustomProductionFlow', '仓管员制生产退料入库单',
    '填写生产退料入库单',
    '填写生产退料入库单并提交'))
comparisons.append(('CustomProductionFlow', '1车间工人签字确认退料',
    '在生产退料入库单上签字',
    '在生产退料入库单上签字确认'))
comparisons.append(('CustomProductionFlow', '1车间工序汇报',
    '1车间生产完成后检查；填写工序汇报单',
    '检查本工序生产情况，填写工序汇报单并提交'))
comparisons.append(('CustomProductionFlow', '1车间半成品转运',
    '将1车间半成品送往2车间',
    '将本工序生产的半成品转运至下一工序对应车间，完成交接'))
comparisons.append(('CustomProductionFlow', '2车间工人生产',
    '工人进行2车间生产操作',
    '开展生产作业'))
comparisons.append(('CustomProductionFlow', '2车间工序汇报',
    '2车间生产完成后检查；填写工序汇报单',
    '检查本工序生产情况，填写工序汇报单并提交'))
comparisons.append(('CustomProductionFlow', '2车间半成品转运',
    '将2车间半成品送往3车间',
    '将本工序生产的半成品转运至下一工序对应车间，完成交接'))
comparisons.append(('CustomProductionFlow', '3车间工人生产',
    '工人进行3车间生产操作',
    '开展生产作业'))
comparisons.append(('CustomProductionFlow', '3车间工序汇报',
    '3车间生产完成后检查；填写工序汇报单',
    '检查本工序生产情况，填写工序汇报单并提交'))
comparisons.append(('CustomProductionFlow', '3车间半成品转运',
    '将3车间半成品送往4车间',
    '将本工序生产的半成品转运至下一工序对应车间，完成交接'))
comparisons.append(('CustomProductionFlow', '4车间工人总装',
    '4车间负责组装来自1/2/3车间的半成品',
    '对来自各车间的半成品进行整机总装'))
comparisons.append(('CustomProductionFlow', '仓管员质检确认成品',
    '对成品进行质检确认',
    '对完工成品开展全项质检工作'))
comparisons.append(('CustomProductionFlow', '仓管员填写完工入库单',
    '填写完工入库单；工人可查看',
    '填写完工入库单并提交，工人可查看'))
comparisons.append(('CustomProductionFlow', '工人在完工入库单签字',
    '工人在完工入库单上签字',
    '在完工入库单上签字，确认成品信息无误'))
comparisons.append(('CustomProductionFlow', '自动通知销售员产品入库',
    '系统自动通知销售员产品已入库',
    '系统自动推送产品入库通知，告知销售员订单货品已配齐'))
comparisons.append(('CustomProductionFlow', '销售员查看入库通知',
    '销售员查看产品入库通知，确认货已配齐',
    '查看系统推送的产品入库通知，确认订单货品已全部配齐'))
comparisons.append(('CustomProductionFlow', '联系客户确认发货',
    '销售员联系客户，确认是否可以发货',
    '告知客户货品已完工入库，沟通确认当前是否可以安排发货'))
comparisons.append(('CustomProductionFlow', '客户确认可发货',
    '客户确认可以发货',
    '客户回复同意发货，敲定发货时间与收货相关要求'))
comparisons.append(('CustomProductionFlow', '销售员点击发货',
    '销售员在销售订单点击发货，自动下推发货通知单',
    '填写发货通知单，明确发货产品、数量、收货地址，提交后销售总监可审核'))
comparisons.append(('CustomProductionFlow', '仓库主管审核发货通知单',
    '仓库主管审核发货通知单',
    '核对发货信息与订单匹配度，确认无误后完成审核'))
comparisons.append(('CustomProductionFlow', '仓库包装出库',
    '仓库包装出库，发货完成',
    '将包装好的定制产品移交物流，完成成品出库，同步更新库存数据'))
comparisons.append(('CustomProductionFlow', '仓管员填写销售出库单',
    '仓管员填写销售出库单（由发货通知单下推）',
    '根据审核后的发货通知单，填写销售出库单并提交'))
comparisons.append(('CustomProductionFlow', '仓库主管审核销售出库单',
    '仓库主管审核销售出库单',
    '核对出库信息与库存数据，确认无误后完成审核'))
comparisons.append(('CustomProductionFlow', '客户收货签字',
    '客户收货，在发货通知单上签字',
    '客户收到货品，核对无误后在发货通知单上签字确认收货'))

print(f'{"文件":<25} {"步骤名":<22} {"旧字数":<6} {"新字数":<6} {"差值":<6} {"增幅":<8}')
print('-' * 85)

comparisons.sort(key=lambda x: len(x[3]) - len(x[2]), reverse=True)

alerts = []
for file, step, old, new in comparisons:
    old_len = len(old)
    new_len = len(new)
    diff = new_len - old_len
    pct = (diff / old_len * 100) if old_len > 0 else 0
    marker = ''
    if diff > 10:
        marker = ' !!'
        alerts.append((file, step, old_len, new_len, diff, f'{pct:.0f}%'))
    print(f'{file:<25} {step:<22} {old_len:<6} {new_len:<6} {diff:+<6}  {pct:+.0f}%{marker}')

print()
print('=' * 85)
print('!! 新文案比旧文案多出超过10个字的步骤（需要重点检查UI是否挤在一起）：')
print('=' * 85)
if alerts:
    for file, step, old, new, diff, pct in alerts:
        print(f'  [{file}]')
        print(f'    步骤: {step}')
        print(f'    旧({old}字): ...')
        print(f'    新({new}字): ...')
        print(f'    增加: +{diff}字 ({pct})')
        print()
else:
    print('  没有超过10字的增幅！')
