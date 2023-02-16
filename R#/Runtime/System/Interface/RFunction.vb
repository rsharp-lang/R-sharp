#Region "Microsoft.VisualBasic::4be6fe6c4e6332505ca55a4c909895e7, E:/GCModeller/src/R-sharp/R#//Runtime/System/Interface/RFunction.vb"

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

    '   Total Lines: 59
    '    Code Lines: 20
    ' Comment Lines: 27
    '   Blank Lines: 12
    '     File Size: 2.04 KB


    '     Interface RFunction
    ' 
    '         Properties: name
    ' 
    '         Function: getArguments, getReturns, (+2 Overloads) Invoke
    ' 
    '     Interface IRuntimeTrace
    ' 
    '         Properties: stackFrame
    ' 
    '     Interface INamespaceReferenceSymbol
    ' 
    '         Properties: [namespace]
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
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
        Function getReturns(env As Environment) As RType
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

    Public Interface IRuntimeTrace

        ReadOnly Property stackFrame As StackFrame

    End Interface

    Public Interface INamespaceReferenceSymbol

        ReadOnly Property [namespace] As String

    End Interface
End Namespace
