Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Vectorization

Public Module ListVector

    <Extension>
    Public Function as_character(list As list) As Dictionary(Of String, String)
        If list Is Nothing OrElse list.slots.IsNullOrEmpty Then
            Return New Dictionary(Of String, String)
        Else
            Return list.slots _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  Return CLRVector.asScalarCharacter(a.Value)
                              End Function)
        End If
    End Function

End Module
