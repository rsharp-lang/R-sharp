Imports System.Drawing
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization

Namespace Runtime.Internal

    ''' <summary>
    ''' R# console nice print supports.
    ''' </summary>
    Module printer

        ReadOnly RtoString As New Dictionary(Of Type, IStringBuilder)

        Sub New()
            RtoString(GetType(Color)) = Function(c) DirectCast(c, Color).ToHtmlColor
        End Sub

        Friend Sub printInternal(x As Object, listPrefix$)
            Dim valueType As Type = x.GetType
            Dim isString As Boolean = valueType Is GetType(String) OrElse valueType Is GetType(String())
            Dim toString = Function(o As Object) As String
                               If isString Then
                                   Return $"""{Scripting.ToString(o, "NULL")}"""
                               Else
                                   Return Scripting.ToString(o, "NULL")
                               End If
                           End Function

            If valueType.IsInheritsFrom(GetType(Array)) Then
                Dim xVec As Array = DirectCast(x, Array)
                Dim stringVec = From element As Object In xVec.AsQueryable Select toString(element)

                Call Console.WriteLine($"[{xVec.Length}] " & stringVec.JoinBy(vbTab))
            ElseIf valueType Is GetType(Dictionary(Of String, Object)) Then
                For Each slot In DirectCast(x, Dictionary(Of String, Object))
                    Dim key$ = slot.Key

                    If key.IsPattern("\d+") Then
                        key = $"{listPrefix}[[{slot.Key}]]"
                    Else
                        key = $"{listPrefix}${slot.Key}"
                    End If

                    Call Console.WriteLine(key)
                    Call printer.printInternal(slot.Value, key)
                    Call Console.WriteLine()
                Next
            ElseIf valueType Is GetType(dataframe) Then
                Call DirectCast(x, dataframe) _
                    .GetTable _
                    .Print(addBorder:=False) _
                    .DoCall(AddressOf Console.WriteLine)
            Else
                Call Console.WriteLine("[1] " & toString(x))
            End If
        End Sub
    End Module
End Namespace