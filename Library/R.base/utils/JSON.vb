#Region "Microsoft.VisualBasic::50816f6235274fdcdff5dff2555e8267, Library\R.base\utils\JSON.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:

' Module JSON
' 
'     Function: buildObject, createRObj, fromJSON, json_decode, parseBSON
'               writeBSON
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Development.Components
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("JSON", Category:=APICategories.UtilityTools, Publisher:="i@xieguigang.me")>
Module JSON

    ''' <summary>
    ''' a short cut method of ``parseJSON`` 
    ''' </summary>
    ''' <param name="str"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("json_decode")>
    Public Function json_decode(str As String, Optional env As Environment = Nothing) As Object
        Return fromJSON(str, raw:=False, env:=env)
    End Function

    <ExportAPI("json_encode")>
    <RApiReturn(GetType(String))>
    Public Function json_encode(x As Object,
                                Optional maskReadonly As Boolean = False,
                                Optional indent As Boolean = False,
                                Optional enumToStr As Boolean = True,
                                Optional unixTimestamp As Boolean = True,
                                Optional env As Environment = Nothing) As Object
        If x Is Nothing Then
            Return "null"
        Else
            x = Encoder.GetObject(x)
        End If

        Dim opts As New JSONSerializerOptions With {
            .indent = indent,
            .maskReadonly = maskReadonly,
            .enumToString = enumToStr,
            .unixTimestamp = unixTimestamp
        }
        Dim json As JsonElement = x.GetType.GetJsonElement(x, opts)
        Dim jsonStr As String = json.BuildJsonString(opts)

        Return jsonStr
    End Function

    <ExportAPI("parseJSON")>
    Public Function fromJSON(str As String, Optional raw As Boolean = False, Optional env As Environment = Nothing) As Object
        Dim rawElement As JsonElement = New JsonParser().OpenJSON(str)

        If raw Then
            Return rawElement
        ElseIf rawElement Is Nothing Then
            If env.globalEnvironment.options.strict Then
                Return Internal.debug.stop("invalid format of the input json string!", env)
            Else
                env.AddMessage("invalid format of the input json string!", MSG_TYPES.WRN)
                Return Nothing
            End If
        Else
            Return rawElement.createRObj(env)
        End If
    End Function

    <ExportAPI("parseBSON")>
    Public Function parseBSON(<RRawVectorArgument> buffer As Object, Optional raw As Boolean = False, Optional env As Environment = Nothing) As Object
        Dim bytes As pipeline = pipeline.TryCreatePipeline(Of Byte)(buffer, env, suppress:=True)
        Dim bufStream As Stream

        If bytes.isError Then
            If TypeOf buffer Is Stream Then
                bufStream = DirectCast(buffer, Stream)
            Else
                Return bytes.getError
            End If
        Else
            bufStream = New MemoryStream(bytes.populates(Of Byte)(env).ToArray)
        End If

        Dim json As JsonObject = New BSON.Decoder(bufStream).decodeDocument

        If raw Then
            Return json
        Else
            Return json.createRObj(env)
        End If
    End Function

    <ExportAPI("object")>
    Public Function buildObject(json As JsonElement, schema As Object, Optional env As Environment = Nothing) As Object
        Dim type As RType = env.globalEnvironment.GetType([typeof]:=schema)
        Dim obj As Object = json.CreateObject(type)

        Return obj
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
