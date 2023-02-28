#Region "Microsoft.VisualBasic::1d1bde7a3e8555614610e32830dfa74f, D:/GCModeller/src/R-sharp/R#//Runtime/Serialize/bufferObjects/vectorBuffer.vb"

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

'   Total Lines: 226
'    Code Lines: 169
' Comment Lines: 10
'   Blank Lines: 47
'     File Size: 8.10 KB


'     Class vectorBuffer
' 
'         Properties: code, names, type, underlyingType, unit
'                     vector
' 
'         Constructor: (+3 Overloads) Sub New
' 
'         Function: CreateBuffer, getValue, getVector
' 
'         Sub: loadBuffer, Serialize
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Serialize

    ''' <summary>
    ''' serialize handler for ``R#`` vector
    ''' </summary>
    Public Class vectorBuffer : Inherits BufferObject

        ''' <summary>
        ''' the full name of the <see cref="Global.System.Type"/> for create <see cref="RType"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property type As String
        Public Property vector As Array
        Public Property names As String()
        Public Property unit As String

        Public ReadOnly Property underlyingType As Type
            Get
                Return Global.System.Type.GetType(type)
            End Get
        End Property

        Public Overrides ReadOnly Property code As BufferObjects
            Get
                Return BufferObjects.vector
            End Get
        End Property

        Dim env As Environment

        Private Sub New()
        End Sub

        Sub New(env As Environment)
            Me.env = env
        End Sub

        Sub New(buffer As Stream)
            Call MyBase.New(buffer)
        End Sub

        Public Function getVector() As vector
            Dim rtype As RType = RType.GetRSharpType(underlyingType)
            Dim unit As New unit With {.name = Me.unit}
            Dim vec As New vector(names, vector, rtype, unit)

            Return vec
        End Function

        Public Shared Function CreateBuffer(vector As vector, env As Environment) As vectorBuffer
            Dim buffer As New vectorBuffer With {
                .names = If(vector.getNames, {}),
                .type = vector.elementType.raw.FullName,
                .unit = If(vector.unit?.name, ""),
                .env = env
            }
            Dim generic = REnv.TryCastGenericArray(vector.data, env)

            If TypeOf generic Is Message Then
                Return generic
            Else
                buffer.vector = generic
            End If

            buffer.type = buffer.safeGetType.FullName

            Return buffer
        End Function

        ''' <summary>
        ''' get the array element type
        ''' </summary>
        ''' <returns></returns>
        Private Function safeGetType() As Type
            Static NULL As Index(Of String) = {"System.Object", "System.Void"}

            If type.StringEmpty OrElse type Like NULL Then
                vector = REnv.UnsafeTryCastGenericArray(vector)

                If vector.AllNothing OrElse vector.Length = 0 Then
                    vector = New String(vector.Length - 1) {}
                    type = GetType(String).FullName
                Else
                    type = vector _
                       .GetType _
                       .GetElementType _
                       .FullName
                End If
            End If

            Dim typeinfo As Type = System.Type.GetType(type)

            ' not working well with the clr object???
            If typeinfo Is Nothing Then
                Throw New InvalidCastException($"error type: '{type}'!")
            Else
                Return typeinfo
            End If
        End Function

        Public Overrides Sub Serialize(buffer As Stream)
            Dim type As Type = safeGetType()
            Dim bytes As Byte()
            Dim text As Encoding = Encodings.UTF8.CodePage
            Dim raw As Byte()
            Dim sizeof As Byte()

            If names Is Nothing Then
                names = New String() {}
            End If
            If vector Is Nothing Then
                vector = New String() {}
            End If

            buffer.Write(BitConverter.GetBytes(names.Length), Scan0, 4)
            buffer.Write(BitConverter.GetBytes(vector.Length), Scan0, 4)

            bytes = text.GetBytes(type.FullName)
            buffer.Write(BitConverter.GetBytes(bytes.Length), Scan0, 4)
            buffer.Write(bytes, Scan0, bytes.Length)

            bytes = text.GetBytes(unit)
            buffer.Write(BitConverter.GetBytes(bytes.Length), Scan0, 4)
            buffer.Write(bytes, Scan0, bytes.Length)

            raw = RawStream.GetBytes(names)
            sizeof = BitConverter.GetBytes(raw.Length)

            buffer.Write(sizeof, Scan0, 4)
            buffer.Write(raw, Scan0, raw.Length)

            If TypeOf vector Is Object() Then
                vector = TryCastGenericArray(vector, env)
            End If

            If vector.GetType.GetElementType Is GetType(list) Then
                Dim rawBlocks As New List(Of Byte)

                rawBlocks.AddRange(BitConverter.GetBytes(vector.Length))

                For Each list As list In DirectCast(vector, list())
                    Dim temp As New listBuffer(list, env)
                    raw = temp.Serialize
                    rawBlocks.AddRange(BitConverter.GetBytes(raw.Length))
                    rawBlocks.AddRange(raw)
                Next

                raw = rawBlocks.ToArray
            ElseIf vector.AsObjectEnumerator.All(Function(xi) xi Is Nothing) Then
                ' contains no data due to the reason of all element
                ' in vector is nothing
                raw = {}
            Else
                raw = RawStream.GetBytes(vector)
            End If

            sizeof = BitConverter.GetBytes(raw.Length)

            buffer.Write(sizeof, Scan0, 4)
            buffer.Write(raw, Scan0, raw.Length)

            buffer.Flush()
        End Sub

        Public Overrides Function getValue() As Object
            Return getVector()
        End Function

        Protected Overrides Sub loadBuffer(bytes As Stream)
            Dim raw As Byte() = New Byte(2 * Marshal.SizeOf(GetType(Integer)) - 1) {}
            Dim text As Encoding = Encodings.UTF8.CodePage

            bytes.Read(raw, Scan0, raw.Length)

            Dim name_size As Integer = BitConverter.ToInt32(raw, Scan0)
            Dim vector_size As Integer = BitConverter.ToInt32(raw, Marshal.SizeOf(GetType(Integer)))
            Dim int_size As Byte() = New Byte(3) {}
            Dim sizeof As Integer

            bytes.Read(int_size, Scan0, int_size.Length)
            sizeof = BitConverter.ToInt32(int_size, Scan0)
            raw = New Byte(sizeof - 1) {}
            bytes.Read(raw, Scan0, raw.Length)

            Dim type As Type = Type.GetType(text.GetString(raw))

            bytes.Read(int_size, Scan0, int_size.Length)
            sizeof = BitConverter.ToInt32(int_size, Scan0)
            raw = New Byte(sizeof - 1) {}
            bytes.Read(raw, Scan0, raw.Length)

            Dim unit As String = text.GetString(raw)

            bytes.Read(int_size, Scan0, int_size.Length)
            sizeof = BitConverter.ToInt32(int_size, Scan0)
            raw = New Byte(sizeof - 1) {}
            bytes.Read(raw, Scan0, raw.Length)

            Dim names As String()

            Using ms As New MemoryStream(raw)
                names = RawStream.GetData(ms, TypeCode.String)
            End Using

            raw = New Byte(3) {}
            bytes.Read(raw, Scan0, raw.Length)
            sizeof = BitConverter.ToInt32(raw, Scan0)
            raw = New Byte(sizeof - 1) {}
            bytes.Read(raw, Scan0, raw.Length)

            Using ms As New MemoryStream(raw)
                Dim vector As Array

                If type Is GetType(list) Then
                    Dim list As New List(Of list)
                    Dim nsize As Integer
                    Dim temp As listBuffer

                    raw = New Byte(3) {}
                    ms.Read(raw, Scan0, raw.Length)
                    nsize = BitConverter.ToInt32(raw, Scan0)

                    For i As Integer = 0 To nsize - 1
                        raw = New Byte(3) {}
                        ms.Read(raw, Scan0, raw.Length)
                        raw = New Byte(BitConverter.ToInt32(raw, Scan0) - 1) {}
                        ms.Read(raw, Scan0, raw.Length)
                        temp = New listBuffer(New MemoryStream(raw))
                        list.Add(temp.getValue)
                    Next

                    vector = list.ToArray
                ElseIf ms.Length = 0 AndAlso vector_size > 0 AndAlso type Is GetType(Object) Then
                    ' all vector content is null
                    vector = Array.CreateInstance(type, vector_size)
                Else
                    vector = RawStream.GetData(ms, type.PrimitiveTypeCode(meltVector:=True))
                End If

                Me.type = type.FullName
                Me.names = names
                Me.unit = unit
                Me.vector = vector
            End Using
        End Sub
    End Class
End Namespace
