#Region "Microsoft.VisualBasic::8108aea9af4452f5e6622a777f0a115c, R#\Runtime\Serialize\bufferObjects\textBuffer.vb"

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
    '         Properties: text
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Text

Namespace Runtime.Serialize

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

        Public Overrides Function Serialize() As Byte()
            Return Encodings.UTF8.CodePage.GetBytes(text)
        End Function
    End Class
End Namespace
