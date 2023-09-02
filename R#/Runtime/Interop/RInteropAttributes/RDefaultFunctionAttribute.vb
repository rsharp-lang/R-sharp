Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Runtime.Interop

    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class RDefaultFunctionAttribute : Inherits Attribute

        Public Overrides Function ToString() As String
            Return $"f()"
        End Function

        Public Shared Function GetDefaultFunction(symbol As String, obj As Object) As RMethodInfo
            Dim funcs = obj.GetType.GetMethods(DataFramework.PublicProperty)
            Dim getFlag = funcs _
                .Where(Function(f) f.GetCustomAttribute(Of RDefaultFunctionAttribute) IsNot Nothing) _
                .FirstOrDefault

            If getFlag Is Nothing Then
                Return Nothing
            Else
                Return New RMethodInfo(symbol, getFlag, obj)
            End If
        End Function

    End Class

    ''' <summary>
    ''' A template for check of the <see cref="RDefaultExpressionAttribute"/>
    ''' </summary>
    Public MustInherit Class RDefaultFunction
    End Class
End Namespace