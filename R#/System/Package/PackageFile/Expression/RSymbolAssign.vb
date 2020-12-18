
Imports System.IO
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace System.Package.File.Expressions

    Public Class RSymbolAssign : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Call WriteBuffer(ms, DirectCast(x, ValueAssign))
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As ValueAssign)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.SymbolAssign))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(CByte(If(x.isByRef, 1, 0)))
                Call outfile.Write(x.targetSymbols.Length)

                For Each symbol As Expression In x.targetSymbols
                    Call outfile.Write(context.GetBuffer(symbol))
                Next

                Call outfile.Write(context.GetBuffer(x.value))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, type As ExpressionTypes, desc As DESCRIPTION) As Expression
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace