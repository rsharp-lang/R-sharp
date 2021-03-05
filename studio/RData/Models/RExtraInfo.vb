
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' Extra information.
'''
''' Contains the Default encoding (only In version 3).
''' </summary>
Public Class RExtraInfo

    Public Property encoding As String = Nothing

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class