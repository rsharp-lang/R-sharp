Imports System.IO
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace Development.Package.File.Expressions

    Public Class RReturns : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Dim returns As ReturnValue = DirectCast(x, ReturnValue)

            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.Return))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(context.GetBuffer(returns.value))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim value As Expression = BlockReader.ParseBlock(bin).Parse(desc)
                Dim rtvls As New ReturnValue(value)

                Return rtvls
            End Using
        End Function
    End Class
End Namespace