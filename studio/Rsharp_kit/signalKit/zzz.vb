Imports SMRUCC.Rsharp.Runtime.Interop

<Assembly: RPackageModule>

Public Class zzz

    Public Shared Sub onLoad()
        Call wavToolkit.Main()
        Call signalProcessing.Main()
    End Sub
End Class
