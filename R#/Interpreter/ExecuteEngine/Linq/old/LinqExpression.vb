#Region "Microsoft.VisualBasic::e4271e1ae5c7f10e992e8dae625f5bd2, R#\Interpreter\ExecuteEngine\Linq\old\LinqExpression.vb"

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

    '     Class LinqExpression
    ' 
    '         Properties: expressionName, stackFrame, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, populateOutputs, produceSequenceVector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Namespace Interpreter.ExecuteEngine.LINQ

    ' from x in seq let y = ... where test select projection order by ... asceding distinct 

    Public Class LinqExpression : Inherits Expression
        Implements IRuntimeTrace

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return sequence.type
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.LinqDeclare
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        ''' <summary>
        ''' in
        ''' </summary>
        Dim sequence As Expression
        ''' <summary>
        ''' from/let
        ''' </summary>
        ''' <remarks>
        ''' 第一个元素是from申明的
        ''' 剩余的元素都是let语句申明的
        ''' </remarks>
        Dim locals As DeclareNewSymbol()
        Dim program As ClosureExpression
        ''' <summary>
        ''' select
        ''' </summary>
        Dim projection As Expression

        ''' <summary>
        ''' 排序之类的操作都被转换为了函数调用
        ''' </summary>
        Friend output As ExpressionSymbols.Closure.FunctionInvoke()

        Sub New(locals As IEnumerable(Of DeclareNewSymbol),
                sequence As Expression,
                program As ClosureExpression,
                projection As Expression,
                output As IEnumerable(Of ExpressionSymbols.Closure.FunctionInvoke),
                stackframe As StackFrame)

            Me.locals = locals.ToArray
            Me.sequence = sequence
            Me.program = program
            Me.projection = projection
            Me.output = output.ToArray
            Me.stackFrame = stackframe
        End Sub

        Friend Shared Function produceSequenceVector(sequence As Object, ByRef isList As Boolean) As Object
            If sequence.GetType Is GetType(list) Then
                sequence = DirectCast(sequence, list).slots
            End If

            If sequence.GetType Is GetType(Dictionary(Of String, Object)) Then
                sequence = DirectCast(sequence, Dictionary(Of String, Object)).ToArray
                isList = True
            ElseIf sequence.GetType.ImplementInterface(GetType(IDictionary)) Then
                With DirectCast(sequence, IDictionary)
                    sequence = (From key In .Keys.AsQueryable
                                Let keyStr As String = Scripting.ToString(key)
                                Let keyVal As Object = .Item(key)
                                Select New KeyValuePair(Of String, Object)(keyStr, keyVal)).ToArray
                    isList = True
                End With
            Else
                sequence = Runtime.asVector(Of Object)(sequence)
            End If

            Return sequence
        End Function

        Public Overrides Function Evaluate(parent As Environment) As Object
            Dim env As New Environment(parent, stackFrame, isInherits:=False)
            Dim isList As Boolean = False
            Dim sequence As Object = Me.sequence.Evaluate(env)

            If sequence Is Nothing Then
                Return {}
            ElseIf RProgram.isException(sequence) Then
                Return sequence
            End If

            ' 20191105
            ' 序列的产生需要放在变量申明之前
            ' 否则linq表达式中的与外部环境中的同名变量会导致NULL错误出现
            Dim source As Object = produceSequenceVector(sequence, isList)

            If RProgram.isException(source) Then
                Return source
            End If

            Dim result As New List(Of LinqOutputUnit)

            For Each unit In populateOutputs(env, source, isList)
                If RProgram.isException(unit.value) Then
                    Return unit.value
                Else
                    result.Add(unit)
                End If
            Next

            Dim output As Object = Me.populateOutput(result)

            Return output
        End Function

        Private Iterator Function populateOutputs(env As Environment, source As Object, isList As Boolean) As IEnumerable(Of LinqOutputUnit)
            Dim key$
            Dim from As Expression() = locals(Scan0).names _
                .Select(Function(name)
                            Return New Literal(name)
                        End Function) _
                .ToArray
            Dim sequence As Array = DirectCast(source, Array)
            Dim getSortKey = getOutputSort
            Dim item As Object
            Dim sortKey As Object

            For Each local As DeclareNewSymbol In locals
                Call local.Evaluate(env)
            Next

            For i As Integer = 0 To sequence.Length - 1
                item = sequence.GetValue(i)

                If isList Then
                    With DirectCast(item, KeyValuePair(Of String, Object))
                        key = .Key
                        item = .Value
                    End With
                Else
                    key = i + 1
                End If

                ' from xxx in sequence
                ValueAssign.doValueAssign(env, from, False, item)
                ' run program
                item = program.Evaluate(env)
                ' if pass the which filter
                If Not item Is Nothing AndAlso item.GetType Is GetType(ReturnValue) Then
                    Continue For
                Else
                    If getSortKey.indexBy Is Nothing Then
                        sortKey = Nothing
                    Else
                        sortKey = getSortKey.indexBy.Evaluate(env)
                    End If

                    Yield New LinqOutputUnit With {
                        .key = key,
                        .value = projection.Evaluate(env),
                        .sortKey = sortKey
                    }
                End If
            Next
        End Function
    End Class
End Namespace
