#Region "Microsoft.VisualBasic::c27da06c46f2d3414cbf08a8aa333ad0, R-sharp\R#\Interpreter\Syntax\SyntaxTree\BinaryExpressionTree\PipelineProcessor.vb"

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

    '   Total Lines: 137
    '    Code Lines: 109
    ' Comment Lines: 7
    '   Blank Lines: 21
    '     File Size: 5.41 KB


    '     Class PipelineProcessor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: buildPipeline, expression, isFunctionTuple, view
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Interpreter.SyntaxParser

    Friend Class PipelineProcessor : Inherits GenericSymbolOperatorProcessor

        Public Sub New(symbols As String())
            MyBase.New(symbols)
        End Sub

        Protected Overrides Function view() As String
            Return "pipeTo -> next()"
        End Function

        Protected Overrides Function expression(a As [Variant](Of SyntaxResult, String), b As [Variant](Of SyntaxResult, String), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim pip As Expression

            If a.VA.isException Then
                Return a
            ElseIf b.VA.isException Then
                Return b
            Else
                pip = buildPipeline(a.VA.expression, b.VA.expression, opts)
            End If

            If pip Is Nothing Then
                If b.VA.expression.DoCall(AddressOf isFunctionTuple) Then
                    Dim invokes As VectorLiteral = b.VA.expression
                    Dim calls As New List(Of Expression)

                    For Each [call] As Expression In invokes
                        calls += buildPipeline(a.VA.expression, [call], opts)
                    Next

                    Return New SyntaxResult(New VectorLiteral(calls))
                Else
                    Return SyntaxResult.CreateError(New SyntaxErrorException, opts)
                End If
            Else
                Return New SyntaxResult(pip)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function isFunctionTuple(b As Expression) As Boolean
            If Not TypeOf b Is VectorLiteral Then
                Return False
            ElseIf Not DirectCast(b, VectorLiteral) _
                .All(Function(e)
                         Return TypeOf e Is FunctionInvoke OrElse TypeOf e Is SymbolReference
                     End Function) Then

                Return False
            End If

            Return True
        End Function

        Public Shared Function buildPipeline(a As Expression, b As Expression, opts As SyntaxBuilderOptions) As Expression
            Dim pip As FunctionInvoke

            If TypeOf a Is VectorLiteral Then
                With DirectCast(a, VectorLiteral)
                    If .length = 1 AndAlso TypeOf .First Is ValueAssignExpression Then
                        a = .First
                    End If
                End With
            End If

            If TypeOf b Is FunctionInvoke Then
                pip = b
                pip.parameters = pip.parameters _
                    .AsList _
                    .With(Sub(list)
                              Call list.Insert(Scan0, a)
                          End Sub) _
                    .ToArray

            ElseIf TypeOf b Is DeclareNewFunction Then
                ' anonymousPipeline
                '
                ' a |> (function(x) {
                '   # xxx
                ' }) 
                '
                pip = New FunctionInvoke(b, DirectCast(b, DeclareNewFunction).stackFrame, a)

            ElseIf TypeOf b Is SymbolReference Then
                Dim name$ = DirectCast(b, SymbolReference).symbol
                Dim stacktrace As New StackFrame With {
                    .File = opts.source.fileName,
                    .Line = "n/a",
                    .Method = New Method With {
                        .Method = name,
                        .[Module] = "call_function",
                        .[Namespace] = SyntaxBuilderOptions.R_runtime
                    }
                }

                pip = New FunctionInvoke(name, stacktrace, a)
            ElseIf TypeOf b Is SymbolIndexer Then
                Dim ref As SymbolIndexer = DirectCast(b, SymbolIndexer)

                If ref.indexType <> SymbolIndexers.nameIndex Then
                    pip = Nothing
                Else
                    ' member of the symbol could be a function
                    Dim stacktrace As New StackFrame With {
                        .File = opts.source.fileName,
                        .Line = "n/a",
                        .Method = New Method With {
                            .Method = ref.ToString,
                            .[Module] = "call_function",
                            .[Namespace] = SyntaxBuilderOptions.R_runtime
                        }
                    }

                    pip = New FunctionInvoke(ref, stacktrace, a)
                End If
            ElseIf TypeOf b Is NamespaceFunctionSymbolReference Then
                pip = New FunctionInvoke(b, DirectCast(b, IRuntimeTrace).stackFrame, a)
            Else
                pip = Nothing
            End If

            Return pip
        End Function
    End Class
End Namespace
