Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Public Module SyntaxParser

    Const SyntaxNotSupport$ = "The syntax is currently not support yet!"

    ''' <summary>
    ''' Convert the string token model as the language runtime model
    ''' </summary>
    ''' <param name="statement"></param>
    ''' <returns></returns>
    <Extension> Public Function Parse(statement As Statement(Of LanguageTokens)) As PrimitiveExpression
        Dim expression As PrimitiveExpression = Nothing

        If TryParseObjectDeclare(statement, expression) Then
            Return expression
        ElseIf TryParseTypedObjectDeclare(statement, expression) Then
            Return expression
        End If

        Dim source As New Exception(statement.GetXml)
        Throw New SyntaxErrorException(SyntaxNotSupport, source)
    End Function

    ''' <summary>
    ''' ```R
    ''' var x &lt;- "string";
    ''' ```
    ''' </summary>
    ''' <param name="statement"></param>
    ''' <returns></returns>
    Public Function TryParseObjectDeclare(statement As Statement(Of LanguageTokens), ByRef out As PrimitiveExpression) As Boolean
        Dim tokens = statement.tokens

        If Not tokens.First.name = LanguageTokens.Variable Then
            Return False
        ElseIf Not tokens(2).name = LanguageTokens.LeftAssign Then
            Return False
        End If

        Dim var$ = tokens(1).Text

    End Function

    ''' <summary>
    ''' ```R
    ''' var x as string &lt;- "string"; 
    ''' ```
    ''' </summary>
    ''' <param name="statement"></param>
    ''' <returns></returns>
    Public Function TryParseTypedObjectDeclare(statement As Statement(Of LanguageTokens), ByRef out As PrimitiveExpression) As Boolean
        Dim tokens = statement.tokens

        If Not tokens.First.name = LanguageTokens.Variable Then
            Return False
        ElseIf Not tokens(2).Text = "as" Then
            Return False
        End If

        Dim var$ = tokens(1).Text

    End Function
End Module
