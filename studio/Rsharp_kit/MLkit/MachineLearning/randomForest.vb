
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.MachineLearning.RandomForests
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("randomForest")>
Public Module randomForest

    <ExportAPI("randomForest")>
    <RApiReturn(GetType(Result))>
    Public Function randomForest(x As MLDataFrame, Optional env As Environment = Nothing) As Object
        Dim data As New Data(x)
        Dim tree As New RanFog With {.max_branch = 3, .max_tree = 1000}
        Dim result As Result = tree.Run(data)
        Return result
    End Function

    <ExportAPI("importance")>
    Public Function importance(tree As Result) As Object
        Return tree.Model.VI
    End Function

    Public Function varImpPlot()

    End Function

End Module
