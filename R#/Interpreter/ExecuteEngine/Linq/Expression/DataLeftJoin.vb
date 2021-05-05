Namespace Interpreter.ExecuteEngine.LINQ

    Public Class DataLeftJoin : Inherits LinqKeywordExpression

        Public Overrides ReadOnly Property keyword As String
            Get
                Return "JOIN"
            End Get
        End Property

        ''' <summary>
        ''' join xxx in xxx
        ''' </summary>
        Dim anotherData As QuerySource
        Dim key1 As MemberReference
        Dim key2 As MemberReference

        Sub New(symbol As SymbolDeclare, sequence As Expression)
            anotherData = New QuerySource(symbol, sequence)
        End Sub

        Public Function SetKeyBinary(left As MemberReference, right As MemberReference) As DataLeftJoin
            key1 = left
            key2 = right

            Return Me
        End Function

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace