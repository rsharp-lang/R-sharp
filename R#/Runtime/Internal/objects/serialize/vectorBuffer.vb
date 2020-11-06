Imports System.IO
Imports Microsoft.VisualBasic.Serialization
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Object.serialize

    Public Class vectorBuffer : Inherits RawStream

        ''' <summary>
        ''' the full name of the <see cref="Global.System.Type"/> for create <see cref="RType"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property type As String
        Public Property vector As Array
        Public Property names As String()
        Public Property unit As String

        Sub New(vector As vector)
            Me.names = vector.getNames
            Me.type = vector.elementType.raw.FullName
            Me.vector = vector.data
            Me.unit = vector.unit?.name
        End Sub

        Public Overrides Function Serialize() As Byte()
            Dim buffer As New MemoryStream



            Return buffer.ToArray
        End Function
    End Class
End Namespace