Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.IO.Xdr

''' <summary>
''' Parser used when the integers and doubles are in XDR format.
''' </summary>
Public Class ParserXDR : Inherits Reader

    ReadOnly data As BinaryDataReader
    ReadOnly xdr_parser As Unpacker

    Sub New(data As BinaryDataReader, Optional position As Integer = 0)
        Me.data = data
        Me.data.Position = position
        Me.xdr_parser = New Unpacker(data)
    End Sub

    Public Overrides Function parse_int() As Integer
        Dim result = xdr_parser.unpack_int()
        Return result
    End Function

    Public Overrides Function parse_double() As Double
        Dim result = xdr_parser.unpack_double()
        Return result
    End Function

    Public Overrides Function parse_string(length As Integer) As Byte()
        Dim result As Byte() = data.ReadBytes(length)
        Return result
    End Function
End Class
