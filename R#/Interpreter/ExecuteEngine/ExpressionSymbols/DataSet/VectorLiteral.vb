#Region "Microsoft.VisualBasic::2b28eaa5e2f66c04dc3e3c5f002eac88, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\VectorLiteral.vb"

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

    '     Class VectorLiteral
    ' 
    '         Properties: type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToArray, ToString
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

    Public Class VectorLiteral : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        ReadOnly values As Expression()

        Sub New(tokens As Token())
            Dim blocks As List(Of Token()) = tokens _
                .Skip(1) _
                .Take(tokens.Length - 2) _
                .SplitByTopLevelDelimiter(TokenType.comma)
            Dim values As New List(Of Expression)

            For Each block As Token() In blocks
                If Not (block.Length = 1 AndAlso block(Scan0).name = TokenType.comma) Then
                    Call values.Add(block.DoCall(AddressOf Expression.CreateExpression))
                End If
            Next

            ' 还会剩余最后一个元素
            ' 所以在这里需要加上
            Me.values = values
            Me.type = values _
                .GroupBy(Function(exp) exp.type) _
                .OrderByDescending(Function(g) g.Count) _
                .FirstOrDefault _
               ?.Key
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim vector = values _
                .Select(Function(exp) exp.Evaluate(envir)) _
                .ToArray
            Dim result As Array = Environment.asRVector(type, vector)

            Return result
        End Function

        Public Function ToArray() As Expression()
            Return values.ToArray
        End Function

        Public Overrides Function ToString() As String
            Return $"[{values.JoinBy(", ")}]"
        End Function
    End Class
End Namespace
