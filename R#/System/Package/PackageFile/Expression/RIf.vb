
Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Namespace System.Package.File.Expressions

    Public Class RIf : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Call WriteBuffer(ms, DirectCast(x, IfBranch))
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As IfBranch)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.If))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(Writer.GetBuffer(x.stackFrame))
                Call outfile.Write(context.GetBuffer(x.ifTest))
                Call outfile.Write(context.GetBuffer(x.trueClosure))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim sourceMap As StackFrame = Writer.ReadSourceMap(bin)
                Dim test As Expression = BlockReader.ParseBlock(bin).Parse(desc)
                Dim trueExpr As DeclareNewFunction = BlockReader.ParseBlock(bin).Parse(desc)

                Return New IfBranch(test, trueExpr, sourceMap)
            End Using
        End Function
    End Class
End Namespace