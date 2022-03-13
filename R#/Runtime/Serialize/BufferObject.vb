#Region "Microsoft.VisualBasic::9f5c555612e73b856cec924d18488009, R#\Runtime\Serialize\BufferObject.vb"

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

    '     Class BufferObject
    ' 
    '         Function: SubStream
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Serialization

Namespace Runtime.Serialize

    Public MustInherit Class BufferObject : Inherits RawStream

        Public MustOverride ReadOnly Property code As BufferObjects
        Public MustOverride Function getValue() As Object

        Public Shared Function SubStream(stream As Stream, from As Long, length As Integer) As MemoryStream
            Dim buffer As Byte() = New Byte(length - 1) {}

            stream.Seek(from, SeekOrigin.Begin)
            stream.Read(buffer, Scan0, length)

            Return New MemoryStream(buffer)
        End Function
    End Class
End Namespace
