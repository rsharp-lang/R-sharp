Namespace Development.Configuration

    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class OptionAttribute : Inherits Attribute

        Public ReadOnly Property Name As String

        Sub New(name As String)
            Me.Name = name
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function

    End Class
End Namespace