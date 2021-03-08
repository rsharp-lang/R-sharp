Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Module createException

    Sub Main()
        Dim env = New RInterpreter().globalEnvir
        Dim msg As Message = Internal.debug.stop({"hello", "12345"}, env, suppress:=True)
        Dim ex As Exception = msg.ToException

        Call App.LogException(ex)

        Throw ex
    End Sub
End Module
