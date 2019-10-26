Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language

    Module Code

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ParseScript(script As String) As Token()
            Return New Scanner(script).GetTokens().ToArray
        End Function
    End Module
End Namespace