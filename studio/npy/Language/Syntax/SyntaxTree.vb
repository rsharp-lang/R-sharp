Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Python.Language
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser.SyntaxImplements
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Public Module SyntaxTree

    <Extension>
    Public Function ParsePyScript(script As Rscript, Optional debug As Boolean = False) As Program
        Dim scanner As New PyScanner(script.script)
        Dim opts As New SyntaxBuilderOptions With {
            .source = script,
            .debug = debug
        }
        Dim tokens As Token() = scanner.GetTokens.ToArray
        Dim lines As PythonLine() = tokens _
            .Split(Function(t) t.name = TokenType.newLine) _
            .Where(Function(l) l.Length > 0) _
            .Select(Function(t) New PythonLine(t)) _
            .Where(Function(l) l.tokens.Length > 0) _
            .ToArray
        Dim stack As New Stack(Of TaggedObject)
        Dim err As SyntaxResult
        Dim current As New TaggedObject With {
            .keyword = "python",
            .level = -1,
            .script = New List(Of Expression)
        }

        stack.Push(current)

        For Each line As PythonLine In lines
            ' 每一行前面的空格数量作为层级关系
            If line(Scan0).name = TokenType.keyword Then
                Select Case line(Scan0).text
                    Case "def"
                        Dim args As New List(Of DeclareNewSymbol)
                        tokens = line.tokens.Skip(3).Take(line.tokens.Length - 5).ToArray
                        err = DeclareNewFunctionSyntax.getParameters(tokens, args, opts)

                        current = New FunctionTag With {
                           .keyword = line(Scan0).text,
                           .level = line.levels,
                           .script = New List(Of Expression),
                           .funcname = line(1).text,
                           .arguments = args
                        }

                        stack.Push(current)
                    Case Else
                        Throw New NotImplementedException
                End Select
            ElseIf line.levels > current.level Then


            End If
        Next

        Return New Program(stack.Pop.script)
    End Function

End Module

Public Class TaggedObject

    Public Property keyword As String
    Public Property level As Integer
    Public Property script As List(Of Expression)

    Public Overrides Function ToString() As String
        Return $"[{level}] {keyword}: {script.JoinBy("; ")}"
    End Function

End Class

Public Class FunctionTag : Inherits TaggedObject

    Public Property funcname As String
    Public Property arguments As Expression()

End Class

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
