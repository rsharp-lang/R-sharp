Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO
Imports SMRUCC.Rsharp.RDataSet.Struct

Module ByteEncoder

    <Extension>
    Public Function EncodeInfoInt32(robjinfo As RObjectInfo) As Integer
        Dim bits As New BitSet(0)

        If is_special_r_object_type(robjinfo.type) Then
            bits(8) = robjinfo.object
            bits(9) = robjinfo.attributes
            bits(10) = robjinfo.tag
            bits.SetBits(CShort(robjinfo.gp), 12)
        End If

        Return bits.ToInteger
    End Function
End Module
