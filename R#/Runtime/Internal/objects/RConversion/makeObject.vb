Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Runtime.Internal.Object.Converts

    Module makeObject

        Public Function isObjectConversion(type As Type, val As Object) As Boolean
            If Not TypeOf val Is list Then
                Return False
            Else
                ' only works for the type which have non-parameter constructor
                If (Not DataFramework.IsPrimitive(type)) Then
                    Return type.GetConstructors.Any(Function(cor) cor.GetParameters.IsNullOrEmpty)
                Else
                    Return False
                End If
            End If
        End Function

        Public Function createObject(type As Type, propertyVals As list, env As Environment) As Object

        End Function
    End Module
End Namespace