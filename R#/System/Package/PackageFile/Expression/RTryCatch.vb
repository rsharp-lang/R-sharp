Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Namespace Development.Package.File.Expressions

    Public Class RTryCatch : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Call WriteBuffer(ms, DirectCast(x, TryCatchExpression))
        End Sub

        Private Overloads Sub WriteBuffer(ms As MemoryStream, x As TryCatchExpression)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.TryCatch))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(context.GetBuffer(sourceMap:=x.sourceMap))
                Call outfile.Write(context.GetBuffer(x.try))
                Call outfile.Write(context.GetBuffer(x.catch))
                Call outfile.Write(context.GetBuffer(x.exception))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim sourceMap As StackFrame = Writer.ReadSourceMap(bin, desc)
                Dim [try] As Expression = BlockReader.ParseBlock(bin).Parse(desc)
                Dim [catch] As Expression = BlockReader.ParseBlock(bin).Parse(desc)
                Dim [exception] As Expression = BlockReader.ParseBlock(bin).Parse(desc)

                Return New TryCatchExpression([try], [catch], exception, sourceMap)
            End Using
        End Function
    End Class
End Namespace