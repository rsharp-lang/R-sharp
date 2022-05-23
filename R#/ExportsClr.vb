
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes

''' <summary>
''' the python.NET helper module
''' </summary>
Public Class ExportsClr

    ReadOnly renv As New RInterpreter

    Public Function HelloWorld() As Object
        Return base.print("Hello World!",, env:=renv.globalEnvir)
    End Function

End Class