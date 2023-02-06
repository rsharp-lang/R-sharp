#Region "Microsoft.VisualBasic::b4151ff64f649f8c10c6ee38ebe94a76, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\If\IfBranch.vb"

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

'   Total Lines: 100
'    Code Lines: 85
' Comment Lines: 0
'   Blank Lines: 15
'     File Size: 3.80 KB


'     Class IfBranch
' 
'         Properties: expressionName, ifTest, stackFrame, trueClosure, type
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: Evaluate, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports devtools = Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Blocks

    Public Class IfBranch : Inherits Expression
        Implements IRuntimeTrace

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return trueClosure.type
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.If
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Public ReadOnly Property ifTest As Expression
        Public ReadOnly Property trueClosure As DeclareNewFunction

        Sub New(ifTest As Expression, trueClosure As DeclareNewFunction, stackframe As StackFrame)
            Me.ifTest = ifTest
            Me.trueClosure = trueClosure
            Me.stackFrame = stackframe
        End Sub

        Sub New(ifTest As Expression, trueClosure As ClosureExpression, stackframe As StackFrame)
            Call Me.New(
                ifTest:=ifTest,
                trueClosure:=New DeclareNewFunction(
                    funcName:="if_closure_internal",
                    parameters:={},
                    body:=trueClosure,
                    stackframe:=stackframe
                ),
                stackframe:=stackframe
            )
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim test As Object = ifTest.Evaluate(envir)

            If test Is Nothing Then
                Return New Message With {
                    .message = {
                        $"missing value where TRUE/FALSE needed"
                    },
                    .level = MSG_TYPES.ERR,
                    .environmentStack = debug.getEnvironmentStack(envir),
                    .trace = devtools.ExceptionData.GetCurrentStackTrace
                }
            ElseIf Program.isException(test) Then
                Return test
            End If

            Dim flags As Boolean() = CLRVector.asLogical(test)

            If flags.Length = 0 Then
                Return Internal.debug.stop({
                    "argument is of length zero",
                    "test: " & ifTest.ToString
                }, envir)
            ElseIf flags.Length > 1 Then
                Call envir.AddMessage("the condition has length > 1 and only the first element will be used", MSG_TYPES.WRN)
            End If

            If True = flags(Scan0) Then
                Dim env As New Environment(envir, stackFrame, isInherits:=False)
                Dim resultVal As Object = trueClosure.Invoke(env, {})

                If Program.isException(resultVal) Then
                    Return resultVal
                Else
                    Return New IfPromise(resultVal, True)
                End If
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
