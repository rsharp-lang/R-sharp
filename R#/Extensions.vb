Imports System.Runtime.CompilerServices

Module Extensions

    <Extension> Public Function GetRTypeCode(type$) As TypeCodes
        If type.StringEmpty Then
            Return TypeCodes.generic
        End If

        Return [Enum].Parse(GetType(TypeCodes), type.ToLower)
    End Function
End Module
