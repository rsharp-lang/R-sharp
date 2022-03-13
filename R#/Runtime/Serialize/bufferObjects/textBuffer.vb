#Region "Microsoft.VisualBasic::372424aeb4d1c0d6c383382d7dc8f836, R#\Runtime\Serialize\bufferObjects\textBuffer.vb"

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

    '     Class textBuffer
    ' 
    '         Properties: code, text
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Text

Namespace Runtime.Serialize

    ''' <summary>
    ''' utf8 text data
    ''' </summary>
    Public Class textBuffer : Inherits BufferObject

        Public Property text As String

        Public Overrides ReadOnly Property code As BufferObjects
            Get
                Return BufferObjects.text
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(raw As Byte())
            text = Encodings.UTF8.CodePage.GetString(raw)
        End Sub

        Public Overrides Sub Serialize(buffer As Stream)
            Dim data As Byte() = Encodings.UTF8.CodePage.GetBytes(text)

            Call buffer.Write(data, Scan0, data.Length)
            Call buffer.Flush()

            Erase data
        End Sub

        Public Overrides Function getValue() As Object
            Return text
        End Function
    End Class
End Namespace
