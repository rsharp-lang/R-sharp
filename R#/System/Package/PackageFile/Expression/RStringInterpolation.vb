Imports System.IO
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

Namespace System.Package.File.Expressions

    Public Class RStringInterpolation : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Call WriteBuffer(ms, DirectCast(x, StringInterpolation))
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As StringInterpolation)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.StringInterpolation))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(x.stringParts.Length)

                For Each part As Expression In x.stringParts
                    Call outfile.Write(context.GetBuffer(part))
                Next

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim parts As Integer = bin.ReadInt32
                Dim partList As New List(Of Expression)

                For i As Integer = 0 To parts - 1
                    Call BlockReader.ParseBlock(bin).Parse(desc).DoCall(AddressOf partList.Add)
                Next

                Return New StringInterpolation(partList.ToArray)
            End Using
        End Function
    End Class
End Namespace