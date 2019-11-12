Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class CommandLine : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Dim cli As Expression

        Const InterpolatePattern$ = "[$]\{.+?\}"

        Sub New(shell As Token)
            If Not shell.text _
                .Match(InterpolatePattern, RegexOptions.Singleline) _
                .StringEmpty Then

                cli = New StringInterpolation(shell)
            Else
                cli = New Literal(shell)
            End If
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim commandline$ = Runtime.getFirst(cli.Evaluate(envir))
            Dim process As New IORedirectFile(commandline)
            Dim std_out$

            process.Run()
            std_out = process.StandardOutput

            Return std_out
        End Function
    End Class
End Namespace