Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

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
            ElseIf sequence Is Nothing OrElse sequence.Length = 0 Then
                Return Nothing
            End If

            If nameIndex Then
                If Not sequence.GetType.ImplementInterface(GetType(RNameIndex)) Then
                    Return Internal.stop("Target object can not be indexed by name!", envir)
                ElseIf indexer.Length = 1 Then
                    Return DirectCast(sequence, RNameIndex).getByName(Scripting.ToString(indexer.GetValue(Scan0)))
                Else
                    Return DirectCast(sequence, RNameIndex).getByName(Runtime.asVector(Of String)(indexer))
                End If
            Else
                ' by element index
                If Not sequence.GetType.ImplementInterface(GetType(RIndex)) Then
                    Return Internal.stop("Target object can not be indexed!", envir)
                ElseIf indexer.Length = 1 Then
                    Return DirectCast(sequence, RIndex).getByIndex(CInt(indexer.GetValue(Scan0)))
                Else
                    Return DirectCast(sequence, RIndex).getByIndex(Runtime.asVector(Of Integer)(indexer))
                End If
            End If
        End Function
    End Class
End Namespace