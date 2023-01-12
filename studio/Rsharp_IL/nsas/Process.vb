Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

''' <summary>
''' PROC keyword
''' </summary>
Public MustInherit Class Process

    ''' <summary>
    ''' IMPORT/PRINT
    ''' </summary>
    ''' <returns></returns>
    Public MustOverride ReadOnly Property Operation As ProcessOperation

    Public Property Parameters As NamedValue(Of String)()

End Class

Public Enum ProcessOperation
    IMPORT
    PRINT
End Enum

