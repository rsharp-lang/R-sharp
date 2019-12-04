Imports Microsoft.VisualBasic.CommandLine.Reflection

Namespace Runtime.Internal.Invokes

    Module math

        <ExportAPI("round")>
        Public Function round(x As Object, Optional decimals% = 0) As Object
            If x.GetType.IsInheritsFrom(GetType(Array)) Then
                Return (From element As Object In DirectCast(x, Array).AsQueryable Select math.round(CDbl(element), decimals)).ToArray
            Else
                Return math.round(CDbl(x), decimals)
            End If
        End Function
    End Module
End Namespace