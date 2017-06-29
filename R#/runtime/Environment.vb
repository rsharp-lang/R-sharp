Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.Abstract

''' <summary>
''' 某一个closure之中的变量环境
''' </summary>
Public Class Environment

    Public ReadOnly Property Variables As Dictionary(Of Variable)
    ''' <summary>
    ''' 最顶层的closure环境的parent是空值来的
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Parent As Environment

    ''' <summary>
    ''' 当前的环境是否为最顶层的全局环境？
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IsGlobal As Boolean
        Get
            Return Parent Is Nothing
        End Get
    End Property

    ''' <summary>
    ''' 每一个closure可以看作为一个函数对象
    ''' 则parent的closure和他的child closure之间相互通信，最直接的方法就是参数传递
    ''' </summary>
    ''' <param name="parent"></param>
    ''' <param name="parameters">closure函数的参数传递列表</param>
    Sub New(parent As Environment, parameters As NamedValue(Of PrimitiveExpression)())
        Me.Parent = parent
        Me.Variables = parameters _
            .Select(Function(expression)
                        Dim expr As PrimitiveExpression = expression.Value
                        Return New Variable(TypeCodes.generic) With {
                            .Name = expression.Name,
                            .Value = expr.Evaluate(envir:=Me)
                        }
                    End Function).ToDictionary
    End Sub

    Public Function GetValue() As GetValue

    End Function

    Public Function Evaluate() As FunctionEvaluate

    End Function

    ''' <summary>
    ''' Add new variable into current stack environment.
    ''' 
    ''' ```
    ''' var x as type &amp;- &lt;expression>
    ''' ```
    ''' </summary>
    ''' <param name="name$"></param>
    ''' <param name="value"></param>
    ''' <param name="type">``R#``类型约束</param>
    ''' <returns></returns>
    Public Function Push(name$, value As Object, type$) As Integer
        If Variables.ContainsKey(name) Then
            Throw New Exception($"Variable ""{name}"" is already existed, can not declare it again!")
        End If

        With New Variable(type.GetRTypeCode) With {
            .Name = name,
            .Value = value
        }
            If Not .ConstraintValid Then
                Throw New Exception("Value can not match the type constraint!")
            End If

            Call Variables.Add(.ref)

            ' 位置值，相当于函数指针
            Return Variables.Count - 1
        End With
    End Function
End Class
