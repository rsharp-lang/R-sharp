#Region "Microsoft.VisualBasic::19a4c2bd7f409d5a1c246371174b4b0c, studio\RData\Parser.vb"

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

    '   Total Lines: 161
    '    Code Lines: 110 (68.32%)
    ' Comment Lines: 28 (17.39%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 23 (14.29%)
    '     File Size: 5.26 KB


    ' Module Parser
    ' 
    '     Function: bits, bytes, decode, file_type, is_special_r_object_type
    '               parse_r_object_info, rdata_format
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser
Imports SMRUCC.Rsharp.RDataSet.Flags
Imports SMRUCC.Rsharp.RDataSet.Struct

''' <summary>
''' Parser interface for a R file.
''' </summary>
Module Parser

    Public ReadOnly NA_INT As Integer = -1 * Math.Pow(2, 31)
    Public Const NA_STRING As Integer = -1

    ''' <summary>
    ''' This is a special R constant value
    ''' </summary>
    Public Const NA_REAL As Long = &H7FF00000000007A2

    Friend ReadOnly magic_dict As New Dictionary(Of FileTypes, Byte()) From {
        {FileTypes.bzip2, bytes("\x42\x5a\x68")},
        {FileTypes.gzip, bytes("\x1f\x8b")},
        {FileTypes.xz, bytes("\xFD7zXZ\x00")},
        {FileTypes.rdata_binary_v2, bytes("RDX2\n")},
        {FileTypes.rdata_binary_v3, bytes("RDX3\n")}
    }

    Friend ReadOnly format_dict As New Dictionary(Of RdataFormats, Byte()) From {
        {RdataFormats.XDR, bytes("X\n")},
        {RdataFormats.ASCII, bytes("A\n")},
        {RdataFormats.binary, bytes("B\n")}
    }

    Private Function bytes(binaryStr As String) As Byte()
        Dim bits As New List(Of Byte)
        Dim bin As Char() = New Char(2 - 1) {}
        Dim i As i32 = Scan0
        Dim chars As CharPtr = binaryStr

        Do While Not chars.EndRead
            Dim c As Char = ++chars

            If c = "\" AndAlso chars.Current = "x" Then
                c = ++chars

                bin(0) = ++chars
                bin(1) = ++chars

                bits += CByte(i32.GetHexInteger(bin(0) & bin(1)))
            ElseIf c = "\" AndAlso chars.Current = "n" Then
                c = ++chars
                bits += CByte(ASCII.Byte.LF)
            Else
                bits += CByte(Asc(c))
            End If
        Loop

        Return bits.ToArray
    End Function

    ''' <summary>
    ''' Returns the type of the file.
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    Public Function file_type(file As BinaryDataReader) As FileTypes
        For Each item In magic_dict
            If item.Value.SequenceEqual(file.ReadBytes(item.Value.Length)) Then
                Return item.Key
            Else
                file.Seek(-item.Value.Length, SeekOrigin.Current)
            End If
        Next

        Return Nothing
    End Function

    Public Function rdata_format(file As BinaryDataReader) As RdataFormats
        For Each item In format_dict
            If item.Value.SequenceEqual(file.ReadBytes(item.Value.Length)) Then
                Return item.Key
            Else
                file.Seek(-item.Value.Length, SeekOrigin.Current)
            End If
        Next

        Return Nothing
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Friend Function decode(bytes As Byte(), encoding As Encodings) As String
        Return encoding.CodePage.GetString(bytes)
    End Function

    ''' <summary>
    ''' Parse the internal information of an object.
    ''' </summary>
    ''' <param name="info_int"></param>
    ''' <returns></returns>
    Public Function parse_r_object_info(info_int As Integer) As RObjectInfo
        Dim type_exp As RObjectType = bits(info_int, 0, 8)
        Dim reference = 0
        Dim object_flag As Boolean
        Dim attributes As Boolean
        Dim tag As Boolean
        Dim gp As Integer

        If is_special_r_object_type(type_exp) Then
            object_flag = False
            attributes = False
            tag = False
            gp = 0
        Else
            object_flag = CBool(bits(info_int, 8, 9))
            attributes = CBool(bits(info_int, 9, 10))
            tag = CBool(bits(info_int, 10, 11))
            gp = bits(info_int, 12, 28)
        End If

        If type_exp = RObjectType.REF Then
            reference = bits(info_int, 8, 32)
        End If

        Return New RObjectInfo With {
            .type = type_exp,
            .[object] = object_flag,
            .attributes = attributes,
            .tag = tag,
            .gp = gp,
            .reference = reference
        }
    End Function

    ''' <summary>
    ''' Check if a R type has a different serialization than the usual one.
    ''' </summary>
    ''' <param name="r_object_type"></param>
    ''' <returns></returns>
    Public Function is_special_r_object_type(r_object_type As RObjectType) As Boolean
        Return r_object_type = RObjectType.NILVALUE OrElse r_object_type = RObjectType.REF
    End Function

    ''' <summary>
    ''' Read bits [start, stop) of an integer.
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="start"></param>
    ''' <param name="[stop]"></param>
    ''' <returns></returns>
    Public Function bits(data As Integer, start As Integer, [stop] As Integer) As Integer
        Dim count = [stop] - start
        Dim mask = ((1 << count) - 1) << start
        Dim bitvalue = data And mask

        Return bitvalue >> start
    End Function
End Module
