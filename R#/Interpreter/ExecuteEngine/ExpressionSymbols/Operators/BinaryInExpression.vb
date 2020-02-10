
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class BinaryInExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.boolean
            End Get
        End Property

        ''' <summary>
        ''' left
        ''' </summary>
        Friend a As Expression
        ''' <summary>
        ''' right
        ''' </summary>
        Friend b As Expression

        Sub New(a As Expression, b As Expression)
            Me.a = a
            Me.b = b
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim index As Index(Of Object) = getIndex(b, envir).Indexing
            Dim findTest As Boolean() = getIndex(a, envir) _
                .Select(Function(x)
                            Return x Like index
                        End Function) _
                .ToArray

            Return findTest
        End Function

        Private Shared Function getIndex(src As Expression, env As Environment) As Object()
            Dim isList As Boolean = False
            Dim seq = LinqExpression.produceSequenceVector(src, env, isList)

            If isList Then
                Return DirectCast(seq, KeyValuePair(Of String, Object)()) _
                    .Select(Function(t) t.Value) _
                    .ToArray
            Else
                Return DirectCast(seq, Object())
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"({a} %in% index<{b}>)"
        End Function
    End Class
End Namespace