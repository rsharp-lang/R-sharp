Imports System.IO
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Namespace Runtime.Serialize

    Public Class functionBuffer : Inherits BufferObject

        Public Property target As DeclareNewFunction

        Public Overrides ReadOnly Property code As BufferObjects
            Get
                Return BufferObjects.function
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(data As Stream)
            Call loadBuffer(data)
        End Sub

        Public Overrides Sub Serialize(buffer As Stream)
            Dim writer As New Writer(buffer)

            Using file As New Writer(buffer)
                Call file.Write(target)
            End Using
        End Sub

        Public Overrides Function getValue() As Object
            Return target
        End Function

        Protected Overrides Sub loadBuffer(stream As Stream)
            Throw New NotImplementedException()
        End Sub
    End Class
End Namespace