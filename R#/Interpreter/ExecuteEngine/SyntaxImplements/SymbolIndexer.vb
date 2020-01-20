Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.ExecuteEngine.SyntaxImplements

    Module SymbolIndexerSyntax

        ''' <summary>
        ''' Simple indexer
        ''' </summary>
        ''' <param name="tokens">
        ''' ``a[x]``
        ''' </param>
        Public Function SymbolIndexer(tokens As Token()) As SyntaxResult
            Dim symbol = {tokens(Scan0)}.DoCall(AddressOf Expression.CreateExpression)
            Dim indexType As SymbolIndexers
            Dim index As SyntaxResult = Nothing

            If symbol.isException Then
                Return symbol
            End If

            tokens = tokens _
                .Skip(2) _
                .Take(tokens.Length - 3) _
                .ToArray

            If tokens.isStackOf("[", "]") Then
                tokens = tokens _
                    .Skip(1) _
                    .Take(tokens.Length - 2) _
                    .ToArray
                indexType = SymbolIndexers.nameIndex
                index = Expression.CreateExpression(tokens)
            Else
                Call tokens.parseIndex(index, indexType)
            End If

            If index.isException Then
                Return index
            End If

            Return New SymbolIndexer(symbol.expression, index.expression, indexType)
        End Function

        ''' <summary>
        ''' Complex indexer
        ''' 
        ''' ```
        ''' func(...)[x]
        ''' ```
        ''' </summary>
        ''' <param name="ref"></param>
        ''' <param name="indexer"></param>
        Public Function SymbolIndexer(ref As Token(), indexer As Token()) As SyntaxResult
            Dim symbol = Expression.CreateExpression(ref)
            Dim index As SyntaxResult = Nothing
            Dim indexType As SymbolIndexers

            If symbol.isException Then
                Return symbol
            End If

            indexer = indexer _
                .Skip(1) _
                .Take(indexer.Length - 2) _
                .ToArray

            Call indexer.parseIndex(index, indexType)

            If index.isException Then
                Return index
            End If

            Return New SymbolIndexer(symbol.expression, index.expression, indexType)
        End Function

        <Extension>
        Private Sub parseIndex(tokens As Token(), ByRef index As SyntaxResult, ByRef indexType As SymbolIndexers)
            Dim blocks As List(Of Token()) = tokens.SplitByTopLevelDelimiter(TokenType.comma, False)

            If blocks > 1 Then
                ' dataframe indexer
                blocks.parseDataframeIndex(index, indexType)
            Else
                ' vector indexer
                indexType = SymbolIndexers.vectorIndex
                index = Expression.CreateExpression(tokens)
            End If
        End Sub

        ''' <summary>
        ''' dataframe indexer
        ''' </summary>
        ''' <param name="blocks"></param>
        ''' <param name="index"></param>
        ''' <param name="indexType"></param>
        <Extension>
        Private Sub parseDataframeIndex(blocks As List(Of Token()), ByRef index As SyntaxResult, ByRef indexType As SymbolIndexers)
            If blocks(0).isComma Then
                ' x[, a] by columns
                indexType = SymbolIndexers.dataframeColumns
                index = Expression.CreateExpression(blocks.Skip(1).IteratesALL)
            ElseIf blocks = 2 AndAlso blocks(1).isComma Then
                ' x[a, ] by row
                indexType = SymbolIndexers.dataframeRows
                index = Expression.CreateExpression(blocks(Scan0))
            Else
                Dim elements As New List(Of Expression)

                ' x[a,b] by range
                indexType = SymbolIndexers.dataframeRanges

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
        End Sub
    End Module
End Namespace