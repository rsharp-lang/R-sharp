Imports System.IO
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Serialize

    Public Class dataframeBuffer : Inherits BufferObject

        Public Overrides ReadOnly Property code As BufferObjects
            Get
                Return BufferObjects.dataframe
            End Get
        End Property

        Public Overrides Sub Serialize(buffer As Stream)
            Throw New NotImplementedException()
        End Sub

        Public Function getFrame() As dataframe
            Throw New NotImplementedException
        End Function

        Public Overrides Function getValue() As Object
            Return getFrame()
        End Function
    End Class
End Namespace