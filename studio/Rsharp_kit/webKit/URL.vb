#Region "Microsoft.VisualBasic::a831616251201033918f60e37315b037, F:/GCModeller/src/R-sharp/studio/Rsharp_kit/webKit//URL.vb"

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

    '   Total Lines: 454
    '    Code Lines: 313
    ' Comment Lines: 96
    '   Blank Lines: 45
    '     File Size: 18.64 KB


    ' Module URL
    ' 
    '     Function: [get], content, encodeTokenPart, getPostPayload, httpCache
    '               HttpClientPost, HttpCookies, is_http_error, post, runHttpRequest
    '               upload, urlcomponent, urlencode, wget
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' the R# http utils
''' </summary>
<Package("http", Category:=APICategories.UtilityTools)>
Public Module URL

    <ExportAPI("post_data")>
    Public Function HttpClientPost(url As String,
                                   <RRawVectorArgument>
                                   data As Object,
                                   Optional env As Environment = Nothing) As Object

        Dim httpClient As New HttpClient
        httpClient.DefaultRequestHeaders.Accept.Clear()
        httpClient.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))

        Dim httpContent As New JSONContent(data, env)
        Dim response = httpClient.PostAsync(url, httpContent).Result
        Dim t As Task(Of String) = response.Content.ReadAsStringAsync

        If t IsNot Nothing Then
            If response.IsSuccessStatusCode Then
                Return t.Result
            Else
                Return Internal.debug.stop(t.Result, env)
            End If
        Else
            Return ""
        End If
    End Function

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
            Dim vec As String() = CLRVector.asCharacter(data)
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
    ''' <param name="cache">
    ''' used for make compatibility to the offline mode
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("requests.get")>
    Public Function [get](url As String,
                          Optional headers As list = Nothing,
                          Optional default404 As Object = "(404) Not Found!",
                          Optional cache As WebTextQuery = Nothing,
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
            Dim request = Function(strUrl As String)
                              If cache Is Nothing Then
                                  Return HttpGet _
                                      .BuildWebRequest(strUrl, httpHeaders, Nothing, Nothing) _
                                      .UrlGet(echo:=verbose)
                              Else
                                  Return New WebResponseResult With {
                                      .url = strUrl,
                                      .html = cache.QueryCacheText(url, cacheType:=".txt"),
                                      .headers = ResponseHeaders.Header200,
                                      .timespan = 0
                                  }
                              End If
                          End Function

            Return runHttpRequest(url, request, CLRVector.asCharacter(default404), jsonlite.toJSON(headers, env))
        End If
    End Function

    Private Function runHttpRequest(url As String,
                                    request As Func(Of String, WebResponseResult),
                                    default404 As String(),
                                    payload As String) As WebResponseResult
        Try
            Return request(url)
        Catch ex As Exception When InStr(ex.Message, "(404) Not Found") > 0
            Return New WebResponseResult With {
                .url = url,
                .html = default404.JoinBy("; "),
                .timespan = 0,
                .headers = ResponseHeaders.Header404NotFound,
                .payload = payload
            }
        Catch ex As Exception When TypeOf ex Is WebException
            Return New WebResponseResult With {
                .url = url,
                .html = ex.Message,
                .timespan = 0,
                .headers = ResponseHeaders.HttpRequestError(ex.Message.Match("\d+").DoCall(AddressOf Integer.Parse)),
                .payload = payload
            }
        Catch ex As Exception
            Throw
        End Try
    End Function

    ''' <summary>
    ''' create a new http cache context
    ''' </summary>
    ''' <param name="fs"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("http.cache")>
    <RApiReturn(GetType(WebTextQuery))>
    Public Function httpCache(fs As Object, Optional env As Environment = Nothing) As Object
        If fs Is Nothing Then
            Return Internal.debug.stop("the required cache context can not be nothing!", env)
        End If
        If TypeOf fs Is String Then
            Return New WebTextQuery(DirectCast(fs, String))
        ElseIf fs.GetType.ImplementInterface(Of IFileSystemEnvironment) Then
            Return New WebTextQuery(DirectCast(fs, IFileSystemEnvironment))
        Else
            Return Message.InCompatibleType(GetType(IFileSystemEnvironment), fs.GetType, env)
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
        Dim err As New Value(Of Message)
        Dim args = payload.getPostPayload(getError:=err, env)

        If err.HasValue Then
            Return err.Value
        End If

        Dim request = Function(strUrl As String)
                          Return WebServiceUtils.PostRequest(url, args, throw_httpErr:=True)
                      End Function

        Return runHttpRequest(url, request, New String() {"404 NOT FOUND"}, jsonlite.toJSON(payload, env))
    End Function

    <Extension>
    Private Iterator Function getPostPayload(payload As list,
                                             getError As Value(Of Message),
                                             env As Environment) As IEnumerable(Of KeyValuePair(Of String, String))
        If payload Is Nothing Then
            Return
        End If

        For Each key As String In payload.slots.Keys
            Dim value As Object = payload.slots(key)

            If value Is Nothing Then
                value = ""
            ElseIf TypeOf value Is String Then
                value = value
            ElseIf TypeOf value Is Message Then
                getError.Value = value
                Return
            Else
                value = jsonlite.toJSON(value, env)

                If TypeOf value Is Message Then
                    getError.Value = value
                    Return
                End If
            End If

            Yield New KeyValuePair(Of String, String)(key, value)
        Next
    End Function

    ''' <summary>
    ''' Test the web response is http error or not via the http status code is not equals to 200(OK)?
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <ExportAPI("is.http_error")>
    Public Function is_http_error(data As WebResponseResult) As Boolean
        Return data.headers.httpCode <> HTTP_RFC.RFC_OK
    End Function

    ''' <summary>
    ''' get content data from the http request result
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="typeof">
    ''' will try to parse json if this typeof parameter is not nothing
    ''' </param>
    ''' <param name="plain_text">
    ''' treat the content data as html/plaintext data. if not then this 
    ''' function will try to parse the content data as
    ''' 
    ''' 1. list: application/json
    ''' 2. dataframe: text/csv
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("content")>
    Public Function content(data As WebResponseResult,
                            Optional [typeof] As Object = Nothing,
                            Optional plain_text As Boolean = False,
                            Optional throw_http_error As Boolean = True,
                            Optional env As Environment = Nothing) As Object

        Dim text As String = data?.html

        If throw_http_error AndAlso data.headers.httpCode <> HTTP_RFC.RFC_OK Then
            Return Internal.debug.stop({
                data.html,
                $"url: {data.url}",
                $"payload: {data.payload}",
                $"you can set parameter 'throw.http.error' to value FALSE in 'http::content' to disable this error."
            }, env)
        End If
        If plain_text Then
            Return text
        End If

        Dim type = data.headers.headers.Item(HttpHeaderName.ContentType)
        Dim tmp As String = Now.ToString.MD5

        If type = MIME.Text OrElse type = MIME.Html Then
            Return text
        ElseIf type = MIME.Json OrElse type = MIME.JSONText Then
            Dim jsonlib As New [Imports]("JSON", "base")
            ' imports JSON from base
            Call jsonlib.Evaluate(env)
            Dim json As Object = env.globalEnvironment.Rscript.Invoke("JSON::json_decode", text, env.globalEnvironment.Rscript.strict, [typeof], env)
            Return json
        ElseIf type = "text/csv" OrElse type = "text/tsv" Then
            ' parse dataframe 
            Dim dataframeUtils As New [Imports]("dataframe", "base")
            Call dataframeUtils.Evaluate(env)
            Dim df = env.globalEnvironment.Rscript.Invoke("dataframe::parseDataframe", text, Nothing, False, True, "#", type = "text/tsv", -1, env)
            Return df
        Else
            Return text
        End If
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
    <RApiReturn(GetType(String))>
    Public Function upload(url As String,
                           <RRawVectorArgument> files As Object,
                           Optional headers As list = Nothing,
                           Optional env As Environment = Nothing) As Object

        If files Is Nothing Then
            Return Internal.debug.stop("the required file list can not be nothing!", env)
        ElseIf env.globalEnvironment.debugMode OrElse env.globalEnvironment.options.verbose Then
            Call env.WriteLineHandler()($"upload file: {url}")
        End If

        Using http As New MultipartForm()
            If TypeOf files Is vector Then
                With DirectCast(files, vector)
                    If .getNames.IsNullOrEmpty Then
                        files = CLRVector.asCharacter(.data)
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
                Dim list As String() = DirectCast(files, String())

                If list.Length = 0 Then
                    Call http.Add("file", list(Scan0).ReadBinary, fileName:=list(Scan0).FileName)
                Else
                    For Each file As String In DirectCast(files, String())
                        Call http.Add(file.FileName, file.ReadBinary, file.FileName)
                    Next
                End If
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
End Module
