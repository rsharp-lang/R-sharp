Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Development

    ''' <summary>
    ''' the R# shell script commandline arguments helper module
    ''' </summary>
    Public Module ShellScript

        <Extension>
        Public Iterator Function AnalysisAllCommands(script As Rscript) As IEnumerable(Of NamedValue(Of String))

        End Function
    End Module
End Namespace