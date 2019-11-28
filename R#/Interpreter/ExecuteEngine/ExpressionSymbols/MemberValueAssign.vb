Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal

Namespace Interpreter.ExecuteEngine

    Public Class MemberValueAssign : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Public ReadOnly Property memberReference As SymbolIndexer
        Public ReadOnly Property value As Expression

        Sub New(member As SymbolIndexer, value As Expression)
            Me.memberReference = member
            Me.value = value
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim names As String() = Runtime.asVector(Of String)(memberReference.index.Evaluate(envir))
            Dim list As RNameIndex = Runtime.getFirst(memberReference.symbol.Evaluate(envir))
            Dim value As Array = Runtime.asVector(Of Object)(Me.value.Evaluate(envir))

            If TypeOf list Is list AndAlso names.Length = 1 Then
                Return list.setByName(names(Scan0), value, envir)
            Else
                Return list.setByName(names, value, envir)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"{memberReference} <- {value}"
        End Function
    End Class
End Namespace