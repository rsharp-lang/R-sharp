Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser


Public Class JuliaCodeDOM

    Public Property keyword As String
    Public Property level As Integer
    Public Property script As List(Of Expression)

    Friend Sub Add(line As SyntaxResult)
        script.Add(line.expression)
    End Sub

    Friend Sub Add(line As Expression)
        script.Add(line)
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{level}] {keyword}: {script.JoinBy("; ")}"
    End Function

    Public Overridable Function ToExpression() As Expression
        Return New ClosureExpression(script.ToArray)
    End Function

End Class

