Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

Namespace Development.Package.File.Expressions

    Public Class RExprLiteral : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Dim literal As ExpressionLiteral = DirectCast(x, ExpressionLiteral)

            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.ExpressionLiteral))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(context.GetBuffer(literal.stackFrame))
                Call outfile.Write(context.GetBuffer(literal.expression))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim sourceMap As StackFrame = Writer.ReadSourceMap(bin, desc)
                Dim literal As Expression = BlockReader.ParseBlock(bin).Parse(desc)

                Return New ExpressionLiteral(literal, sourceMap)
            End Using
        End Function
    End Class
End Namespace