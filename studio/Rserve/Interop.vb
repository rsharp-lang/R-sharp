Public Module Interop

    Public Function CreateServer() As Rserver.CLI.Rserve
        Return Rserver.CLI.Rserve.FromEnvironment(App.HOME)
    End Function
End Module
