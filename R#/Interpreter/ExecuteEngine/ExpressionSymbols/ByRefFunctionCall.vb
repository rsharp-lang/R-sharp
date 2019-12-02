#Region "Microsoft.VisualBasic::46f0ec6e29974db768e0507a3b8dcc54, R#\Interpreter\ExecuteEngine\ExpressionSymbols\ByRefFunctionCall.vb"

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

    '     Class ByRefFunctionCall
    ' 
    '         Properties: type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' ``func(a) &lt;- value``
    ''' </summary>
    Public Class ByRefFunctionCall : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        ReadOnly funcName$
        ReadOnly target As Expression
        ReadOnly value As Expression

        Sub New(invoke As Expression, value As Expression)
            Dim target As FunctionInvoke = invoke

            Me.value = value
            Me.funcName = DirectCast(target.funcName, Literal).ToString
            Me.target = target.parameters(Scan0)
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Select Case funcName
                Case "names"
                    Return base.names(target.Evaluate(envir), value.Evaluate(envir), envir)
                Case Else
                    Return Message.SyntaxNotImplemented(envir, $"byref call of {funcName}")
            End Select
        End Function
    End Class
End Namespace
