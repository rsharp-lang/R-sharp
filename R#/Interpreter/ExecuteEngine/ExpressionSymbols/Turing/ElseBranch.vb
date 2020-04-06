#Region "Microsoft.VisualBasic::9355c6d4a19365621a8b9f2deb97f710, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\ElseBranch.vb"

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

    '     Class ElseBranch
    ' 
    '         Properties: stackFrame, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    '     Class ElseIfBranch
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Blocks

    Public Class ElseBranch : Inherits Expression
        Implements IRuntimeTrace

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.closure
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Dim closure As DeclareNewFunction

        Sub New(bodyClosure As ClosureExpression, stackframe As StackFrame)
            closure = New DeclareNewFunction(
                body:=bodyClosure,
                funcName:="else_branch_internal",
                params:={},
                stackframe:=stackframe
            )
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            If envir.ifPromise = 0 Then
                Return Internal.debug.stop(New SyntaxErrorException, envir)
            Else
                Dim last As IfBranch.IfPromise

                If envir.ifPromise.Last.Result = True Then
                    last = envir.ifPromise.Pop
                Else
                    last = New IfBranch.IfPromise(closure.Invoke(envir, {}), False) With {
                        .assignTo = envir.ifPromise.Last.assignTo
                    }
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

    Public Class ElseIfBranch : Inherits IfBranch

        Public Sub New(ifTest As Expression, trueClosure As ClosureExpression, stackframe As StackFrame)
            MyBase.New(ifTest, trueClosure, stackframe)

            stackframe.Method.Method = "elseif_closure"
        End Sub
    End Class
End Namespace
