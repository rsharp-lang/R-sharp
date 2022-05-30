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
            Dim fake As New DESCRIPTION With {
                .Author = "xieguigang",
                .[Date] = Now.ToString,
                .Maintainer = .Author,
                .License = "MIT",
                .Package = NameOf(functionBuffer),
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