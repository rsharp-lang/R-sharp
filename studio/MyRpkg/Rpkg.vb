Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes

<Package("RPkg")>
Public Module Rpkg

    Public Sub Main()

    End Sub

    <ExportAPI("hello_world")>
    Public Sub HelloWorld(Optional env As Environment = Nothing)
        Call base.print("Hello world!",, env)
    End Sub
End Module
