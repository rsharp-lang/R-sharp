Imports System.IO
Imports Microsoft.VisualBasic

Public Class SlaveTask

    Dim toBuffers As New Dictionary(Of Type, Func(Of Object, Stream))
    Dim fromBuffer As New Dictionary(Of Type, Func(Of Stream, Object))

    Sub New()
    End Sub

    Public Function Emit(Of T)(streamAs As Func(Of T, Stream)) As SlaveTask
        toBuffers(GetType(T)) = Function(obj) streamAs(obj)
        Return Me
    End Function

    Public Function Emit(Of T)(fromStream As Func(Of Stream, T)) As SlaveTask
        fromBuffer(GetType(T)) = Function(buf) fromStream(buf)
        Return Me
    End Function

    Public Function RunTask(entry As [Delegate], ParamArray parameters As Object()) As Object

    End Function

End Class

Public Class IDelegate

    Public Property name As String
    Public Property type As typeinfo

End Class