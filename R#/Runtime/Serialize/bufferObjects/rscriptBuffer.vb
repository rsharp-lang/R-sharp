Imports System.IO
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Runtime.Serialize

    Public Class rscriptBuffer : Inherits BufferObject

        Public Property target As Expression

        Public Overrides ReadOnly Property code As BufferObjects
            Get
                Return BufferObjects.rscript
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(data As Stream)
            Call loadBuffer(data)
        End Sub

        Public Overrides Sub Serialize(taskPayload As Stream)
            Call New Writer(taskPayload).Write(target)
            Call taskPayload.Flush()
        End Sub

        Public Overrides Function getValue() As Object
            Return target
        End Function

        Protected Overrides Sub loadBuffer(stream As Stream)
            Dim fake As New DESCRIPTION With {
                .Author = "xieguigang",
                .[Date] = Now.ToString,
                .Maintainer = .Author,
                .License = "MIT",
                .Package = NameOf(rscriptBuffer),
                .Title = .Package,
                .Type = "runtime",
                .Version = App.Version,
                .Description = .Package
            }

            Using reader As New BinaryReader(stream)
                Call BlockReader.Read(reader).Parse(fake, expr:=_target)
            End Using
        End Sub
    End Class
End Namespace