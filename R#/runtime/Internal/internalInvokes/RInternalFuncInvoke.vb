#Region "Microsoft.VisualBasic::9a62cc2c01878142fd3ba8d71c75f2c4, R#\Runtime\Internal\internalInvokes\RInternalFuncInvoke.vb"

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

    '     Class RInternalFuncInvoke
    ' 
    ' 
    ' 
    '     Class GenericInternalInvoke
    ' 
    '         Properties: funcName
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: invoke, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' 内部函数的调用接口
    ''' </summary>
    Public MustInherit Class RInternalFuncInvoke

        ''' <summary>
        ''' 函数名称（函数名称主要是在抛出错误的时候添加调试信息所使用的）
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property funcName As String

        ''' <summary>
        ''' 执行这个内部函数
        ''' </summary>
        ''' <param name="envir">代码所执行的环境对象</param>
        ''' <param name="paramVals">函数的参数</param>
        ''' <returns></returns>
        Public MustOverride Function invoke(envir As Environment, paramVals As Object()) As Object

    End Class

    Public Class GenericInternalInvoke : Inherits RInternalFuncInvoke

        ReadOnly handle As Func(Of Environment, Object(), Object)

        Public Overrides ReadOnly Property funcName As String

        Sub New(name$, invoke As Func(Of Object, Object))
            funcName = name
            handle = Function(envir, params) invoke(params(Scan0))
        End Sub

        Sub New(name$, internal As Func(Of Environment, Object(), Object))
            funcName = name
            handle = internal
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function invoke(envir As Environment, paramVals() As Object) As Object
            Return handle(envir, paramVals)
        End Function

        Public Overrides Function ToString() As String
            Return funcName
        End Function
    End Class
End Namespace
