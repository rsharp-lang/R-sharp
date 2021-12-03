Imports System.Runtime.CompilerServices

Public Module Deserializer

    <Extension>
    Public Function ToObject(Of T As Class)(robj As RObject) As T

    End Function

End Module
