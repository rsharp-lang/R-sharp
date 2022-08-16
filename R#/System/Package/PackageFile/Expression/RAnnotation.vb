Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Annotation

Namespace Development.Package.File.Expressions

    Public Class RAnnotation : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            If TypeOf x Is Profiler Then
                Dim profiler As Profiler = DirectCast(x, Profiler)

                Using outfile As New BinaryWriter(ms)
                    Call outfile.Write(CInt(ExpressionTypes.Annotation))
                    Call outfile.Write(0)
                    Call outfile.Write(CByte(x.type))

                    Call outfile.Write(context.GetBuffer(sourceMap:=profiler.stackFrame))
                    Call outfile.Write(context.GetBuffer(x:=profiler.target))

                    Call outfile.Flush()
                    Call saveSize(outfile)
                End Using
            Else
                Throw New NotImplementedException
            End If
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using reader As New BinaryReader(buffer)
                Dim sourceMap As StackFrame = Writer.ReadSourceMap(reader, desc)
                Dim target As Expression = BlockReader.ParseBlock(reader).Parse(desc)

                Return New Profiler(target, sourceMap)
            End Using
        End Function
    End Class
End Namespace