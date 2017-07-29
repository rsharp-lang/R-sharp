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

        Public Overrides Function Evaluate(envir As Environment) As (value As Object, Type As TypeCodes)
            Select Case Type
                Case LanguageTokens.String
                    Return (Value, Type)
                Case LanguageTokens.Numeric
                    Return (Val(Value), Type)
                Case LanguageTokens.Boolean
                    Return (Value.ParseBoolean, Type)
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
        Public Overrides Function Evaluate(envir As Environment) As (value As Object, Type As TypeCodes)
            Return Nothing
        End Function
    End Class

    Public Class StringLiteral : Inherits PrimitiveExpression


    End Class
End Namespace