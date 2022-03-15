#Region "Microsoft.VisualBasic::978423893e4299b18017d0ee0e97174f, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\If\ElseIfBranch.vb"

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

        Total Lines:   48
        Code Lines:    38
        Comment Lines: 0
        Blank Lines:   10
        File Size:     1.73 KB


    '     Class ElseIfBranch
    ' 
    '         Properties: expressionName
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Blocks

    Public Class ElseIfBranch : Inherits IfBranch

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.ElseIf
            End Get
        End Property

        Public Sub New(ifTest As Expression, trueClosure As ClosureExpression, stackframe As StackFrame)
            MyBase.New(ifTest, trueClosure, stackframe)

            stackframe.Method.Method = "elseif_closure"
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            If envir.ifPromise = 0 Then
                Return Internal.debug.stop(New SyntaxErrorException, envir)
            Else
                Dim last As IfPromise

                If envir.ifPromise.Last.Result = True Then
                    last = envir.ifPromise.Pop
                Else
                    Dim resultVal As Object = MyBase.Evaluate(envir)

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
    End Class
End Namespace
