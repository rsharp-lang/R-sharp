#Region "Microsoft.VisualBasic::b1e655ebf182a8e883f82d968ce3fe6b, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\Closure\AcceptorClosure.vb"

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

    '   Total Lines: 48
    '    Code Lines: 40
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 1.88 KB


    '     Class AcceptorClosure
    ' 
    '         Properties: expressionName
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate
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
                Dim arguments = envir.acceptorArguments
                Dim newProgram As New Program(program.Take(program.lines - 1))
                Dim lastFunc As FunctionInvoke = program.Last
                Dim value As Object = newProgram.Execute(envir)

                If Program.isException(value) Then
                    Return value
                Else
                    Dim lastClosureCopy As New FunctionInvoke(lastFunc)

                    lastClosureCopy.parameters = lastClosureCopy.parameters _
                        .JoinIterates(arguments _
                            .Select(Function(arg)
                                        Return New ValueAssignExpression(arg.Key, New RuntimeValueLiteral(arg.Value))
                                    End Function)) _
                        .ToArray
                    value = lastClosureCopy.Evaluate(envir)
                End If

                Return value
            Else
                Return MyBase.Evaluate(envir)
            End If
        End Function
    End Class
End Namespace
