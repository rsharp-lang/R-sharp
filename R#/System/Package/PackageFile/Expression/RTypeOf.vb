Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

Namespace Development.Package.File.Expressions

    Public Class RTypeOf : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Dim modeOf As ModeOf = x

            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.TypeOf))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(Encoding.ASCII.GetBytes(modeOf.keyword))
                Call outfile.Write(CByte(0))
                Call outfile.Write(context.GetBuffer(modeOf.target))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim key As String = Writer.readZEROBlock(bin) _
                    .DoCall(Function(bytes)
                                Return Encoding.ASCII.GetString(bytes.ToArray)
                            End Function)
                Dim target As Expression = BlockReader _
                    .ParseBlock(bin) _
                    .Parse(desc)

                Return New ModeOf(key, target)
            End Using
        End Function
    End Class
End Namespace