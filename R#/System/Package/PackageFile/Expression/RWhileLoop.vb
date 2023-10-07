Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Namespace Development.Package.File.Expressions

    Public Class RWhileLoop : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Dim whileL As WhileLoop = x

            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.While))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(context.GetBuffer(whileL.stackFrame))
                Call outfile.Write(context.GetBuffer(whileL.test))
                Call outfile.Write(context.GetBuffer(whileL.loopBody))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim sourceMap As StackFrame = Writer.ReadSourceMap(bin, desc)
                Dim test As Expression = BlockReader.ParseBlock(bin).Parse(desc)
                Dim trueExpr As ClosureExpression = BlockReader.ParseBlock(bin).Parse(desc)

                Return New WhileLoop(test, trueExpr, sourceMap)
            End Using
        End Function
    End Class
End Namespace