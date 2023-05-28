#Region "Microsoft.VisualBasic::94a0ef5790ce59d32ca31224421f1987, F:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/ExpressionSymbols/Turing/Closure/RepeatClosure.vb"

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

    '   Total Lines: 52
    '    Code Lines: 39
    ' Comment Lines: 0
    '   Blank Lines: 13
    '     File Size: 1.64 KB


    '     Class RepeatClosure
    ' 
    '         Properties: expressionName, stackFrame, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Development.Package.File

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Closure

    Public Class RepeatClosure : Inherits Expression
        Implements IRuntimeTrace

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return closure.type
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Do
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        ReadOnly closure As ClosureExpression

        Sub New(closure As ClosureExpression, stackframe As StackFrame)
            Me.closure = closure
            Me.stackFrame = stackframe
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim break As Boolean = False
            Dim value As Object = Nothing

            Using env As New Environment(envir, stackFrame, isInherits:=False)
                Do While Not break
                    value = closure.Evaluate(env)

                    If Program.isException(value) Then
                        Return value
                    End If


                Loop
            End Using

            Return value
        End Function
    End Class
End Namespace
