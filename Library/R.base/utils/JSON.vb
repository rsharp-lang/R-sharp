Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

<Package("JSON", Category:=APICategories.UtilityTools, Publisher:="i@xieguigang.me")>
Module JSON

    <ExportAPI("parseJSON")>
    Public Function fromJSON(str As String, Optional raw As Boolean = False, Optional env As Environment = Nothing) As Object
        Dim rawElement As JsonElement = New JsonParser().OpenJSON(str)

        If raw Then
            Return rawElement
        Else
            Return rawElement.createRObj(env)
        End If
    End Function

    <ExportAPI("write.bson")>
    Public Function writeBSON(obj As Object, Optional file As Object = Nothing, Optional env As Environment = Nothing) As Object
        Dim stream As Stream

        If file Is Nothing Then
            stream = New MemoryStream
        Else
            If TypeOf file Is Stream Then
                stream = DirectCast(file, Stream)
            ElseIf TypeOf file Is String Then
                stream = DirectCast(file, String).Open
            Else
                Return Message.InCompatibleType(GetType(Stream), file.GetType, env)
            End If
        End If

        If TypeOf obj Is vbObject Then
            obj = DirectCast(obj, vbObject).target
        End If

        If Not TypeOf obj Is JsonObject Then
            obj = ObjectSerializer.GetJsonElement(
                schema:=obj.GetType,
                obj:=obj,
                opt:=New JSONSerializerOptions
            )
        End If

        Call BSON.WriteBuffer(DirectCast(obj, JsonObject), stream)

        If file Is Nothing Then
            Return DirectCast(stream, MemoryStream).ToArray
        Else
            Call stream.Flush()
            Call stream.Close()
            Call stream.Dispose()

            Return Nothing
        End If
    End Function

    <Extension>
    Private Function createRObj(json As JsonElement, env As Environment) As Object
        If TypeOf json Is JsonValue Then
            Return DirectCast(json, JsonValue).GetStripString
        ElseIf TypeOf json Is JsonArray Then
            Dim array As JsonArray = DirectCast(json, JsonArray)

            If array.All(Function(a) TypeOf a Is JsonValue) Then
                Return array _
                    .Select(Function(a) a.createRObj(env)) _
                    .ToArray
            Else
                Dim list As New list With {.slots = New Dictionary(Of String, Object)}
                Dim i As i32 = 1

                For Each item As JsonElement In array
                    list.slots.Add($"[[{++i}]]", item.createRObj(env))
                Next

                Return list
            End If
        ElseIf TypeOf json Is JsonObject Then
            Dim list As New list With {
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
