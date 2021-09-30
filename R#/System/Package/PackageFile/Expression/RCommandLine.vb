Imports System.IO
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols

Namespace Development.Package.File.Expressions

    Public Class RCommandLine : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As IO.MemoryStream, x As Expression)
            Dim shell As ExternalCommandLine = DirectCast(x, ExternalCommandLine)

            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.Shell))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(shell.ioRedirect)
                Call outfile.Write(context.GetBuffer(shell.cli))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As IO.MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim flag As Boolean = bin.ReadBoolean
                Dim literal As Expression = BlockReader.ParseBlock(bin).Parse(desc)

                Return New ExternalCommandLine(literal) With {
                    .ioRedirect = flag
                }
            End Using
        End Function
    End Class
End Namespace