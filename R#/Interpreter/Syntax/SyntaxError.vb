
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Namespace Interpreter.SyntaxParser

    Public Class SyntaxError

        Public Property upstream As String
        Public Property errorBlock As String
        Public Property downstream As String
        Public Property from As CodeSpan
        Public Property [to] As CodeSpan
        Public Property exception As Exception
        Public Property file As String

        Public Overrides Function ToString() As String
            Dim rawText As String = errorBlock
            Dim err As Exception = exception
            Dim message As String = err.ToString
            Dim nlen As Integer = rawText.LineTokens.MaxLengthString.Length
            Dim errorsPromptLine = New String("~"c, nlen)

            message &= vbCrLf & vbCrLf & "Syntax error nearby:"
            message &= vbCrLf & upstream
            message &= vbCrLf & vbCrLf & "-->>>"
            message &= vbCrLf & rawText
            message &= vbCrLf & errorsPromptLine
            message &= vbCrLf & "<<<--" & vbCrLf
            message &= vbCrLf & downstream
            message &= vbCrLf & vbCrLf & $"Range from {from.start} at line {from.line}, to {[to].stops} at line {[to].line}."
            message &= vbCrLf & "Rscript: " & file

            Return message
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Shared Function CreateError(opts As SyntaxBuilderOptions, err As Exception) As SyntaxError
            Return CreateError(opts, err, opts.fromSpan, opts.toSpan)
        End Function

        Friend Shared Function CreateError(opts As SyntaxBuilderOptions,
                                           err As Exception,
                                           from As CodeSpan,
                                           [to] As CodeSpan) As SyntaxError

            Dim syntaxErr As New SyntaxError With {
                .exception = err,
                .from = from,
                .[to] = [to],
                .file = opts.source.ToString
            }
            Dim scriptLines As String() = opts.source.script.LineTokens

            syntaxErr.upstream = scriptLines.Skip(from.line - 3).Take(2).JoinBy(vbCrLf)
            syntaxErr.downstream = scriptLines.Skip([to].line).Take(3).JoinBy(vbCrLf)
            syntaxErr.errorBlock = scriptLines.Skip(from.line - 1).Take([to].line - from.line + 1).JoinBy(vbCrLf)

            Return syntaxErr
        End Function

    End Class
End Namespace