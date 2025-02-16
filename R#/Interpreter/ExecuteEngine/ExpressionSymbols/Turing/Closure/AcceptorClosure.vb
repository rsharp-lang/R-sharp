﻿#Region "Microsoft.VisualBasic::928dfb000c71cd17595993ffa677a7a2, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\Closure\AcceptorClosure.vb"

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

    '   Total Lines: 74
    '    Code Lines: 48 (64.86%)
    ' Comment Lines: 14 (18.92%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 12 (16.22%)
    '     File Size: 2.46 KB


    '     Class AcceptorClosure
    ' 
    '         Properties: expressionName
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, evaluateAcceptor
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Closure

    ''' <summary>
    ''' ```
    ''' func(yyy) {
    '''     func2(xxx);
    ''' }
    ''' 
    ''' # will accept the closure as the first parameter
    ''' # which is equals to the function invoke expression
    ''' 
    ''' func({
    '''     func2(xxx);
    ''' }, yyy);
    ''' ```
    ''' </summary>
    Public Class AcceptorClosure : Inherits ClosureExpression

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.AcceptorDeclare
            End Get
        End Property

        Public Sub New(code() As Expression)
            MyBase.New(code)
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            If TypeOf program.Last Is FunctionInvoke Then
                Return evaluateAcceptor(envir)
            Else
                Return MyBase.Evaluate(envir)
            End If
        End Function

        Private Function evaluateAcceptor(envir As Environment) As Object
            Dim arguments As Dictionary(Of String, Object) = envir.acceptorArguments
            Dim newProgram As New Program(program.Take(program.lines - 1))
            Dim lastFunc As FunctionInvoke = program.Last
            Dim value As Object = newProgram.Execute(envir)

            If Program.isException(value) OrElse TypeOf value Is RExit Then
                Return value
            End If

            Dim lastClosureCopy As New FunctionInvoke(lastFunc)
            Dim accepts As New List(Of Expression)
            Dim valExpr As Expression

            For Each arg In arguments
                valExpr = New RuntimeValueLiteral(arg.Value)

                Call accepts.Add(New ValueAssignExpression(
                    symbol:=arg.Key,
                    value:=valExpr)
                )
            Next

            lastClosureCopy.parameters = lastClosureCopy.parameters _
                .JoinIterates(accepts) _
                .ToArray
            value = lastClosureCopy.Evaluate(envir)

            Return value
        End Function
    End Class
End Namespace
