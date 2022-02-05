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
