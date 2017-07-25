Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

''' <summary>
''' Type proxy for <see cref="TypeCodes.list"/> or system primitives
''' </summary>
Public Class RType : Implements IReadOnlyId

    Public ReadOnly Property TypeCode As TypeCodes = TypeCodes.list
    Public ReadOnly Property FullName As String

    Public ReadOnly Property Identity As String Implements IReadOnlyId.Identity
        Get
            Return Me.ToString.MD5
        End Get
    End Property

    Dim UnaryOperators As Dictionary(Of String, MethodInfo)
    Dim BinaryOperator1 As Dictionary(Of String, MethodInfo)
    Dim BinaryOperator2 As Dictionary(Of String, MethodInfo)

    Public Overrides Function ToString() As String
        Return $"[{TypeCode}] {FullName}"
    End Function

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
        Dim operators = dotnet _
            .GetMethods(PublicShared) _
            .Where(Function(m) m.Name.StartsWith("op_")) _
            .ToArray

        Return New RType With {
            ._TypeCode = dotnet.GetRTypeCode,
            ._FullName = dotnet.FullName
        }
    End Function
End Class
