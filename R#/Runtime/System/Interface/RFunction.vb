#Region "Microsoft.VisualBasic::871c5eba548b7e1f10b998cded88153f, E:/GCModeller/src/R-sharp/R#//Runtime/System/Interface/RFunction.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 56
    '    Code Lines: 13
    ' Comment Lines: 36
    '   Blank Lines: 7
    '     File Size: 2.17 KB


    '     Interface RFunction
    ' 
    '         Properties: name
    ' 
    '         Function: getArguments, getReturns, (+2 Overloads) Invoke
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Components.Interface

    ''' <summary>
    ''' the abstract model of these:
    ''' 
    ''' 1. <see cref="DeclareLambdaFunction"/>
    ''' 2. <see cref="DeclareNewFunction"/>
    ''' 3. <see cref="RMethodInfo"/>
    ''' </summary>
    Public Interface RFunction

        ''' <summary>
        ''' 函数名
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property name As String

        ''' <summary>
        ''' 获取这个函数可能所返回的类型，如果没有额外的信息，这个属性一般是any类型
        ''' </summary>
        ''' <returns></returns>
        Function getReturns(env As Environment) As IRType

        ''' <summary>
        ''' get the parameter list of current function
        ''' </summary>
        ''' <returns>
        ''' please note that:
        ''' 
        ''' + if the value of the item is nothing, then it means the parameter is required!
        ''' + if the value of the item is a literal expression of NULL, then it means the parameter is optional and the default value is NULL
        ''' </returns>
        Function getArguments() As IEnumerable(Of NamedValue(Of Expression))

        ''' <summary>
        ''' 执行当前的这个函数对象然后获取得到结果值
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <param name="arguments"></param>
        ''' <returns></returns>
        Function Invoke(envir As Environment, arguments As InvokeParameter()) As Object
        ''' <summary>
        ''' 直接传入参数进行函数的调用，这个接口一般是直接用于生成lambda函数所使用的
        ''' </summary>
        ''' <param name="arguments"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Function Invoke(arguments As Object(), env As Environment) As Object

    End Interface
End Namespace
