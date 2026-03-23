#Region "Microsoft.VisualBasic::78a1f6a1763fa22a59073aa0d3634e66, R#\System\Components\FileReference.vb"

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

    '   Total Lines: 24
    '    Code Lines: 14 (58.33%)
    ' Comment Lines: 4 (16.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (25.00%)
    '     File Size: 817 B


    '     Class FileReference
    ' 
    '         Properties: filepath, fs
    ' 
    '         Function: open, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices

Namespace Development.Components

    Public Class FileReference

        ''' <summary>
        ''' The file object reference inside the given <see cref="fs"/> wrapper
        ''' </summary>
        ''' <returns></returns>
        Public Property filepath As String
        Public Property fs As IFileSystemEnvironment

        Public Function open(Optional mode As FileMode = FileMode.OpenOrCreate, Optional access As FileAccess = FileAccess.Read) As Stream
            Return fs.OpenFile(filepath, mode, access)
        End Function

        Public Overrides Function ToString() As String
            Return $"<&H0x{fs.GetHashCode.ToHexString}, {fs.ToString}> '{filepath}'"
        End Function

    End Class
End Namespace
