#Region "Microsoft.VisualBasic::f425b63a21d67d1c6768081bcea11d11, R#\System\Components\FileReference.vb"

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

    '   Total Lines: 19
    '    Code Lines: 10
    ' Comment Lines: 4
    '   Blank Lines: 5
    '     File Size: 578 B


    '     Class FileReference
    ' 
    '         Properties: filepath, fs
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices

Namespace Development.Components

    Public Class FileReference

        ''' <summary>
        ''' The file object reference inside the given <see cref="fs"/> wrapper
        ''' </summary>
        ''' <returns></returns>
        Public Property filepath As String
        Public Property fs As IFileSystemEnvironment

        Public Overrides Function ToString() As String
            Return $"<&H0x{fs.GetHashCode.ToHexString}, {fs.ToString}> '{filepath}'"
        End Function

    End Class
End Namespace
