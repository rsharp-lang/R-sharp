
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MIME.Html.Document
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components.[Interface]
Imports SMRUCC.Rsharp.Runtime.Vectorization

<Package("jQuery")>
Public Class jQuery : Implements RIndexer

    Dim page As HtmlDocument

    Public Function EvaluateIndexer(expr As Expression, env As Environment) As Object Implements RIndexer.EvaluateIndexer
        Dim q As String = CLRVector.asCharacter(expr.Evaluate(env)).First

        Return New jQuery
    End Function

    <ExportAPI("load")>
    Public Shared Function load(url As String) As jQuery
        Return New jQuery With {.page = HtmlDocument.LoadDocument(url.GET)}
    End Function
End Class
