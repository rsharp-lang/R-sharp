Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.serialize
Imports SMRUCC.Rsharp.Runtime.Interop

Module bufferTest

    Dim R As New RInterpreter With {.debug = True}

    Sub Main()
        Dim vec As New vector({1, 2, 3, 4, 5}, RType.GetRSharpType(GetType(Integer)))

        vec.setNames({"a", "b", "c", "d", "e"}, R.globalEnvir)
        vec.unit = New unit With {.name = "abc"}

        Dim serial As vectorBuffer = vectorBuffer.CreateBuffer(vec, R.globalEnvir)
        Dim bytes = serial.Serialize

        Dim temp = "./test_vector.dat"

        Call bytes.FlushStream(temp)

        Dim vec2 = vectorBuffer.CreateBuffer(temp.Open).GetVector

        Pause()
    End Sub
End Module
