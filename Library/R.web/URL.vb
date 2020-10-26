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

''' <summary>
''' the R# http utils
''' </summary>
<Package("http", Category:=APICategories.UtilityTools)>
Public Module URL

    <ExportAPI("requests.get")>
    Public Function [get](url As String,
                          Optional params As list = Nothing,
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
                         Optional env As Environment = Nothing) As String

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
