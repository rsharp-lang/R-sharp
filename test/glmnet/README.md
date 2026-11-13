# glmnet R# 程序包与 GNU R 对拍测试

本目录用于验证 `R-sharp/Library/Rlapack/glmnet.vb` 中封装的 R# `glmnet` 包，
其底层算法来自 `runtime/sciBASIC#/Data_science/MachineLearning/GLMNet/GLMNet.vbproj`，
对照基准是 GNU R 4.5.0 的 `glmnet` 5.1。

## 环境

| 工具 | 路径 |
|---|---|
| GNU R | `C:\Program Files\R\R-4.5.0\bin\Rscript.exe` |
| .NET SDK | `dotnet build`，目标框架 `net10.0` |
| R# 运行时 | `G:\GCModeller\src\R-sharp\App\net10.0\R#.exe` |
| 解决方案 | `G:\GCModeller\src\R-sharp\R_system.NET5.slnx` |

## 一键运行

```bat
G:\GCModeller\src\R-sharp\test\glmnet\run-tests.cmd
```

脚本依次执行：版本核验 → 导出黄金数据 → 以 `Rsharp_app_release|x64`
编译解决方案 → 运行三个 R# demo → 运行 GNU R 对拍报告。
`R#.exe` 的脚本错误不会稳定地反映在进程退出码上，因此**最终判定以
`compare.R` 为准**（它会校验所有结果 CSV 是否存在并逐字段断言）。

分步执行：

```powershell
$RSCRIPT = "C:\Program Files\R\R-4.5.0\bin\Rscript.exe"
$RSHARP  = "G:\GCModeller\src\R-sharp\App\net10.0\R#.exe"
$ROOT    = "G:\GCModeller\src\R-sharp\test\glmnet"

& $RSCRIPT "$ROOT\R\check_version.R"
& $RSCRIPT "$ROOT\R\export_golden.R" "$ROOT\data"
dotnet build "G:\GCModeller\src\R-sharp\R_system.NET5.slnx" -c Rsharp_app_release -p:Platform=x64
& $RSHARP "$ROOT\demo_fit.R"
& $RSHARP "$ROOT\demo_cv.R"
& $RSHARP "$ROOT\demo_predict.R"
& $RSCRIPT "$ROOT\R\compare.R" "$ROOT\data" "$ROOT\result" "$ROOT\report"
```

## 目录约定

| 路径 | 内容 |
|---|---|
| `R/check_version.R` | 核验 R / glmnet 版本、默认参数与六个内置数据集 |
| `R/export_golden.R` | 导出 GNU R 黄金 CSV 到 `data/` |
| `R/compare.R` | 逐字段断言，输出 `report/compare_detail.csv` 与 `report/compare_report.txt` |
| `demo_fit.R` | R#：`glmnet()` 六家族路径拟合 |
| `demo_cv.R` | R#：`cv.glmnet()` 六家族交叉验证 |
| `demo_predict.R` | R#：`predict.glmnet` / `coef.glmnet` / `deviance.glmnet` |
| `data/` | GNU R 生成的输入与基准（`*_x/_y/_foldid/_fit/_beta/_cv/_s/_newx/_coef/_predict.csv`） |
| `result/` | R# demo 生成的同构结果 CSV |
| `report/` | 对拍明细 CSV 与文本报告 |

R# 脚本用 `imports "glmnet" from "Rlapack";` 加载包，随后用
`glmnet::glmnet(...)` 这类命名空间前缀调用，避免与其他包的同名泛型冲突。
包内同时导出了裸名 `predict` / `coef` / `deviance` 别名（对齐 GNU R 调用习惯），
导入 `glmnet` 后它们会 mask 其它包的同名符号；需要稳定无副作用时请使用
`glmnet::predict.glmnet` / `glmnet::coef.glmnet` / `glmnet::deviance.glmnet`。
`demo_cv.R` 额外输出 `*_cv_frame.csv`，仅用于演示
`as.data.frame.glmnet(cv)`，`compare.R` 不读取它。

## R# API

