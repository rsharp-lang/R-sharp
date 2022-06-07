Imports System.IO
Imports System.Text
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks

Namespace Development.Package.File.Expressions

    ''' <summary>
    ''' break/next
    ''' </summary>
    Public Class RBreakControls : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            If TypeOf x Is BreakLoop OrElse TypeOf x Is ContinuteFor Then
                Using outfile As New BinaryWriter(ms)
                    Call outfile.Write(CInt(x.expressionName))
                    Call outfile.Write(0)
                    Call outfile.Write(CByte(x.type))

                    Call outfile.Write(Encoding.UTF8.GetBytes(x.ToString))

                    Call outfile.Flush()
                    Call saveSize(outfile)
                End Using
            Else
                Throw New NotImplementedException(x.GetType.FullName)
            End If
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            If raw.expression = ExpressionTypes.Break Then
                Return New BreakLoop
            ElseIf raw.expression = ExpressionTypes.Continute Then
                Return New ContinuteFor
            Else
                Throw New NotImplementedException(raw.expression.ToString)
            End If
        End Function
    End Class
End Namespace