Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Interop

    ''' <summary>
    ''' Literal of any .NET runtime value object
    ''' </summary>
    Public Class RuntimeValueLiteral : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                If value Is Nothing Then
                    Return TypeCodes.NA
                Else
                    Return value.GetType.GetRTypeCode
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Literal
            End Get
        End Property

        Public ReadOnly Property value As Object

        Sub New(value As Object)
            Me.value = value
        End Sub

        <DebuggerStepThrough>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Return value
        End Function

        Public Overrides Function ToString() As String
            If value Is Nothing Then
                Return "NULL"
            Else
                Return value.ToString
            End If
        End Function
    End Class
End Namespace