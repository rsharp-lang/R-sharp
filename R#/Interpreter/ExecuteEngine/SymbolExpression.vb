Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' An expression that will create a new symbol in R# environment
    ''' </summary>
    Public MustInherit Class SymbolExpression : Inherits Expression

        ''' <summary>
        ''' the annotation data from the attribute annotation:
        ''' 
        ''' ```R
        ''' 
        ''' ```
        ''' </summary>
        Protected Friend ReadOnly attributes As New Dictionary(Of String, String())

    End Class
End Namespace