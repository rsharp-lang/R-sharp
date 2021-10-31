#Region "Microsoft.VisualBasic::ae7ba56df138758a5e428bdaa81f35eb, R#\Runtime\Internal\internalInvokes\string\stringr.vb"

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

    '     Module stringr
    ' 
    '         Function: [objToString], base64Decode, base64Str, bencode, charAt
    '                   chr, Csprintf, decodeObject, findToStringWithFormat, fromBstring
    '                   grep, html, json, loadXml, match
    '                   nchar, paste, rawBufferBase64, regexp, splitSingleStrAuto
    '                   sprintfSingle, str_empty, str_pad, (+2 Overloads) str_replace, strPad_internal
    '                   strsplit, substr, tagvalue, tolower, urldecode
    '                   xml
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ApplicationServices.Development.NetCore5
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Serialization.Bencoding
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting
Imports encoder = SMRUCC.Rsharp.Development.Components.Encoder
Imports REnv = SMRUCC.Rsharp.Runtime
Imports Rset = SMRUCC.Rsharp.Runtime.Internal.Invokes.set
Imports VBStr = Microsoft.VisualBasic.Strings
Imports vector = SMRUCC.Rsharp.Runtime.Internal.Object.vector

Namespace Runtime.Internal.Invokes

    Public Module stringr

        ''' <summary>
        ''' comvert any object to html text document
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("html")>
        Public Function html(<RRawVectorArgument> x As Object, env As Environment) As Object
            If x Is Nothing Then
                Return Nothing
            Else
                Return htmlPrinter.GetHtml(x, env)
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
                                      Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return ""
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
                Dim type As Type = seqData.GetType.GetElementType
                Dim toStringF As MethodInfo = type _
                    .GetMethods(PublicProperty) _
                    .Where(Function(f)
                               Return findToStringWithFormat(f)
                           End Function) _
                    .FirstOrDefault

                If toStringF Is Nothing Then
                    toString = Function(xi) If(xi Is Nothing, "", xi.ToString)
                    env.AddMessage($"a format text '{format}' is given, but typeof '{type.Name}' is not accept such format parameter...")
                Else
                    toString = Function(xi)
                                   If xi Is Nothing Then
                                       Return ""
                                   Else
                                       Return DirectCast(toStringF.Invoke(xi, {format}), String)
                                   End If
                               End Function
                End If
            End If

            Return seqData _
                .AsObjectEnumerator _
                .Select(toString) _
                .ToArray
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

        ''' <summary>
        ''' load a .NET object from the xml data file
        ''' </summary>
        ''' <param name="file"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this function will try to parse the object model information
        ''' from the meta data in the xml data file
        ''' </remarks>
        <ExportAPI("loadXml")>
        Public Function loadXml(file As String, Optional env As Environment = Nothing) As Object
            Dim type = DigitalSignature.GetModelInfo(file)

            If type Is Nothing Then
                Return Internal.debug.stop(New NotImplementedException(), env)
            Else
                Dim model As Type = type.GetType
                Call deps.TryHandleNetCore5AssemblyBugs(model)
                Return file.LoadXml(model)
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
            Dim result = BencodeDecoder.Decode(bstr).Select(Function(node) node.decodeObject()).ToArray

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
                             Optional env As Environment = Nothing) As Object

            If x Is Nothing Then
                Return "null"
            Else
                x = encoder.GetObject(x)
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
                Return JsonContract.GetObjectJson(type, x, indent:=Not compress, knownTypes:=genericTypes)
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
        <RApiReturn(GetType(Byte))>
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
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' if the base64 encoded data is text data, that parameter 
        ''' ``asText_encoding`` assign of value ``utf8`` usually. 
        ''' </remarks>
        <ExportAPI("base64_decode")>
        <RApiReturn(GetType(String), GetType(Byte))>
        Public Function base64Decode(base64 As String,
                                     Optional asText_encoding As Object = Nothing,
                                     Optional env As Environment = Nothing) As Object

            If Not asText_encoding Is Nothing Then
                ' decode as string
                Dim encoding As Encoding = GetEncoding(asText_encoding)
                Dim raw As Byte() = base64.Base64RawBytes

                Return encoding.GetString(raw)
            Else
                Return New MemoryStream(base64.Base64RawBytes)
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
        Public Function grep(<RRawVectorArgument> text As Object, greps As String(), Optional fixed As Boolean = False, Optional env As Environment = Nothing) As Object
            If text Is Nothing Then
                Return Nothing
            ElseIf fixed Then
                Return Rset _
                    .getObjectSet(text, env) _
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
                Return Rset.getObjectSet(text, env) _
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
                Return Runtime.asVector(Of String)(strs) _
                    .AsObjectEnumerator(Of String) _
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
        <ExportAPI("match")>
        Public Function match(regexp As Regex, <RRawVectorArgument> strings As Object, Optional env As Environment = Nothing) As Object
            If regexp Is Nothing Then
                Return Internal.debug.stop("regular expression object can not be null!", env)
            End If

            Return asVector(Of String)(strings) _
                .AsObjectEnumerator(Of String) _
                .Select(Function(str)
                            Return regexp.Match(str).Value
                        End Function) _
                .ToArray
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
        Public Function Csprintf(format As Array, <RListObjectArgument> Optional arguments As Object = Nothing, Optional env As Environment = Nothing) As Object
            Dim sprintf As Func(Of String, Object(), String) = AddressOf CLangStringFormatProvider.sprintf
            Dim listValues As Object() = DirectCast(base.Rlist(arguments, env), list).slots.Values.ToArray
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
                                 Optional env As Environment = Nothing) As Object

            If delimiter Is Nothing Then
                Return debug.stop("the given delimiter is nothing!", env)
            End If

            If text.IsNullOrEmpty Then
                Return Nothing
            ElseIf text.Length = 1 Then
                Return text(Scan0).splitSingleStrAuto(delimiter, fixed)
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
        ''' string join with given delimiter
        ''' </summary>
        ''' <param name="strings$"></param>
        ''' <param name="deli$"></param>
        ''' <returns></returns>
        <ExportAPI("paste")>
        Friend Function paste(strings$(), Optional deli$ = " ") As Object
            Return strings.JoinBy(deli)
        End Function

        ''' <summary>
        ''' Pattern Matching and Replacement
        ''' </summary>
        ''' <param name="subj">a character vector</param>
        ''' <param name="search$"></param>
        ''' <param name="replaceAs$"></param>
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
        Public Function str_pad([string] As String(), width%, Optional side As str_padSides = str_padSides.left, Optional pad As Char = " "c) As String()
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

        <ExportAPI("str_empty")>
        Public Function str_empty([string] As String()) As Boolean()
            Return [string].Select(AddressOf StringEmpty).ToArray
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
        Public Function tagvalue([string] As String(), Optional delimiter$ = " ", Optional trim_value As Boolean = True, Optional env As Environment = Nothing) As Object
            Dim values As NamedValue(Of String)() = [string] _
                .SafeQuery _
                .Select(Function(str)
                            Return str.GetTagValue(delimiter, trim:=trim_value)
                        End Function) _
                .ToArray
            Dim vec As vector = vector.asVector(values.Values)
            Call vec.setNames(values.Keys, env)
            Return vec
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

        ''' <summary>
        ''' ``chr`` returns the characters corresponding to the specified ASCII codes.
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
            Dim strs As String() = REnv.asVector(Of String)(x)
            Dim substrs As String() = strs _
                .Select(Function(str)
                            Return str.Substring(
                                startIndex:=start - 1,
                                length:=[stop] - start + 1
                            )
                        End Function) _
                .ToArray

            Return substrs
        End Function

        ''' <summary>
        ''' The str_replace() function from the stringr package in R can be used to replace matched patterns in a string.
        ''' </summary>
        ''' <param name="strings">Character vector</param>
        ''' <param name="pattern">Pattern to look for</param>
        ''' <param name="replacement">A character vector of replacements</param>
        ''' <returns></returns>
        <ExportAPI("str_replace")>
        Public Function str_replace(strings As String(), pattern As String, replacement As String, Optional fixed As Boolean = False) As String()
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
    End Module
End Namespace
