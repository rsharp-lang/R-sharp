Imports Microsoft.VisualBasic.My.JavaScript
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class Options

        Dim pipeline As PipelineKeyword()

        Sub New(pipeline As IEnumerable(Of Expression))
            Me.pipeline = pipeline _
                .Select(Function(l) DirectCast(l, PipelineKeyword)) _
                .ToArray
        End Sub

        Public Function RunOptionPipeline(output As IEnumerable(Of JavaScriptObject), context As ExecutableContext) As IEnumerable(Of JavaScriptObject)
            Dim raw As JavaScriptObject() = output.ToArray
            Dim allNames As String() = raw(Scan0).GetNames
            Dim env As Environment = context

            For Each name As String In allNames
                If env.FindSymbol(name, [inherits]:=False) Is Nothing Then
                    Call env.Push(name, Nothing, [readonly]:=False, mode:=TypeCodes.generic)
                End If
            Next

            For Each line As PipelineKeyword In pipeline
                raw = line.Exec(raw, context).ToArray
            Next

            Return raw
        End Function

        Public Overrides Function ToString() As String
            Return pipeline.JoinBy(vbCrLf)
        End Function

    End Class
End Namespace