﻿#Region "Microsoft.VisualBasic::455088ca3ac7d051f8aee93d2711eaf9, R#\Runtime\Internal\internalInvokes\string\stringr.vb"

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

    '   Total Lines: 1522
    '    Code Lines: 944 (62.02%)
    ' Comment Lines: 419 (27.53%)
    '    - Xml Docs: 89.02%
    ' 
    '   Blank Lines: 159 (10.45%)
    '     File Size: 64.28 KB


    '     Module stringr
    ' 
    '         Function: [objToString], base64Decode, base64Str, bencode, charAt
    '                   chr, concatenate, Csprintf, decodeObject, findToStringWithFormat
    '                   fromBstring, getElementFormat, grep, html, json
    '                   loadJson, loadXml, match, nchar, paste
    '                   paste0, randomAsciiStr, rawBufferBase64, regexp, splitSingleStrAuto
    '                   sprintfSingle, str_empty, str_pad, (+2 Overloads) str_replace, str_squish
    '                   str_trim, strPad_internal, strsplit, substr, tagvalue
    '                   text_equals, text_grep, tolower, toupper, urldecode
    '                   utf8_decode, xml
    ' 
    '     Class TextGrepLambda
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetTokens, TextGrep
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Data.Trinity
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.Bencoding
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports any = Microsoft.VisualBasic.Scripting
Imports ASCII = Microsoft.VisualBasic.Text.ASCII
Imports encoder = SMRUCC.Rsharp.Development.Components.Encoder
Imports REnv = SMRUCC.Rsharp.Runtime
Imports std = System.Math
Imports VBStr = Microsoft.VisualBasic.Strings
Imports vector = SMRUCC.Rsharp.Runtime.Internal.Object.vector

#If NET48 Then
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
#Else
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
#End If

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' the R text data helper
    ''' </summary>
    <Package("stringr")>
    Public Module stringr

        ''' <summary>
        ''' comvert any object to html text document
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI(htmlPrinter.toHtml_apiName)>
        Public Function html(<RRawVectorArgument> x As Object,
                             <RListObjectArgument>
                             Optional args As list = Nothing,
                             Optional env As Environment = Nothing) As Object

            If x Is Nothing Then
                Return Nothing
            Else
                Return htmlPrinter.GetHtml(x, args, env)
            End If
        End Function

        ''' <summary>
        ''' # Convert an R Object to a Character String
        ''' 
        ''' This is a helper function for format to produce a 
        ''' single character string describing an R object.
        ''' </summary>
        ''' <param name="x">The object to be converted.</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this function just invoke the <see cref="ToString"/> method.
        ''' </remarks>
        <ExportAPI("toString")>
        <RApiReturn(GetType(String))>
        Public Function [objToString](<RRawVectorArgument> x As Object,
                                      Optional format$ = Nothing,
                                      Optional culture As CultureInfo = Nothing,
                                      <RListObjectArgument>
                                      Optional args As list = Nothing,
                                      Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return ""
            ElseIf TypeOf x Is WebResponseResult Then
                Return DirectCast(x, WebResponseResult).html
            End If

            If Internal.generic.exists("toString") Then
                Dim func = Internal.generic.getGenericCallable(x, x.GetType, "toString", env, suppress:=True)

                If Not func Like GetType(Message) Then
                    Return func.TryCast(Of GenericFunction)()(x, args, env)
                End If
            End If

            ' NULL will be translate to empty string in R
            '
            ' > toString(NULL)
            ' [1] ""

            Dim seqData As Array = REnv.TryCastGenericArray(REnv.asVector(Of Object)(x), env)
            Dim toString As Func(Of Object, String)

            If format.StringEmpty Then
                toString = Function(xi) If(xi Is Nothing, "", xi.ToString)
            ElseIf seqData.GetType.GetElementType Is Nothing Then
                toString = Function(xi) If(xi Is Nothing, "", xi.ToString)
            Else
                toString = seqData _
                    .GetType _
                    .GetElementType _
                    .getElementFormat(
                        format:=format,
                        culture:=culture,
                        env:=env
                     )
            End If

            Return seqData _
                .AsObjectEnumerator _
                .Select(toString) _
                .ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="type">
        ''' the element type
        ''' </param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Private Function getElementFormat(type As Type, format As String, culture As CultureInfo, env As Environment) As Func(Of Object, String)
            Dim toStringF As MethodInfo = type _
                .GetMethods(PublicProperty) _
                .Where(Function(f)
                           Return findToStringWithFormat(f)
                       End Function) _
                .FirstOrDefault

            If toStringF Is Nothing Then
                env.AddMessage($"a format text '{format}' is given, but typeof '{type.Name}' is not accept such format parameter...")
                Return Function(xi) If(xi Is Nothing, "", xi.ToString)
            Else
                Return Function(xi)
                           If xi Is Nothing Then
                               Return ""
                           ElseIf TypeOf xi Is Date Then
                               If culture Is Nothing Then
                                   Return DirectCast(xi, Date).ToString(format)
                               Else
                                   Return DirectCast(xi, Date).ToString(format, culture)
                               End If
                           Else
                               ' Call 1.0.ToString(format:="")
                               ' Call #2022-04-25#.ToString(format:="d MMM yyyy")

                               Return DirectCast(toStringF.Invoke(xi, {format}), String)
                           End If
                       End Function
            End If
        End Function

        <Extension>
        Private Function findToStringWithFormat(f As MethodInfo) As Boolean
            ' find a ToString method with a format parameter 
            Dim args As ParameterInfo() = f.GetParameters
            Dim isToStringName As Boolean = f.Name = "ToString"

            If Not isToStringName Then
                Return False
            ElseIf args.Length <> 1 Then
                Return False
            Else
                Return args(Scan0).ParameterType Is GetType(String)
            End If
        End Function

        <ExportAPI("loadJSON")>
        Public Function loadJson(file As Object, what As Object, Optional env As Environment = Nothing) As Object
            Dim type As RType = env.globalEnvironment.GetType(what)
            Dim info As Type

            If type Is Nothing OrElse type Is RType.any Then
                Return Internal.debug.stop("unknow data type!", env)
            Else
                info = type.raw
            End If

            Dim text As String

            If TypeOf file Is String Then
                text = CStr(file).SolveStream
            Else
                Dim s = GetFileStream(file, FileAccess.Read, env)

                If s Like GetType(Message) Then
                    Return s.TryCast(Of Message)
                End If

                text = New StreamReader(s.TryCast(Of Stream)).ReadToEnd
            End If

            text = VBStr.Trim(text)

            If text.First = "["c Then
                ' is array type
                If Not info.IsArray Then
                    info = info.MakeArrayType
                End If
            End If

            Return text.LoadObject(info)
        End Function

        ''' <summary>
        ''' load a .NET object from the xml data file
        ''' </summary>
        ''' <param name="file">
        ''' any kind of data value inputs:
        ''' 
        ''' 1. file path to the xml document file
        ''' 2. the xml document content text
        ''' 3. a stream object that contains the xml document text data
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this function will try to parse the object model information
        ''' from the meta data in the xml data file
        ''' </remarks>
        <ExportAPI("loadXml")>
        Public Function loadXml(file As Object,
                                Optional [typeof] As Object = Nothing,
                                Optional env As Environment = Nothing) As Object

            Dim text As String = Nothing

            If file Is Nothing Then
                Return Nothing
            End If

            If TypeOf file Is String Then
                text = file
            ElseIf TypeOf file Is Stream Then
                Using buf As Stream = DirectCast(file, Stream)
                    Dim reader As New StreamReader(buf)
                    text = reader.ReadToEnd
                End Using
            Else
                Return Message.InCompatibleType(GetType(String), file.GetType, env)
            End If

            Dim type As MetaData.TypeInfo = DigitalSignature.GetModelInfo(text)
            Dim model As Type
            Dim winOpt As Boolean

