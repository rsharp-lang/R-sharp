Imports System.Reflection
Imports Microsoft.VisualBasic.Linq

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
            Dim values As [Enum]() = raw _
                .GetEnumValues _
                .AsObjectEnumerator _
                .Select(Function(flag) DirectCast(flag, [Enum])) _
                .ToArray
            Dim members As Dictionary(Of String, FieldInfo)

            For Each flag As [Enum] In values
                namedValues.Add(flag.ToString.ToLower, flag)

            Next
        End Sub

        Public Function GetByName(name As String) As Object
            Return namedValues.TryGetValue(name.ToLower)
        End Function

        Public Function getByIntVal(int As Long) As Object

        End Function
    End Class
End Namespace