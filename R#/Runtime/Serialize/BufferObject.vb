#Region "Microsoft.VisualBasic::773c3c0b62f4bd8c0ba97ce593b286c1, R-sharp\R#\Runtime\Serialize\BufferObject.vb"

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

    '   Total Lines: 35
    '    Code Lines: 25
    ' Comment Lines: 0
    '   Blank Lines: 10
    '     File Size: 1016 B


    '     Class BufferObject
    ' 
    '         Constructor: (+3 Overloads) Sub New
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

        Sub New()
        End Sub

        Sub New(buffer As Stream)
            Call loadBuffer(buffer)
        End Sub

        Sub New(bytes As Byte())
            Using ms As New MemoryStream(bytes)
                Call loadBuffer(ms)
            End Using
        End Sub

        Protected MustOverride Sub loadBuffer(stream As Stream)

        Public Shared Function SubStream(stream As Stream, from As Long, length As Integer) As MemoryStream
            Dim buffer As Byte() = New Byte(length - 1) {}

            stream.Seek(from, SeekOrigin.Begin)
            stream.Read(buffer, Scan0, length)

            Return New MemoryStream(buffer)
        End Function
    End Class
End Namespace
