#Region "Microsoft.VisualBasic::d7801329434d48be16b8039993ec60ff, G:/GCModeller/src/R-sharp/R#//Runtime/Serialize/bufferObjects/listBuffer.vb"

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

    '   Total Lines: 110
    '    Code Lines: 83
    ' Comment Lines: 4
    '   Blank Lines: 23
    '     File Size: 3.68 KB


    '     Class listBuffer
    ' 
    '         Properties: code, listData
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: getList, getValue
    ' 
    '         Sub: loadBuffer, Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Serialize

    Public Class listBuffer : Inherits BufferObject

        Public Overrides ReadOnly Property code As BufferObjects
            Get
                Return BufferObjects.list
            End Get
        End Property

        Public Property listData As list

        Dim env As Environment

        Sub New(env As Environment)
            Me.env = env
        End Sub

        Sub New(list As list, env As Environment)
            Me.env = env
            Me.listData = list
        End Sub

        Sub New(stream As Stream)
            Call MyBase.New(stream)
        End Sub

        Public Function getList() As list
            Return listData
        End Function

        Public Overrides Sub Serialize(buffer As Stream)
            Dim names As String() = listData.slotKeys
            Dim text As Encoding = Encodings.UTF8.CodePage
            Dim tmpstr As String = names.GetJson
            Dim bytes As Byte() = text.GetBytes(tmpstr)

            Call buffer.Write(BitConverter.GetBytes(bytes.Length), Scan0, 4)
            Call buffer.Write(bytes, Scan0, bytes.Length)

            For Each name As String In names
                Dim value As Object = listData.getByName(name)
                Dim bufferData As New Buffer With {
                    .data = BufferHandler.getBufferObject(value, env)
                }

                Using tmp As New MemoryStream
                    Call bufferData.Serialize(tmp)
                    Call tmp.Flush()
                    Call tmp.Seek(Scan0, SeekOrigin.Begin)

                    bytes = tmp.ToArray

                    Call buffer.Write(BitConverter.GetBytes(bytes.Length), Scan0, 4)
                    Call buffer.Write(bytes, Scan0, bytes.Length)
                End Using
            Next
        End Sub

        ''' <summary>
        ''' a wrapper to the <see cref="getList"/> function
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function getValue() As Object
            Return getList()
        End Function

        Protected Overrides Sub loadBuffer(stream As Stream)
            Dim bytes As Byte() = New Byte(4 - 1) {}
            Dim tmpstr As String
            Dim names As String()
            Dim nsize As Integer
            Dim text As Encoding = Encodings.UTF8.CodePage
            Dim list As New Dictionary(Of String, Object)

            stream.Read(bytes, Scan0, bytes.Length)
            nsize = BitConverter.ToInt32(bytes, Scan0)
            bytes = New Byte(nsize - 1) {}
            stream.Read(bytes, Scan0, bytes.Length)
            tmpstr = text.GetString(bytes)
            names = tmpstr.LoadJSON(Of String())

            For Each name As String In names
                Dim value As Object
                Dim buffer As Buffer

                bytes = New Byte(4 - 1) {}
                stream.Read(bytes, Scan0, bytes.Length)
                nsize = BitConverter.ToInt32(bytes, Scan0)
                bytes = New Byte(nsize - 1) {}
                stream.Read(bytes, Scan0, bytes.Length)

                Using tmp As New MemoryStream(bytes)
                    buffer = Buffer.ParseBuffer(tmp)
                    value = buffer.data.getValue
                End Using

                Call list.Add(name, value)
            Next

            listData = New list With {.slots = list}
        End Sub
    End Class
End Namespace
