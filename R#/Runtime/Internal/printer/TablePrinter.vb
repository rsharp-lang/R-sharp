Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter.Flags
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Internal.ConsolePrinter

    Module tablePrinter

        <Extension>
        Public Function ToContent(table As dataframe) As ConsoleTableBaseData
            Dim data As New ConsoleTableBaseData With {
                .Column = New List(Of Object)(New Object() {""}.JoinIterates(table.colnames)),
                .Rows = New List(Of List(Of Object))
            }

            For Each row As NamedCollection(Of Object) In table.forEachRow
                Call data.AppendLine(New Object() {row.name}.JoinIterates(row))
            Next

            Return data
        End Function

        <Extension>
        Public Sub PrintTable(table As dataframe, output As RContentOutput, env As GlobalEnvironment)
            Call ConsoleTableBuilder.From(table.ToContent).WithFormat(ConsoleTableBuilderFormat.Minimal).ExportAndWriteLine()
        End Sub
    End Module
End Namespace