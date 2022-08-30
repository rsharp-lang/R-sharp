Imports Microsoft.VisualBasic.CommandLine.Reflection

Namespace Runtime.Internal.Invokes

    Module time

        <ExportAPI("hours")>
        Public Function hours(h As Double) As TimeSpan
            Return TimeSpan.FromHours(h)
        End Function

        <ExportAPI("minutes")>
        Public Function minutes(m As Double) As TimeSpan
            Return TimeSpan.FromMinutes(m)
        End Function
    End Module
End Namespace