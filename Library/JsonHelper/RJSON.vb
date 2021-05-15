Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Module RJSON

    <Extension>
    Friend Function createRObj(json As JsonElement, env As Environment) As Object
        If TypeOf json Is JsonValue Then
            Return DirectCast(json, JsonValue).GetStripString
        ElseIf TypeOf json Is JsonArray Then
            Dim array As JsonArray = DirectCast(json, JsonArray)

            If array.All(Function(a) TypeOf a Is JsonValue) Then
                Return array _
                    .Select(Function(a) a.createRObj(env)) _
                    .ToArray
            Else
                Dim list As New List With {.slots = New Dictionary(Of String, Object)}
                Dim i As i32 = 1

                For Each item As JsonElement In array
                    list.slots.Add($"[[{++i}]]", item.createRObj(env))
                Next

                Return list
            End If
        ElseIf TypeOf json Is JsonObject Then
            Dim list As New List With {
                .slots = New Dictionary(Of String, Object)
            }

            For Each item As NamedValue(Of JsonElement) In DirectCast(json, JsonObject)
                Call list.slots.Add(item.Name, item.Value.createRObj(env))
            Next

            Return list
        Else
            Return Internal.debug.stop(Message.InCompatibleType(GetType(JsonElement), json.GetType, env), env)
        End If
    End Function
End Module
