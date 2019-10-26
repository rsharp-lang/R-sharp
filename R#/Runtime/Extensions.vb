Imports System.Runtime.CompilerServices

Namespace Runtime

    <HideModuleName> Module Extensions

        ''' <summary>
        ''' Get R type code from the type constraint expression value.
        ''' </summary>
        ''' <param name="type$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetRTypeCode(type As String) As TypeCodes
            If type.StringEmpty Then
                Return TypeCodes.generic
            Else
                Return [Enum].Parse(GetType(TypeCodes), type.ToLower)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsPrimitive(type As TypeCodes) As Boolean
            Return type = TypeCodes.boolean OrElse
                   type = TypeCodes.char OrElse
                   type = TypeCodes.double OrElse
                   type = TypeCodes.integer OrElse
                   type = TypeCodes.list OrElse
                   type = TypeCodes.string
        End Function

        ''' <summary>
        ''' DotNET type to R type code
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetRTypeCode(type As Type) As TypeCodes
            Select Case type
                Case GetType(String), GetType(String())
                    Return TypeCodes.string
                Case GetType(Integer), GetType(Integer()), GetType(Long()), GetType(Long)
                    Return TypeCodes.integer
                Case GetType(Double), GetType(Double())
                    Return TypeCodes.double
                Case GetType(Char), GetType(Char())
                    Return TypeCodes.char
                Case GetType(Boolean), GetType(Boolean())
                    Return TypeCodes.boolean
                Case GetType(Dictionary(Of String, Object)), GetType(Dictionary(Of String, Object)())
                    Return TypeCodes.list
                Case Else
                    Return TypeCodes.generic
            End Select
        End Function

        Public Function ClosureStackName(func$, script$, line%) As String
            Return $"<{script.FileName}#{line}::{func}()>"
        End Function
    End Module
End Namespace