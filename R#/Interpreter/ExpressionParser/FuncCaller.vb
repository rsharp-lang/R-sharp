#Region "Microsoft.VisualBasic::0ca028d2ef97514b35389e6e09634d91, ..\sciBASIC#\Data_science\Mathematical\Math\Scripting\Arithmetic.Expression\FuncCaller.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Abstract

''' <summary>
''' Function object model.(调用函数的方法)
''' </summary>
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
        Dim args As String() = Params.ToArray(Function(x) x.ToString)
        Return $"{Name}({args.JoinBy(", ")})"
    End Function

    Public Function Evaluate(envir As Environment) As Double
        Dim params = Me.Params.Select(Function(x)
                                          With x
                                              Return New NamedValue(Of Object)(.Name, .Value(envir).Evaluate(envir))
                                          End With
                                      End Function).ToArray
        ' Return __calls(Name, params)
    End Function
End Class

