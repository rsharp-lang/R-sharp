Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports r = System.Text.RegularExpressions.Regex

Namespace Interpreter.ExecuteEngine

    Public Class Regexp : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.string
            End Get
        End Property

        Public ReadOnly Property pattern As String

        Sub New(regexp As String)
            pattern = regexp
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Return New r(pattern)
        End Function

        Public Overrides Function ToString() As String
            Return $"/{pattern}/"
        End Function

        Public Shared Function Matches(r As Regex, text As Expression, env As Environment) As Object
            Dim strData As Object = text.Evaluate(env)

            If Program.isException(strData) Then
                Return strData
            End If

            Dim inputs As String() = Runtime.asVector(Of String)(strData)

            If inputs.Length = 1 Then
                Return r.Matches(inputs(Scan0)).ToArray
            Else
                Return inputs _
                    .SeqIterator _
                    .ToDictionary(Function(i) $"[[{i.i + 1}]]",
                                  Function(str)
                                      Return CObj(r.Matches(str.value).ToArray)
                                  End Function) _
                    .DoCall(Function(data)
                                Return New list With {.slots = data}
                            End Function)
            End If
        End Function
    End Class
End Namespace