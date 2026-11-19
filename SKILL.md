---
name: rsharp-package-testing
description: 指导 LLM agent 编译 R# 程序包（VB.NET 源码 + R# 脚本）并通过 R#.exe 执行 R# 脚本测试与回归验证。当任务涉及 GCModeller、metaboanalyst、biocad_registry、cell-render、Daisy、environment-designer、GMN-Designer、graphQL 等 R# 程序包的代码修改后验证、.NET 程序集重新编译、运行时底层 CLR 刷新、或编写/运行 *.R 测试脚本时使用。
---

# R# 程序包脚本测试

## 用途

给出在 R# 脚本语言运行时环境（`G:\GCModeller\src\R-sharp`）中，对一个 R# 程序包做「编译 → 刷新运行时 → 执行脚本验证」的标准流程。核心目标是避免最常见的两类误判：**用旧程序集测新代码**、**在错误的工作目录或错误的 `--attach` 参数下运行**。

## 何时使用

- 修改了某个 R# 程序包的 `src` 下 VB.NET 代码，需要验证改动生效。
- 修改了 `src\R-sharp` 或 `src\runtime\sciBASIC#` 的底层代码，需要确认下游包行为。
- 需要为某个 R# 程序包编写或运行 `*.R` 测试脚本。
- 测试结果与预期不符，需要判断是「代码错」还是「程序集没更新」。

## 关键路径事实

| 项目 | 路径 |
| --- | --- |
| R# 运行时入口 | `G:\GCModeller\src\R-sharp\App\net10.0\R#.exe` |
| R# 运行时解决方案 | `G:\GCModeller\src\R-sharp\R_system.NET5.slnx` |
| 基础运行时源码 | `G:\GCModeller\src\runtime\sciBASIC#` |
| 程序集落地目录 | `<package_root>\assembly\`（含 `net10.0\` 子目录与 `*.nupkg`） |
| 构建配置名 | `Rsharp_app_release`（Configuration）+ `x64`（Platform） |

R# 程序包根目录必须具备：`*.Rproj`、`.Rbuildassets`、`.Rbuildignore`、`NAMESPACE`、`DESCRIPTION`、`LICENSE`、`R\`（R# 脚本源码）、`assembly\`（已编译的 .NET 程序集）。缺失这些文件时，`--attach` 会失败或静默加载不到符号。

## 流程

三个步骤**按顺序执行**，不可跳跃；只在确认不涉及对应代码时才跳过某一步。

### 步骤 1：编译程序包自身的 .NET 代码

`R\` 目录下的 R# 脚本是解释执行的，**不需要编译**。只有 `src\` 下的 VB.NET 项目需要构建。

```powershell
dotnet build "G:\GCModeller\src\GCModeller.slnx" -c Rsharp_app_release -p:Platform=x64
```

- 配置必须拆成 `-c Rsharp_app_release` 与 `-p:Platform=x64` 两个参数；不要写成 `-c "Rsharp_app_release|x64"`，dotnet 不接受管道组合写法。
- `.sln` 与 `.slnx` 都适用同一命令形式。
- 构建失败时先修编译错误，禁止带着旧 dll 继续测试。
- 只想验证单个项目时可对 `.vbproj` 单独构建，但**只有**解决方案级构建才保证所有程序集都落到 `assembly`；因此默认走解决方案。

程序包与解决方案的对应关系：

| 程序包根目录 | 解决方案 |
| --- | --- |
| `G:\GCModeller` | `G:\GCModeller\src\GCModeller.slnx` |
| `G:\metaboanalyst` | `G:\metaboanalyst\src\metaboanalyst.sln` |
| `G:\biocad_registry` | `G:\biocad_registry\src\biocad_registry.slnx` |
| `G:\cell-render` | `G:\cell-render\src\CellRender.sln` |
| `G:\Daisy` | `G:\Daisy\Daisy.slnx` |
| `G:\environment-designer` | `G:\environment-designer\src\EnvironmentDesigner.slnx` |
| `G:\GMN-Designer` | `G:\GMN-Designer\GMN-Designer.slnx` |
| `G:\graphQL` | `G:\graphQL\src\graphQL.sln`（辅助包另见 `src\pubmed.slnx`） |

例外：`EnvironmentDesigner.slnx` 与 `GMN-Designer.slnx` 未声明 `Rsharp_app_release` 构建类型，其项目只定义 `Release`。对这类包，先核对项目内的 `<Configurations>`；确实没有该配置时改传 `-c Release -p:Platform=x64`，并**确认 `assembly` 下程序集时间戳真的变了**，不能假定配置名有效。

### 步骤 2：刷新运行时底层 CLR 程序集

只要本次改动触及以下任一处，就必须执行本步骤，否则包的测试仍加载旧的基础库：

- `G:\GCModeller\src\runtime\sciBASIC#`
- `G:\GCModeller\src\R-sharp` 自身（`R#`、`Library`、`snowFall`、`studio` 等）

