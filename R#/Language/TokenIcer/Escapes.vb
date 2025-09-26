Namespace Language.TokenIcer

    ''' <summary>
    ''' common language escape model between multiple language syntax parser
    ''' </summary>
    Public Class Escapes

        Public comment, [string] As Boolean
        Public stringEscape As Char

        ''' <summary>
        ''' apply for the block comment in other language parser, example as:
        ''' 
        ''' 1. javascript/typescript 
        ''' 
        ''' /**
        '''  *
        '''  *
        ''' */
        ''' 
        ''' 2. matlab/octave language
        ''' 
        ''' %{
        ''' ...
        ''' %}
        ''' </summary>
        Public isBlockComment As Boolean

        Public Overrides Function ToString() As String
            If comment Then
                Return "comment"
            ElseIf [string] Then
                Return $"{stringEscape}string{stringEscape}"
            Else
                Return "code"
            End If
        End Function
    End Class
End Namespace