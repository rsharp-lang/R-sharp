Namespace Runtime.Interop

    ''' <summary>
    ''' VB.NET enum type wrapper in R#
    ''' </summary>
    Public Class REnum

        Public Property raw As Type

        ReadOnly namedValues As New Dictionary(Of String, Object)
        ReadOnly intValues As New Dictionary(Of String, Object)

        Sub New(type As Type)
            raw = type

            ' parsing enum type values for 
            ' named values and 
            ' int values
            Call doEnumParser()
        End Sub

        Private Sub doEnumParser()
            Enums(Of ()
        End Sub

        Public Function GetByName(name As String) As Object
            Return namedValues.TryGetValue(name.ToLower)
        End Function

        Public Function getByIntVal(int As Long) As Object

        End Function
    End Class
End Namespace