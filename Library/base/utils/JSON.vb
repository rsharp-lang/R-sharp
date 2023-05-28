#Region "Microsoft.VisualBasic::d94866907aacf0e3bf29f8101d1b87a4, F:/GCModeller/src/R-sharp/Library/base//utils/JSON.vb"

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


    ' Code Statistics:

    '   Total Lines: 273
    '    Code Lines: 166
    ' Comment Lines: 79
    '   Blank Lines: 28
    '     File Size: 10.71 KB


    ' Module JSON
    ' 
    '     Function: buildObject, fromJSON, json_decode, json_encode, parseBSON
    '               unescape, writeBSON
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' JSON (JavaScript Object Notation) is a lightweight data-interchange format. 
''' It is easy for humans to read and write. It is easy for machines to parse and 
''' generate. It is based on a subset of the JavaScript Programming Language 
''' Standard ECMA-262 3rd Edition - December 1999. JSON is a text format that 
''' is completely language independent but uses conventions of the ``R#`` language. 
''' JSON is an ideal data-interchange language.
'''
''' JSON Is built On two structures:
''' 
''' + A collection Of name/value pairs. In various languages, this Is realized As 
'''      an Object, record, struct, dictionary, hash table, keyed list, Or 
'''      associative array.
''' + An ordered list Of values. In most languages, this Is realized As an array, 
'''      vector, list, Or sequence.
'''      
''' These are universal data structures. Virtually all modern programming languages 
''' support them In one form Or another. It makes sense that a data format that 
''' Is interchangeable With programming languages also be based On these structures.
''' </summary>
<Package("JSON", Category:=APICategories.UtilityTools, Publisher:="i@xieguigang.me")>
Module JSON

    ''' <summary>
    ''' do string unescape
    ''' </summary>
    ''' <param name="str"></param>
    ''' <returns></returns>
    <ExportAPI("unescape")>
    Public Function unescape(str As String()) As String()
        Return str _
            .SafeQuery _
            .Select(Function(si)
                        Return JsonParser.StripString(si, decodeMetaChar:=True)
                    End Function) _
            .ToArray
    End Function

    ''' <summary>
    ''' ### Decodes a JSON string
    ''' 
    ''' a short cut method of ``parseJSON`` 
    ''' </summary>
    ''' <param name="str">The json string being decoded.</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' Takes a JSON encoded string and converts it into a R variable.
    ''' </remarks>
    <ExportAPI("json_decode")>
    Public Function json_decode(str As String,
                                Optional throwEx As Boolean = True,
                                Optional [typeof] As Object = Nothing,
                                Optional env As Environment = Nothing) As Object

        Dim schema As Type = env.globalEnvironment.GetType([typeof])

        If env.globalEnvironment.debugMode Then
            If Schema Is Nothing Then
                Return fromJSON(str, raw:=False, env:=env)
            Else
                Return str.LoadObject(Schema)
            End If
        Else
            Try
                If Schema Is Nothing Then
                    Return fromJSON(str, raw:=False, env:=env)
                Else
                    Return str.LoadObject(Schema)
                End If
            Catch ex As Exception When throwEx
                Return Internal.debug.stop(ex, env)
            Catch ex As Exception
                Call env.AddMessage(ex.ToString, MSG_TYPES.WRN)
                Return Nothing
            End Try
        End If
    End Function

    ''' <summary>
    ''' Returns the JSON representation of a value
    ''' </summary>
    ''' <param name="x">The value being encoded. Can be any type except a resource.</param>
    ''' <param name="maskReadonly"></param>
    ''' <param name="indent"></param>
    ''' <param name="enumToStr"></param>
    ''' <param name="unixTimestamp"></param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' Returns a string containing the JSON representation of the supplied value.
    ''' </returns>
    <ExportAPI("json_encode")>
    <RApiReturn(GetType(String))>
    Public Function json_encode(<RRawVectorArgument> x As Object,
                                Optional maskReadonly As Boolean = False,
                                Optional indent As Boolean = False,
                                Optional enumToStr As Boolean = True,
                                Optional unixTimestamp As Boolean = True,
                                Optional env As Environment = Nothing) As Object

        Return jsonlite.toJSON(x, env, maskReadonly, indent, enumToStr, unixTimestamp)
    End Function

    ''' <summary>
    ''' parse JSON string into the raw JSON model or R data object
    ''' </summary>
    ''' <param name="str">The json string being decoded.</param>
    ''' <param name="raw"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("parseJSON")>
    Public Function fromJSON(str As Object,
                             Optional raw As Boolean = False,
                             Optional env As Environment = Nothing) As Object

        If TypeOf str Is String Then
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
        ElseIf TypeOf str Is JsonElement Then
            If raw Then
                Return str
            Else
                Return DirectCast(str, JsonElement).createRObj(env)
            End If
        Else
            Return Message.InCompatibleType(GetType(String), str.GetType, env)
        End If
    End Function

    ''' <summary>
    ''' parse the binary JSON data into the raw JSON model or R data object
    ''' </summary>
    ''' <param name="buffer">the binary data package in BSON format</param>
    ''' <param name="raw"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("parseBSON")>
    Public Function parseBSON(<RRawVectorArgument> buffer As Object,
                              Optional raw As Boolean = False,
                              Optional env As Environment = Nothing) As Object

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
            Return json.createRObj(env, decodeMetachar:=False)
        End If
    End Function

    ''' <summary>
    ''' Convert the raw json model into R data object
    ''' </summary>
    ''' <param name="json"></param>
    ''' <param name="schema"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("object")>
    Public Function buildObject(json As JsonElement, schema As Object,
                                Optional decodeMetachar As Boolean = True,
                                Optional env As Environment = Nothing) As Object

        Dim type As RType = env.globalEnvironment.GetType([typeof]:=schema)
        Dim obj As Object = json.CreateObject(type, decodeMetachar:=decodeMetachar)

        Return obj
    End Function

    ''' <summary>
    ''' save any R object into BSON stream data
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <param name="file">
    ''' the file resource that used for save the BSON data, if this parameter is empty, then
    ''' a binary data stream that contains the BSON data will be returned.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("writeBSON")>
    Public Function writeBSON(obj As Object,
                              Optional file As Object = Nothing,
                              Optional maskReadonly As Boolean = False,
                              Optional enumToStr As Boolean = True,
                              Optional unixTimestamp As Boolean = True,
                              Optional env As Environment = Nothing) As Object

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

        Dim opts As New JSONSerializerOptions With {
            .indent = False,
            .maskReadonly = maskReadonly,
            .enumToString = enumToStr,
            .unixTimestamp = unixTimestamp
        }

        Dim err As Message = Nothing
        Dim json As JsonElement

        If obj Is Nothing Then
            Return Internal.debug.stop("the given object data can not be nothing!", env)
        Else
            json = opts.GetJsonLiteralRaw(obj, err, env)
        End If

        If Not err Is Nothing Then
            Return err
        ElseIf Not TypeOf json Is JsonObject Then
            Return Internal.debug.stop($"the given json data model must be an object! ({json.GetType.Name} was given...)", env)
        End If

        Call BSON.WriteBuffer(DirectCast(json, JsonObject), stream)

        If file Is Nothing Then
            Return DirectCast(stream, MemoryStream).ToArray
        Else
            Call stream.Flush()
            Call stream.Close()
            Call stream.Dispose()

            Return Nothing
        End If
    End Function
End Module
