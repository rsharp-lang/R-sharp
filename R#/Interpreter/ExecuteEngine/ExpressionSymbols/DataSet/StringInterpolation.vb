#Region "Microsoft.VisualBasic::4d80b2d1b259a87eff53cad393c5d8a8, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\StringInterpolation.vb"

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

    '     Class StringInterpolation
    ' 
    '         Properties: type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class StringInterpolation : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.string
            End Get
        End Property

        ''' <summary>
        ''' 这些表达式产生的全部都是字符串结果值
        ''' </summary>
        ReadOnly stringParts As Expression()

        Sub New(token As Token)
            Dim tokens = TokenIcer.StringInterpolation.ParseTokens(token.text)
            Dim block = tokens.SplitByTopLevelDelimiter(TokenType.stringLiteral)
            Dim parts As New List(Of Expression)

            For Each part As Token() In block
                If part.isLiteral(TokenType.stringLiteral) Then
                    parts += New Literal(part(Scan0))
                Else
                    parts += part _
                        .Skip(1) _
                        .Take(part.Length - 2) _
                        .DoCall(AddressOf Expression.CreateExpression)
                End If
            Next

            stringParts = parts
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim current As Array = Runtime.asVector(Of String)(stringParts(Scan0).Evaluate(envir))
            Dim [next] As Object

            For Each part As Expression In stringParts.Skip(1)
                [next] = part.Evaluate(envir)

                With Runtime.asVector(Of Object)([next])
                    If .Length = 1 Then
                        [next] = .GetValue(Scan0)
                    End If
                End With

                If Program.isException([next]) Then
                    Return [next]
                Else
                    current = StringBinaryExpression.DoStringBinary(Of String)(
                        a:=current,
                        b:=[next],
                        op:=Function(x, y) x & y
                    )
                End If
            Next

            Return current
        End Function

        Public Overrides Function ToString() As String
            Return stringParts.JoinBy(" & ")
        End Function
    End Class
End Namespace
