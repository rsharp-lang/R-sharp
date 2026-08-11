---
name: RData-R4.5.0-compat
overview: 针对 studio\RData 项目测试并修复对 GNU R 4.5.0 生成的 rda/rds 文件兼容性 bug，重点解决 ALTREP 缺失实现、引用表/REF 处理、版本头解析等问题，使复杂对象能正确读取并转换为 R# 基础数据结构。
todos:
  - id: gen-samples
    content: 用 Rscript 生成覆盖各类型及 ALTREP 场景的 rda/rds 测试样本到 test/data
    status: completed
  - id: explore-chain
    content: 用 [subagent:code-explorer] 定位 ALTREP/REF/版本头调用链与缺失的 AltRepConstructor 引用点
    status: completed
    dependencies:
      - gen-samples
  - id: impl-altrep
    content: 新建 Convertor/AltRepConstructor.vb 实现 compact_intseq/realseq/deferred_string/wrap 展开
    status: completed
    dependencies:
      - explore-chain
  - id: fix-reader
    content: 修复 Reader.vb 的 ALTREP 分支与 REF 引用表入表时机及安全降级
    status: completed
    dependencies:
      - impl-altrep
  - id: fix-convert
    content: 修复 ConvertToR.vb/RStreamReader.vb 补齐 CPLX/RAW 等向量转换分支
    status: completed
    dependencies:
      - impl-altrep
  - id: fix-parser
    content: 校验 Parser.vb 的 flags 位解析与字符编码匹配 R 4.x
    status: completed
    dependencies:
      - explore-chain
  - id: test-verify
    content: 在 Module1.vb 加载样本比对 R 原始值，验证 list/dataframe/vector 读取正确
    status: completed
    dependencies:
      - fix-reader
      - fix-convert
      - fix-parser
---

## 用户需求

针对 R# 项目中的 studio\RData 模块（rda/rds 文件解析库）进行系统化测试与缺陷修复，使其能够正确读取由 GNU R 4.5.0（位于 C:\Program Files\R\R-4.5.0\bin\Rscript.exe）生成的 rda/rds 文件，并将复杂对象正确转换为 R# 基础数据结构（list、dataframe、vector 等）。

## 产品概述

studio\RData 提供 Reader.vb（解析 R 二进制序列化）+ Convertor\ConvertToR.vb（转换为 R# 对象）。当前对 R 4.5.0 兼容性差，表现为部分复杂对象无法读取或抛 NotImplementedException。本次任务以测试驱动方式定位并修复兼容性 bug。

## 核心特性

- 生成覆盖各数据类型的 R 4.5.0 测试文件（基础向量、list、data.frame、factor、matrix、嵌套 list、命名向量、带 attributes 对象，以及触发 ALTREP 的对象：1:100 紧凑整数序列、seq/rep 紧凑序列、大字符向量 deferred string）。
- 修复 ALTREP（type=238）解析与展开逻辑，使 compact_intseq/compact_realseq/deferred_string/wrap 等 ALTREP 对象能正确展开为普通向量。
- 修复 REF（type=255）引用表写入与解析，保证嵌套/大对象递归正确。
- 修复版本头（format 2/3）与字符编码（UTF8/LATIN1/BYTES）读取，匹配 R 4.x 写入规则。
- 基于 Module1.vb 测试入口对比读取结果与 R 原始值，验证 list/dataframe/vector 转换正确。

## 技术栈

- 语言：VB.NET（.NET Framework 4.8，test 项目）
- 核心库：studio\RData（RData.vbproj）、R#\R.vbproj、sciBASIC# BinaryData / Microsoft.VisualBasic.Core
- 测试工具：GNU R 4.5.0 Rscript.exe 生成 rda/rds 样本
- 序列化格式：R XDR 二进制（RDX2/3 头 + XDR 整数/双精度）

## 实现方法

采用“测试驱动 + 分类型比对”策略：先用 Rscript 生成覆盖所有 R 类型及 ALTREP 触发场景的 rda/rds 样本，在 Module1.vb 中加载并与 R 原始值比对，逐一定位 Reader/ConvertToR 的解析与转换缺陷并修复。

关键技术决策：

