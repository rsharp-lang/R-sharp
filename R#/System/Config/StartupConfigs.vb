#Region "Microsoft.VisualBasic::8ece6745dd9109991fc5b1b36ca08c6d, R#\System\Config\StartupConfigs.vb"

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

    '   Total Lines: 12
    '    Code Lines: 8 (66.67%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (33.33%)
    '     File Size: 318 B


    '     Class StartupConfigs
    ' 
    '         Properties: loadingPackages
    ' 
    '         Function: DefaultLoadingPackages
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Development.Configuration

    Public Class StartupConfigs

        Public Property loadingPackages As String()

        Public Shared Function DefaultLoadingPackages() As String()
            Return {"base", "utils", "grDevices", "stats", "math"}
        End Function

    End Class
End Namespace
