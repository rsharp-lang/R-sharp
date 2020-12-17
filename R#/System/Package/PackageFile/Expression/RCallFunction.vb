Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Namespace System.Package.File.Expressions

    Public Class RCallFunction : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Call WriteBuffer(ms, DirectCast(x, FunctionInvoke))
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As FunctionInvoke)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.FunctionCall))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(Writer.GetBuffer(sourceMap:=x.stackFrame))
                Call outfile.Write(Encoding.ASCII.GetBytes(x.namespace Or "n/a".AsDefault))
                Call outfile.Write(CByte(0))
                Call outfile.Write(context.GetBuffer(x.funcName))

                Call outfile.Write(CByte(x.parameters.Length))

                For Each arg As Expression In x.parameters
                    Call outfile.Write(context.GetBuffer(arg))
                Next

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, desc As DESCRIPTION) As Expression
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace