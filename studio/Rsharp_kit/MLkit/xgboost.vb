
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Internal.Object

<Package("xgboost")>
Public Module xgboost

    <ExportAPI("xgboost")>
    Public Function xgboost()

    End Function

    <ExportAPI("xgb.DMatrix")>
    Public Function DMatrix(data As dataframe, label As String())

    End Function

End Module
