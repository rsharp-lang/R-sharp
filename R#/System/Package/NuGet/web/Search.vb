#Region "Microsoft.VisualBasic::61b90349fb6889f0a6a588d2c144afa3, R#\System\Package\NuGet\web\Search.vb"

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

    '   Total Lines: 44
    '    Code Lines: 34 (77.27%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: NaN%
    ' 
    '   Blank Lines: 10 (22.73%)
    '     File Size: 1.34 KB


    '     Class Search
    ' 
    '         Properties: data, totalHits
    ' 
    '     Class PackageData
    ' 
    '         Properties: authors, description, iconUrl, id, licenseUrl
    '                     owners, packageTypes, projectUrl, registration, summary
    '                     tags, title, totalDownloads, verified, version
    '                     versions
    ' 
    '     Class PackageVersion
    ' 
    '         Properties: downloads, version
    ' 
    '     Class PackageType
    ' 
    '         Properties: name
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Development.Package.NuGet.web

    Public Class Search

        Public Property data As PackageData()
        Public Property totalHits As Integer

        Public Shared Narrowing Operator CType(search As Search) As PackageData()
            Return search.data
        End Operator

    End Class

    Public Class PackageData

        Public Property authors As String()
        Public Property description As String
        Public Property iconUrl As String
        Public Property id As String
        Public Property licenseUrl As String
        Public Property owners As String()
        Public Property packageTypes As PackageType()
        Public Property projectUrl As String
        Public Property registration As String
        Public Property summary As String
        Public Property tags As String()
        Public Property title As String
        Public Property totalDownloads As Integer
        Public Property verified As Boolean
        Public Property version As String
        Public Property versions As PackageVersion()

    End Class

    Public Class PackageVersion
        Public Property version As String
        Public Property downloads As Integer
    End Class

    Public Class PackageType

        Public Property name As String
    End Class
End Namespace
