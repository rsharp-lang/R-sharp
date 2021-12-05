Imports System.Runtime.CompilerServices
Imports SMRUCC.Python.Language
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Public Module SyntaxTree

    <Extension>
    Public Function ParsePyScript(script As Rscript) As Program
        Dim scanner As New PyScanner(script.script)
        Dim tokens As Token() = scanner.GetTokens.ToArray
        Dim lines As PythonLine() = tokens _
            .Split(Function(t) t.name = TokenType.newLine) _
            .Where(Function(l) l.Length > 0) _
            .Select(Function(t) New PythonLine(t)) _
            .ToArray
        Dim code As New List(Of Expression)
        Dim stack As New Stack(Of )

        For Each line As PythonLine In lines
            ' 每一行前面的空格数量作为层级关系
        Next

        Return New Program(code)
    End Function

End Module

Public Class TaggedObject

    Public Property keyword As String
    Public Property expresion As Expression
    Public Property script As New List(Of TaggedObject)

End Class

Public Class PythonLine

    Public ReadOnly Property tokens As Token()
    Public ReadOnly Property levels As Integer

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
