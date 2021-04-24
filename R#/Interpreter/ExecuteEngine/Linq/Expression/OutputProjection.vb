Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.My.JavaScript
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' data projection: ``SELECT &lt;fields>``
    ''' </summary>
    Public Class OutputProjection : Inherits LinqKeywordExpression

        ''' <summary>
        ''' produce a javascript object or dataframe row
        ''' </summary>
        ''' <returns></returns>
        Public Property fields As NamedValue(Of Expression)()

        Public Overrides ReadOnly Property keyword As String
            Get
                Return "SELECT"
            End Get
        End Property

        Sub New(fields As IEnumerable(Of NamedValue(Of Expression)))
            Me.fields = fields.ToArray
        End Sub

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Dim obj As New JavaScriptObject

            For Each field In fields
                obj(field.Name) = field.Value.Exec(context)
            Next

            Return obj
        End Function

        Public Overrides Function ToString() As String
            Return $"new {{{fields.Select(Function(a) $"{a.Name} = {a.Value}").JoinBy(", ")}}}"
        End Function
    End Class
End Namespace