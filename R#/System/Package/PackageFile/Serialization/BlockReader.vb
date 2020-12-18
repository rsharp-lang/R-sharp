#Region "Microsoft.VisualBasic::d280a26fe64786d87c786099b092b76e, R#\System\Package\PackageFile\Serialization\BlockReader.vb"

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

    '     Class BlockReader
    ' 
    '         Properties: body, Expression, type
    ' 
    '         Function: Read
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports SMRUCC.Rsharp.Runtime.Components

Namespace System.Package.File

    Public Class BlockReader

        Public Property Expression As ExpressionTypes
        Public Property type As TypeCodes
        Public Property body As Byte()

        Public Shared Function Read(reader As BinaryReader, i As Long) As BlockReader
            Call reader.BaseStream.Seek(i, SeekOrigin.Begin)


        End Function

    End Class
End Namespace
