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

Public Class PrintProcess : Inherits Process

    Public Overrides ReadOnly Property Operation As ProcessOperation
        Get
            Return ProcessOperation.PRINT
        End Get
    End Property

End Class

Public Class ImportProcess : Inherits Process

    Public Overrides ReadOnly Property Operation As ProcessOperation
        Get
            Return ProcessOperation.IMPORT
        End Get
    End Property

End Class