#Region "Microsoft.VisualBasic::cf0918dff3f1ecb37385c16ebf30fcbd, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\WhileLoop.vb"

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

'   Total Lines: 69
'    Code Lines: 56
' Comment Lines: 0
'   Blank Lines: 13
'     File Size: 2.28 KB


'     Class WhileLoop
' 
'         Properties: expressionName, stackFrame, type
' 
'         Constructor: (+1 Overloads) Sub New
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
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Blocks

    Public Class WhileLoop : Inherits Expression
        Implements IRuntimeTrace

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return loopBody.type
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.While
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Dim test As Expression
        Dim loopBody As ClosureExpression

        Sub New(test As Expression, loopBody As ClosureExpression, stackframe As StackFrame)
            Me.stackFrame = stackframe
            Me.test = test
            Me.loopBody = loopBody
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim result As New List(Of Object)
            Dim test As Object
            Dim value As Object
            Dim env As New Environment(envir, stackFrame, isInherits:=False)

            Do While True
                test = Me.test.Evaluate(envir)

                If Program.isException(test) Then
                    Return test
                ElseIf False = CLRVector.asLogical(test)(Scan0) Then
                    Exit Do
                Else
                    value = Me.loopBody.Evaluate(env)
                End If

                If Program.isException(value) Then
                    Return value
                Else
                    result.Add([single](value))
                End If
            Loop

            Return result.ToArray
        End Function

        Public Overrides Function ToString() As String
            Return $"do while ({test}){{
    {loopBody}
}}"
        End Function
    End Class
End Namespace
