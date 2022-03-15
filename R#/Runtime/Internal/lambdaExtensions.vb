#Region "Microsoft.VisualBasic::c551a531fe3cfcb46fa3b52179d1303e, R-sharp\R#\Runtime\Internal\lambdaExtensions.vb"

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


     Code Statistics:

        Total Lines:   33
        Code Lines:    22
        Comment Lines: 6
        Blank Lines:   5
        File Size:     1.24 KB


    '     Module lambdaExtensions
    ' 
    '         Function: getRFunc, invokeArgument
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Runtime.Internal

    Module lambdaExtensions

        ''' <summary>
        ''' 主要是应用于单个参数的R运行时函数的调用
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' 
        <DebuggerStepThrough>
        Friend Function invokeArgument(value As Object) As InvokeParameter()
            Return InvokeParameter.CreateLiterals(value)
        End Function

        Friend Function getRFunc(FUN As Object, env As Environment) As [Variant](Of RFunction, Message)
            If FUN Is Nothing Then
                Return Internal.debug.stop({"Missing apply function!"}, env)
            ElseIf TypeOf FUN Is Message Then
                Return DirectCast(FUN, Message)
            ElseIf Not FUN.GetType.ImplementInterface(GetType(RFunction)) Then
                Return Internal.debug.stop({"Target is not a function!"}, env)
            End If

            Return DirectCast(FUN, RFunction)
        End Function
    End Module
End Namespace
