
Imports System.IO
Imports System.Text
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks

Namespace System.Package.File.Expressions

    Public Class RFor : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Call WriteBuffer(ms, DirectCast(x, ForLoop))
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As ForLoop)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.For))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(Writer.GetBuffer(x.stackFrame))
                Call outfile.Write(CByte(If(x.parallel, 1, 0)))

                Call outfile.Write(x.variables.Length)

                For Each item As String In x.variables
                    Call outfile.Write(Encoding.ASCII.GetBytes(item))
                    Call outfile.Write(CByte(0))
                Next

                Call outfile.Write(context.GetBuffer(x.sequence))
                Call outfile.Write(context.GetBuffer(x.body))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace