#Region "Microsoft.VisualBasic::0bf9adeb611abff75157721f8d466d9f, R#\Runtime\Components\Interface\RFunction.vb"

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

'     Interface RFunction
' 
'         Properties: name
' 
'         Function: Invoke
' 
' 
' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Components.Interface

    ''' <summary>
    ''' 1. <see cref="DeclareLambdaFunction"/>
    ''' 2. <see cref="DeclareNewFunction"/>
    ''' 3. <see cref="RMethodInfo"/>
    ''' 4. <see cref="RMethodOverloads"/>
    ''' </summary>
    Public Interface RFunction

        ''' <summary>
        ''' 函数名
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property name As String

        ''' <summary>
        ''' 执行当前的这个函数对象然后获取得到结果值
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <param name="arguments"></param>
        ''' <returns></returns>
        Function Invoke(envir As Environment, arguments As InvokeParameter()) As Object

    End Interface
End Namespace
