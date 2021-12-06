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


Public Class PythonLine

    Public ReadOnly Property tokens As Token()
    Public ReadOnly Property levels As Integer

    Default Public ReadOnly Property Token(i As Integer) As Token
        Get
            Return tokens(i)
        End Get
    End Property

    Sub New(tokens As IEnumerable(Of Token))
        Me.tokens = tokens.ToArray
        Me.levels = Me.tokens _
            .TakeWhile(Function(t)
                           Return t.name = TokenType.delimiter
                       End Function) _
            .Count
        Me.tokens = Me.tokens _
            .Where(Function(t)
                       Return Not t.name = TokenType.delimiter
                   End Function) _
            .ToArray
    End Sub

    Public Overrides Function ToString() As String
        Return tokens.Select(Function(t) t.text).JoinBy(" ")
    End Function

End Class
