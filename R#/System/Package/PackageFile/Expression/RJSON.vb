Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace Development.Package.File.Expressions

    ''' <summary>
    ''' <see cref="JSONLiteral"/>
    ''' </summary>
    Public Class RJSON : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Dim json As JSONLiteral = x

            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.JSONLiteral))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(json.size)

                For Each member As NamedValue(Of Expression) In json.members
                    Call outfile.Write(Encoding.UTF8.GetBytes(member.Name))
                    Call outfile.Write(CByte(0))
                    Call outfile.Write(context.GetBuffer(member.Value))
                Next

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim nsize As Integer = bin.ReadInt32
                Dim members As New List(Of NamedValue(Of Expression))

                For i As Integer = 0 To nsize - 1
                    Dim name As String = Writer.readZEROBlock(bin) _
                        .DoCall(Function(bytes)
                                    Return Encoding.UTF8.GetString(bytes.ToArray)
                                End Function)
                    Dim expr As Expression = BlockReader.ParseBlock(bin).Parse(desc)

                    Call members.Add(New NamedValue(Of Expression)(name, expr))
                Next

                Return New JSONLiteral(members)
            End Using
        End Function
    End Class
End Namespace