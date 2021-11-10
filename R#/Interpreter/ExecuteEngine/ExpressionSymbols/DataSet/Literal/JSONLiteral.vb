Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' 这个数据对象模型只兼容json object的申明
    ''' </remarks>
    Public Class JSONLiteral : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.list
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.JSONLiteral
            End Get
        End Property

        Public Property members As NamedValue(Of Expression)()

        Sub New(members As IEnumerable(Of NamedValue(Of Expression)))
            Me.members = members.ToArray
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim json As New list With {.slots = New Dictionary(Of String, Object)}
            Dim value As Object

            For Each member As NamedValue(Of Expression) In members
                value = member.Value.Evaluate(envir)

                If TypeOf value Is Message Then
                    Return value
                Else
                    json.slots(member.Name) = value
                End If
            Next

            Return json
        End Function

        Public Overrides Function ToString() As String
            Dim memberStrings As String() = members _
                .Select(Function(m) $"""{m.Name}"": {m.Value.ToString}") _
                .ToArray

            Return $"{{
    {memberStrings.JoinBy("," & vbCrLf)}
}}"
        End Function
    End Class
End Namespace