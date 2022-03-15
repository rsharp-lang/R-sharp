#Region "Microsoft.VisualBasic::c0248a3c9a3e737846dcf5c04417f51b, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\If\ElseBranch.vb"

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

    '   Total Lines: 79
    '    Code Lines: 64
    ' Comment Lines: 0
    '   Blank Lines: 15
    '     File Size: 2.53 KB


    '     Class ElseBranch
    ' 
    '         Properties: expressionName, stackFrame, type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Development.Package.File

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Blocks

    Public Class ElseBranch : Inherits Expression
        Implements IRuntimeTrace

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.closure
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Else
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame
            Get
                Return closure.stackFrame
            End Get
        End Property

        Friend ReadOnly closure As DeclareNewFunction

        Friend Sub New(bodyClosure As DeclareNewFunction)
            Me.closure = bodyClosure
        End Sub

        Sub New(bodyClosure As ClosureExpression, stackframe As StackFrame)
            Call Me.New(New DeclareNewFunction(
                body:=bodyClosure,
                funcName:="else_branch_internal",
                parameters:={},
                stackframe:=stackframe
            ))
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            If envir.ifPromise = 0 Then
                Return Internal.debug.stop(New SyntaxErrorException, envir)
            Else
                Dim last As IfPromise

                If envir.ifPromise.Last.Result = True Then
                    last = envir.ifPromise.Pop
                Else
                    Dim resultVal As Object = closure.Invoke(envir, {})

                    If Program.isException(resultVal) Then
                        Return resultVal
                    Else
                        last = New IfPromise(resultVal, False) With {
                            .assignTo = envir.ifPromise.Last.assignTo
                        }
                    End If
                End If

                Call last.DoValueAssign(envir)

                Return last
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"else {{
    {closure}
}}"
        End Function
    End Class

End Namespace
