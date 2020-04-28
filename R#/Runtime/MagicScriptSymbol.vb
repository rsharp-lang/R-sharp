Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime

    Public Class MagicScriptSymbol

        Public Property dir As String
        Public Property file As String
        Public Property fullName As String

        <RNameAlias("startup.time")>
        Public Property startup_time As String
        Public Property debug As Boolean
        Public Property silent As Boolean

    End Class
End Namespace