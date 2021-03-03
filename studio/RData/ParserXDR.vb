Imports Microsoft.VisualBasic.Data.IO

''' <summary>
''' Parser used when the integers and doubles are in XDR format.
''' </summary>
Public Class ParserXDR : Inherits Reader

    ReadOnly data As BinaryDataReader
    ReadOnly xdr_parser As Xdr.Unpacker

    Dim position As Integer

    Sub New(data As BinaryDataReader, Optional position As Integer = 0)
        Me.data = data
        Me.position = position
        Me.xdr_parser = New Xdr.Unpacker(data)
    End Sub

    Public Overrides Function parse_int() As Integer
        Call xdr_parser.set_position(position)
        Dim result = xdr_parser.unpack_int()
        position = xdr_parser.get_position()

        Return result
    End Function

    Public Overrides Function parse_double() As Double
        xdr_parser.set_position(position)
        Dim result = xdr_parser.unpack_double()
        position = xdr_parser.get_position()

        Return result
    End Function

    Public Overrides Function parse_string(length As Integer) As Byte()
        Dim result = data.ReadBytes(length)
        position += length
        Return result
    End Function
End Class
