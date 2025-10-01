#Region "Microsoft.VisualBasic::3e0084a9db71bfc778cf5bf6a7096ce9, R#\Runtime\Serialize\BufferObject.vb"

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

    '   Total Lines: 67
    '    Code Lines: 44 (65.67%)
    ' Comment Lines: 7 (10.45%)
    '    - Xml Docs: 71.43%
    ' 
    '   Blank Lines: 16 (23.88%)
    '     File Size: 1.93 KB


    '     Class NullObject
    ' 
    '         Properties: [nothing], code
    ' 
    '         Function: getValue
    ' 
    '         Sub: loadBuffer, Serialize
    ' 
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

    Public Class NullObject : Inherits BufferObject

        Public Overrides ReadOnly Property code As BufferObjects
            Get
                Return BufferObjects.vector
            End Get
        End Property

        Public Shared ReadOnly Property [nothing] As BufferObject
            Get
                Return New NullObject
            End Get
        End Property

        Public Overrides Sub Serialize(buffer As Stream)
            ' do nothing/no value
        End Sub

        Protected Overrides Sub loadBuffer(stream As Stream)
            ' do nothing/no value
        End Sub

        ''' <summary>
        ''' returns nothing, due to the reason of NULL means
        ''' no value
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function getValue() As Object
            Return Nothing
        End Function
    End Class

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

        <CodeAnalysis.SuppressMessage("Reliability", "CA2022:Avoid inexact read with 'Stream.Read'", Justification:="<Pending>")>
        Public Shared Function SubStream(stream As Stream, from As Long, length As Integer) As MemoryStream
            Dim buffer As Byte() = New Byte(length - 1) {}

            stream.Seek(from, SeekOrigin.Begin)
            stream.Read(buffer, Scan0, length)

            Return New MemoryStream(buffer)
        End Function
    End Class
End Namespace
