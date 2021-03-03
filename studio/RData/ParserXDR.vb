Imports Microsoft.VisualBasic.Data.IO
''' <summary>
''' Parser used when the integers and doubles are in XDR format.
''' </summary>
Public Class ParserXDR : Inherits Reader

    ReadOnly data As BinaryDataReader
    ReadOnly position As Integer
    ReadOnly xdr_parser

    Sub New(data As BinaryDataReader, Optional position As Integer = 0)
        Me.data = data
        Me.position = position
    End Sub

    Public Overrides Function parse_int() As Integer
        Throw New NotImplementedException()
    End Function

    Public Overrides Function parse_double() As Double
        Throw New NotImplementedException()
    End Function

    Public Overrides Function parse_string(length As Integer) As Byte()
        Throw New NotImplementedException()
    End Function
End Class
