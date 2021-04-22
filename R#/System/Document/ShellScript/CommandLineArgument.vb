Namespace Development.CommandLine

    Friend Class CommandLineArgument

        Public Property name As String
        Public Property defaultValue As String
        Public Property type As String
        Public Property description As String

        Public ReadOnly Property isNumeric As Boolean
            Get
                Return type = "integer" OrElse
                    type = "numeric" OrElse
                    type = "double"
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{name}: {description}"
        End Function

    End Class
End Namespace