Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Annotation

    ''' <summary>
    ''' ``@dir``
    ''' </summary>
    Public Class ScriptFolder : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.string
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Annotation
            End Get
        End Property

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim scriptFile As String = ScriptSymbol.TryGetScriptFileName(envir)

            If scriptFile.StringEmpty Then
                Return Nothing
            Else
                Return scriptFile.ParentPath
            End If
        End Function

        Public Overrides Function ToString() As String
            Return "@dir"
        End Function
    End Class
End Namespace