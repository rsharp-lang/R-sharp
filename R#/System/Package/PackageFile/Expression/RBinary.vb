Imports System.IO
Imports System.Text
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace System.Package.File.Expressions

    Public Class RBinary : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.Binary))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(Encoding.ASCII.GetBytes(getOperator(x)))
                Call outfile.Write(context.GetBuffer(left(x)))
                Call outfile.Write(context.GetBuffer(right(x)))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Private Shared Function left(x As Expression) As Expression
            If TypeOf x Is BinaryBetweenExpression Then
                Return DirectCast(x, BinaryBetweenExpression).collectionSet
            ElseIf TypeOf x Is BinaryInExpression Then
                Return DirectCast(x, BinaryInExpression).a
            ElseIf TypeOf x Is BinaryOrExpression Then
                Return DirectCast(x, BinaryOrExpression).left
            ElseIf TypeOf x Is BinaryExpression Then
                Return DirectCast(x, BinaryExpression).left
            Else
                Throw New NotImplementedException(x.GetType.FullName)
            End If
        End Function

        Private Shared Function right(x As Expression) As Expression
            If TypeOf x Is BinaryBetweenExpression Then
                Return DirectCast(x, BinaryBetweenExpression).range
            ElseIf TypeOf x Is BinaryInExpression Then
                Return DirectCast(x, BinaryInExpression).b
            ElseIf TypeOf x Is BinaryOrExpression Then
                Return DirectCast(x, BinaryOrExpression).right
            ElseIf TypeOf x Is BinaryExpression Then
                Return DirectCast(x, BinaryExpression).right
            Else
                Throw New NotImplementedException(x.GetType.FullName)
            End If
        End Function

        Private Shared Function getOperator(x As Expression) As String
            If TypeOf x Is BinaryBetweenExpression Then
                Return "between"
            ElseIf TypeOf x Is BinaryInExpression Then
                Return "in"
            ElseIf TypeOf x Is BinaryOrExpression Then
                Return "||"
            ElseIf TypeOf x Is BinaryExpression Then
                Return DirectCast(x, BinaryExpression).operator
            Else
                Throw New NotImplementedException(x.GetType.FullName)
            End If
        End Function

        Public Overrides Function GetExpression(buffer As MemoryStream, desc As DESCRIPTION) As Expression
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace