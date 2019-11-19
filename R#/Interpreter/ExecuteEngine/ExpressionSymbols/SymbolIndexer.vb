Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal

Namespace Interpreter.ExecuteEngine

    Public Class SymbolIndexer : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Friend ReadOnly index As Expression
        Friend ReadOnly symbol As Expression

        ''' <summary>
        ''' X[[name]]
        ''' </summary>
        Friend ReadOnly nameIndex As Boolean = False

        Sub New(tokens As Token())
            symbol = {tokens(Scan0)}.DoCall(AddressOf Expression.CreateExpression)
            tokens = tokens.Skip(2).Take(tokens.Length - 3).ToArray

            If tokens(Scan0) = (TokenType.open, "[") AndAlso tokens.Last = (TokenType.close, "]") Then
                nameIndex = True
                tokens = tokens _
                    .Skip(1) _
                    .Take(tokens.Length - 2) _
                    .ToArray
            End If

            index = Expression.CreateExpression(tokens)
        End Sub

        ''' <summary>
        ''' symbol$byName
        ''' </summary>
        ''' <param name="symbol"></param>
        ''' <param name="byName"></param>
        Sub New(symbol As Expression, byName As String)
            Me.symbol = symbol
            Me.index = New Literal(byName)
            Me.nameIndex = True
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim sequence As Object = Runtime.asVector(Of Object)(symbol.Evaluate(envir))
            Dim indexer = Runtime.asVector(Of Object)(index.Evaluate(envir))

            If indexer.Length = 0 Then
                Return Internal.stop({
                    $"Attempt to select less than one element in get1index",
                    $"expression: {symbol}[[{index}]]"
                }, envir)
            ElseIf sequence Is Nothing OrElse sequence.Length = 0 Then
                Return Nothing
            ElseIf sequence.Length = 1 AndAlso sequence.GetValue(Scan0).GetType Is GetType(list) Then
                sequence = sequence.GetValue(Scan0)
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

        Public Overrides Function ToString() As String
            Return $"{symbol}[{index}]"
        End Function
    End Class
End Namespace