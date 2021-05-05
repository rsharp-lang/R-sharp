Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

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
        Friend ReadOnly anotherData As QuerySource

        Dim key1 As MemberReference
        Dim key2 As MemberReference

        Sub New(symbol As SymbolDeclare, sequence As Expression)
            anotherData = New QuerySource(symbol, sequence)
        End Sub

        Public Function FindKeySymbol(side As String) As String
            If QuerySource.getSymbolName(key1.symbol) = side Then
                Return key1.memberName
            Else
                Return key2.memberName
            End If
        End Function

        Public Function SetKeyBinary(left As MemberReference, right As MemberReference) As DataLeftJoin
            key1 = left
            key2 = right

            Return Me
        End Function

        Public Function EnumerateFields() As IEnumerable(Of NamedValue(Of Expression))
            Return anotherData.EnumerateFields(addSymbol:=True)
        End Function

        Public Function SetKeyBinary(equivalent As BinaryExpression) As DataLeftJoin
            Return SetKeyBinary(equivalent.left, equivalent.right)
        End Function

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Return anotherData.sequence.Exec(context)
        End Function

        Public Overrides Function ToString() As String
            Return New String() {
                $"JOIN {anotherData.symbol} IN {anotherData.sequence}",
                $"ON ({key1} == {key2})"
            }.JoinBy(vbCrLf)
        End Function
    End Class
End Namespace