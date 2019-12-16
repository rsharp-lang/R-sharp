Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class BinaryOrExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        ReadOnly left, right As Expression

        Sub New(a As Expression, b As Expression)
            left = a
            right = b
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            ' 20191216
            ' fix for
            ' value || stop(xxxx)
            Dim a As Object = left.Evaluate(envir)
            Dim ta As Type

            If a Is Nothing Then
                ta = GetType(Void)
            Else
                ta = a.GetType
            End If

            If ta Like BinaryExpression.logicals Then
                ' boolean = a || b
                Dim b As Object = right.Evaluate(envir)

                If Program.isException(b) Then
                    Return b
                Else
                    Return Runtime.Core _
                        .BinaryCoreInternal(Of Boolean, Boolean, Boolean)(
                            x:=Core.asLogical(a),
                            y:=Core.asLogical(b),
                            [do]:=Function(x, y) x OrElse y
                        ).ToArray
                End If
            Else
                ' let arg as string = ?"--opt" || default;
                If Internal.Invokes.base.isEmpty(a) Then
                    Return right.Evaluate(envir)
                Else
                    Return a
                End If
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"({left} || {right})"
        End Function
    End Class
End Namespace