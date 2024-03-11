Imports System.IO
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Development

    Public Module ConsoleSyntaxHighlightPrinter

        ''' <summary>
        ''' helper function for print code with syntax highlights
        ''' </summary>
        ''' <param name="code"></param>
        ''' <param name="dev"></param>
        Public Sub PrintCode(code As String, dev As RContentOutput)
            Call PrintCode(code, dev, dev.env)
        End Sub

        ''' <summary>
        ''' helper function for print code with syntax highlights
        ''' </summary>
        ''' <param name="code"></param>
        ''' <param name="dev"></param>
        Public Sub PrintCode(code As String, dev As TextWriter, env As OutputEnvironments)
            Dim tokens = New Scanner(code, tokenStringMode:=False, keepsDelimiter:=True) _
                .GetTokens _
                .ToArray


        End Sub
    End Module
End Namespace