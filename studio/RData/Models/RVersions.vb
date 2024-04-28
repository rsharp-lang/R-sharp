#Region "Microsoft.VisualBasic::59b37ed32670c37bd936f19dd3476204, E:/GCModeller/src/R-sharp/studio/RData//Models/RVersions.vb"

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

    '   Total Lines: 17
    '    Code Lines: 10
    ' Comment Lines: 3
    '   Blank Lines: 4
    '     File Size: 421 B


    '     Class RVersions
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Struct

    ''' <summary>
    ''' R versions.
    ''' </summary>
    Public Class RVersions

        Public format As Integer
        Public serialized As Integer
        Public minimum As Integer

        Public Overrides Function ToString() As String
            Return $"RVersions(format={format}, serialized={serialized}, minimum={minimum})"
        End Function

    End Class
End Namespace
