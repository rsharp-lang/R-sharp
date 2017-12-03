#Region "Microsoft.VisualBasic::8f326cbd5c2b7723cfb7ea65eed03562, ..\R-sharp\R#\Interpreter\ExpressionParser\FuncCaller.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Abstract
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.Expression

    ''' <summary>
    ''' Function object model.(调用函数的方法)
    ''' </summary>
    ''' <remarks>
    ''' 这个可能需要同时兼容下面的函数形式：
    ''' 
    ''' 1. R#脚本之中使用function所申明的脚本函数
    ''' 2. 使用``imports``所导入的.NET命名空间之中的Module的API函数(直接解析assembly type获取得到的<see cref="MethodInfo"/>)
    ''' 3. 使用``library``所导入的R#包的函数(通过<see cref="ExportAPIAttribute"/>所标记的API函数)
    ''' </remarks>
    Public Class FuncCaller

        Public ReadOnly Property Name As String
        ''' <summary>
        ''' name属性是为了兼容: name=value这种带名称的无序参数形式
        ''' 假若参数没有名称，则默认使用#num来表示位置
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Params As New List(Of NamedValue(Of Func(Of Environment, SimpleExpression)))

        ReadOnly __calls As FunctionEvaluate

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Name">The function name</param>
        ''' <param name="evaluate">Engine handle</param>
        Sub New(name$, evaluate As FunctionEvaluate)
            Me.Name = name
            Me.__calls = evaluate
        End Sub

        Sub New(name$, params As IEnumerable(Of NamedValue(Of Func(Of Environment, SimpleExpression))))
            Me.Name = name
            Me.Params = params.AsList
        End Sub

        Public Overrides Function ToString() As String
            Dim args As String() = Params _
                .Select(Function(x) x.ToString) _
                .ToArray
            Return $"{Name}({args.JoinBy(", ")})"
        End Function

        Public Function Evaluate(envir As Environment) As Object
            Dim params As NamedValue(Of MetaExpression)() = Me _
                .Params _
                .Select(Function(x)
                            With x
                                Dim out = .Value(envir).Evaluate(envir)
                                Dim value As New MetaExpression(out.value, out.type)
                                Return New NamedValue(Of MetaExpression)(.Name, value)
                            End With
                        End Function) _
                .ToArray
            Return run(params, envir)
        End Function

        Protected Overridable Function run(params As NamedValue(Of MetaExpression)(), envir As Environment) As Object
            If Not __calls Is Nothing Then
                Return __calls(Name, params.Select(Function(x) x.Value.LEFT).ToArray)
            End If

            If envir.Closures.ContainsKey(Name) Then
                Dim closure = envir.Closures(Name)
                Dim args = params _
                    .Select(Function(x)
                                Return New NamedValue(Of Object) With {
                                    .Name = x.Name,
                                    .Value = envir.GetValue(x.Value)
                                }
                            End Function) _
                    .ToArray
                Dim value = closure(args)
                Return value
            End If
        End Function
    End Class

    ''' <summary>
    ''' 使用``imports``命令所导入的.NET函数，具有重载特性
    ''' </summary>
    Public Class DotNETOverloadsFunction : Inherits FuncCaller

        ReadOnly [overloads] As OverloadsFunction

        Public Sub New(name As String, evaluate As FunctionEvaluate)
            MyBase.New(name, evaluate)
        End Sub

        Public Overrides Function ToString() As String
            Return MyBase.ToString()
        End Function
    End Class

    ''' <summary>
    ''' 用户自己构建的lambda函数，没有重载特性
    ''' </summary>
    Public Class DotNetClosure : Inherits FuncCaller

        Public Sub New(name As String, evaluate As FunctionEvaluate)
            MyBase.New(name, evaluate)
        End Sub
    End Class

    ''' <summary>
    ''' 因为R#之中也将function当作为一种变量类型，所以所有的R#函数都是变量来的，故而不会存在重载函数的概念
    ''' </summary>
    Public Class RSharpClosure : Inherits FuncCaller

        Public Sub New(name As String, evaluate As FunctionEvaluate)
            MyBase.New(name, evaluate)
        End Sub
    End Class
End Namespace
