#Region "Microsoft.VisualBasic::80f00232c8e30b7e61cd73d3649a8b9b, studio\Rsharp_kit\webKit\URL.vb"

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

' Module URL
' 
'     Function: [get], content, HttpCookies, post, upload
'               urlcomponent, urlencode, wget
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' the R# http utils
''' </summary>
<Package("http", Category:=APICategories.UtilityTools)>
Public Module URL

    Private Function encodeTokenPart(argv As KeyValuePair(Of String, Object), env As Environment) As String
        Dim str = urlencode(argv.Value, env)

        If TypeOf str Is String() Then
            If DirectCast(str, String()).Length = 1 Then
                Return $"{argv.Key}={DirectCast(str, String())(Scan0)}"
            Else
                Return DirectCast(str, String()) _
                    .Select(Function(val) $"{argv.Key}={val}") _
                    .JoinBy("&")
            End If
        Else
            Return $"{argv.Key}={str}"
        End If
    End Function

    <ExportAPI("urlcomponent")>
    Public Function urlcomponent(query As list, Optional env As Environment = Nothing) As String
        If query Is Nothing Then
            Return ""
        Else
            Return query.slots _
                .Select(Function(argv)
                            Return encodeTokenPart(argv, env)
                        End Function) _
                .JoinBy("&")
        End If
    End Function

    ''' <summary>
    ''' ### URL-encodes string
    ''' 
    ''' This function is convenient when encoding a string to be 
    ''' used in a query part of a URL, as a convenient way to 
    ''' pass variables to the next page.
    ''' </summary>
    ''' <param name="data">The string to be encoded.</param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' Returns a string in which all non-alphanumeric characters except ``-_.`` 
    ''' have been replaced with a percent (%) sign followed by two hex digits and 
    ''' spaces encoded as plus (+) signs. It is encoded the same way that the 
    ''' posted data from a WWW form is encoded, that is the same way as in 
    ''' ``application/x-www-form-urlencoded`` media type. This differs from the 
    ''' RFC 3986 encoding (see rawurlencode()) in that for historical reasons, 
    ''' spaces are encoded as plus (+) signs.
    ''' </returns>
    ''' <remarks>
    ''' Be careful about variables that may match HTML entities. Things like 
    ''' ``&amp;amp``, ``&amp;copy`` and ``&amp;pound`` are parsed by the browser and 
    ''' the actual entity is used instead of the desired variable name. 
    ''' 
    ''' This is an obvious hassle that the W3C has been telling people about for 
    ''' years. The reference is here: 
    ''' http://www.w3.org/TR/html4/appendix/notes.html#h-B.2.2.
    ''' </remarks>
    <ExportAPI("urlencode")>
    <RApiReturn(GetType(String))>
    Public Function urlencode(<RRawVectorArgument> data As Object, Optional env As Environment = Nothing) As Object
        If data Is Nothing Then
            Return Nothing
        ElseIf TypeOf data Is vector Then
            data = DirectCast(data, vector).data
        End If

        If TypeOf data Is String Then
            Return DirectCast(data, String).UrlEncode
        ElseIf data.GetType.IsArray Then
            Dim vec As String() = REnv.asVector(Of String)(data)
            Dim tokens = vec _
                .Select(Function(str) str.UrlEncode) _
                .ToArray

            Return tokens
        ElseIf TypeOf data Is list Then
            Return DirectCast(data, list).AsGeneric(Of String)(env).BuildUrlData(escaping:=True, stripNull:=False)
        Else
            Return Message.InCompatibleType(GetType(String), data.GetType, env)
        End If
    End Function

    ''' <summary>
    ''' http get request
    ''' </summary>
    ''' <param name="url"></param>
    ''' <param name="headers"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("requests.get")>
    Public Function [get](url As String,
                          Optional headers As list = Nothing,
                          Optional default404 As Object = "(404) Not Found!",
                          Optional env As Environment = Nothing) As WebResponseResult

        Dim httpHeaders As Dictionary(Of String, String) = headers?.AsGeneric(Of String)(env)
        Dim verbose As Boolean = env.globalEnvironment.options.verbose

        If url.FileExists Then
            ' is a local file
            Return New WebResponseResult With {
                .url = url,
                .html = url.ReadAllText,
                .timespan = 0
            }
        Else
            Try
                Return HttpGet _
                    .BuildWebRequest(url, httpHeaders, Nothing, Nothing) _
                    .UrlGet(echo:=verbose)
            Catch ex As Exception When InStr(ex.Message, "(404) Not Found") > 0
                Return New WebResponseResult With {
                    .url = url,
                    .html = default404,
                    .timespan = 0,
                    .headers = ResponseHeaders.Header404NotFound
                }
            Catch ex As Exception
                Throw
            End Try
        End If
    End Function

    ''' <summary>
    ''' parse cookies data from the http web request result
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <ExportAPI("cookies")>
    Public Function HttpCookies(data As WebResponseResult) As list
        Dim cookieStr As String = data.headers.TryGetValue(HttpHeaderName.SetCookie)

        If cookieStr.StringEmpty Then
            Return New list With {.slots = New Dictionary(Of String, Object)}
        End If

        Dim tokens = Strings.Split(cookieStr, "; ")
        Dim cookies As New Dictionary(Of String, Object)

        For Each part As String In tokens
            With part.GetTagValue("=", trim:=True)
                cookies(.Name) = .Value
            End With
        Next

        Return New list With {.slots = cookies}
    End Function

    ''' <summary>
    ''' send http post request to the target web server.
    ''' </summary>
    ''' <param name="url">the url of target web services</param>
    ''' <param name="payload">post body, should be in key-value pair format.</param>
    ''' <param name="headers">http headers</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("requests.post")>
    <RApiReturn(GetType(WebResponseResult))>
    Public Function post(url As String,
                         Optional payload As list = Nothing,
                         Optional headers As list = Nothing,
                         Optional env As Environment = Nothing) As Object

        Dim httpHeaders As Dictionary(Of String, String) = headers?.AsGeneric(Of String)(env)
        Dim verbose As Boolean = env.globalEnvironment.options.verbose
        Dim args As New List(Of KeyValuePair(Of String, String))
        Dim value As Object

        If Not payload Is Nothing Then
            For Each key As String In payload.slots.Keys
                value = payload.slots(key)

                If value Is Nothing Then
                    value = ""
                ElseIf TypeOf value Is String Then
                    value = value
                ElseIf TypeOf value Is Message Then
                    Return value
                Else
                    value = jsonlite.toJSON(value, env)

                    If TypeOf value Is Message Then
                        Return value
                    End If
                End If

                args += New KeyValuePair(Of String, String)(key, value)
            Next
        End If

        Return WebServiceUtils.PostRequest(url, args)
    End Function

    ''' <summary>
    ''' get content string from the http request result
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <ExportAPI("content")>
    Public Function content(data As WebResponseResult) As String
        Return data?.html
    End Function

    ''' <summary>
    ''' upload files through http post
    ''' </summary>
    ''' <param name="url">the target network location.</param>
    ''' <param name="files">a list of file path for upload to the specific network location.</param>
    ''' <param name="headers">set cookies or other http headers at here.</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("upload")>
    Public Function upload(url As String,
                           <RRawVectorArgument> files As Object,
                           Optional headers As list = Nothing,
                           Optional env As Environment = Nothing) As Object

        Using http As New MultipartForm()
            If files Is Nothing Then
                Return Internal.debug.stop("the required file list can not be nothing!", env)
            End If

            If TypeOf files Is vector Then
                With DirectCast(files, vector)
                    If .getNames.IsNullOrEmpty Then
                        files = REnv.asVector(Of String)(.data)
                        GoTo uploadbyfiles
                    Else
                        Dim filepath As String

                        For Each name As String In .getNames
                            filepath = Scripting.ToString(REnv.single(.getByName(name)))
                            http.Add(name, filepath.ReadBinary, filepath.FileName)
                        Next
                    End If
                End With
            ElseIf TypeOf files Is String() Then
uploadbyfiles:
                For Each file As String In DirectCast(files, String())
                    Call http.Add(file.FileName, file.ReadBinary, file.FileName)
                Next
            ElseIf TypeOf files Is list Then
                For Each file In DirectCast(files, list).AsGeneric(Of String)(env)
                    Call http.Add(file.Key, file.Value.ReadBinary, file.Value.FileName)
                Next
            Else
                Return Message.InCompatibleType(GetType(String), files.GetType, env)
            End If

            Return http.POST(url, headers?.AsGeneric(Of String)(env))
        End Using
    End Function

    ''' <summary>
    ''' Do file download
    ''' </summary>
    ''' <param name="url$"></param>
    ''' <param name="saveAs$"></param>
    ''' <returns></returns>
    <ExportAPI("wget")>
    Public Function wget(url$, saveAs$) As Boolean
        Return Http.wget.Download(url, saveAs)
    End Function
End Module
