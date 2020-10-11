#Region "Microsoft.VisualBasic::702eb4d2432890879375d061adec7432, R#\Runtime\Internal\internalInvokes\string.vb"

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
    '         Function: [string], base64, base64Decode, bencode, Csprintf
    '                   decodeObject, fromBstring, grep, html, json
    '                   match, nchar, paste, regexp, sprintfSingle
    '                   str_empty, str_pad, str_replace, strPad_internal, strsplit
    '                   xml
    ' 
    '     Enum str_padSides
    ' 
    '         both, left, right
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Serialization.Bencoding
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports encoder = SMRUCC.Rsharp.System.Components.Encoder
Imports Rset = SMRUCC.Rsharp.Runtime.Internal.Invokes.set
Imports VBStr = Microsoft.VisualBasic.Strings
Imports vector = SMRUCC.Rsharp.Runtime.Internal.Object.vector

Namespace Runtime.Internal.Invokes

    Module stringr

        <ExportAPI("html")>
        Public Function html(<RRawVectorArgument> x As Object, env As Environment) As Object
            If x Is Nothing Then
                Return Nothing
            Else
                Return htmlPrinter.GetHtml(x, env)
            End If
        End Function

        ''' <summary>
        ''' Convert an R Object to a Character String
        ''' 
        ''' This is a helper function for format to produce a 
        ''' single character string describing an R object.
        ''' </summary>
        ''' <param name="x">The object to be converted.</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("toString")>
        <RApiReturn(GetType(String))>
        Public Function [string](<RRawVectorArgument> x As Object, env As Environment) As Object
            If x Is Nothing Then
                Return ""
            ElseIf x.GetType.IsArray Then
                Return printer.getStrings(x, Nothing, env.globalEnvironment).ToArray
            Else
                Return printer.ToString(x.GetType, env.globalEnvironment, True, False)(x)
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
                GetType(String())
            }

            Try
                Return JsonContract.GetObjectJson(type, x, indent:=Not compress, knownTypes:=genericTypes)
            Catch ex As Exception
                Return debug.stop(ex, env)
            End Try
        End Function

        ''' <summary>
        ''' encode byte stream into base64 string
        ''' </summary>
        ''' <param name="raw"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("base64")>
        <RApiReturn(GetType(Byte))>
        Public Function base64(<RRawVectorArgument> raw As Object, Optional env As Environment = Nothing) As Object
            Dim bytes As pipeline = pipeline.TryCreatePipeline(Of Byte)(raw, env)

            If raw Is Nothing Then
                Return Nothing
            End If

            If bytes.isError Then
                If TypeOf raw Is Stream Then
                    Return DirectCast(raw, Stream).PopulateBlocks.IteratesALL.ToBase64String
                Else
                    Return bytes.getError
                End If
            Else
                Return bytes.populates(Of Byte)(env).ToBase64String
            End If
        End Function

        ''' <summary>
        ''' decode base64 string as text or the raw bytes buffer object.
        ''' </summary>
        ''' <param name="base64">a string in base64 encode pattern</param>
        ''' <param name="asText_encoding">if this parameter is not nothing, then the output will be convert as text</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("base64_decode")>
        <RApiReturn(GetType(String), GetType(Byte))>
        Public Function base64Decode(base64 As String, Optional asText_encoding As Object = Nothing, Optional env As Environment = Nothing) As Object
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
        Public Function grep(<RRawVectorArgument> text As Object, greps As String(), Optional env As Environment = Nothing) As Object
            If text Is Nothing Then
                Return Nothing
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
                                Return textgrep(Scripting.ToString(o))
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
        Public Function Csprintf(format As Array, <RListObjectArgument> arguments As Object, Optional env As Environment = Nothing) As Object
            Dim sprintf As Func(Of String, Object(), String) = AddressOf CLangStringFormatProvider.sprintf
            Dim args As Array() = DirectCast(base.Rlist(arguments, env), list).slots.Values _
                .Skip(1) _
                .Select(Function(a)
                            Return Runtime.asVector(Of Object)(a)
                        End Function) _
                .ToArray
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
        Friend Function strsplit(text$(), Optional delimiter$ = " ", Optional fixed As Boolean = False, Optional env As Environment = Nothing) As Object
            If delimiter Is Nothing Then
                Return debug.stop("the given delimiter is nothing!", env)
            End If

            If text.IsNullOrEmpty Then
                Return Nothing
            ElseIf text.Length = 1 Then
                If fixed Then
                    Return VBStr.Split(text(Scan0), delimiter)
                Else
                    Return text(Scan0).StringSplit(delimiter)
                End If
            Else
                Return text _
                    .SeqIterator _
                    .ToDictionary(Function(i) $"[[{i.i + 1}]]",
                                  Function(i)
                                      If fixed Then
                                          Return VBStr.Split(i.value, delimiter)
                                      Else
                                          Return i.value.StringSplit(delimiter)
                                      End If
                                  End Function)
            End If
        End Function

        <ExportAPI("paste")>
        Friend Function paste(strings$(), Optional deli$ = " ") As Object
            Return strings.JoinBy(deli)
        End Function

        ''' <summary>
        ''' Pattern Matching and Replacement
        ''' </summary>
        ''' <param name="subj$"></param>
        ''' <param name="search$"></param>
        ''' <param name="replaceAs$"></param>
        ''' <param name="regexp"></param>
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
                Return subj.Select(Function(s) s.Replace(search, replaceAs)).ToArray
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
    End Module

    Public Enum str_padSides
        left
        right
        both
    End Enum
End Namespace
