Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Development

    ''' <summary>
    ''' the R# shell script commandline arguments helper module
    ''' </summary>
    Public Class ShellScript

        ReadOnly Rscript As Program

        Public ReadOnly Property message As String

        Sub New(Rscript As Rscript)
            Me.Rscript = Program.CreateProgram(Rscript, [error]:=message)
        End Sub

        Public Function AnalysisAllCommands() As ShellScript

        End Function

        Public Shared Widening Operator CType(Rscript As Rscript) As ShellScript
            Return New ShellScript(Rscript)
        End Operator
    End Class
End Namespace