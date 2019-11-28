
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' ```
    ''' a &lt;&lt; b
    ''' ```
    ''' </summary>
    Public Class Append : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return a.type
            End Get
        End Property

        ReadOnly a, b As Expression

        Sub New(a As Expression, b As Expression)
            Me.a = a
            Me.b = b
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim x As Object = a.Evaluate(envir)
            Dim y As Object = b.Evaluate(envir)

            If x Is Nothing Then
                Return y
            ElseIf y Is Nothing Then
                Return x
            ElseIf x Is Nothing AndAlso y Is Nothing Then
                Return Nothing
            End If

            Dim type1 As Type = x.GetType
            Dim type2 As Type = y.GetType

            If type1.IsArray OrElse type1 Is GetType(vector) Then
                ' y should be vector
                Return Runtime.asVector(Of Object)(x) _
                    .AsObjectEnumerator _
                    .JoinIterates(Runtime.asVector(Of Object)(y).AsObjectEnumerator) _
                    .ToArray
            ElseIf type1 Is GetType(list) Then
                Throw New NotImplementedException
            End If

            Throw New NotImplementedException
        End Function
    End Class
End Namespace