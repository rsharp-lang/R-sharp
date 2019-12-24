#Region "Microsoft.VisualBasic::215a95ebbf91d0784f5f9b9b92b5afb8, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\IfBranch.vb"

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

    '     Class IfBranch
    ' 
    '         Properties: type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Evaluate, ToString
    '         Class IfPromise
    ' 
    '             Properties: assignTo, Result, Value
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: DoValueAssign
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports devtools = Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics

Namespace Interpreter.ExecuteEngine

    Public Class IfBranch : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Dim ifTest As Expression
        Dim trueClosure As DeclareNewFunction

        Friend Class IfPromise

            Public ReadOnly Property Result As Boolean
            Public ReadOnly Property Value As Object
            Public Property assignTo As Expression

            Sub New(value As Object, result As Boolean)
                Me.Value = value
                Me.Result = result
            End Sub

            Public Function DoValueAssign(envir As Environment) As Object
                ' 没有变量需要进行closure的返回值设置
                ' 则跳过
                If assignTo Is Nothing Then
                    Return Value
                End If

                Select Case assignTo.GetType
                    Case GetType(ValueAssign)
                        Return DirectCast(assignTo, ValueAssign).DoValueAssign(envir, Value)
                    Case Else
                        Return Internal.stop(New NotImplementedException, envir)
                End Select
            End Function
        End Class

        Sub New(tokens As IEnumerable(Of Token))
            Dim blocks = tokens.SplitByTopLevelDelimiter(TokenType.close)

            ifTest = Expression.CreateExpression(blocks(Scan0).Skip(1))
            trueClosure = New DeclareNewFunction With {
                .funcName = "if_closure_internal",
                .params = {},
                .body = blocks(2) _
                    .Skip(1) _
                    .DoCall(AddressOf ClosureExpression.ParseExpressionTree)
            }
        End Sub

        Sub New(ifTest As Expression, trueClosure As ClosureExpression)
            Me.ifTest = ifTest
            Me.trueClosure = New DeclareNewFunction With {
                .funcName = "if_closure_internal",
                .params = {},
                .body = trueClosure
            }
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim test As Object = ifTest.Evaluate(envir)

            If test Is Nothing Then
                Return New Message With {
                    .message = {
                        $"missing value where TRUE/FALSE needed"
                    },
                    .level = MSG_TYPES.ERR,
                    .environmentStack = envir.getEnvironmentStack,
                    .trace = devtools.ExceptionData.GetCurrentStackTrace
                }
            End If

            If True = Runtime.asLogical(test)(Scan0) Then
                Return New IfPromise(trueClosure.Invoke(envir, {}), True)
            Else
                Return New IfPromise(Nothing, False)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"if ({ifTest}) then {{
    {trueClosure}
}}"
        End Function
    End Class
End Namespace
