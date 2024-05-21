#Region "Microsoft.VisualBasic::65a92a11094f77f72ba53f8a754170ac, R#\Runtime\Serialize\bufferObjects\bitmapBuffer.vb"

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
    '    Code Lines: 46 (68.66%)
    ' Comment Lines: 7 (10.45%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (20.90%)
    '     File Size: 1.82 KB


    '     Class bitmapBuffer
    ' 
    '         Properties: bitmap, code
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: getGZip, getValue
    ' 
    '         Sub: loadBuffer, Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.Net.Http

Namespace Runtime.Serialize

    ''' <summary>
    ''' handler for gdi+ image data.
    ''' </summary>
    Public Class bitmapBuffer : Inherits BufferObject

        ''' <summary>
        ''' the gdi+ raster image
        ''' </summary>
        ''' <returns></returns>
        Public Property bitmap As Image

        Public Overrides ReadOnly Property code As BufferObjects
            Get
                Return BufferObjects.bitmap
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(buffer As Stream)
            Call MyBase.New(buffer)
        End Sub

        Sub New(bytes As Byte())
            Using ms As New MemoryStream(bytes)
                Call loadBuffer(ms)
            End Using
        End Sub

        Public Overrides Sub Serialize(buffer As Stream)
            Dim data As Byte() = getGZip()

            Call buffer.Write(data, Scan0, data.Length)
            Call buffer.Flush()

            Erase data
        End Sub

        Private Function getGZip() As Byte()
            Using buffer As New MemoryStream
                Call bitmap.Save(buffer, Imaging.ImageFormat.Png)
                Call buffer.Flush()

                Return buffer.GZipStream.ToArray
            End Using
        End Function

        Public Overrides Function getValue() As Object
            Return bitmap
        End Function

        Protected Overrides Sub loadBuffer(buffer As Stream)
            If buffer.CheckGZipMagic Then
                bitmap = Image.FromStream(buffer.UnGzipStream)
            Else
                bitmap = Image.FromStream(buffer)
            End If
        End Sub
    End Class
End Namespace
