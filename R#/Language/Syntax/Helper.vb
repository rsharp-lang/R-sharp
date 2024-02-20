
Imports System.Runtime.InteropServices

Namespace Language.Syntax

    Public Module Helper

        ''' <summary>
        ''' check current line input is string stack open?
        ''' </summary>
        ''' <param name="line"></param>
        ''' <param name="string">get last open string part value</param>
        ''' <returns></returns>
        Public Function IsStringOpen(line As String, <Out> ByRef [string] As String) As Boolean
            Return False
        End Function
    End Module
End Namespace