```powershell
dotnet build "G:\GCModeller\src\R-sharp\R_system.NET5.slnx" -c Rsharp_app_release -p:Platform=x64
```

产物刷新到 `App\net10.0`，包括 `R#.exe`、`R#.dll` 与各基础程序集。漏做这一步的典型症状是 `MissingMethodException`、`TypeLoadException`，或新加的方法/参数完全「不生效」。

### 步骤 3：在运行时目录执行脚本

```powershell
cd G:\GCModeller\src\R-sharp\App\net10.0
& ".\R#.exe" "G:\GCModeller\test\fuzzygene_api_test.R" --attach "G:\GCModeller" --debug=none
```

- 必须先 `cd` 到 `App\net10.0`：原生 dll、`R#.runtimeconfig.json`、依赖解析都相对该目录，在别处执行会因找不到原生库失败。
- 第一个位置参数是脚本路径，不是选项；路径含空格时必须加引号。
- `--attach` 传**程序包根目录**（不是 `assembly`，也不是 `assembly\*.nupkg`）。传入单个目录时，运行时还会自动把 `<dir>\assembly\` 加入依赖 dll 搜索路径。
- 多个包写成一次 `--attach "A;B"`（分号或逗号分隔）。重复书写 `--attach` 会直接报 `switch key ... already been added`。
- `--attach` 也可传已安装包名或 R 包 zip，但常规测试使用包根目录即可。
- 常用开关：`--debug[=none|memory|stack|all]`、`--silent`、`--verbose`、`--WORKDIR <dir>`、`--pkg_attach <dir>`、`-e <expr>`。
- `R#.exe` **没有内置超时**，长时间计算会一直阻塞；执行时需自行设置命令超时。

退出码：

| 退出码 | 含义 |
| --- | --- |
| `0` | 脚本正常执行完毕 |
| `500` | 脚本内错误（`stop()`、语法错误、CLR 异常），错误信息打印在标准输出 |
| `404` | 脚本路径不存在 |

注意：脚本内自行捕获了异常并正常结束时，退出码仍是 `0`。因此通过与否必须以脚本输出的 `[PASS]`/`[FAIL]` 结论为准，不能只看退出码。

## 测试脚本编写约定

- 脚本放在程序包的 `test\` 目录，命名 `*_test.R`。
- 文件头写清：测试目的、涉及的 API、运行命令、前置编译条件。
- 顶部用 `imports` 引入被测命名空间：

```r
imports "FuzzyGene" from "biosystem";
imports "geneExpression" from "phenotype_kit";
```

  命名空间**大小写敏感**，`fuzzygene::` 与 `FuzzyGene::` 是两回事；写错时报 `we can not found any namespace called: '...'`。命名空间名从包内 `NAMESPACE`、`man\index.json` 或对应 VB.NET 源码确认，不要猜。
- 断言用 `stop()`，失败即中止并让退出码变成 `500`：

```r
if (nrow(result) == 0) {
    stop("[FAIL] step 2: result is empty");
}
```

- 把一个用例的中间产物与判定同时写入结果文件，便于事后核查：

```r
writeLines(c("[PASS] step 3: got 18533 genes"), con = "G:/GCModeller/test/report.txt");
```

- 若某步失败仍需继续跑后续用例，用 `tryCatch({...}, error = function(e) {...}, finally = {...})` 包裹，但**必须在结尾显式 `stop()`**，否则失败会被退出码 0 掩盖。
- 结果文件与中间产物写到 `test\` 下的独立目录，避免污染程序包根目录。
- 涉及 .NET 对象（如 `HTS.DataFrame.Matrix`）时，不要用 R 的 `dim()`/`nrow()`/`ncol()` 取形状，这些函数对 CLR 对象只会返回 1；改用返回的 `data.frame` 或落盘后读回验证。
- R# 不是 R 的全集。模板里用到的函数若报「函数不存在」，改用最简写法（`print` + `if` + `writeLines`），不要引入额外的 R 包。

最小模板：

```r
# ============================================================
# <被测功能> R# script test
# 运行：
#   cd G:\GCModeller\src\R-sharp\App\net10.0
#   & ".\R#.exe" "<this script>" --attach "G:\GCModeller" --debug=none
# 前置：dotnet build "G:\GCModeller\src\GCModeller.slnx" -c Rsharp_app_release -p:Platform=x64
# ============================================================

