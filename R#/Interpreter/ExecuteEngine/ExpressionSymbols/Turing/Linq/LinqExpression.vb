#Region "Microsoft.VisualBasic::091231756e3a7b0873fbaa5b4931e487, R#\Interpreter\ExecuteEngine\ExpressionSymbols\LinqExpression.vb"

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
'         Properties: type
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: Evaluate, produceSequenceVector
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Interpreter.ExecuteEngine.Linq

    ' from x in seq let y = ... where test select projection order by ... asceding distinct 

    Public Class LinqExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return sequence.type
            End Get
        End Property

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
        Dim locals As DeclareNewVariable()
        Dim program As ClosureExpression
        ''' <summary>
        ''' select
        ''' </summary>
        Dim projection As Expression

        ''' <summary>
        ''' 排序之类的操作都被转换为了函数调用
        ''' </summary>
        Dim output As FunctionInvoke()

        Sub New(locals As IEnumerable(Of DeclareNewVariable),
                sequence As Expression,
                program As ClosureExpression,
                projection As Expression,
                output As IEnumerable(Of FunctionInvoke))

            Me.locals = locals.ToArray
            Me.sequence = sequence
            Me.program = program
            Me.projection = projection
            Me.output = output.ToArray
        End Sub

        Friend Shared Function produceSequenceVector(seq As Expression, env As Environment, ByRef isList As Boolean) As Object
            Dim sequence As Object = seq.Evaluate(env)

            If sequence Is Nothing Then
                Return {}
            ElseIf Interpreter.Program.isException(sequence) Then
                Return sequence
            ElseIf sequence.GetType Is GetType(list) Then
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
            Dim envir As New Environment(parent, "linq_closure")
            Dim result As New List(Of LinqOutputUnit)
            Dim key$
            Dim from As Expression() = locals(Scan0).names _
                .Select(Function(name)
                            Return New Literal(name)
                        End Function) _
                .ToArray
            Dim isList As Boolean = False

            ' 20191105
            ' 序列的产生需要放在变量申明之前
            ' 否则linq表达式中的与外部环境中的同名变量会导致NULL错误出现
            Dim source As Object = produceSequenceVector(Me.sequence, envir, isList)

            If Interpreter.Program.isException(source) Then
                Return source
            End If

            Dim sequence As Array = DirectCast(source, Array)
            Dim getSortKey As Expression = output.Where(Function(e) e.funcName = "sort").FirstOrDefault

            For Each local As DeclareNewVariable In locals
                Call local.Evaluate(envir)
            Next

            For i As Integer = 0 To sequence.Length - 1
                Dim item As Object = sequence.GetValue(i)

                If isList Then
                    With DirectCast(item, KeyValuePair(Of String, Object))
                        key = .Key
                        item = .Value
                    End With
                Else
                    key = i + 1
                End If

                ' from xxx in sequence
                ValueAssign.doValueAssign(envir, from, False, item)
                ' run program
                item = program.Evaluate(envir)
                ' if pass the which filter
                If Not item Is Nothing AndAlso item.GetType Is GetType(ReturnValue) Then
                    Continue For
                Else
                    result += New LinqOutputUnit With {
                        .key = key,
                        .value = projection.Evaluate(envir),
                        .sortKey =
                    }
                End If
            Next

            If output.isEmpty Then
                Return result
            Else
                envir.Push("$", result)

                Return output.Evaluate(envir)
            End If
        End Function
    End Class
End Namespace