| R# 函数 | GNU R 对应 |
|---|---|
| `glmnet(x, y, family, alpha, nlambda, lambda, lambda.min.ratio, weights, offset, standardize, intercept, penalty.factor, lower.limits, upper.limits, exclude, dfmax, pmax, thresh, maxit, type.gaussian, type.multinomial, type.logistic, standardize.response, relax, feature.names, response.names)` | `glmnet()` |
| `cv.glmnet(..., nfolds, foldid, type.measure, grouped, keep, seed)` | `cv.glmnet()` |
| `predict.glmnet(fit, newx, type, s, exact, newoffset)` / `predict` | `predict.glmnet()` |
| `coef.glmnet(fit, s, exact)` / `coef` | `coef.glmnet()` |
| `deviance.glmnet(fit, newx, newy, s)` / `deviance` | `deviance.glmnet()` |
| `print.glmnet` / `summary.glmnet` / `as.data.frame.glmnet` | S3 方法 |
| `glmnet.beta` / `glmnet.cv` | 结果表导出辅助函数 |

VB 参数名用下划线（`lambda_min_ratio`、`type_measure`、`penalty_factor`），
R# 运行时会自动把 `_` 映射为 `.`，所以脚本里既可用点号也可用下划线。
`foldid` 与 `exclude` 按 GNU R 约定传入 1-based 下标，适配层内部转换为 0-based。

结果以 R 风格命名列表返回：`fit$lambda`、`fit$beta`、`fit$dev.ratio`、
`fit$nobs`、`cv$cvm`、`cv$lambda.min`、`cv$lambda.1se` 等；
`predict` 返回含 `values` / `coefficients` / `indices` / `prediction` 的列表。

## 对拍要点

- **lambda 网格对齐**：GNU R 会因 `fdev` 早停把默认 100 步截断
  （实测 67 / 98 / 68 / 71 / 49 / 78 步）。R# demo 从 `data/*_fit.csv` 读出
  R 的实际 lambda 序列并作为 `lambda` 参数传入，从而逐步对齐。
- **CV 去噪**：`export_golden.R` 显式导出 1-based `foldid`，R# 侧读入后
  传给 `cv.glmnet`；`nfolds` 必须设为 5（与导出的折数一致），否则空折会把
  `cvm` 拉偏。
- **s 参考点**：`data/*_s.csv` 保存网格上的第 2 个 lambda（不插值），
  两侧 `coef()` / `predict()` 都在同一 `s` 上取值。

## 容差与已知限制

`compare.R` 按家族分级容差（依据底层 `GLMNet/README.md` 的实测校准记录）：

- `lambda`、`nulldev` 近似严格（六家族实测偏差在机器精度量级）；
- `dev.ratio` 最大偏差实测：gaussian 3.7e-6、binomial 3.3e-4、poisson 9.1e-4、
  multinomial 1.4e-2、cox 1.4e-5、mgaussian 2.6e-6；
- `df` 允许 1 个网格步差异；
- **binomial**：接近完全分离时系数解不唯一（个别系数可差约 2.4），
  系数容差放宽到 3.0，以 `dev.ratio` 与 CV 曲线为主判据；
- **多响应家族**：`a0`（黄金侧为零占位）与 `nzero`（变量数 vs 系数个数）
  语义不同，明确标为 SKIP；
- **CV**：`cvm`/`cvsd` 绝对尺度会因权重归一化与指标定义而不同，因此以
  `lambda.min` / `lambda.1se` 的**网格位置**为主要判据；multinomial 的
  CV 指标语义差异较大，最优点不参与断言；
- **cox**：GNU R 的 `coef(fit)` 没有截距行，比较前会丢弃 R# 结果的第一行。

报告中的 `SKIP` 项均为上述已知语义差异，不是测试失败。
当前结果：`84 checks, 79 PASS, 0 FAIL, 5 SKIP`，`RESULT: PASS`。

## 故障排查

- `require(glmnet)` 找不到包：R# 的 DLL 包需要先进入包数据库；本目录脚本
  统一使用 `imports "glmnet" from "Rlapack"`，可绕过该限制。
- `write.csv` 报 `arguments imply differing number of rows`：结果 `dataframe`
  中存在长度不一致的列；`cv.glmnet` 的 `foldid` 是逐样本的，已刻意不放入
  CV 曲线表，仍保留在 `cv$foldid` 列表中。
- R# 输出仍为旧行为：确认编译的是 `Rsharp_app_release|x64`，`Rlapack` 的
  输出路径为 `../../App\`（即 `App/net10.0/Rlapack.dll`），并检查 dll 时间戳。
- 黄金数据格式异常：`export_golden.R` 用 `formatC(digits=17)` + `trimws`
  写全精度数值，避免 `write.table` 默认 7 位有效数字损失精度。
- `R#.exe` 打印的 `no dll runtime module was found: ...` 是本仓库其它可选
  模块的既有告警，不影响 glmnet 对拍。
