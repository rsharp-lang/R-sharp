Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Interop

    ''' <summary>
    ''' 表示当前的函数参数为一个 ``...`` 可以产生一个字典list对象值的参数列表
    ''' </summary>
    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class RListObjectArgumentAttribute : Inherits RInteropAttribute

        ''' <summary>
        ''' Safe get a collection of argument name and value tuple
        ''' </summary>
        ''' <param name="objects"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Shared Iterator Function getObjectList(<RListObjectArgument> objects As Object, envir As Environment) As IEnumerable(Of NamedValue(Of Object))
            Dim type As Type

            If objects Is Nothing Then
                Return
            Else
                type = objects.GetType
            End If

            If type Is GetType(List) Then
                objects = DirectCast(objects, List).slots
                type = GetType(Dictionary(Of String, Object))
            End If

            If type Is GetType(Dictionary(Of String, Object)) Then
                For Each item In DirectCast(objects, Dictionary(Of String, Object))
                    Yield New NamedValue(Of Object) With {
                        .Name = item.Key,
                        .Value = item.Value
                    }
                Next
            ElseIf type Is GetType(InvokeParameter()) Then
                For Each item As InvokeParameter In DirectCast(objects, InvokeParameter())
                    Yield New NamedValue(Of Object) With {
                        .Name = item.name,
                        .Value = item.Evaluate(envir)
                    }
                Next
            Else
                Throw New NotImplementedException
            End If
        End Function
    End Class
End Namespace