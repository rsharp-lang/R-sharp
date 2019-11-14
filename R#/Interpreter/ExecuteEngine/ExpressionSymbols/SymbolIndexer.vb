Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Language

Namespace Interpreter.ExecuteEngine

    Public Class SymbolIndexer : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Dim index As Expression
        Dim symbol As Expression
        ''' <summary>
        ''' X[[name]]
        ''' </summary>
        Dim nameIndex As Boolean = False

        Sub New(tokens As Token())
            symbol = {tokens(Scan0)}.DoCall(AddressOf Expression.CreateExpression)
            tokens = tokens.Skip(2).Take(tokens.Length - 3).ToArray

            If tokens(Scan0) = (TokenType.open, "[") AndAlso tokens.Last = (TokenType.close, "]") Then
                nameIndex = True
                tokens = tokens.Skip(2).Take(tokens.Length - 3).ToArray
            End If

            index = Expression.CreateExpression(tokens)
        End Sub

        Sub New(symbol As Expression, byName As Expression)
            Me.symbol = symbol
            Me.index = byName
            Me.nameIndex = True
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim sequence = Runtime.asVector(Of Object)(symbol.Evaluate(envir))
            Dim indexer = Runtime.asVector(Of Object)(index.Evaluate(envir))

            If indexer.Length = 0 Then
                Return Internal.stop({
                    $"Attempt to select less than one element in get1index",
                    $"expression: {symbol}[[{index}]]"
                }, envir)
            End If

            If nameIndex Then
            Else
                ' by element index

            End If
        End Function
    End Class
End Namespace