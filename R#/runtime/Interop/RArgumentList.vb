Imports System.Reflection

Namespace Runtime.Interop

    Public Class RArgumentList

        ''' <summary>
        ''' Create argument value for <see cref="MethodInfo.Invoke(Object, Object())"/>
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function CreateArguments() As IEnumerable(Of Object)

        End Function
    End Class
End Namespace