Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Language.Syntax

    ''' <summary>
    ''' Helper for R shell terminal multiple line editing
    ''' </summary>
    Public NotInheritable Class IncompleteExpression : Inherits RDefaultFunction

        Dim lines As New List(Of String)

        Public ReadOnly Property Check() As Boolean
            Get
                Dim script As String = lines.JoinBy(vbCrLf)
                Dim tokens As Token() = Language.ParseScript(script)

                Return CheckTokenSequence(tokens)
            End Get
        End Property

        Sub New()
        End Sub

        <RDefaultFunction>
        Public Function Append(line As String) As IncompleteExpression
            Call lines.Add(line)
            Return Me
        End Function

        Public Function GetRScript() As Rscript
            Return Rscript.AutoHandleScript(lines.JoinBy(vbCrLf))
        End Function

        Public Function eval(env As Environment) As Object
            If Check() Then
                Return Internal.debug.stop("in-complete expression to evaluate.", env)
            End If

            Return Program.CreateProgram(GetRScript,,).Execute(env)
        End Function

        ''' <summary>
        ''' test the given line tokens is in-complete expression or not?
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' in-complete expression:
        ''' 
        ''' 1. ends with operator token
        ''' 2. bracket stack not closed
        ''' </remarks>
        Public Shared Function CheckTokenSequence(tokens As Token()) As Boolean
            If tokens.IsNullOrEmpty Then
                ' special case:
                ' empty expression is completed
                Return False
            End If

            ' ends with open:  xxx(
            ' ends with operator: xxx *
            ' ends with comma: xxx(aaa,
            If tokens.Last.name = TokenType.open OrElse
                tokens.Last.name = TokenType.operator OrElse
                tokens.Last.name = TokenType.comma Then

                Return True
            End If

            ' check of the stack closed?


            Return False
        End Function
    End Class
End Namespace