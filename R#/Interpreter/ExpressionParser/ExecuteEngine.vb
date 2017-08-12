#Region "Microsoft.VisualBasic::5e62672aa006fa2fd020f64d120117f3, ..\R-sharp\R#\Interpreter\ExpressionParser\ExecuteEngine.vb"

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

Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.Expression

    Public Module ExecuteEngine

        ''' <summary>
        ''' Operator expression evaluate
        ''' </summary>
        ''' <param name="left"></param>
        ''' <param name="[next]"></param>
        ''' <param name="operator$"></param>
        ''' <returns></returns>
        <Extension> Public Function EvaluateBinary(envir As Environment, left As Variable, [next] As Variable, operator$) As Object
            Dim ta As RType = envir.Types(left.TypeID)
            Dim tb As RType = envir.Types([next].TypeID)

        End Function

        <Extension> Public Function EvaluateUnary(envir As Environment, x As Variable, operator$) As Object
            Dim type As RType = envir.Types(x.TypeID)
            Dim method = type.GetUnaryOperator([operator])

            If method Is Nothing Then
                Throw New NotImplementedException($"Operator '{[operator]}' is not defined!")
            End If

            Dim result As Object = method(x.Value)
            Return result
        End Function
    End Module
End Namespace
