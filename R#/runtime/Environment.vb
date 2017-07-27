Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.Abstract
Imports SMRUCC.Rsharp.Runtime.CodeDOM

Namespace Runtime

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
        Public ReadOnly Property Stack As String

        ''' <summary>
        ''' Key is <see cref="RType.Identity"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Types As New Dictionary(Of String, RType)

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
        ''' Get/set variable value
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' If the current stack does not contains the target variable, then the program will try to find the variable in his parent
        ''' if variable in format like [var], then it means a global or parent environment variable
        ''' </remarks>
        Default Public Property Value(name$) As Object
            Get
                If Variables.ContainsKey(name) AndAlso (Not (name.First = "["c AndAlso name.Last = "]"c)) Then
                    Return Variables(name).Value
                ElseIf Not Parent Is Nothing Then
                    Return Parent(name)
                Else
                    Throw New EntryPointNotFoundException(name & " was not found in any stack enviroment!")
                End If
            End Get
            Set(value)
                If name.First = "["c AndAlso name.Last = "]"c Then
                    Parent(name.GetStackValue("[", "]")) = value
                Else
                    Variables(name).Value = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' 每一个closure可以看作为一个函数对象
        ''' 则parent的closure和他的child closure之间相互通信，最直接的方法就是参数传递
        ''' </summary>
        ''' <param name="parent"></param>
        ''' <param name="parameters">closure函数的参数传递列表</param>
        Sub New(parent As Environment, parameters As NamedValue(Of PrimitiveExpression)(), <CallerMemberName> Optional stack$ = Nothing)
            Me.Parent = parent
            Me.Variables = parameters _
                .Select(Function(expression)
                            Dim expr As PrimitiveExpression = expression.Value
                            Return New Variable(TypeCodes.generic) With {
                                .Name = expression.Name,
                                .Value = expr.Evaluate(envir:=Me)
                            }
                        End Function).ToDictionary
            Me.Stack = stack
        End Sub

        ''' <summary>
        ''' Construct the global environment
        ''' </summary>
        Friend Sub New()
            Call Me.New(Nothing, {})
        End Sub

        Public Overrides Function ToString() As String
            If IsGlobal Then
                Return $"Global({NameOf(Environment)})"
            Else
                Return Parent?.ToString & "->" & Stack
            End If
        End Function

        Public Function GetValue() As GetValue
            Return Function(name$)
                       Return Me(name)
                   End Function
        End Function

        Public Function Evaluate() As FunctionEvaluate
            Return Function(funcName, args)

                   End Function
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
End Namespace