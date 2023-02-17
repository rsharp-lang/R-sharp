#Region "Microsoft.VisualBasic::5df0df7baa0a9e6f04afc8840005cfdb, D:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/ExpressionSymbols/DataSet/Literal/SequenceLiteral.vb"

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

    '   Total Lines: 204
    '    Code Lines: 147
    ' Comment Lines: 26
    '   Blank Lines: 31
    '     File Size: 7.44 KB


    '     Class SequenceLiteral
    ' 
    '         Properties: expressionName, stackFrame, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: characterSeq, Evaluate, getChar, integerSeq, measureSequence
    '                   numericSeq, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

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

        ''' <summary>
        ''' nothing means default steps
        ''' </summary>
        Friend ReadOnly steps As Expression

        Sub New(from As Expression, [to] As Expression, steps As Expression, stackFrame As StackFrame)
            Me.from = from
            Me.to = [to]
            Me.steps = steps
            Me.stackFrame = stackFrame
        End Sub

        ''' <summary>
        ''' test the value mode of the target sequence literal
        ''' </summary>
        ''' <param name="init"></param>
        ''' <param name="stops"></param>
        ''' <param name="offset"></param>
        ''' <returns>
        ''' literal value mode could be:
        ''' 
        ''' + num for float number
        ''' + int for integer number
        ''' + chr for characters
        ''' + n/a for error message
        ''' 
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function measureSequence(init As Object, stops As Object, offset As Object) As String
            Dim test = New Object() {init, stops, offset} _
                .Select(Function(num)
                            Dim rtype As RType = RType.TypeOf(num)
                            Dim ntype As Type = rtype.raw

                            If ntype Like RType.floats Then
                                Return "num"
                            ElseIf ntype Like RType.integers Then
                                Return "int"
                            ElseIf ntype Like RType.characters Then
                                Return "chr"
                            Else
                                Return "n/a"
                            End If
                        End Function) _
                .ToArray

            If test.Any(Function(t) t = "n/a") Then
                Return "n/a"
            End If

            For Each testType As String In {"num", "int"}
                If test.All(Function(t) t = testType) Then
                    Return testType
                End If
            Next

            Static numeric As Index(Of String) = {"num", "int"}

            If test(0) = "chr" AndAlso test(1) = "chr" AndAlso test(2) Like numeric Then
                Return "chr"
            ElseIf test.All(Function(t) t Like numeric) Then
                Return "num"
            Else
                Return "n/a"
            End If
        End Function

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim init = from.Evaluate(envir)
            Dim stops = [to].Evaluate(envir)
            Dim offset = steps.Evaluate(envir)

            ' 1. 1:5 = [1,2,3,4,5]
            ' 2. 1.0:5.0 = [1.0,2.0,3.0,4.0,5.0]
            ' 3. A:E = [A,B,C,D,E]

            If Program.isException(init) Then
                Return init
            ElseIf Program.isException(stops) Then
                Return stops
            ElseIf Program.isException(offset) Then
                Return offset
            End If

            Select Case measureSequence(init, stops, offset)
                Case "num" : Return numericSeq(start:=REnv.getFirst(init), steps:=REnv.getFirst(offset), ends:=REnv.getFirst(stops))
                Case "int" : Return integerSeq(start:=REnv.getFirst(init), steps:=REnv.getFirst(offset), ends:=REnv.getFirst(stops))
                Case "chr"
                    Return characterSeq(
                        start:=getChar(REnv.getFirst(init)),
                        steps:=REnv.getFirst(offset),
                        ends:=getChar(REnv.getFirst(stops))
                    )
                Case Else
                    Return Internal.debug.stop({
                        $"data type of the sequence literal is not supported!",
                        $"init: {RType.TypeOf(init)}",
                        $"stops: {RType.TypeOf(stops)}",
                        $"offset: {RType.TypeOf(offset)}"
                    }, envir)
            End Select
        End Function

        Private Shared Function getChar(o As Object) As Char
            Dim s As String() = REnv.asVector(Of String)(o)
            Dim c As Char = s.First.FirstOrDefault

            Return c
        End Function

        Private Shared Function characterSeq(start As Char, steps As Integer, ends As Char) As Object
            Dim ints As Long() = integerSeq(AscW(start), steps, AscW(ends))
            Dim chrs As Char() = ints.Select(Function(i) ChrW(CInt(i))).ToArray

            Return chrs
        End Function

        Private Shared Function integerSeq(start As Long, steps As Integer, ends As Long) As Object
            Dim seq As New List(Of Long)

            If start > ends Then
                If steps = 1 Then
                    ' steps is the default value
                    steps = -1
                End If
            End If

            If steps > 0 Then
                Do While start <= ends
                    seq += start
                    start += steps
                Loop
            Else
                Do While start >= ends
                    seq += start
                    start += steps
                Loop
            End If

            Return seq.ToArray
        End Function

        Private Shared Function numericSeq(start As Double, steps As Double, ends As Double) As Object
            Dim seq As New List(Of Double)

            If start > ends Then
                If steps = 1.0 Then
                    ' steps is the default value
                    steps = -1
                End If
            End If

            If steps > 0 Then
                Do While start <= ends
                    seq += start
                    start += steps
                Loop
            Else
                Do While start >= ends
                    seq += start
                    start += steps
                Loop
            End If

            Return seq.ToArray
        End Function

        Public Overrides Function ToString() As String
            Return $"{from}:{[to]} step {steps}"
        End Function
    End Class
End Namespace
