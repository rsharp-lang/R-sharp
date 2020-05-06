Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    Public Enum SymbolIndexers
        ''' <summary>
        ''' a[x]
        ''' </summary>
        vectorIndex
        ''' <summary>
        ''' a[[x]], a$x
        ''' </summary>
        nameIndex
        ''' <summary>
        ''' a[, x]
        ''' </summary>
        dataframeColumns
        ''' <summary>
        ''' a[x, ]
        ''' </summary>
        dataframeRows
        ''' <summary>
        ''' a[x,y]
        ''' </summary>
        dataframeRanges
    End Enum
End Namespace