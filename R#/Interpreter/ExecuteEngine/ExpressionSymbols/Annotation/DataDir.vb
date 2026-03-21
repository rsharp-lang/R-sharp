Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Annotation

    Public MustInherit Class AnnotationSymbol : Inherits Expression

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

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' must be prefixed with symbol "@"
        ''' </remarks>
        Public MustOverride ReadOnly Property symbol As String

        Public Overrides Function ToString() As String
            Return symbol
        End Function

        Public Shared Function CheckSymbolText(symbol As String) As Boolean
            Select Case Strings.LCase(symbol)
                Case "@stop", "@script", "@home", "@host", "@dir", "@datadir", "@profile" : Return True
                Case Else
                    Return False
            End Select
        End Function

        Public Shared Function CreateSymbol(symbol As String) As AnnotationSymbol
            Select Case Strings.LCase(symbol)
                Case "@stop" : Return New BreakPoint
                Case "@script" : Return New ScriptSymbol
                Case "@home" : Return New HomeSymbol
                Case "@host" : Return New HostSymbol
                Case "@dir" : Return New ScriptFolder
                Case "@datadir" : Return New DataDir
                Case "@profile" : Throw New NotImplementedException(symbol)

                Case Else
                    Throw New NotImplementedException(symbol)
            End Select
        End Function

    End Class

    ''' <summary>
    ''' @datadir
    ''' 
    ''' directory path of the /data in current package
    ''' </summary>
    Public Class DataDir : Inherits AnnotationSymbol

        Public Overrides ReadOnly Property symbol As String
            Get
                Return "@datadir"
            End Get
        End Property

        Public Overrides Function Evaluate(envir As Environment) As Object
            Throw New NotImplementedException
        End Function
    End Class
End Namespace