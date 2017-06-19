Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

''' <summary>
''' 某一个closure之中的变量环境
''' </summary>
Public Class Environment

    Public ReadOnly Property Variables As Dictionary(Of Variable)
    Public ReadOnly Property Parent As Environment

    Sub New(parent As Environment, parameters As NamedValue(Of PrimitiveExpression)())
        Me.Parent = parent
        Me.Variables = parameters _
            .Select(Function(expression)
                        Dim expr As PrimitiveExpression = expression.Value
                        Return New Variable With {
                            .Name = expression.Name,
                            .Value = expr.Evaluate(envir:=Me)
                        }
                    End Function).ToDictionary
    End Sub

    ''' <summary>
    ''' Add new variable into current stack environment.
    ''' </summary>
    ''' <param name="name$"></param>
    ''' <param name="value"></param>
    ''' <returns></returns>
    Public Function Push(name$, value As Object) As Integer

    End Function
End Class
