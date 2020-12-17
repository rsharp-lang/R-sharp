Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Namespace System.Package.File.Expressions

    Public Class RFunction : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Call WriteBuffer(ms, DirectCast(x, DeclareNewFunction))
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As DeclareNewFunction)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.FunctionDeclare))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(Writer.GetBuffer(sourceMap:=x.stackFrame))
                Call outfile.Write(Encoding.ASCII.GetBytes(x.funcName))
                Call outfile.Write(CByte(0))

                Call outfile.Write(CByte(x.params.Length))

                For Each arg As DeclareNewSymbol In x.params
                    Call outfile.Write(context.GetBuffer(arg))
                Next

                Call outfile.Write(x.body.bodySize)

                For Each exec As Expression In x.body.EnumerateCodeLines
                    Call outfile.Write(context.GetBuffer(exec))
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