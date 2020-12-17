Imports System.IO
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components

Namespace System.Package.File.Expressions

    Public Class RLiteral : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Call WriteBuffer(ms, DirectCast(x, Literal))
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As Literal)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(DirectCast(ExpressionTypes.Literal, Integer))
                Call outfile.Write(0)
                Call outfile.Write(DirectCast(x.type, Byte))

                Select Case x.type
                    Case TypeCodes.boolean : Call outfile.Write(CType(If(DirectCast(x.value, Boolean), 1, 0), Byte))
                    Case TypeCodes.double : Call outfile.Write(CType(x.value, Double))
                    Case TypeCodes.integer : Call outfile.Write(CType(x.value, Long))
                    Case Else
                        Call outfile.Write(Scripting.ToString(x.value))
                End Select

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, desc As DESCRIPTION) As Expression
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace