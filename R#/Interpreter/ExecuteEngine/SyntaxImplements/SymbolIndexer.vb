Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.ExecuteEngine.SyntaxImplements

    Module SymbolIndexerSyntax

        Public Function SymbolIndexer(tokens As Token()) As SyntaxResult

        End Function

        Private Sub parseIndex(tokens As Token(), ByRef index As SyntaxResult, ByRef indexType As SymbolIndexers)
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
    End Module
End Namespace