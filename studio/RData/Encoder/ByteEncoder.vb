#Region "Microsoft.VisualBasic::8cf359870f3e4f9cdcb4836e3f43b017, studio\RData\Encoder\ByteEncoder.vb"

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

    ' Module ByteEncoder
    ' 
    '     Function: EncodeInfoInt32
    ' 
    '     Sub: EncodeSymbol, intScalar, logicalScalar, realScalar, stringScalar
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.RDataSet.Flags
Imports SMRUCC.Rsharp.RDataSet.Struct

Module ByteEncoder

    ''' <summary>
    ''' encode flags
    ''' </summary>
    ''' <param name="robjinfo"></param>
    ''' <returns></returns>
    <Extension>
    Public Function EncodeInfoInt32(robjinfo As RObjectInfo) As Integer
        Dim bits As Integer = CInt(robjinfo.type)

        If robjinfo.object Then
            bits = bits Or ObjectBitMask.IS_OBJECT_BIT_MASK
        End If
        If robjinfo.attributes Then
            bits = bits Or ObjectBitMask.HAS_ATTR_BIT_MASK
        End If
        If robjinfo.tag Then
            bits = bits Or ObjectBitMask.HAS_TAG_BIT_MASK
        End If

        Call Console.WriteLine($"[{bits}] => {robjinfo.ToString}")

        Return bits
    End Function

    Public Sub EncodeSymbol(name As String, file As IByteWriter)
        Call Xdr.EncodeInt32(RObjectInfo.SYMSXP.EncodeInfoInt32, file)
        Call stringScalar(name, file)
    End Sub

    Public Sub stringScalar(str As String, file As IByteWriter)
        Dim type As Integer = RObjectType.CHAR Or (CharFlags.UTF8 << 12)

        If str Is Nothing Then
            type = RObjectType.CHAR

            Call Xdr.EncodeInt32(type, file)
            Call Xdr.EncodeInt32(NA_STRING, file)
        Else
            Static utf8 As Encoding = Encodings.UTF8WithoutBOM.CodePage

            Dim bytes As Byte() = utf8.GetBytes(str)

            Call Xdr.EncodeInt32(type, file)
            Call Xdr.EncodeInt32(bytes.Length, file)
            Call file.Write(bytes)
        End If
    End Sub

    Public Sub realScalar(real As Double, file As IByteWriter)
        If real.IsNaNImaginary Then
            real = NA_REAL
        End If

        Call Xdr.EncodeDouble(real, file)
    End Sub

    Public Sub intScalar(int As Long, file As IByteWriter)
        Call Xdr.EncodeInt64(int, file)
    End Sub

    Public Sub logicalScalar(bool As Boolean, file As IByteWriter)
        Call Xdr.EncodeInt32(If(bool = True, 1, 0), file)
    End Sub
End Module

