#Region "Microsoft.VisualBasic::67309d048afff977ace42b7079f86f94, R#\runtime\CodeDOM\LiteralExpression.vb"

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

    '     Class LiteralExpression
    ' 
    '         Properties: NULL, Type, Value
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Evaluate
    ' 
    '     Class NULL
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    '     Class StringLiteral
    ' 
    '         Function: Evaluate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Interpreter.Language

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
        Public ReadOnly Property Type As Tokens

        Sub New(token As Token(Of Tokens))
            Call Me.New(token.Text, token.name)
        End Sub

        Sub New(value$, type As Tokens)

        End Sub

        Public Overrides Function Evaluate(envir As Environment) As TempValue
            Select Case Type
                Case Tokens.String
                    Return TempValue.Tuple(Value, Type)
                Case Tokens.Numeric
                    Return TempValue.Tuple(Val(Value), Type)
                Case Tokens.Boolean
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
            Call MyBase.New(New Token(Of Tokens)(Tokens.Object, "NULL"))
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

        Public Overrides Function Evaluate(envir As Environment) As TempValue
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
