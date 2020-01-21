Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Interop

Module defaultValueTest

    Dim R As New RInterpreter With {.debug = True}

    Sub Main()
        Call R.LoadLibrary(GetType(defaultValueTest))
        Call R.Print("testApi")

        Pause()
    End Sub

    <ExportAPI(NameOf(testApi))>
    Sub testApi(<RDefaultValue("1,1,1,1,0,0,0,0,0,0,yes,false,no,0,0,0,0,0,0,1,0,1,0,1,0,0")>
                Optional flags As defaultVector = Nothing)

        Call Console.WriteLine(flags.data.GetJson)
    End Sub
End Module

Class defaultVector

    Public Property data As Boolean()

    Public Shared Widening Operator CType(default$) As defaultVector
        Return New defaultVector With {.data = [default].Split(","c).Select(AddressOf ParseBoolean).ToArray}
    End Operator
End Class