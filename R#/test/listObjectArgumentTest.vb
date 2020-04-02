Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("bbbbb")>
Module listObjectArgumentTest

    Dim R As New RInterpreter With {.debug = True}

    Sub Main()
        Call R.LoadLibrary(GetType(listObjectArgumentTest))

        Call R.Evaluate("left(aa=1,bb=3,cc='d',a='99999')")
        Call R.Evaluate("right(888,b='a', a= 'b', aa = 1111,c=FALSE,d=[12,34,6])")

        Pause()
    End Sub

    <ExportAPI("left")>
    Public Function left(<RListObjectArgument> l As Object, Optional XX As Integer = 9, Optional a As String = "aaaa", Optional env As Environment = Nothing)
        Dim list = base.Rlist(l, env)

        Console.WriteLine(a)
        Console.WriteLine(XX)
        Console.WriteLine(DirectCast(list, list).slots.GetJson)
    End Function

    <ExportAPI("right")>
    Public Function right(XX As Integer, a As String, b As String, <RListObjectArgument> l As Object, Optional env As Environment = Nothing)
        Dim list = base.Rlist(l, env)

        Console.WriteLine(a)
        Console.WriteLine(b)
        Console.WriteLine(XX)
        Console.WriteLine(DirectCast(list, list).slots.GetJson)
    End Function
End Module