#If WINDOWS Then
            winOpt = True
#Else
            winOpt = False
#End If

            If type Is Nothing Then
                If Not [typeof] Is Nothing Then
                    model = env.globalEnvironment.GetType([typeof])
                Else
                    Return Internal.debug.stop(New NotImplementedException(), env)
                End If
            Else
                model = RType.GetType(type, env.globalEnvironment)
            End If

            If text.isFilePath(includeWindowsFs:=winOpt) Then
                Return text.LoadXml(model)
            Else
                Return text.LoadFromXml(model)
            End If
        End Function

        ''' <summary>
        ''' Convert most of the R# object or VB.NET object to xml document string. 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("xml")>
        <RApiReturn(GetType(String))>
        Public Function xml(<RRawVectorArgument> x As Object, env As Environment) As Object
            If x Is Nothing Then
                Return "<?xml version=""1.0"" encoding=""utf-16""?>"
            ElseIf x.GetType.IsArray Then
                Dim template As Type = GetType(XmlList(Of ))
                Dim type As Type = template.MakeGenericType(x.GetType.GetElementType)
                Dim list As Object = Activator.CreateInstance(type)
                Dim writer As PropertyInfo = type _
                    .GetProperties(BindingFlags.Public Or BindingFlags.Instance) _
                    .Where(Function(p) p.Name = NameOf(XmlList(Of String).items)) _
                    .FirstOrDefault

                Call writer.SetValue(list, x)
            End If

            Try
                Return XmlExtensions.GetXml(x, x.GetType)
            Catch ex As Exception
                Return Internal.debug.stop(ex, env)
            End Try
        End Function

        <ExportAPI("bdecode")>
        Public Function fromBstring(bstr As String) As Object
            Dim result = BencodeDecoder.Decode(bstr) _
                .Select(Function(node) node.decodeObject()) _
                .ToArray

            If result.Length = 1 Then
                Return result(Scan0)
            Else
                Return result
            End If
        End Function

        <Extension>
        Private Function decodeObject(element As BElement) As Object
            Select Case element.GetType
                Case GetType(BString)
                    Return DirectCast(element, BString).Value
                Case GetType(BInteger)
                    Return DirectCast(element, BInteger).Value
                Case GetType(BList)
                    Dim array As New List(Of Object)

                    For Each item As BElement In DirectCast(element, BList)
                        array.Add(item.decodeObject)
                    Next

                    Return array.ToArray
                Case Else
                    Dim table As BDictionary = DirectCast(element, BDictionary)
                    Dim list As New list With {.slots = New Dictionary(Of String, Object)}

                    For Each item In table
                        list.slots.Add(item.Key.Value, item.Value.decodeObject)
                    Next

                    Return list
            End Select
        End Function

        <ExportAPI("bencode")>
        <RApiReturn(GetType(String))>
        Public Function bencode(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
            Return ToBEncode(
                obj:=x,
                digest:=Function(any) encoder.DigestRSharpObject(any, env)
            ).ToBencodedString
        End Function

        ''' <summary>
        ''' Convert most of the R# object or VB.NET object to json string. 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="compress"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("json")>
        <RApiReturn(GetType(String))>
        Public Function json(<RRawVectorArgument>
                             x As Object,
                             Optional compress As Boolean = True,
                             <RListObjectArgument>
                             Optional args As list = Nothing,
                             Optional env As Environment = Nothing) As Object

            Dim is_general As Boolean = False

            If x Is Nothing Then
                Return "null"
            Else
                x = encoder.CreateEncoderWithOptions(args, env).GetObject(x, is_general)
            End If

            Dim type As Type = x.GetType

            Static genericTypes As Type() = {
                GetType(Integer),
                GetType(Boolean),
                GetType(String),
                GetType(Dictionary(Of String, Object)),
                GetType(Integer()),
                GetType(Boolean()),
                GetType(String()),
                GetType(Double),
                GetType(Double()),
                GetType(Single),
                GetType(Single()),
                GetType(Date),
                GetType(Date()),
                GetType(Long),
                GetType(Long()),
                GetType(Byte),
                GetType(Byte()),
                GetType(Short),
                GetType(Short()),
                GetType(Color),
                GetType(Color())
            }

            Try
                Dim known As Type() = genericTypes

                If Not is_general Then
                    known = Nothing
                End If

                Return JsonContract.GetObjectJson(type, x, indent:=Not compress, knownTypes:=known)
            Catch ex As Exception
                Return debug.stop(ex, env)
            End Try
        End Function

        ''' <summary>
        ''' encode byte stream or text content into base64 string
        ''' </summary>
        ''' <param name="raw">
        ''' R# object or any supported .NET object:
        ''' 
        ''' 1. text content
        ''' 2. bytes buffer
        ''' 3. image data
        ''' 4. any object can be converts to bytes
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("base64")>
        <RApiReturn(GetType(String))>
        Public Function base64Str(<RRawVectorArgument> raw As Object,
                                  Optional chunkSize As Integer = -1,
                                  Optional env As Environment = Nothing) As Object

            Dim base64 As String

            If TypeOf raw Is Image OrElse TypeOf raw Is Bitmap Then
                base64 = CType(raw, Image).ToBase64String()
            ElseIf TypeOf raw Is String Then
                base64 = Encoding.UTF8.GetBytes(DirectCast(raw, String)).ToBase64String
            Else
                base64 = rawBufferBase64(raw, env)
            End If

            If chunkSize > 0 Then
                base64 = base64 _
                    .Chunks(lineBreak:=chunkSize) _
                    .JoinBy(vbLf)
            End If

            Return base64
        End Function

        Private Function rawBufferBase64(raw As Object, env As Environment) As Object
            Dim buffer As [Variant](Of Byte(), Message) = Rsharp.Buffer(raw, env)

            If buffer Is Nothing Then
                Return Nothing
            ElseIf buffer Like GetType(Message) Then
                Dim strings As pipeline = pipeline.TryCreatePipeline(Of String)(raw, env)

                If strings.isError Then
                    Return buffer.TryCast(Of Message)
                Else
                    Return strings _
                        .populates(Of String)(env) _
                        .GetJson _
                        .DoCall(AddressOf Encoding.UTF8.GetBytes) _
                        .ToBase64String
                End If
            End If

            Dim bytes As Byte() = buffer.TryCast(Of Byte())
            Dim base64Str As String = bytes.ToBase64String

            Return base64Str
        End Function

        ''' <summary>
        ''' decode base64 string as text or the raw bytes buffer object.
        ''' </summary>
        ''' <param name="base64">a string in base64 encode pattern</param>
        ''' <param name="asText_encoding">
        ''' if this parameter is not nothing, then the output will be convert as text
        ''' </param>
        ''' <param name="wrap">
        ''' wrap the result as memory stream object instead of the raw bytes vector output
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' this function returns a text data or memory stream buffer object.
        ''' </returns>
        ''' <remarks>
        ''' if the base64 encoded data is text data, that parameter 
        ''' ``asText_encoding`` assign of value ``utf8`` usually. 
        ''' </remarks>
        <ExportAPI("base64_decode")>
        <RApiReturn(GetType(String), GetType(Byte), GetType(MemoryStream))>
        Public Function base64Decode(base64 As String,
                                     Optional asText_encoding As Object = Nothing,
                                     Optional wrap As Boolean = True,
                                     Optional env As Environment = Nothing) As Object

            If Not asText_encoding Is Nothing Then
                ' decode as string
                Dim encoding As Encoding = GetEncoding(asText_encoding)
                Dim raw As Byte() = base64.Base64RawBytes

                Return encoding.GetString(raw)
            ElseIf wrap Then
                Return New MemoryStream(base64.Base64RawBytes)
            Else
                Return base64.Base64RawBytes
            End If
        End Function

        ''' <summary>
        ''' Decodes URL-encoded string
        ''' 
        ''' Decodes any ``%##`` encoding in the given string. 
        ''' Plus symbols ('+') are decoded to a space character.
        ''' </summary>
        ''' <param name="str">The string to be decoded.</param>
        ''' <returns>Returns the decoded string.</returns>
        <ExportAPI("urldecode")>
        Public Function urldecode(str As String()) As Object
            Return str.SafeQuery.Select(Function(s) s.UrlDecode).ToArray
        End Function

        <ExportAPI("grep")>
        Public Function grep(<RRawVectorArgument>
                             text As Object,
                             greps As String(),
                             Optional fixed As Boolean = False,
                             Optional env As Environment = Nothing) As Object

            If text Is Nothing Then
                Return Nothing
            ElseIf fixed Then
                Return ObjectSet _
                    .GetObjectSet(text, env) _
                    .Select(Function(o)
                                Return greps.Any(Function(part) VBStr.InStr(o, part) > 0)
                            End Function) _
                    .ToArray
            End If

            Dim textgrep As TextGrepMethod = TextGrepScriptEngine _
                .CompileFromTokens(greps) _
                .PipelinePointer

            If TypeOf text Is pipeline Then
                Return DirectCast(text, pipeline) _
                    .populates(Of String)(env) _
                    .Select(Function(str)
                                Return textgrep(str)
                            End Function) _
                    .DoCall(AddressOf pipeline.CreateFromPopulator)
            Else
                Return ObjectSet.GetObjectSet(text, env) _
                    .Select(Function(o)
                                Return textgrep(any.ToString(o))
                            End Function) _
                    .ToArray
            End If
        End Function

        ''' <summary>
        ''' ### Count the Number of Characters (or Bytes or Width)
        ''' 
        ''' nchar takes a character vector as an argument and 
        ''' returns a vector whose elements contain the sizes 
        ''' of the corresponding elements of x.
        ''' </summary>
        ''' <param name="strs">
        ''' character vector, or a vector to be coerced to a character 
        ''' vector. Giving a factor is an error.</param>
        ''' <returns></returns>
        <ExportAPI("nchar")>
        <RApiReturn(GetType(Integer))>
        Public Function nchar(<RRawVectorArgument> strs As Object) As Object
            If strs Is Nothing Then
                Return 0
            End If

            If strs.GetType Is GetType(String) Then
                Return DirectCast(strs, String).Length
            Else
                Return CLRVector.asCharacter(strs) _
                    .Select(AddressOf VBStr.Len) _
                    .DoCall(AddressOf vector.asVector)
            End If
        End Function

        ''' <summary>
        ''' Initializes a new instance of the ``RegularExpression`` class
        ''' for the specified regular expression <paramref name="pattern"/>.
        ''' </summary>
        ''' <param name="pattern">the specified regular expression</param>
        ''' <returns></returns>
        <ExportAPI("regexp")>
        <RApiReturn(GetType(Regex))>
        Public Function regexp(pattern$, Optional options$ = Nothing, Optional env As Environment = Nothing) As Object
            Dim opts As RegexOptions = RegexOptions.None

            If pattern.StringEmpty Then
                Return Internal.debug.stop("the input regular expression could not be empty!", env)
            End If

            If Not options Is Nothing Then
                If options.IndexOf("i") Then
                    opts = opts Or RegexOptions.IgnoreCase
                End If
                If options.IndexOf("m") Then
                    opts = opts Or RegexOptions.Multiline
                End If
                If options.IndexOf("s") Then
                    opts = opts Or RegexOptions.Singleline
                End If
                If options.IndexOf("r") Then
                    ' reverse
                    opts = opts Or RegexOptions.RightToLeft
                End If
            End If

            Return New Regex(pattern, opts)
        End Function

        ''' <summary>
        ''' Searches the specified input string for the first 
        ''' occurrence of the regular expression specified in 
        ''' the System.Text.RegularExpressions.Regex 
        ''' constructor.
        ''' </summary>
        ''' <param name="regexp">Represents an immutable regular expression.
        ''' To browse the .NET Framework source code for this type, 
        ''' see the Reference Source.</param>
        ''' <param name="strings">The string to search for a match.</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("regex_match")>
        Public Function match(regexp As Regex,
                              <RRawVectorArgument>
                              strings As Object,
                              Optional env As Environment = Nothing) As Object

            If regexp Is Nothing Then
                Return Internal.debug.stop("regular expression object can not be null!", env)
            Else
                Return env.EvaluateFramework(Of String, String)(
                    x:=strings,
                    eval:=Function(str)
                              Return regexp.Match(str).Value
                          End Function)
            End If
        End Function

        ''' <summary>
        ''' #### Use C-style String Formatting Commands
        ''' 
        ''' A wrapper for the C function sprintf, that returns a character 
        ''' vector containing a formatted combination of text and variable 
        ''' values.
        ''' </summary>
        ''' <param name="format"></param>
        ''' <param name="arguments"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("sprintf")>
        <RApiReturn(GetType(String))>
        Public Function Csprintf(format As Array,
                                 <RListObjectArgument>
                                 Optional arguments As Object = Nothing,
                                 Optional env As Environment = Nothing) As Object

            Dim sprintf As Func(Of String, Object(), String) = AddressOf CLangStringFormatProvider.sprintf
            Dim listdata As Object = base.Rlist(arguments, env)

            If Program.isException(listdata) Then
                Return listdata
            End If

            Dim listValues As Object() = DirectCast(listdata, list).slots.Values.ToArray
            Dim args As Array()

            If listValues.Length = 0 Then
                args = {}
            Else
                args = listValues _
                    .Select(Function(a)
                                Return REnv.asVector(Of Object)(a)
                            End Function) _
                    .ToArray
            End If

            Dim inputTemplates As String() = format _
                .AsObjectEnumerator _
                .Select(AddressOf Scripting.ToString) _
                .ToArray

            If inputTemplates.Length = 1 Then
                Return sprintfSingle(inputTemplates(Scan0), args)
            Else
                Return New list With {
                    .slots = inputTemplates _
                        .ToDictionary(Function(key) key,
                                      Function(strformat)
                                          Return CObj(sprintfSingle(strformat, args))
                                      End Function)
                }
            End If
        End Function

        Private Function sprintfSingle(strformat$, args As Array()) As String()
            If args.Length = 0 Then
                Return {sprintf(strformat)}
            Else
                Dim result As String() = args(Scan0) _
                    .AsObjectEnumerator _
                    .Select(Function(o, i)
                                Dim aList As New List(Of Object) From {o}

                                For j As Integer = 1 To args.Length - 1
                                    aList.Add(args(j).GetValue(i))
                                Next

                                Return sprintf(strformat, aList.ToArray)
                            End Function) _
                    .ToArray

                Return result
            End If
        End Function

        ''' <summary>
        ''' ### Split the Elements of a Character Vector
        ''' 
        ''' Split the elements of a character vector x into substrings
        ''' according to the matches to substring split within them.
        ''' </summary>
        ''' <param name="text">
        ''' character vector, each element of which is to be split. Other inputs, 
        ''' including a factor, will give an error.
        ''' </param>
        ''' <param name="delimiter">
        ''' character vector (or object which can be coerced to such) containing 
        ''' regular expression(s) (unless fixed = TRUE) to use for splitting. 
        ''' If empty matches occur, in particular if split has length 0, x is 
        ''' split into single characters. If split has length greater than 1, 
        ''' it is re-cycled along x.
        ''' </param>
        ''' <param name="fixed">
        ''' logical. If TRUE match split exactly, otherwise use regular expressions. 
        ''' Has priority over perl.</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("strsplit")>
        <RApiReturn(GetType(String))>
        Friend Function strsplit(text$(),
                                 Optional delimiter As Object = " ",
                                 Optional fixed As Boolean = False,
                                 Optional drop1 As Boolean = True,
                                 Optional env As Environment = Nothing) As Object

            If delimiter Is Nothing Then
                Return debug.stop("the given delimiter for make string split should not be nothing!", env)
            End If

            If Not TypeOf delimiter Is Regex Then
                Dim deli_str = CLRVector.asCharacter(delimiter)
                Dim decode_str = deli_str.Select(Function(s) CString.Decode(s)).ToArray

                If decode_str.Length = 1 Then
                    delimiter = decode_str(Scan0)
                Else
                    delimiter = decode_str
                End If
            End If

            If text.IsNullOrEmpty Then
                Return Nothing
            ElseIf text.Length = 1 Then
                text = text(Scan0).splitSingleStrAuto(delimiter, fixed)

                If drop1 Then
                    Return text
                Else
                    Return New list(slot("[[1]]") = text)
                End If
            Else
                Return text _
                    .SeqIterator _
                    .ToDictionary(Function(i) $"[[{i.i + 1}]]",
                                  Function(i)
                                      Return i.value.splitSingleStrAuto(delimiter, fixed)
                                  End Function)
            End If
        End Function

        <Extension>
        Private Function splitSingleStrAuto(str As String, delimiter As Object, fixed As Boolean) As String()
            If fixed Then
                Return VBStr.Split(str, any.ToString(delimiter))
            ElseIf TypeOf delimiter Is Regex Then
                Return str.StringSplit(DirectCast(delimiter, Regex))
            Else
                Return str.StringSplit(any.ToString(delimiter))
            End If
        End Function

        ''' <summary>
        ''' ## Remove whitespace
        ''' 
        ''' str_trim() removes whitespace from start and end of string; str_squish() removes whitespace at 
        ''' the start and end, and replaces all internal whitespace with a single space.
        ''' </summary>
        ''' <param name="string">Input vector. Either a character vector, Or something coercible To one.</param>
        ''' <param name="side">Side on which to remove whitespace: "left", "right", or "both", the default.</param>
        ''' <returns>A character vector the same length as string.</returns>
        <ExportAPI("str_trim")>
        Public Function str_trim(<RRawVectorArgument> [string] As Object,
                                 <RRawVectorArgument(TypeCodes.string)>
                                 Optional side As Object = "both|left|right") As String()

            Dim sides = CLRVector.asCharacter(side)
            Dim strs = CLRVector.asCharacter([string])
            Dim side_flag As String = sides.ElementAtOrDefault(0, "both")

            If strs.IsNullOrEmpty Then
                Return Nothing
            End If

            Select Case LCase(side_flag)
                Case "left"
                    Return strs _
                        .Select(Function(s) s.TrimStart(ASCII.Whitespace)) _
                        .ToArray
                Case "right"
                    Return strs _
                        .Select(Function(s) s.TrimEnd(ASCII.Whitespace)) _
                        .ToArray
                Case Else
                    Return strs _
                        .Select(Function(s) s.Trim(ASCII.Whitespace)) _
                        .ToArray
            End Select
        End Function

        ''' <summary>
        ''' ## Remove whitespace
        ''' 
        ''' str_trim() removes whitespace from start and end of string; str_squish() removes whitespace at 
        ''' the start and end, and replaces all internal whitespace with a single space.
        ''' </summary>
        ''' <param name="string">	
        ''' Input vector. Either a character vector, Or something coercible To one.</param>
        ''' <returns>
        ''' A character vector the same length as string.
        ''' </returns>
        <ExportAPI("str_squish")>
        Public Function str_squish(<RRawVectorArgument> [string] As Object) As String()
            Dim strs = CLRVector.asCharacter([string])

            If strs.IsNullOrEmpty Then
                Return Nothing
            End If

            Return strs _
                .Select(Function(s) s.StringReplace("\s+", " ").Trim) _
                .ToArray
        End Function

        ''' <summary>
        ''' ## Concatenate Strings
        ''' 
        ''' Concatenate vectors after converting to character. Concatenation happens in two basically different ways, 
        ''' determined by collapse being a string or not.
        ''' 
        ''' ``paste0(..., collapse)`` is equivalent to ``paste(..., sep = "", collapse)``, slightly more efficiently.
        ''' </summary>
        ''' <param name="x">one or more R objects, to be converted to character vectors.</param>
        ''' <param name="collapse">an optional character string to separate the results. Not NA_character_. When collapse
        ''' is a string, the result is always a string (character of length 1).</param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' A character vector of the concatenated values. This will be of length zero if all the objects are, unless 
        ''' collapse is non-NULL, in which case it is "" (a single empty string).
        '''
        ''' If any input into an element Of the result Is In UTF-8 (And none are declared With encoding "bytes", see Encoding), 
        ''' that element will be In UTF-8, otherwise In the current encoding In which Case the encoding Of the element 
        ''' Is declared If the current locale Is either Latin-1 Or UTF-8, at least one Of the corresponding inputs (including 
        ''' separators) had a declared encoding And all inputs were either ASCII Or declared.
        '''
        ''' If an input into an element Is declared With encoding "bytes", no translation will be done Of any Of the elements 
        ''' And the resulting element will have encoding "bytes". If collapse Is non-NULL, this applies also To the second, 
        ''' collapsing, phase, but some translation may have been done In pasting Object together In the first phase.
        ''' </returns>
        ''' 
        <ExportAPI("paste0")>
        <RApiReturn(TypeCodes.string)>
        Public Function paste0(<RListObjectArgument> x As list,
                               Optional collapse$ = Nothing,
                               Optional env As Environment = Nothing)

            Return paste(x, sep:="", collapse:=collapse, env:=env)
        End Function

        ''' <summary>
        ''' ### Concatenate Strings
        ''' 
        ''' string join with given delimiter, concatenate vectors after converting to character.
        ''' </summary>
        ''' <param name="x">one Or more R objects, to be converted to character vectors.</param>
        ''' <param name="sep">
        ''' a character String To separate the terms. Not NA_character_.
        ''' </param>
        ''' <param name="collapse">
        ''' an optional character string to separate the results. Not NA_character_.
        ''' </param>
        ''' <returns>A character vector of the concatenated values. This will be of length zero
        ''' if all the objects are, unless collapse is non-NULL, in which case it is "" 
        ''' (a single empty string).
        '''
        '''        If any input into an element Of the result Is In UTF-8 (And none are declared
        '''        With encoding "bytes", see Encoding), that element will be In UTF-8, otherwise 
        '''        In the current encoding In which Case the encoding Of the element Is declared 
        '''        If the current locale Is either Latin-1 Or UTF-8, at least one Of the corresponding
        '''        inputs (including separators) had a declared encoding And all inputs were either 
        '''        ASCII Or declared.
        '''
        ''' If an input into an element Is declared With encoding "bytes", no translation will be done
        ''' Of any Of the elements And the resulting element will have encoding "bytes". If collapse 
        ''' Is non-NULL, this applies also To the second, collapsing, phase, but some translation may
        ''' have been done In pasting Object together In the first phase.</returns>
        ''' <remarks>
        ''' paste converts its arguments (via as.character) to character strings, 
        ''' and concatenates them (separating them by the string given by sep). 
        ''' If the arguments are vectors, they are concatenated term-by-term to 
        ''' give a character vector result. Vector arguments are recycled as needed,
        ''' with zero-length arguments being recycled to "" only if recycle0 is not 
        ''' true or collapse is not NULL.
        '''
        ''' Note that paste() coerces NA_character_, the character missing value, To 
        ''' "NA" which may seem undesirable, e.g., When pasting two character vectors,
        ''' Or very desirable, e.g. In paste("the value of p is ", p).
        '''
        ''' paste0(..., collapse) Is equivalent to paste(..., sep = "", collapse), 
        ''' slightly more efficiently.
        '''
        ''' If a value Is specified For collapse, the values In the result are Then 
        ''' concatenated into a Single String, With the elements being separated by
        ''' the value Of collapse.
        ''' </remarks>
        <ExportAPI("paste")>
        <RApiReturn(TypeCodes.string)>
        Friend Function paste(<RListObjectArgument> x As list,
                              Optional sep$ = " ",
                              Optional collapse$ = Nothing,
                              Optional env As Environment = Nothing) As Object

            Static args As Index(Of String) = New String() {
                NameOf(sep),
                NameOf(collapse),
                NameOf(env)
            }

            Dim chrs As New List(Of String())
            Dim check As Integer = 1

            sep = sprintf(If(sep, ""))

            For Each xi As KeyValuePair(Of String, Object) In x.slots
                If Not xi.Key Like args Then
                    Dim si As String() = CLRVector.asCharacter(xi.Value)

                    If si.IsNullOrEmpty Then
                        Call chrs.Add({""})
                    Else
                        Call chrs.Add(si)
                    End If

                    If chrs.Last.Length <> check Then
                        If check <> 1 AndAlso chrs.Last.Length <> 1 Then
                            Return Internal.debug.stop("the required character size should be matches others!", env)
                        End If
                    End If
                End If
            Next

            If chrs.Count = 0 Then
                Return Nothing
            ElseIf chrs.Count = 1 Then
                Return chrs(0).JoinBy(sep)
            Else
                Dim v As String() = StringInterpolation.UnsafeStringConcatenate(
                    strs:=chrs,
                    sep:=sep
                )

                If Not collapse.StringEmpty Then
                    Return v.JoinBy(collapse)
                Else
                    Return v
                End If
            End If
        End Function

        ''' <summary>
        ''' Pattern Matching and Replacement
        ''' </summary>
        ''' <param name="subj">a character vector</param>
        ''' <param name="search$"></param>
        ''' <param name="replaceAs"></param>
        ''' <param name="regexp">
        ''' the search target is a regex pattern expression?
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("gsub")>
        Friend Function str_replace(subj$(), search$,
                                    Optional replaceAs$ = "",
                                    Optional regexp As Boolean = False,
                                    Optional env As Environment = Nothing) As Object
            If search Is Nothing Then
                Return Internal.debug.stop("the search pattern can not be nothing!", env)
            ElseIf subj.IsNullOrEmpty Then
                Return {}
            End If

            If regexp Then
                Return subj.Select(Function(s) s.StringReplace(search, replaceAs)).ToArray
            Else
                Return subj _
                    .Select(Function(s)
                                If s Is Nothing OrElse s = "" Then
                                    Return ""
                                Else
                                    Return s.Replace(search, replaceAs)
                                End If
                            End Function) _
                    .ToArray
            End If
        End Function

        ''' <summary>
        ''' Pad A String.
        ''' </summary>
        ''' <param name="string">A character vector.</param>
        ''' <param name="width">Minimum width of padded strings.</param>
        ''' <param name="side">Side on which padding character is added (left, right or both).</param>
        ''' <param name="pad">Single padding character (default is a space).</param>
        ''' <returns></returns>
        <ExportAPI("str_pad")>
        Public Function str_pad([string] As String(), width%,
                                Optional side As str_padSides = str_padSides.left,
                                Optional pad As Char = " "c) As String()
            Return [string] _
                .SafeQuery _
                .Select(Function(s)
                            Return strPad_internal(s, width, side, pad)
                        End Function) _
                .ToArray
        End Function

        Private Function strPad_internal(s As String, width As Integer, side As str_padSides, pad As Char) As String
            If s.StringEmpty Then
                Return New String(pad, width)
            End If

            If side = str_padSides.left Then
                Return s.PadLeft(width, pad)
            ElseIf side = str_padSides.right Then
                Return s.PadRight(width, pad)
            Else
                Dim l As Integer = s.Length
                Dim left As Integer = (width - l) / 2
                Dim right As Integer = width - l - left

                If left <= 0 Then
                    Return s
                Else
                    Return New String(pad, left) & s & New String(pad, right)
                End If
            End If
        End Function

        ''' <summary>
        ''' test of the give character vector is empty string or not
        ''' </summary>
        ''' <param name="x">a character vector that contains multiple string for do the test</param>
        ''' <returns>
        ''' a logical vector of the test result. TRUE means the given string
        ''' is asserted as an empty factor.
        ''' </returns>
        <ExportAPI("str_empty")>
        <RApiReturn(TypeCodes.boolean)>
        Public Function str_empty(x As String(),
                                  Optional whitespace_empty As Boolean = True,
                                  Optional test_empty_factor As Boolean = False) As Object

            If x Is Nothing Then
                Return True
            End If

            Return (From si As String
                    In x
                    Select si.StringEmpty(
                        whitespaceAsEmpty:=whitespace_empty,
                        testEmptyFactor:=test_empty_factor)).ToArray
        End Function

        ''' <summary>
        ''' parse the string text in format like ``tag{delimiter}value``
        ''' </summary>
        ''' <param name="string"></param>
        ''' <param name="delimiter"></param>
        ''' <param name="trim_value"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("tagvalue")>
        <RApiReturn(TypeCodes.string)>
        Public Function tagvalue([string] As String(),
                                 Optional delimiter$ = " ",
                                 Optional trim_value As Boolean = True,
                                 Optional as_list As Boolean = False,
                                 Optional union_list As Boolean = False,
                                 Optional env As Environment = Nothing) As Object

            Dim values As NamedValue(Of String)() = [string] _
                .SafeQuery _
                .Select(Function(str)
                            Return str.GetTagValue(delimiter,
                                                   trim:=trim_value,
                                                   failureNoName:=True)
                        End Function) _
                .ToArray

            If as_list Then
                Dim vec As New list With {
                    .slots = New Dictionary(Of String, Object)
                }

                If union_list Then
                    Dim union_vals = values.GroupBy(Function(t) t.Name).ToArray

                    For Each val As IGrouping(Of String, NamedValue(Of String)) In union_vals
                        Call vec.add(val.Key, From vi In val Select vi.Value)
                    Next
                Else
                    Dim names As String() = values _
                        .Select(Function(a) a.Name) _
                        .UniqueNames

                    For i As Integer = 0 To values.Length - 1
                        Call vec.add(names(i), values(i).Value)
                    Next
                End If

                Return vec
            Else
                Dim vec As vector = vector.asVector(values.Values)
                Call vec.setNamesSafe(values.Keys, env)
                Return vec
            End If
        End Function

        ''' <summary>
        ''' ### Character Translation and Casefolding
        ''' 
        ''' Translate characters in character vectors, in particular 
        ''' from upper to lower case or vice versa.
        ''' </summary>
        ''' <param name="x">
        ''' a character vector, or an object that can be coerced to character by 
        ''' ``as.character``.
        ''' </param>
        ''' <returns>
        ''' A character vector of the same length and with the same attributes as 
        ''' ``x`` (after possible coercion).
        ''' 
        ''' Elements of the result will be have the encoding declared as that of 
        ''' the current locale (see Encoding) if the corresponding input had a 
        ''' declared encoding And the current locale Is either Latin-1 Or UTF-8. 
        ''' The result will be in the current locale's encoding unless the 
        ''' corresponding input was in UTF-8, when it will be in UTF-8 when the 
        ''' system has Unicode wide characters.
        ''' </returns>
        ''' <remarks>
        ''' chartr translates each character in x that is specified in old 
        ''' to the corresponding character specified in new. Ranges are 
        ''' supported in the specifications, but character classes and 
        ''' repeated characters are not. If old contains more characters than 
        ''' new, an error is signaled; if it contains fewer characters, the 
        ''' extra characters at the end of new are ignored.
        ''' 
        ''' ``tolower`` And ``toupper`` convert upper-case characters in a 
        ''' character vector to lower-case, Or vice versa. Non-alphabetic 
        ''' characters are left unchanged.
        ''' </remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("tolower")>
        Public Function tolower(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
            Return env.EvaluateFramework(Of String, String)(x, AddressOf VBStr.LCase)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("toupper")>
        Public Function toupper(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
            Return env.EvaluateFramework(Of String, String)(x, AddressOf VBStr.UCase)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="comma"></param>
        ''' <param name="andalso"></param>
        ''' <param name="etc"></param>
        ''' <param name="joinSpace"></param>
        ''' <param name="enUS"></param>
        ''' <returns>
        ''' if the input data list contains no elements or it is 
        ''' nothing, then this function will returns nothing
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("concatenate")>
        Public Function concatenate(<RRawVectorArgument>
                                    data As Object,
                                    Optional comma$ = ",",
                                    Optional andalso$ = "and",
                                    Optional etc$ = "etc",
                                    Optional joinSpace As Boolean = True,
                                    Optional enUS As Boolean = False) As String

            Return CLRVector.asCharacter(data).Concatenate(comma, [andalso], etc, joinSpace, enUS)
        End Function

        ''' <summary>
        ''' ``chr`` returns the characters corresponding to the 
        ''' specified ASCII codes.
        ''' </summary>
        ''' <param name="ascii">
        ''' vector or list of vectors containing integer ASCII codes
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("chr")>
        Public Function chr(ascii As Integer()) As Char()
            Return ascii.SafeQuery.Select(Function(i) ChrW(i)).ToArray
        End Function

        <ExportAPI("charAt")>
        Public Function charAt(str As String, i As Integer) As Char
            If Global.System.String.IsNullOrEmpty(str) Then
                Return Nothing
            ElseIf i <= 0 OrElse i > str.Length Then
                Return Nothing
            Else
                Return str(i - 1)
            End If
        End Function

        ''' <summary>
        ''' ### Substrings of a Character Vector
        ''' 
        ''' Extract or replace substrings in a character vector.
        ''' </summary>
        ''' <param name="x">
        ''' a character vector.
        ''' </param>
        ''' <param name="start">integer. The first element To be replaced.</param>
        ''' <param name="stop">integer. The last element to be replaced.</param>
        ''' <returns>For ``substr``, a character vector of the same length 
        ''' and with the same attributes as x (after possible coercion).
        ''' </returns>
        <ExportAPI("substr")>
        Public Function substr(<RRawVectorArgument> x As Object, start%, stop%, Optional env As Environment = Nothing) As String()
            Dim strs As String() = CLRVector.asCharacter(x)
            Dim substrs As String() = strs _
                .Select(Function(str)
                            Return VBStr.Mid(str, start, [stop] - start + 1)
                        End Function) _
                .ToArray

            Return substrs
        End Function

        ''' <summary>
        ''' The str_replace() function from the stringr package in R 
        ''' can be used to replace matched patterns in a string.
        ''' </summary>
        ''' <param name="strings">Character vector</param>
        ''' <param name="pattern">Pattern to look for</param>
        ''' <param name="replacement">
        ''' A character vector of replacements
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("str_replace")>
        Public Function str_replace(strings As String(),
                                    pattern As String,
                                    replacement As String,
                                    Optional fixed As Boolean = False) As String()
            If fixed Then
                Return strings _
                    .SafeQuery _
                    .Select(Function(str) str.Replace(pattern, replacement)) _
                    .ToArray
            Else
                Return strings _
                    .SafeQuery _
                    .Select(Function(str)
                                Return str.StringReplace(pattern, replacement)
                            End Function) _
                    .ToArray
            End If
        End Function

        ''' <summary>
        ''' generate random string which is all consist 
        ''' with ascii chars.
        ''' </summary>
        ''' <param name="nchar"></param>
        ''' <param name="count"></param>
        ''' <returns></returns>
        <ExportAPI("random_str")>
        Public Function randomAsciiStr(nchar As Integer,
                                       Optional count As Integer = 1,
                                       Optional no_symbols As Boolean = True) As String()
            Return count _
                .Sequence _
                .Select(Function(any)
                            Return RandomASCIIString(
                                len:=nchar,
                                skipSymbols:=no_symbols
                            )
                        End Function) _
                .ToArray
        End Function

        ''' <summary>
        ''' check of the text equals between two character vector.
        ''' </summary>
        ''' <param name="x">should be a character vector</param>
        ''' <param name="y">another character vector</param>
        ''' <param name="null_equals">null string value as equals? example as NULL is equals to NULL, or NULL is equals to empty string</param>
        ''' <param name="empty_equals">empty factor string value as equals? example as NA is equals to n/a, or NA is equals to NULL, etc.</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("text_equals")>
        Public Function text_equals(<RRawVectorArgument> x As Object,
                                    <RRawVectorArgument> y As Object,
                                    Optional null_equals As Boolean = False,
                                    Optional empty_equals As Boolean = True,
                                    Optional env As Environment = Nothing) As Object

            Dim v1 = GetVectorElement.Create(Of String)(x)
            Dim v2 = GetVectorElement.Create(Of String)(y)
            Dim size As Integer = std.Max(v1.size, v2.size)
            Dim op As op_evaluator =
                Function(xi, yi, envir)
                    Return TextEquals(DirectCast(xi, String), DirectCast(yi, String), null_equals, empty_equals)
                End Function

            If Not GetVectorElement.DoesSizeMatch(v1, v2) Then
                Return Internal.debug.stop($"the size of x({v1.size}) is not matched with size of y({v2.size})!", env)
            Else
                Return Core.BinaryCoreInternal(Of String, String, Boolean)(v1, v2, op, env)
            End If
        End Function

        ''' <summary>
        ''' apply for batch text grep/trimming/strip
        ''' </summary>
        ''' <param name="grep_regexp">a regular expression value for produce new text value.</param>
        ''' <param name="x">should be a character vector for apply such operation. omit this parameter value will create a lambda function for grep text.</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <example>
        ''' let names = ["258_Herniarin_[M+H2O+H]+" "993_Geranyl acetate_[M+H2O+H]+" "3229_Glycerol_[M+NH4]+" "398587_Cytidine_[M+H]+" "398007_Adenosine_[M+H]+"];
        ''' let grep = "$(\d+)_$(\[\d*M.*\]\d*[+-])";
        ''' 
        ''' print(text_grep(grep, names));
        ''' # [1] "258_[M+H2O+H]+" "993_[M+H2O+H]+" "3229_[M+NH4]+" "398587_[M+H]+" "398007_[M+H]+"
        ''' </example>
        ''' <remarks>
        ''' the text grep regexp pattern token should be wrapped inside a bracket: ``$(regexp_token)``, 
        ''' any text outside this bracket will be treated as an regular string.
        ''' 
        ''' example as the token parser string: ``$(\d+)_$(\[\d*M.*\]\d*[+-])`` will be split into
        ''' 3 tokens: ``$(\d+)`` for matches integer string token, ``_`` will always produce a regular string token ``_``,
        ''' ``$(\[\d*M.*\]\d*[+-])`` for matches another string token that wrapped with bracket [] and 
        ''' ends with signed integer number as suffix.
        ''' </remarks>
        <ExportAPI("text_grep")>
        <RApiReturn(GetType(String), GetType(TextGrepLambda))>
        Public Function text_grep(grep_regexp As String,
                                  <RRawVectorArgument>
                                  Optional x As Object = Nothing,
                                  Optional env As Environment = Nothing) As Object

            Dim grep As New TextGrepLambda(grep_regexp)

            If x Is Nothing Then
                ' just create lambda function
                Return grep
            Else
                Return grep.TextGrep(CLRVector.asCharacter(x))
            End If
        End Function

        <ExportAPI("utf8_decode")>
        <RApiReturn(TypeCodes.string)>
        Public Function utf8_decode(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
            Return env.EvaluateFramework(Of String, String)(x,
                eval:=Function(str)
                          Return CString.Decode(str)
                      End Function)
        End Function

    End Module

    Public Class TextGrepLambda

        ReadOnly tokens As IToString(Of String)()

        Sub New(exp As String)
            tokens = GetTokens(exp).ToArray
        End Sub

        Private Shared Iterator Function GetTokens(exp As String) As IEnumerable(Of IToString(Of String))
            Dim stack As New Stack(Of Char)
            Dim buf As New CharBuffer

            For Each c As Char In exp
                If c = "$"c Then
                    If stack.Count = 0 Then
                        If buf > 0 Then
                            Dim s_temp As New String(buf.PopAllChars)
                            Yield Function(o) s_temp
                        End If

                        buf.Add(c)
                    Else
                        buf.Add(c)
                    End If
                ElseIf c = "("c Then
                    If stack.Count = 0 Then
                        If buf = "$"c Then
                            stack.Push(c)
                            buf.Pop()
                        Else
                            buf.Add(c)
                        End If
                    Else
                        ' an regular string
                        buf.Add(c)
                    End If
                ElseIf c = ")"c Then
                    If stack.Count > 0 Then
                        ' pop a (
                        stack.Pop()

                        If stack.Count = 0 Then
                            ' end of a regexp token
                            Dim token_r As New String(buf.PopAllChars)
                            Dim r As New Regex(token_r, RegexOptions.Compiled)

                            Yield Function(str) r.Match(str).Value
                        Else
                            ' still inside a regexp token
                            buf.Add(c)
                        End If
                    Else
                        ' inside an regular string
                        buf.Add(c)
                    End If
                Else
                    buf.Add(c)
                End If
            Next

            If buf > 0 Then
                Dim s_temp As New String(buf.PopAllChars)
                Yield Function(any) s_temp
            End If
        End Function

        Public Function TextGrep(text As String()) As String()
            Return text _
                .SafeQuery _
                .Select(Function(si)
                            Dim t As String() = tokens _
                                .Select(Function(r) r(si)) _
                                .ToArray
                            Dim s_new As String = String.Join("", t)
                            Return s_new
                        End Function) _
                .ToArray
        End Function
    End Class
End Namespace
