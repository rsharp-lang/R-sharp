Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting

Public Class Variable

    Implements INamedValue
    Implements Value(Of Object).IValueOf

    Public Property Name As String Implements IKeyedEntity(Of String).Key
    Public Overridable Property Value As Object Implements Value(Of Object).IValueOf.value

    ''' <summary>
    ''' Get the type of the current object <see cref="Value"/>.
    ''' </summary>
    ''' <returns></returns>
    Public Overloads ReadOnly Property [TypeOf] As Type
        Get
            If Value Is Nothing Then
                Return GetType(Object)
            Else
                Return Value.GetType
            End If
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"Dim {Name} As {Me.TypeOf.FullName} = {CStrSafe(Value)}"
    End Function
End Class
