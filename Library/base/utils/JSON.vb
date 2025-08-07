#Region "Microsoft.VisualBasic::d18cfe1d8933059d571ac6909bbfcf0b, Library\base\utils\JSON.vb"

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

    '   Total Lines: 443
    '    Code Lines: 288 (65.01%)
    ' Comment Lines: 99 (22.35%)
    '    - Xml Docs: 94.95%
    ' 
    '   Blank Lines: 56 (12.64%)
    '     File Size: 17.62 KB


    ' Module JSON
    ' 
    '     Function: buildObject, fromJSON, json_decode, json_encode, loadClrObjectFromJson
    '               parseBSON, (+2 Overloads) read_jsonl, unescape, writeBSON
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports renv = SMRUCC.Rsharp.Runtime
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

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

    Private Function loadClrObjectFromJson(json_str As String, clr As Type, env As Environment) As Object
        Dim ex As Exception = Nothing

        json_str = Strings.Trim(json_str).Trim(ASCII.CR, ASCII.LF, ASCII.TAB, " "c)

        If json_str.StartsWith("[") Then
            ' is an array
            If Not clr.IsArray Then
                clr = clr.MakeArrayType
            End If
        End If

        Static primitiveTypes As Type() = DataFramework _
            .GetPrimitiveTypes _
            .JoinIterates(DataFramework.GetPrimitiveTypes _
            .Select(Function(scalar)
                        Return scalar.MakeArrayType
                    End Function)) _
            .ToArray

        Return json_str.LoadObject(clr, throwEx:=False, exception:=ex, knownTypes:=primitiveTypes)
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
                                Optional strict_vector_syntax As Boolean = True,
                                Optional env As Environment = Nothing) As Object

        Dim schema As Type = env.globalEnvironment.GetType([typeof])

        If env.globalEnvironment.debugMode Then
            If schema Is Nothing Then
                Return fromJSON(str,
                                raw:=False,
                                strict_vector_syntax:=strict_vector_syntax,
                                env:=env)
            Else
                Return loadClrObjectFromJson(str, schema, env)
            End If
        Else
            Try
                If schema Is Nothing Then
                    Return fromJSON(str,
                                    raw:=False,
                                    strict_vector_syntax:=strict_vector_syntax,
                                    env:=env)
                Else
                    Return loadClrObjectFromJson(str, schema, env)
                End If
            Catch ex As Exception When throwEx
                Return RInternal.debug.stop(ex, env)
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
                                Optional unicode_escape As Boolean = True,
                                <RListObjectArgument>
                                Optional args As list = Nothing,
                                Optional env As Environment = Nothing) As Object

        Return jsonlite.toJSON(x, env,
                               maskReadonly, indent, enumToStr, unixTimestamp,
                               unicode_escape:=unicode_escape,
                               args:=args)
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
                             Optional strict_vector_syntax As Boolean = True,
                             Optional env As Environment = Nothing) As Object

        If TypeOf str Is String Then
            Return CStr(str).ParseJSONinternal(raw, strict_vector_syntax, env)
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
    ''' Convert the raw json model into R data object
    ''' </summary>
    ''' <param name="json"></param>
    ''' <param name="schema"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("object")>
    <Extension>
    Public Function buildObject(json As JsonElement, schema As Object,
                                Optional decodeMetachar As Boolean = True,
                                Optional env As Environment = Nothing) As Object

        Dim type As RType = env.globalEnvironment.GetType([typeof]:=schema)
        Dim obj As Object = json.CreateObject(type, decodeMetachar:=decodeMetachar)

        Return obj
    End Function

    ''' <summary>
    ''' read json list 
    ''' </summary>
    ''' <param name="file">the file path to the json list, usually be the ``*.jsonl`` file.</param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' a tuple list that contains the json objects that parsed from the json list file.
    ''' </returns>
    <ExportAPI("read.jsonl")>
    Public Function read_jsonl(<RRawVectorArgument> file As Object,
                               Optional lazy As Boolean = False,
                               Optional env As Environment = Nothing) As Object

        Dim is_file As Boolean = False
        Dim s = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env, is_filepath:=is_file)

        If s Like GetType(Message) Then
            Return s.TryCast(Of Message)
        End If

        Dim read As New StreamReader(s.TryCast(Of Stream))

        If lazy Then
            Dim line As Value(Of String) = ""
            Dim list As IEnumerable(Of Object) =
                Iterator Function() As IEnumerable(Of Object)
                    Do While (line = read.ReadLine) IsNot Nothing
                        Yield CStr(line).ParseJSONinternal(
                            raw:=False,
                            strict_vector_syntax:=False,
                            env:=env)
                    Loop
                End Function()

            Return pipeline.CreateFromPopulator(
                list, finalize:=Sub()
                                    If is_file Then
                                        Try
                                            Call read.Dispose()
                                            Call s.TryCast(Of Stream).Close()
                                        Catch ex As Exception

                                        End Try
                                    End If
                                End Sub)
        Else
            Dim line As Value(Of String) = ""
            Dim list As New List(Of Object)

            Do While (line = read.ReadLine) IsNot Nothing
                Call list.Add(CStr(line).ParseJSONinternal(
                     raw:=False,
                     strict_vector_syntax:=False,
                     env:=env))
            Loop

            If is_file Then
                Try
                    Call read.Dispose()
                    Call s.TryCast(Of Stream).Close()
                Catch ex As Exception

                End Try
            End If

            Return list.ToArray
        End If
    End Function

    ''' <summary>
    ''' read json list as dataframe
    ''' </summary>
    ''' <param name="file">the file path to the json list, usually be the ``*.jsonl`` file.</param>
    ''' <param name="env"></param>
    ''' <returns>a dataframe object that loaded data from the json list file</returns>
    ''' <remarks>
    ''' the given json list file its json structure format should be simple, and contains no nested 
    ''' structure for each field. each field in one json object from the list will be treated as 
    ''' the fields in the generated dataframe table.
    ''' </remarks>
    <ExportAPI("read.jsonl_table")>
    Public Function read_jsonl(<RRawVectorArgument> file As Object, cols As String(),
                               Optional row_names As String = Nothing,
                               Optional env As Environment = Nothing) As Object

        Dim s = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env)

        If s Like GetType(Message) Then
            Return s.TryCast(Of Message)
        End If

        Dim read As New StreamReader(s.TryCast(Of Stream))
        Dim line As Value(Of String) = ""
        Dim fields As New Dictionary(Of String, List(Of Object))
        Dim labels As New List(Of String)
        Dim has_rowNames As Boolean = Not row_names.StringEmpty(, True)

        For Each name As String In cols
            Call fields.Add(name, New List(Of Object))
        Next

        Do While (line = read.ReadLine) IsNot Nothing
            Dim x As Object = CStr(line).ParseJSONinternal(raw:=False, strict_vector_syntax:=False, env)

            If TypeOf x Is Message Then
                Return x
            End If

            If Not TypeOf x Is list Then
                Return RInternal.debug.stop("read dataframe from jsonl only enable when each line is a tuple list object!", env)
            End If

            Dim row As list = DirectCast(x, list)

            For Each name As String In cols
                Call fields(name).Add(row.getByName(name))
            Next

            If has_rowNames Then
                Call labels.Add(row.getValue(Of String)(row_names, env, ""))
            End If
        Loop

        Dim df As New rdataframe With {
            .rownames = If(has_rowNames, labels.ToArray, Nothing),
            .columns = New Dictionary(Of String, Array)
        }

        For Each col As String In cols
            Call df.add(col, renv.TryCastGenericArray(fields(col).ToArray, env))
        Next

        Return df
    End Function
End Module
