Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime

    Public Module RTypeExtensions

        ''' <summary>
        ''' Converts the input string text to value <see cref="TypeCodes"/>
        ''' </summary>
        ReadOnly parseTypecode As Dictionary(Of String, TypeCodes) = Enums(Of TypeCodes) _
            .ToDictionary(Function(e) e.Description.ToLower,
                          Function(code)
                              Return code
                          End Function)

        ''' <summary>
        ''' Get R type code from the type constraint expression value.
        ''' </summary>
        ''' <param name="type$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetRTypeCode(type As String) As TypeCodes
            If type.StringEmpty Then
                Return TypeCodes.generic
            ElseIf parseTypecode.ContainsKey(type) Then
                Return parseTypecode(type)
            Else
                ' .NET type
                Return TypeCodes.ref
            End If
        End Function

        ''' <summary>
        ''' It is R# primitive type?
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsPrimitive(type As TypeCodes, Optional includeComplexList As Boolean = True) As Boolean
            Return type = TypeCodes.boolean OrElse
                   type = TypeCodes.double OrElse
                   type = TypeCodes.integer OrElse
                  (type = TypeCodes.list AndAlso includeComplexList) OrElse
                   type = TypeCodes.string
        End Function

        ''' <summary>
        ''' VB.NET type to R type code mapping
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
                    Return TypeCodes.string
                Case GetType(Boolean), GetType(Boolean())
                    Return TypeCodes.boolean
                Case GetType(Dictionary(Of String, Object)), GetType(Dictionary(Of String, Object)())
                    Return TypeCodes.list
                Case GetType(RMethodInfo), GetType(DeclareNewFunction) ', GetType(envir)
                    Return TypeCodes.closure
                Case Else
                    Return TypeCodes.generic
            End Select
        End Function

        ''' <summary>
        ''' Mapping R# <see cref="TypeCodes"/> to VB.NET type
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function [GetType](type As TypeCodes) As Type
            Select Case type
                Case TypeCodes.boolean : Return GetType(Boolean())
                Case TypeCodes.double : Return GetType(Double())
                Case TypeCodes.integer : Return GetType(Long())
                Case TypeCodes.list : Return GetType(Dictionary(Of String, Object))
                Case TypeCodes.string : Return GetType(String())
                Case TypeCodes.closure : Return GetType([Delegate])
                Case Else
                    Throw New InvalidCastException(type.Description)
            End Select
        End Function
    End Module
End Namespace