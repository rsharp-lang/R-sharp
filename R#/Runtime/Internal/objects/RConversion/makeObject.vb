Imports System.Reflection
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
            Dim obj As Object = Activator.CreateInstance(type)
            Dim val As Object

            For Each [property] As PropertyInfo In type _
                .GetProperties(PublicProperty) _
                .Where(Function(pi)
                           Return pi.CanWrite AndAlso pi.GetIndexParameters.IsNullOrEmpty
                       End Function)

                If propertyVals.hasName([property].Name) Then
                    val = propertyVals.getByName([property].Name)
                    val = RCType.CTypeDynamic(val, [property].PropertyType, env)

                    [property].SetValue(obj, val)
                End If
            Next

            Return obj
        End Function
    End Module
End Namespace