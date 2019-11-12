Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal
Imports devtools = Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics

Namespace Interpreter.ExecuteEngine

    Public Class CommandLine : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Dim cli As Expression

        Const InterpolatePattern$ = "[$]\{.+?\}"

        Sub New(shell As Token)
            If Not shell.text _
                .Match(InterpolatePattern, RegexOptions.Singleline) _
                .StringEmpty Then

                ' 如果是字符串插值，在Windows平台上会需要注意转义问题
                ' 因为windows平台上文件夹符号为\
                ' 可能会对$产生转义，从而导致字符串插值失败
                cli = New StringInterpolation(shell)
            Else
                cli = New Literal(shell)
            End If
        End Sub

        Private Shared Function isInterpolation(text As String) As Boolean
            Return Not text.Match(InterpolatePattern, RegexOptions.Singleline).StringEmpty
        End Function

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim commandline$ = Runtime.getFirst(cli.Evaluate(envir))
            Dim process As New IORedirectFile(commandline)
            Dim std_out$

            If commandline.DoCall(AddressOf isInterpolation) Then
                Call commandline _
                    .DoCall(Function(cli)
                                Return possibleInterpolationFailure(cli, envir)
                            End Function) _
                    .DoCall(AddressOf envir.GlobalEnvironment.messages.Add)
            End If

            process.Run()
            std_out = process.StandardOutput

            Return std_out
        End Function

        Private Shared Function possibleInterpolationFailure(commandline As String, envir As Environment) As Message
            Return New Message With {
                .Message = {
                    $"The input commandline string contains string interpolation syntax tag...",
                    $"commandline: " & commandline
                },
                .MessageLevel = MSG_TYPES.WRN,
                .EnvironmentStack = envir.getEnvironmentStack,
                .Trace = devtools.ExceptionData.GetCurrentStackTrace
            }
        End Function
    End Class
End Namespace