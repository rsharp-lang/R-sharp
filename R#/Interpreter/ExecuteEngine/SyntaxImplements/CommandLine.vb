Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.ExecuteEngine.SyntaxImplements

    Module CommandLineSyntax

        Const InterpolatePattern$ = "[$]\{.+?\}"

        Public Function CommandLine(shell As Token) As SyntaxResult
            If Not shell.text _
                .Match(InterpolatePattern, RegexOptions.Singleline) _
                .StringEmpty Then

                ' 如果是字符串插值，在Windows平台上会需要注意转义问题
                ' 因为windows平台上文件夹符号为\
                ' 可能会对$产生转义，从而导致字符串插值失败
                Dim temp As SyntaxResult = SyntaxImplements.StringInterpolation(shell)

                If temp.isException Then
                    Return temp
                Else
                    Return New CommandLine(temp.expression)
                End If
            Else
                Return New CommandLine(New Literal(shell))
            End If
        End Function

        <Extension>
        Friend Function isInterpolation(text As String) As Boolean
            Return Not text.Match(InterpolatePattern, RegexOptions.Singleline).StringEmpty
        End Function
    End Module
End Namespace