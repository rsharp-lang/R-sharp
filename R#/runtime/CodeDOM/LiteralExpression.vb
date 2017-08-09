#Region "Microsoft.VisualBasic::731eee7d952897c32e0fd08832757a87, ..\R-sharp\R#\runtime\CodeDOM\LiteralExpression.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Interpreter

Namespace Runtime.CodeDOM

    ''' <summary>
    ''' 数字，字符串，逻辑值之类的值表达式，也可以称作为常数
    ''' 
    ''' ```R
    ''' # integer numeric literal
    ''' 12345;
    ''' 
    ''' # string literal
    ''' "hello world!"
    ''' 
    ''' # boolean literal
    ''' TRUE
    ''' ```
    ''' </summary>
    Public Class LiteralExpression : Inherits PrimitiveExpression
        Implements Value(Of String).IValueOf

        ''' <summary>
        ''' ``Nothing`` in VisualBasic
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NULL As New NULL

        Public Property Value As String Implements Value(Of String).IValueOf.value
        Public ReadOnly Property Type As LanguageTokens

        Sub New(token As Token(Of LanguageTokens))
            Call Me.New(token.Text, token.name)
        End Sub

        Sub New(value$, type As LanguageTokens)

        End Sub

        Public Overrides Function Evaluate(envir As Environment) As TempValue
            Select Case Type
                Case LanguageTokens.String
                    Return TempValue.Tuple(Value, Type)
                Case LanguageTokens.Numeric
                    Return TempValue.Tuple(Val(Value), Type)
                Case LanguageTokens.Boolean
                    Return TempValue.Tuple(Value.ParseBoolean, Type)
                Case Else
                    Throw New InvalidExpressionException($"Expression ""{Value}"" is a unrecognized data literal!")
            End Select
        End Function
    End Class

    ''' <summary>
    ''' No values
    ''' </summary>
    Public Class NULL : Inherits LiteralExpression

        Sub New()
            Call MyBase.New(New Token(Of LanguageTokens)(LanguageTokens.Object, "NULL"))
        End Sub

        Public Overrides Function ToString() As String
            Return NameOf(NULL)
        End Function

        ''' <summary>
        ''' ``NULL`` means no value in R
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Overrides Function Evaluate(envir As Environment) As TempValue
            Return Nothing
        End Function
    End Class

    Public Class StringLiteral : Inherits PrimitiveExpression


    End Class
End Namespace
