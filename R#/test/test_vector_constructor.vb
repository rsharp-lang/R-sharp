Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Interop

Module test_vector_constructor

    Sub Main()
        Dim env As GlobalEnvironment = GlobalEnvironment.defaultEmpty
        Dim a As Object() = {"65", 144, 1567643245564664456&, 1.231, 5.456, 4.5, 4, 5.64, 56.4, 5.6, 45.6, 4.56, 4, 56}
        Dim numeric As Type = GetType(Double)
        Dim v As New vector(numeric, a, env)
        Pause()
    End Sub
End Module
