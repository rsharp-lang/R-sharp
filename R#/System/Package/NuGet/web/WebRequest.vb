#Region "Microsoft.VisualBasic::f460854186ca9a737f7c6ba7f377092c, R-sharp\R#\System\Package\NuGet\WebRequest.vb"

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

    '   Total Lines: 43
    '    Code Lines: 30
    ' Comment Lines: 3
    '   Blank Lines: 10
    '     File Size: 1.35 KB


    '     Enum NuGetMirrors
    ' 
    '         azure_cn, azuresearch, nuget
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class WebRequest
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Query
    ' 
    '         Sub: (+2 Overloads) SetMirror
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Development.Package.NuGet.web

    Public Enum NuGetMirrors
        nuget
        azuresearch
        azure_cn
    End Enum

    ''' <summary>
    ''' use nuget package repository system as R# package repository
    ''' </summary>
    Public Class WebRequest

        Public Shared ReadOnly mirrors As IReadOnlyDictionary(Of String, String) =
            New Dictionary(Of String, String) From {
                {"azuresearch", "https://azuresearch-usnc.nuget.org"}
            }

        Shared m_mirror As String

        Sub New()
            Call SetMirror(NuGetMirrors.azuresearch)
        End Sub

        Public Shared Sub SetMirror(mirror As String)
            WebRequest.m_mirror = mirror
        End Sub

        Public Shared Sub SetMirror(mirror As NuGetMirrors)
            WebRequest.m_mirror = mirrors(mirror.Description)
        End Sub

        Public Shared Function Query(term As String, Optional pre_release As Boolean = False) As PackageData()
            Dim url As String = $"{m_mirror}/query?q={term.UrlEncode}&prerelease={pre_release.ToString.ToLower}"
            Dim json As String = url.GET
            Dim data = json.LoadJSON(Of NuGet.web.Search)

            Return data
        End Function
    End Class
End Namespace
