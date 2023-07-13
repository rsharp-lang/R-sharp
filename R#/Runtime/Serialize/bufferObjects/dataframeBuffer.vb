#Region "Microsoft.VisualBasic::5b8e34f53eda161775cb0c77e3d2e602, G:/GCModeller/src/R-sharp/R#//Runtime/Serialize/bufferObjects/dataframeBuffer.vb"

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

    '   Total Lines: 131
    '    Code Lines: 108
    ' Comment Lines: 0
    '   Blank Lines: 23
    '     File Size: 4.70 KB


    '     Class dataframeBuffer
    ' 
    '         Properties: code, dataframe, tsv
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: getFrame, getValue
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
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Serialize

    Public Class dataframeBuffer : Inherits BufferObject

        Public Overrides ReadOnly Property code As BufferObjects
            Get
                Return BufferObjects.dataframe
            End Get
        End Property

        Public Property dataframe As dataframe
        Public Property tsv As Boolean

        Dim env As Environment

        Sub New(data As dataframe, env As Environment)
            Me.env = env
            Me.dataframe = data
        End Sub

        Sub New(env As Environment)
            Me.env = env
        End Sub

        Sub New(data As Stream)
            Call MyBase.New(data)
        End Sub

        Public Overrides Sub Serialize(buffer As Stream)
            Dim colnames As String() = dataframe.colnames
            Dim rownames As String() = dataframe.getRowNames
            Dim text As Encoding = Encodings.UTF8.CodePage
            Dim bytes As Byte()
            Dim tmpstr As String

            tmpstr = colnames.GetJson
            bytes = text.GetBytes(tmpstr)
            buffer.Write(BitConverter.GetBytes(bytes.Length), Scan0, 4)
            buffer.Write(bytes, Scan0, bytes.Length)

            tmpstr = rownames.GetJson
            bytes = text.GetBytes(tmpstr)
            buffer.Write(BitConverter.GetBytes(bytes.Length), Scan0, 4)
            buffer.Write(bytes, Scan0, bytes.Length)
            buffer.Write(CByte(If(tsv, 1, 0)))

            For Each name As String In colnames
                Dim vec As New vector With {
                    .data = dataframe.getColumnVector(name)
                }

                Using tmp As New MemoryStream
                    Dim result As Object = vectorBuffer.CreateBuffer(vec, env)
                    Dim vec2 As vectorBuffer

                    If TypeOf result Is Message Then
                        Throw DirectCast(result, Message).ToException
                    Else
                        vec2 = DirectCast(result, vectorBuffer)
                        vec2.Serialize(tmp)
                        tmp.Flush()
                        tmp.Seek(Scan0, SeekOrigin.Begin)
                        buffer.Write(BitConverter.GetBytes(tmp.Length), Scan0, 4)
                        buffer.Write(tmp.ToArray, Scan0, tmp.Length)
                    End If
                End Using
            Next
        End Sub

        Protected Overrides Sub loadBuffer(stream As Stream)
            Dim colnames As String()
            Dim rownames As String()
            Dim text As Encoding = Encodings.UTF8.CodePage
            Dim bytes As Byte() = New Byte(4 - 1) {}
            Dim tmpstr As String
            Dim nsize As Integer
            Dim fields As New Dictionary(Of String, Array)

            stream.Read(bytes, Scan0, bytes.Length)
            nsize = BitConverter.ToInt32(bytes, Scan0)
            bytes = New Byte(nsize - 1) {}
            stream.Read(bytes, Scan0, nsize)
            tmpstr = text.GetString(bytes)
            colnames = tmpstr.LoadJSON(Of String())

            bytes = New Byte(4 - 1) {}
            stream.Read(bytes, Scan0, bytes.Length)
            nsize = BitConverter.ToInt32(bytes, Scan0)
            bytes = New Byte(nsize - 1) {}
            stream.Read(bytes, Scan0, nsize)
            tmpstr = text.GetString(bytes)
            rownames = tmpstr.LoadJSON(Of String())
            tsv = If(stream.ReadByte, True, False)

            For Each name As String In colnames
                bytes = New Byte(4 - 1) {}
                stream.Read(bytes, Scan0, bytes.Length)
                nsize = BitConverter.ToInt32(bytes, Scan0)
                bytes = New Byte(nsize - 1) {}
                stream.Read(bytes, Scan0, bytes.Length)

                Using tmp As New MemoryStream(bytes)
                    Dim vec As New vectorBuffer(tmp)
                    Dim vector As vector = vec.getVector

                    Call fields.Add(name, vector.data)
                End Using
            Next

            _dataframe = New dataframe With {
                .columns = fields,
                .rownames = rownames
            }
        End Sub

        Public Function getFrame() As dataframe
            Return dataframe
        End Function

        Public Overrides Function getValue() As Object
            Return getFrame()
        End Function
    End Class
End Namespace
