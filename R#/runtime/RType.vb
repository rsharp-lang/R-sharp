

''' <summary>
''' Type proxy for <see cref="TypeCodes.list"/> or system primitives
''' </summary>
Public Class RType

    Public ReadOnly Property TypeCode As TypeCodes = TypeCodes.list
    Public ReadOnly Property FullName As String

    ''' <summary>
    ''' ``operator me``
    ''' </summary>
    ''' <param name="operator$"></param>
    ''' <returns></returns>
    Public Function GetUnaryOperator(operator$) As Func(Of Object, Object)

    End Function

    ''' <summary>
    ''' ``other operator me``
    ''' </summary>
    ''' <param name="operator$"></param>
    ''' <param name="a"></param>
    ''' <returns></returns>
    Public Function GetBinaryOperator1(operator$, a As Type) As Func(Of Object, Object, Object)

    End Function

    ''' <summary>
    ''' ``me operator other``
    ''' </summary>
    ''' <param name="operator$"></param>
    ''' <param name="b"></param>
    ''' <returns></returns>
    Public Function GetBinaryOperator2(operator$, b As Type) As Func(Of Object, Object, Object)

    End Function

    ''' <summary>
    ''' Imports the .NET type
    ''' </summary>
    ''' <param name="dotnet"></param>
    ''' <returns></returns>
    Public Shared Function [Imports](dotnet As Type) As RType
        Dim operators = dotnet.GetMethods(publicshared)

        Return New RType With {
            ._TypeCode = dotnet.GetRTypeCode,
            ._FullName = dotnet.FullName
        }
    End Function
End Class
