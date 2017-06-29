Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Public Class Interpreter

    ''' <summary>
    ''' 全局环境
    ''' </summary>
    Dim globalEnvir As New Environment

    Public Function Evaluate(script$) As Object
        Return Codes.TryParse(script).RunProgram(globalEnvir)
    End Function

    Public Function Source(path$, args As IEnumerable(Of NamedValue(Of Object))) As Object

    End Function
End Class
