Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace jsstd

    <Package("Math")>
    Public Module Math

        <ExportAPI("random")>
        Public Function random() As Double
            Return randf.seeds.NextDouble
        End Function
    End Module
End Namespace