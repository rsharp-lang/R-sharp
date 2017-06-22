Imports Microsoft.VisualBasic.Scripting.TokenIcer

''' <summary>
''' The R# script code model
''' </summary>
Public Class Codes: Inherits Main(Of LanguageTokens)

    Sub New(main As Main(Of LanguageTokens))
		Me.program = main.program
    End Sub

    Sub New(script As IEnumerable(Of Statement(Of LanguageTokens)))
        Me.program = script.ToArray.Trim
    End Sub
End Class
