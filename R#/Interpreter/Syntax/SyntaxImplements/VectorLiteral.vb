Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module VectorLiteralSyntax

        Public Function VectorLiteral(tokens As Token()) As SyntaxResult
            Dim blocks As List(Of Token()) = tokens _
                .Skip(1) _
                .Take(tokens.Length - 2) _
                .SplitByTopLevelDelimiter(TokenType.comma)
            Dim values As New List(Of Expression)
            Dim syntaxTemp As SyntaxResult

            For Each block As Token() In blocks
                If Not (block.Length = 1 AndAlso block(Scan0).name = TokenType.comma) Then
                    syntaxTemp = block.DoCall(AddressOf Expression.CreateExpression)

                    If syntaxTemp.isException Then
                        Return syntaxTemp
                    Else
                        values.Add(syntaxTemp.expression)
                    End If
                End If
            Next

            ' 还会剩余最后一个元素
            ' 所以在这里需要加上
            Return New SyntaxResult(New VectorLiteral(values, values.DoCall(AddressOf TypeCodeOf)))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Function TypeCodeOf(values As IEnumerable(Of Expression)) As TypeCodes
            With values.ToArray
                ' fix for System.InvalidOperationException: Nullable object must have a value.
                '
                If .Length = 0 Then
                    Return TypeCodes.generic
                Else
                    Return values _
                        .GroupBy(Function(exp) exp.type) _
                        .OrderByDescending(Function(g) g.Count) _
                        .First _
                       ?.Key
                End If
            End With
        End Function
    End Module
End Namespace