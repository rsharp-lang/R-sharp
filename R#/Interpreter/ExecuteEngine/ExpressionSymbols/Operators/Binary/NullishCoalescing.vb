﻿#Region "Microsoft.VisualBasic::537a591389cf1bbd2ccbd15d568afd57, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\Binary\NullishCoalescing.vb"

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

    '   Total Lines: 75
    '    Code Lines: 40 (53.33%)
    ' Comment Lines: 26 (34.67%)
    '    - Xml Docs: 73.08%
    ' 
    '   Blank Lines: 9 (12.00%)
    '     File Size: 2.91 KB


    '     Class NullishCoalescing
    ' 
    '         Properties: [default], [operator], expressionName, type, value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    ''' <summary>
    ''' The nullish coalescing (??) operator is a logical operator that returns
    ''' its right-hand side operand when its left-hand side operand is null 
    ''' or undefined, and otherwise returns its left-hand side operand.
    ''' 
    ''' ```
    ''' x ?? default
    ''' ```
    ''' </summary>
    ''' <remarks>
    ''' The nullish coalescing operator can be seen as a special case of the logical OR (||) 
    ''' operator. The latter returns the right-hand side operand if the left operand is any 
    ''' falsy value, not only null or undefined. In other words, if you use || to provide 
    ''' some default value to another variable foo, you may encounter unexpected behaviors 
    ''' if you consider some falsy values as usable (e.g., '' or 0). 
    ''' 
    ''' The nullish coalescing Operator has the fifth-lowest Operator precedence, directly lower 
    ''' than || And directly higher than the conditional (ternary) Operator.
    ''' 
    ''' It Is Not possible to combine both the And (&amp;&amp;) And Or operators (||) directly 
    ''' with ??. A syntax error will be thrown in such cases.
    ''' </remarks>
    Public Class NullishCoalescing : Inherits Expression
        Implements IBinaryExpression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.generic
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Binary
            End Get
        End Property

        Public ReadOnly Property value As Expression Implements IBinaryExpression.left
        Public ReadOnly Property [default] As Expression Implements IBinaryExpression.right
        Public ReadOnly Property [operator] As String Implements IBinaryExpression.operator
            Get
                Return "??"
            End Get
        End Property

        Sub New(a As Expression, b As Expression)
            value = a
            [default] = b
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            ' 20191216
            ' fix for
            ' value || stop(xxxx)
            Dim a As Object = value.Evaluate(envir)

            ' let arg as string = ?"--opt" || default;
            If Internal.Invokes.base.isEmpty(a) Then
                Return [default].Evaluate(envir)
            Else
                Return a
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"({value} || {[default]})"
        End Function
    End Class
End Namespace
