Imports Microsoft.VisualBasic.Scripting.TokenIcer

''' <summary>
''' The R# script code model
''' </summary>
Public Class Codes

    Sub New(main As Main(Of LanguageTokens))

    End Sub

    Sub New(script As IEnumerable(Of Statement(Of LanguageTokens)))
        Dim codes = script.ToArray.Trim
    End Sub
End Class
