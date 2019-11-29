#Region "Microsoft.VisualBasic::df581c58e0576e84aad5af194c1783c5, R#\Interpreter\ExecuteEngine\ExpressionSymbols\SymbolIndexer.vb"

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

    '     Class SymbolIndexer
    ' 
    '         Properties: type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Evaluate, ToString
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

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' get elements by index 
    ''' (X$name或者X[[name]])
    ''' </summary>
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
        Sub New(symbol As Expression, byName As Expression)
            Me.symbol = symbol
            Me.index = byName
            Me.nameIndex = True
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim obj As Object = symbol.Evaluate(envir)
            Dim indexer = Runtime.asVector(Of Object)(index.Evaluate(envir))

            If indexer.Length = 0 Then
                Return Internal.stop({
                    $"Attempt to select less than one element in get1index",
                    $"expression: {symbol}[[{index}]]"
                }, envir)
            End If

            If nameIndex Then
                Return getByName(obj, indexer, envir)
            Else
                Return getByIndex(obj, indexer, envir)
            End If
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

        Private Function getByIndex(obj As Object, indexer As Array, envir As Environment) As Object
            Dim sequence As Array = Runtime.asVector(Of Object)(obj)

            If sequence Is Nothing OrElse sequence.Length = 0 Then
                Return Nothing
            ElseIf sequence.Length = 1 AndAlso sequence.GetValue(Scan0).GetType.ImplementInterface(GetType(IReflector)) Then
                sequence = sequence.GetValue(Scan0)
            End If

            ' by element index
            If Not sequence.GetType.ImplementInterface(GetType(RIndex)) Then
                Return Internal.stop("Target object can not be indexed!", envir)
            ElseIf indexer.Length = 1 Then
                Return DirectCast(sequence, RIndex).getByIndex(CInt(indexer.GetValue(Scan0)))
            Else
                Return DirectCast(sequence, RIndex).getByIndex(Runtime.asVector(Of Integer)(indexer))
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"{symbol}[{index}]"
        End Function
    End Class
End Namespace
