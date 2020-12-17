Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime.Components

Namespace System.Package.File.Expressions

    Public Class RSymbol : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Call WriteBuffer(ms, DirectCast(x, DeclareNewSymbol))
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As DeclareNewSymbol)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(DirectCast(ExpressionTypes.SymbolDeclare, Integer))
                Call outfile.Write(0)
                Call outfile.Write(DirectCast(x.type, Byte))

                Call outfile.Write(CType(If(x.is_readonly, 1, 0), Byte))
                Call outfile.Write(CType(x.names.Length, Byte))

                For Each name As String In x.names
                    Call outfile.Write(Encoding.ASCII.GetBytes(name))
                    Call outfile.Write(CType(0, Byte))
                Next

                Call outfile.Write(context.GetBuffer(x.value))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, desc As DESCRIPTION) As Expression
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace