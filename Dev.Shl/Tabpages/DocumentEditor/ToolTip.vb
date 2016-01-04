Imports Pavel.CodeEditor
Imports Microsoft.VisualBasic.Scripting.ShoalShell.Runtime

Namespace DocumentEditor.Components

    Public Class ToolTip

        Public Function GetTooltipInfo(obj As String, compiled As Microsoft.VisualBasic.Scripting.ShoalShell.Runtime.ScriptEngine) As String
            If compiled Is Nothing Then GoTo GET_HELP

            Dim Type As String = compiled.MMUDevice(obj).TypeOf.FullName

            If String.IsNullOrEmpty(Type) Then
                Dim objct = Program.ScriptEngine.GetValue(obj)

                If Not obj Is Nothing Then
                    Type = obj.GetType.FullName

                    If String.Equals(Type, "System.String") AndAlso String.Equals(obj, Type) Then
                        Type = ""
                    End If
                Else
                    Type = "NULL"
                End If
            End If

            If Not String.IsNullOrEmpty(Type) Then
                Return Type
            End If

GET_HELP:   Return String.Join(vbCrLf, Program.ScriptEngine.GetHelpInfo(obj, False))
        End Function
    End Class
End Namespace