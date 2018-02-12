#Region "Microsoft.VisualBasic::3c9d533befcd88f637e233a843bac153, R#\runtime\CodeDOM\PrimitiveExpression.vb"

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

    '     Structure TempValue
    ' 
    '         Function: ToString, Tuple
    ' 
    '     Class PrimitiveExpression
    ' 
    '         Function: ToString
    ' 
    '     Class ValueExpression
    ' 
    '         Function: Evaluate, ToString
    ' 
    '         Sub: New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Interpreter.Expression
Imports SMRUCC.Rsharp.Interpreter.Language

Namespace Runtime.CodeDOM

    Public Structure TempValue

        Dim type As TypeCodes
        Dim value As Object

        Public Shared Function Tuple(value As Object, type As TypeCodes) As TempValue
            Return New TempValue With {
                .type = type,
                .value = value
            }
        End Function

        Public Overrides Function ToString() As String
            Return $"[{type}] {CStrSafe(value)}"
        End Function
    End Structure

    ''' <summary>
    ''' The very base expression in the R# language
    ''' </summary>
    Public MustInherit Class PrimitiveExpression

        Public MustOverride Function Evaluate(envir As Environment) As TempValue

        Public Overrides Function ToString() As String
            Return "R# Expression"
        End Function
    End Class

    ''' <summary>
    ''' The expression which can produce the values
    ''' </summary>
    Public Class ValueExpression : Inherits PrimitiveExpression

        ReadOnly tokens As Token(Of Tokens)()

        ''' <summary>
        ''' 构建一个表达式求值的数
        ''' </summary>
        ''' <param name="tokens"></param>
        Sub New(tokens As IEnumerable(Of Token(Of Tokens)))
            Me.tokens = tokens.ToArray
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As TempValue
            Dim buffer As New Pointer(Of Token(Of Tokens))(tokens)
            Dim tree As SimpleExpression = buffer.TryParse(envir, False)
            Dim out = tree.Evaluate(envir)
            Return out
        End Function

        Public Overrides Function ToString() As String
            Return tokens _
                .SafeQuery _
                .Select(Function(t) t.Text) _
                .JoinBy(" ")
        End Function
    End Class
End Namespace
