Imports Microsoft.VisualBasic.Data.Repository
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' a R# function wrapper for <see cref="SnowflakeIdGenerator"/>
''' </summary>
Public Class SnowflakeIdFunction : Inherits RDefaultFunction

    ReadOnly snowflakeId As SnowflakeIdGenerator

    Sub New(config As SnowflakeIdGenerator)
        snowflakeId = config
    End Sub

    <RDefaultFunctionAttribute>
    Public Function nextId() As Long
        Return snowflakeId.GenerateId
    End Function
End Class