1. **补齐缺失的 AltRepConstructor 实现**（最高优先级）：Convertor/Constructor.vb 声明了 wrap_constructor、compact_realseq_constructor、compact_intseq_constructor、deferred_string_constructor 等字段，但 AltRepConstructor 类定义文件在整个仓库不存在，导致 Reader.vb ALTREP 分支（line 312-332）与 ConvertToR 无法正常展开。需新建 AltRepConstructor 类型，按 R 官方 ALTREP 协议（class/state/attributes 三元组）实现 compact_intseq（start/step/n）、compact_realseq、deferred_string、wrap_* 的向量展开。
2. **REF 引用表一致性**：校验 reference_list 在 ReadObject 中的写入时机（line 218-222, 371-373）与各类型（SYM/LIST/LANG/ENV/ALTREP）是否需要入表，修复嵌套对象解析错位。
3. **版本头与编码**：确认 parse_versions 对 format=2/3 的识别；characters 解码依据 gp 标志（UTF8/LATIN1/ASCII/BYTES）匹配 R 4.x。
4. **复用现有 RList/RObject 中间表示与 RStreamReader**，避免引入新数据结构，保持与现有 ConvertToR 流水线一致。

性能与可靠性：解析为单遍流式（O(n)），ALTREP 展开仅对受影响对象构造数组，无全文件二次遍历；保持 expand_altrep 默认 True 的同时，对不支持的 ALTREP 类安全降级（保留原始节点）而非抛异常。

## 实现要点

- 复用现有 parse_string/parse_int/parse_double（XDR Unpacker），不重写底层 IO。
- AltRepConstructor 展开后在 ConvertToR.PullRObject 中与普通向量走同一 CreateRVector/CreateRTable 分支，保证转换一致。
- 日志沿用 Console.WriteLine($"[{info_int}] => ...") 的 debug 模式，不新增日志框架；避免打印大 payload。
- 保持向后兼容：未触发 ALTREP/REF 的文件行为不变；不改动 R#\R.vbproj 核心对象定义。

## 架构设计

数据流：Rscript 生成样本 -> Reader.ParseData(Stream) -> ReadObject(递归, reference_list) -> RObject 中间树 -> ConvertToR.ToRObject -> R# list/dataframe/vector。
ALTREP 修复位置在 Reader.parse_R_object 的 ALTREP 分支与新建 AltRepConstructor 展开器，ConvertToR 仅消费展开后的普通节点。

## 目录结构

g:/GCModeller/src/R-sharp/studio/RData/
├── Convertor/
│   └── AltRepConstructor.vb        # [NEW] 实现 AltRepConstructor 委托及 compact_intseq/compact_realseq/deferred_string/wrap_* 展开逻辑，对齐 R 官方 ALTREP 协议；供 Reader.expand_altrep_to_object 与 Constructor 字段使用。
├── Reader.vb                      # [MODIFY] 校验 ALTREP 分支引用 AltRepConstructor 的正确性；修复 REF 入表时机；未知 ALTREP 安全降级而非抛异常。
├── Convertor/ConvertToR.vb        # [MODIFY] 确保 ALTREP 展开后的普通节点经 CreateRVector/CreateRTable 正确转换；补充 CPLX/RAW 等缺失向量类型的 ReadVector 分支。
├── Convertor/RStreamReader.vb     # [MODIFY] 补充 CPLX/RAW 向量读取，避免复杂对象抛 NotImplementedException。
├── Convertor/Constructor.vb       # [MODIFY] 关联新 AltRepConstructor 具体实现，确认字符编码解码与 R 4.x 一致。
├── Parser.vb                      # [MODIFY] 确认 parse_r_object_info 位解析覆盖 R 4.x flags（gp 范围、object/attr/tag 位），必要时扩展。
└── test/
├── Module1.vb                 # [MODIFY] 新增各类型及 ALTREP 场景的 TestRDA/TestRDS 加载与比对用例，输出读取结果与 R 原始值对照。
└── data/                      # [NEW] 由 Rscript 生成的 rda/rds 测试样本目录。

## Agent Extensions

### SubAgent

- **code-explorer**
- 用途：在修复前深入检索 Reader.vb、ConvertToR.vb、Constructor.vb 中 ALTREP/REF/版本头相关调用链与符号定义，确认 AltRepConstructor 缺失范围及受影响调用点。
- 预期结果：产出精确的待修改函数清单与引用关系，避免遗漏调用点导致编译或运行错误。