Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
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
    End Module
End Namespace