Imports System.IO
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace System.Package.File.Expressions

    Public Class RUnary : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Call WriteBuffer(ms, DirectCast(x, UnaryNot))
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As UnaryNot)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.UnaryNot))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(context.GetBuffer(x.logical))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, desc As DESCRIPTION) As Expression
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace