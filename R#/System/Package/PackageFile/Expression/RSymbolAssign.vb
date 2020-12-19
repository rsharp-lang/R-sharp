
Imports System.IO
Imports Microsoft.VisualBasic.Linq
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
                Call outfile.Write(CByte(x.targetSymbols.Length))

                For Each symbol As Expression In x.targetSymbols
                    Call outfile.Write(context.GetBuffer(symbol))
                Next

                Call outfile.Write(context.GetBuffer(x.value))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim isByRef As Boolean = If(bin.ReadByte = 0, False, True)
                Dim symbolSize As Integer = bin.ReadByte
                Dim symbols As New List(Of Expression)

                For i As Integer = 0 To symbolSize - 1
                    Call BlockReader.ParseBlock(bin).Parse(desc).DoCall(AddressOf symbols.Add)
                Next

                Dim value As Expression = BlockReader.ParseBlock(bin).Parse(desc)
                Dim assign As New ValueAssign(symbols.ToArray, value)

                Return assign
            End Using
        End Function
    End Class
End Namespace