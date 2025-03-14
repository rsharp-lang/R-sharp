﻿#Region "Microsoft.VisualBasic::eac6d4ebc1228a22c1b336d4d9b9a356, R#\Runtime\Internal\internalInvokes\dev\bitView.vb"

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

    '   Total Lines: 92
    '    Code Lines: 66 (71.74%)
    ' Comment Lines: 7 (7.61%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 19 (20.65%)
    '     File Size: 3.31 KB


    '     Module bitView
    ' 
    '         Function: bitConvert, doubles, floats, int64, integers
    '                   magicNumber, shorts
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports ASCII = Microsoft.VisualBasic.Text.ASCII

Namespace Runtime.Internal.Invokes

    <Package("bitView")>
    Module bitView

        <ExportAPI("doubles")>
        Public Function doubles(buffer As MemoryStream) As Double()
            Return buffer.bitConvert(sizeof:=8, AddressOf BitConverter.ToDouble)
        End Function

        <ExportAPI("integers")>
        Public Function integers(buffer As MemoryStream) As Integer()
            Return buffer.bitConvert(sizeof:=4, AddressOf BitConverter.ToInt32)
        End Function

        <ExportAPI("floats")>
        Public Function floats(buffer As MemoryStream) As Single()
            Return buffer.bitConvert(sizeof:=4, AddressOf BitConverter.ToSingle)
        End Function

        <ExportAPI("int16s")>
        Public Function shorts(buffer As MemoryStream) As Short()
            Return buffer.bitConvert(sizeof:=2, AddressOf BitConverter.ToInt16)
        End Function

        <ExportAPI("int64s")>
        Public Function int64(buffer As MemoryStream) As Long()
            Return buffer.bitConvert(sizeof:=8, AddressOf BitConverter.ToInt64)
        End Function

        <Extension>
        Private Function bitConvert(Of T)(buffer As MemoryStream, sizeof As Integer, load As Func(Of Byte(), Integer, T)) As T()
            Return buffer.ToArray _
                .Split(sizeof) _
                .Where(Function(b) b.Length = sizeof) _
                .Select(Function(byts) load(byts, Scan0)) _
                .ToArray
        End Function

        ''' <summary>
        ''' try to get the magic number of the given binary file
        ''' </summary>
        ''' <param name="file"></param>
        ''' <param name="max_offset"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("magic_number")>
        <RApiReturn(TypeCodes.string)>
        Public Function magicNumber(<RRawVectorArgument> file As Object,
                                    Optional max_offset As Integer = 128,
                                    Optional env As Environment = Nothing) As Object

            Dim buf = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env)

            If buf Like GetType(Message) Then
                Return buf.TryCast(Of Message)
            End If

            Dim s As Stream = buf.TryCast(Of Stream)

            If max_offset > s.Length Then
                max_offset = s.Length
            End If

            Dim bytes As Byte() = New Byte(max_offset - 1) {}

            Call s.Read(bytes, Scan0, max_offset)
            Call s.Close()
            Call s.Dispose()

            bytes = bytes _
                .TakeWhile(Function(b)
                               Return b < 128 AndAlso Not ASCII.IsNonPrinting(b)
                           End Function) _
                .ToArray

            Dim str As String = Encoding.ASCII.GetString(bytes)

            Return str
        End Function

    End Module
End Namespace
