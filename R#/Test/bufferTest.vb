Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Serialize

Module bufferTest

    Dim R As New RInterpreter With {.debug = False}

    Sub Main()
        Dim vec As New vector({1, 2, 3, 4, 5}, RType.GetRSharpType(GetType(Integer)))

        vec.setNames({"a", "b", "c", "d", "e"}, R.globalEnvir)
        vec.unit = New unit With {.name = "abc"}

        Dim serial As vectorBuffer = vectorBuffer.CreateBuffer(vec, R.globalEnvir)
        Dim bytes = serial.Serialize

        Dim temp = "./test_vector.dat"

        Call bytes.FlushStream(temp)

        Dim vec2 = vectorBuffer.CreateBuffer(temp.Open).GetVector


        Dim message As Message = R.Evaluate("stop(['123456666','babala']);")

        Dim msgSerial As messageBuffer = New messageBuffer(message)
        bytes = msgSerial.Serialize

        temp = "./test_message.dat"
        Call bytes.FlushStream(temp)

        Dim msgNew = messageBuffer.CreateBuffer(temp.Open)


        Pause()
    End Sub
End Module
