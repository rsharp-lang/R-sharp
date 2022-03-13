#Region "Microsoft.VisualBasic::064137c6b68569cb6df56c18362f4f52, snowFall\Context\Serialization.vb"

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

' Module Serialization
' 
'     Function: GetBytes
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Data.IO
Imports SMRUCC.Rsharp.Runtime.Components

''' <summary>
''' R# data object serializer
''' </summary>
Public Module Serialization

    Sub New()
        Call Global.Parallel.RegisterDiagnoseBuffer()
    End Sub

    ''' <summary>
    ''' serialize R# object to byte buffer
    ''' </summary>
    ''' <param name="R"></param>
    ''' <returns></returns>
    Public Function GetBuffer(R As Object) As Byte()

    End Function

    ''' <summary>
    ''' parse R# object from byte buffer
    ''' </summary>
    ''' <param name="buffer"></param>
    ''' <returns></returns>
    Public Function ParseBuffer(buffer As Byte()) As Object

    End Function

    ''' <summary>
    ''' serialize R# symbol to byte buffer
    ''' </summary>
    ''' <param name="symbol"></param>
    ''' <returns></returns>
    Public Function GetBytes(symbol As Symbol) As Byte()
        Using buffer As New MemoryStream, writer As New BinaryDataWriter(buffer)
            Call writer.Write(symbol.name, BinaryStringFormat.ZeroTerminated)
            Call writer.Write(symbol.readonly)
            Call writer.Write(symbol.constraint)

            Dim stackTrace As Byte()
            Dim value As Byte() = GetBuffer(symbol.value)

            Call writer.Write(stackTrace.Length)
            Call writer.Write(stackTrace)
            Call writer.Write(value.Length)
            Call writer.Write(value)
            Call writer.Flush()
            Call writer.Seek(Scan0, SeekOrigin.Begin)

            Return buffer.ToArray
        End Using
    End Function

    ''' <summary>
    ''' parse R# symbol from a byte buffer
    ''' </summary>
    ''' <param name="buffer"></param>
    ''' <returns>
    ''' a symbol object that contains the stack trace 
    ''' information of the parent environment from the 
    ''' master node. decode at slave node.
    ''' </returns>
    Public Function GetValue(buffer As Stream) As Symbol
        Using reader As New BinaryDataReader(buffer)



        End Using
    End Function
End Module
