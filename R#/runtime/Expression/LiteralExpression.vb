Imports Microsoft.VisualBasic.Scripting.TokenIcer

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

    ''' <summary>
    ''' ``Nothing`` in VisualBasic
    ''' </summary>
    ''' <returns></returns>
    Public Shared ReadOnly Property NULL As New NULL

    Sub New(token As Token(Of LanguageTokens))

    End Sub
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
    Public Overrides Function Evaluate(envir As Environment) As Object
        Return Nothing
    End Function
End Class