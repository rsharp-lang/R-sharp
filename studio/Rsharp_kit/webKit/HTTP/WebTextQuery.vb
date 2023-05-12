#Region "Microsoft.VisualBasic::d1613bbd23d7e7471dcad35a87ee449a, D:/GCModeller/src/R-sharp/studio/Rsharp_kit/webKit//HTTP/WebTextQuery.vb"

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

'   Total Lines: 56
'    Code Lines: 36
' Comment Lines: 11
'   Blank Lines: 9
'     File Size: 1.67 KB


' Class WebTextQuery
' 
'     Properties: fs
' 
'     Constructor: (+2 Overloads) Sub New
'     Function: contextPrefix, doParseGuid, doParseObject, doParseUrl, GetText
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Net.Http

Public Class WebTextQuery : Inherits WebQueryModule(Of String)
    Implements IHttpGet

    Public ReadOnly Property fs As IFileSystemEnvironment
        Get
            Return cache
        End Get
    End Property

    Sub New(dir As String)
        Call MyBase.New(dir)
    End Sub

    Sub New(fs As IFileSystemEnvironment)
        Call MyBase.New(fs)
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetText(url As String) As String Implements IHttpGet.GetText
        Return QueryCacheText(url, cacheType:=".txt")
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="context">The query context is the url string</param>
    ''' <returns></returns>
    Protected Overrides Function doParseUrl(context As String) As String
        Return context
    End Function

    ''' <summary>
    ''' a general method just used for get html text
    ''' </summary>
    ''' <param name="html"></param>
    ''' <param name="schema"></param>
    ''' <returns></returns>
    Protected Overrides Function doParseObject(html As String, schema As Type) As Object
        Return html
    End Function

    Protected Overrides Function doParseGuid(context As String) As String
        Return MD5(context)
    End Function

    Protected Overrides Function contextPrefix(guid As String) As String
        If TypeOf cache Is Directory Then
            Return guid.Substring(2, 2)
        Else
            Return $"/.cache/${guid.Substring(2, 2)}"
        End If
    End Function
End Class
