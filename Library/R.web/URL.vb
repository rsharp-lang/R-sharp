#Region "Microsoft.VisualBasic::5fd8a903cca9f12433c176344556b65a, Library\R.web\URL.vb"

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
'     Function: [get], post, wget
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Net
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports Microsoft.VisualBasic.Net.Http
Imports SMRUCC.Rsharp.Runtime.Components
Imports REnv = SMRUCC.Rsharp.Runtime
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' the R# http utils
''' </summary>
<Package("http", Category:=APICategories.UtilityTools)>
Public Module URL

    <ExportAPI("urlencode")>
    <RApiReturn(GetType(String))>
    Public Function urlencode(<RRawVectorArgument> data As Object, Optional env As Environment = Nothing) As Object
        If data Is Nothing Then
            Return Nothing
        ElseIf TypeOf data Is String Then
            Return DirectCast(data, String).UrlEncode
        ElseIf TypeOf data Is String() Then
            Return DirectCast(data, String()).Select(Function(str) str.UrlEncode).ToArray
        ElseIf TypeOf data Is vector Then
            Return DirectCast(data, vector).data.AsObjectEnumerator(Of String).Select(Function(str) str.UrlEncode).ToArray
        ElseIf TypeOf data Is list Then
            Return DirectCast(data, list).AsGeneric(Of String)(env).BuildUrlData(escaping:=True, stripNull:=False)
        Else
            Return Message.InCompatibleType(GetType(String), data.GetType, env)
        End If
    End Function

    <ExportAPI("requests.get")>
    Public Function [get](url As String,
                          Optional headers As list = Nothing,
                          Optional env As Environment = Nothing) As WebResponseResult

        Dim httpHeaders As Dictionary(Of String, String) = headers?.AsGeneric(Of String)(env)
        Dim verbose As Boolean = env.globalEnvironment.options.verbose

        Return HttpGet _
            .BuildWebRequest(url, httpHeaders, Nothing, Nothing) _
            .UrlGet(echo:=verbose)
    End Function

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

    <ExportAPI("requests.post")>
    Public Function post(url As String,
                         Optional params As list = Nothing,
                         Optional headers As list = Nothing,
                         Optional env As Environment = Nothing) As WebResponseResult

        Dim httpHeaders As Dictionary(Of String, String) = headers?.AsGeneric(Of String)(env)
        Dim verbose As Boolean = env.globalEnvironment.options.verbose

        Return WebServiceUtils.PostRequest(url, params?.AsGeneric(Of String)(env))
    End Function

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
