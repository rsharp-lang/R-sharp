#Region "Microsoft.VisualBasic::b271fa43be4df78bae493441e70e71b5, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\IIfExpression.vb"

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

    '     Class IIfExpression
    ' 
    '         Properties: type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' ifelse expression
    ''' </summary>
    Public Class IIfExpression : Inherits Expression

        Friend ifTest As Expression
        Friend trueResult As Expression
        Friend falseResult As Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Sub New(iftest As Expression, trueResult As Expression, falseResult As Expression)
            Me.ifTest = iftest
            Me.trueResult = trueResult
            Me.falseResult = falseResult
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim ifTestResult = ifTest.Evaluate(envir)
            Dim test As Boolean = Runtime.asLogical(ifTestResult)(Scan0)

            If test = True Then
                Return trueResult.Evaluate(envir)
            Else
                Return falseResult.Evaluate(envir)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"({ifTest} ? {trueResult} : {falseResult})"
        End Function
    End Class
End Namespace
