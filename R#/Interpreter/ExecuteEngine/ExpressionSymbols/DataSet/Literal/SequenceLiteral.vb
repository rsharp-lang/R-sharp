#Region "Microsoft.VisualBasic::b3d32f61a3659eab907c1f90c0f94582, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\Literal\SequenceLiteral.vb"

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

    '   Total Lines: 97
    '    Code Lines: 77
    ' Comment Lines: 3
    '   Blank Lines: 17
    '     File Size: 3.55 KB


    '     Class SequenceLiteral
    ' 
    '         Properties: expressionName, stackFrame, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, isIntegerSequence, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Development.Package.File

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' a syntax for generate a numeric sequence: ``from:to step diff``
    ''' </summary>
    Public Class SequenceLiteral : Inherits Expression
        Implements IRuntimeTrace

        Public Overrides ReadOnly Property type As TypeCodes

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.SequenceLiteral
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Friend ReadOnly from As Expression
        Friend ReadOnly [to] As Expression
        Friend ReadOnly steps As Expression

        Sub New(from As Expression, [to] As Expression, steps As Expression, stackFrame As StackFrame)
            Me.from = from
            Me.to = [to]
            Me.steps = steps
            Me.stackFrame = stackFrame
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function isIntegerSequence(init As Object, stops As Object, offset As Object) As Boolean
            Return Not New Object() {init, stops, offset} _
                .Any(Function(num)
                         Dim ntype As Type = num.GetType

                         If ntype Like RType.floats Then
                             Return True
                         Else
                             Return False
                         End If
                     End Function)
        End Function

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim init = from.Evaluate(envir)
            Dim stops = [to].Evaluate(envir)
            Dim offset = steps.Evaluate(envir)

            If Program.isException(init) Then
                Return init
            ElseIf Program.isException(stops) Then
                Return stops
            ElseIf Program.isException(offset) Then
                Return offset
            End If

            If Not isIntegerSequence(init, stops, offset) Then
                Dim start As Double = Runtime.getFirst(init)
                Dim steps As Double = Runtime.getFirst(offset)
                Dim ends As Double = Runtime.getFirst(stops)
                Dim seq As New List(Of Double)

                Do While start <= ends
                    seq += start
                    start += steps
                Loop

                Return seq.ToArray
            Else
                Dim start As Long = Runtime.getFirst(init)
                Dim steps As Long = Runtime.getFirst(offset)
                Dim ends As Integer = Runtime.getFirst(stops)
                Dim seq As New List(Of Long)

                Do While start <= ends
                    seq += start
                    start += steps
                Loop

                Return seq.ToArray
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"{from}:{[to]} step {steps}"
        End Function
    End Class
End Namespace