imports "<Namespace>" from "<package>";

print("[STEP] 1/2 load input");
let data = read.csv("<input.csv>");

print("[CHECK] input is not empty");
if (nrow(data) == 0) {
    stop("[FAIL] input is empty");
}

print("[STEP] 2/2 call api");
let result = <package>::<api>(data);

print("[CHECK] output shape");
print(dim(result));

print("[PASS] all checks passed");
```

## 结果验证与故障定位

编译后必须核对程序集是否真的更新，这是判断「代码问题 vs 环境问题」的第一依据：

```powershell
Get-ChildItem "G:\GCModeller\assembly\net10.0\biosystem.dll" | Select-Object Name,LastWriteTime,Length
Get-ChildItem "G:\GCModeller\assembly\*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 3 Name,LastWriteTime
Get-ChildItem "G:\GCModeller\src\R-sharp\App\net10.0\R#.exe" | Select-Object Name,LastWriteTime
```

`assembly` 下可能同时存在两种形态：`assembly\net10.0\*.dll`（直接输出）与 `assembly\*.nupkg`（以 nuget 包形式发布，包内为 `lib\net10.0\*.dll`）。两者都需按时间戳确认，不要只看其中一处。

| 现象 | 最可能的原因 | 处理 |
| --- | --- | --- |
| `we can not found any namespace called` | 命名空间大小写错误，或包未被 `--attach` | 核对 `NAMESPACE` 与 `imports` 写法，确认 `--attach` 指向包根 |
| 找不到程序集 / 找不到类型 | 步骤 1 未编译，或 `assembly` 未更新 | 重编包解决方案，核对 dll 时间戳 |
| `MissingMethodException` / `TypeLoadException` | 步骤 2 未执行，运行时仍是旧基础库 | 编 `R_system.NET5.slnx`，核对 `App\net10.0` 时间戳 |
| 新逻辑完全不生效、结果与修改前一致 | 加载了旧 dll，或 `--attach` 指向了别的包副本 | 核对 dll 时间戳与 `--attach` 路径 |
| 原生 dll / runtimeconfig 相关错误 | 工作目录不是 `App\net10.0` | `cd` 到该目录后重跑 |
| 命令挂起无输出 | R# 无超时机制，计算量大 | 缩短数据量或中断后缩小用例 |

## 测试报告

- R# 脚本测试的报告由脚本自身产出：`writeLines` 写 `test\` 下的结果文件（`[PASS]`/`[FAIL]` 逐条记录 + 关键数值），配合退出码与标准输出结论。
- `DebugAudit.vb`（`G:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Core\src\ApplicationServices\Debugger\DebugAudit.vb`）仅适用于用 VB.NET 编写的 console 型测试工程，**不用于 R# 脚本测试**；只有当测试本身是一个 VB.NET 控制台程序时才使用它生成报告。

## 执行前检查清单

- [ ] 改动落在 `R\`（脚本，无需编译）还是 `src\`（VB.NET，必须编译）。
- [ ] 是否触及 `src\runtime\sciBASIC#` 或 `src\R-sharp`，若是则规划步骤 2。
- [ ] 使用的解决方案路径正确，且确实包含 `Rsharp_app_release|x64` 配置。
- [ ] 编译无错误，目标 dll/nupkg 与 `R#.exe` 的时间戳已刷新。
- [ ] 已 `cd` 到 `G:\GCModeller\src\R-sharp\App\net10.0`。
- [ ] `--attach` 指向包根目录，多个包写在同一次 `--attach` 内。
- [ ] 脚本结尾有 `[PASS]` 输出或 `stop()`，结论不依赖退出码单独判断。
- [ ] 命令设置了超时，避免长时间挂起。
