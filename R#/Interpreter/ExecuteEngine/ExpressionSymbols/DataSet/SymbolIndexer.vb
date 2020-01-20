#Region "Microsoft.VisualBasic::c50e074089e71bebd928728d2e0758f0, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\SymbolIndexer.vb"

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

    '     Enum SymbolIndexers
    ' 
    '         dataframeColumns, dataframeRanges, dataframeRows, nameIndex, vectorIndex
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class SymbolIndexer
    ' 
    '         Properties: type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: emptyIndexError, Evaluate, getByIndex, getByName, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Interpreter.ExecuteEngine

    Public Enum SymbolIndexers
        ''' <summary>
        ''' a[x]
        ''' </summary>
        vectorIndex
        ''' <summary>
        ''' a[[x]], a$x
        ''' </summary>
        nameIndex
        ''' <summary>
        ''' a[, x]
        ''' </summary>
        dataframeColumns
        ''' <summary>
        ''' a[x, ]
        ''' </summary>
        dataframeRows
        ''' <summary>
        ''' a[x,y]
        ''' </summary>
        dataframeRanges
    End Enum

    ''' <summary>
    ''' get/set elements by index 
    ''' (X$name或者X[[name]])
    ''' </summary>
    Public Class SymbolIndexer : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Friend ReadOnly index As Expression
        Friend ReadOnly symbol As Expression

        ''' <summary>
        ''' X[[name]]
        ''' </summary>
        Friend ReadOnly indexType As SymbolIndexers

        ''' <summary>
        ''' Simple indexer
        ''' </summary>
        ''' <param name="tokens">
        ''' ``a[x]``
        ''' </param>
        Sub New(tokens As Token())
            symbol = {tokens(Scan0)}.DoCall(AddressOf Expression.CreateExpression)
            tokens = tokens.Skip(2).Take(tokens.Length - 3).ToArray

            If tokens.isStackOf("[", "]") Then
                tokens = tokens _
                    .Skip(1) _
                    .Take(tokens.Length - 2) _
                    .ToArray
                indexType = SymbolIndexers.nameIndex
                index = Expression.CreateExpression(tokens)
            Else
                Call parseIndex(tokens, index, indexType)
            End If
        End Sub

        Private Shared Sub parseIndex(tokens As Token(), ByRef index As SyntaxResult, ByRef indexType As SymbolIndexers)
            Dim blocks = tokens.SplitByTopLevelDelimiter(TokenType.comma, False)

            If blocks > 1 Then
                ' dataframe indexer
                If blocks(0).isComma Then
                    ' x[, a] by columns
                    indexType = SymbolIndexers.dataframeColumns
                    index = Expression.CreateExpression(blocks.Skip(1).IteratesALL)
                ElseIf blocks = 2 AndAlso blocks(1).isComma Then
                    ' x[a, ] by row
                    indexType = SymbolIndexers.dataframeRows
                    index = Expression.CreateExpression(blocks(Scan0))
                Else
                    ' x[a,b] by range
                    indexType = SymbolIndexers.dataframeRanges

                    Dim elements As New List(Of Expression)

                    For Each result As SyntaxResult In blocks _
                        .Where(Function(t) Not t.isComma) _
                        .Select(AddressOf Expression.CreateExpression)

                        If result.isException Then
                            index = result
                            Return
                        Else
                            elements.Add(result.expression)
                        End If
                    Next

                    index = New VectorLiteral(elements.ToArray)
                End If
            Else
                ' vector indexer
                indexType = SymbolIndexers.vectorIndex
                index = Expression.CreateExpression(tokens)
            End If
        End Sub

        ''' <summary>
        ''' Complex indexer
        ''' 
        ''' ```
        ''' func(...)[x]
        ''' ```
        ''' </summary>
        ''' <param name="ref"></param>
        ''' <param name="indexer"></param>
        Sub New(ref As Token(), indexer As Token())
            symbol = Expression.CreateExpression(ref)
            indexer = indexer _
                .Skip(1) _
                .Take(indexer.Length - 2) _
                .ToArray

            Call parseIndex(indexer, index, indexType)
        End Sub

        ''' <summary>
        ''' symbol$byName
        ''' </summary>
        ''' <param name="symbol"></param>
        ''' <param name="byName"></param>
        Sub New(symbol As Expression, byName As Expression)
            Me.symbol = symbol
            Me.index = byName
            Me.indexType = SymbolIndexers.nameIndex
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim obj As Object = symbol.Evaluate(envir)
            Dim indexer = Runtime.asVector(Of Object)(index.Evaluate(envir))

            If indexer.Length = 0 Then
                Return emptyIndexError(Me, envir)
            ElseIf Program.isException(obj) Then
                Return obj
            ElseIf obj Is Nothing Then
                Return Nothing
            End If

            ' now obj is always have values:
            If indexType = SymbolIndexers.nameIndex Then
                ' a[[name]]
                ' a$name
                Return getByName(obj, indexer, envir)
            ElseIf indexType = SymbolIndexers.vectorIndex Then
                ' a[name]
                ' a[index]
                Return getByIndex(obj, indexer, envir)
            ElseIf indexType = SymbolIndexers.dataframeColumns Then
                Return getColumn(obj, indexer, envir)
            Else
                Return Internal.stop(New NotImplementedException(indexType.ToString), envir)
            End If
        End Function

        Private Function getColumn(obj As dataframe, indexer As Array, envir As Environment) As Object
            If indexer.Length = 0 Then
                Return Nothing
            ElseIf indexer.Length = 1 Then
                Return obj.GetColumnVector(Scripting.ToString(indexer.GetValue(Scan0)))
            Else
                ' dataframe projection
                Return obj.projectByColumn(indexer)
            End If
        End Function

        Friend Shared Function emptyIndexError(symbol As SymbolIndexer, env As Environment) As Message
            Return Internal.stop({
                $"attempt to select less than one element in OneIndex!",
                $"SymbolName: {symbol.symbol}",
                $"Index: {symbol.index}"
            }, env)
        End Function

        Private Function getByName(obj As Object, indexer As Array, envir As Environment) As Object
            If Not obj.GetType.ImplementInterface(GetType(RNameIndex)) Then
                Return Internal.stop("Target object can not be indexed by name!", envir)
            ElseIf indexer.Length = 1 Then
                Return DirectCast(obj, RNameIndex).getByName(Scripting.ToString(indexer.GetValue(Scan0)))
            Else
                Return DirectCast(obj, RNameIndex).getByName(Runtime.asVector(Of String)(indexer))
            End If
        End Function

        ''' <summary>
        ''' vec[names/x] or list[names/index]
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="indexer"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Private Function getByIndex(obj As Object, indexer As Array, envir As Environment) As Object
            If obj.GetType Is GetType(list) Then
                Dim list As list = DirectCast(obj, list)

                If Runtime.isVector(Of String)(indexer) Then
                    ' get by names
                    Return list.getByName(Runtime.asVector(Of String)(indexer))
                Else
                    ' get by index
                    Return list.getByIndex(Runtime.asVector(Of Integer)(indexer))
                End If
            Else
                Dim sequence As Array = Runtime.asVector(Of Object)(obj)
                Dim Rarray As RIndex

                If sequence.Length = 0 Then
                    Return Nothing
                ElseIf sequence.Length = 1 AndAlso sequence.GetValue(Scan0).GetType.ImplementInterface(GetType(RIndex)) Then
                    Rarray = sequence.GetValue(Scan0)

                    '' by element index
                    'If Not sequence.GetType.ImplementInterface(GetType(RIndex)) Then
                    '    Return Internal.stop("Target object can not be indexed!", envir)
                    'End If
                Else
                    Rarray = New vector With {.data = sequence}
                End If

                If indexer.Length = 1 Then
                    Return Rarray.getByIndex(CInt(indexer.GetValue(Scan0)))
                Else
                    Return Rarray.getByIndex(Runtime.asVector(Of Integer)(indexer))
                End If
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"{symbol}[{index}]"
        End Function
    End Class
End Namespace
