Imports System.Data
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Python.Language
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser.SyntaxImplements
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components


Public Class PythonCodeDOM

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

    Public Overridable Function ToExpression(release As Index(Of String)) As Expression
        Call release.Add(Me.GetHashCode.ToHexString)
        Return New ClosureExpression(script.ToArray)
    End Function

End Class

