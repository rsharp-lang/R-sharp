Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports SMRUCC.Rsharp.Development.Components
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports REnv = SMRUCC.Rsharp.Runtime

Module jsonlite

    Public Function toJSON(x As Object, env As Environment,
                           Optional maskReadonly As Boolean = False,
                           Optional indent As Boolean = False,
                           Optional enumToStr As Boolean = True,
                           Optional unixTimestamp As Boolean = True) As Object

        Dim json As JsonElement
        Dim opts As New JSONSerializerOptions With {
            .indent = indent,
            .maskReadonly = maskReadonly,
            .enumToString = enumToStr,
            .unixTimestamp = unixTimestamp
        }

        If x Is Nothing Then
            Return "null"
        End If

        If TypeOf x Is vector Then
            x = DirectCast(x, vector).data
        End If

        If x.GetType.IsArray Then
            If DirectCast(x, Array).Length = 1 Then
                x = DirectCast(x, Array).GetValue(Scan0)
            ElseIf DirectCast(x, Array).Length = 0 Then
                Return "[]"
            Else
                x = REnv.TryCastGenericArray(DirectCast(x, Array), env)
            End If
        End If

        If Program.isException(x) Then
            Return x
        End If

        If Not TypeOf x Is JsonElement Then
            x = Encoder.GetObject(x)
            json = x.GetType.GetJsonElement(x, opts)
        Else
            json = DirectCast(x, JsonElement)
        End If

        Dim jsonStr As String = json.BuildJsonString(opts)

        Return jsonStr
    End Function
End Module
