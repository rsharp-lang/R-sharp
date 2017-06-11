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

    Sub New(token As Token(Of LanguageTokens))

    End Sub
End Class
