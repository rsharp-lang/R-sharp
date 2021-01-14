Imports Microsoft.VisualBasic.CommandLine.InteropService
Imports Parallel
Imports snowFall.RscriptCommandLine

Namespace Protocol

    Public Class Host

        Public Shared Function CreateSlave() As SlaveTask
            Return New SlaveTask(Host.CreateProcessor, AddressOf Host.SlaveTask)
        End Function

        Public Shared Function CreateProcessor() As Rscript
            Return Rscript.FromEnvironment(App.HOME)
        End Function

        Public Shared Function SlaveTask(processor As InteropService, port As Integer) As String
            Dim cli As String = DirectCast(processor, Rscript).GetparallelModeCommandLine(master:=port, [delegate]:="Parallel::snowFall")
            Return cli
        End Function
    End Class
End Namespace