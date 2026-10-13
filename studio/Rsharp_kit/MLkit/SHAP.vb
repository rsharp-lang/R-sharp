Imports System.Globalization
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MachineLearning.SHAP
Imports Microsoft.VisualBasic.Math.Statistics.ShapleyValue
Imports Microsoft.VisualBasic.Scripting.MetaData

<Package("SHAP")>
Module SHAP

    ReadOnly invariant As CultureInfo = CultureInfo.InvariantCulture

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="datasetName"></param>
    ''' <param name="modelName"></param>
    ''' <param name="explainer"></param>
    ''' <param name="dataset"></param>
    ''' <param name="output"></param>
    ''' <param name="shapSampleLimit">
    ''' 参与 SHAP 分析的样本数量上限（0 表示全部）。RanFog 的 TreeSHAP
    ''' 计算量较大，默认只解释前若干个样本。
    ''' </param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("analysis")>
    Public Function analysis(datasetName As String,
                        modelName As String,
                        explainer As IShapExplainer,
                        dataset As ShapDataset,
                        output As Func(Of Double(), Double), outputDir$, Optional shapSampleLimit As Integer = 200)

        Dim result As ShapAnalysisResult = ShapAnalyzer.Analyze(explainer, dataset, modelName, shapSampleLimit, output)
        Dim prefix As String = $"{datasetName}_{modelName}"

        Call ShapCsvWriter.WriteExplanations(result, $"{outputDir}/{prefix}_shap.csv")
        Call ShapCsvWriter.WriteGlobalImportance(result, $"{outputDir}/{prefix}_importance.csv")

        Console.WriteLine($"  SHAP: baseline={result.Baseline.ToString("G6", invariant)}, 解释样本数={result.Explanations.Count}")

        Console.WriteLine("  Top-5 全局重要性 (平均 |SHAP|):")

        For Each item In result.GlobalImportance().Take(5)
            Console.WriteLine($"    {item.name,-8} {item.importance.ToString("G6", invariant)}")
        Next

        Dim first As ShapExplanation = result.Explanations(0)
        Dim reconstructed As Double = first.Baseline + first.Contributions.Sum()

        Console.WriteLine($"  单样本解释 [{first.ID}]: baseline+sum(phi)={reconstructed.ToString("G6", invariant)}, 模型输出={first.Output.ToString("G6", invariant)}")

        If dataset.IsCategorical AndAlso System.Math.Abs(reconstructed - first.Output) > 0.000001 Then
            Console.WriteLine($"    (注: 分类模型输出经过 sigmoid 变换, sigmoid(baseline+sum(phi))={LinearShapExplainer.Sigmoid(reconstructed).ToString("G6", invariant)})")
        End If

        Console.WriteLine("    贡献 Top-3:")

        For Each item In first.Ranked().Take(3)
            Console.WriteLine($"      {item.name,-8} {item.value.ToString("G6", invariant)}")
        Next
    End Function
End Module
