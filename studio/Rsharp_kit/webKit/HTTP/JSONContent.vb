Imports System.Net.Http
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting

Public Class JSONContent : Inherits StringContent

    Public Sub New(obj As Object, env As Environment)
        MyBase.New(json_encode(obj, env), Encodings.UTF8WithoutBOM.CodePage, "application/json")
    End Sub

    Private Shared Function json_encode(obj As Object, env As Environment) As String
        If TypeOf obj Is String Then
            Return obj
        ElseIf TypeOf obj Is vector AndAlso
            DirectCast(obj, vector).length = 1 AndAlso
            DirectCast(obj, vector).elementType Is RType.GetRSharpType(GetType(String)) Then

            Return any.ToString(DirectCast(obj, vector).data.GetValue(Scan0))
        Else
            Return jsonlite.toJSON(obj, env)
        End If
    End Function
End Class
