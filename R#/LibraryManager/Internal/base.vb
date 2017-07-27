Imports System.ComponentModel
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Library.Internal

    ''' <summary>
    ''' The R# Base Package
    ''' </summary>
    <Package("base", Category:=APICategories.UtilityTools)>
    <Description("This package contains the basic functions which let R function as a language: arithmetic, input/output, basic programming support, etc. Its contents are available through inheritance from any environment.

For a complete list of functions, use ``library(help = ""base"")``.")>
    Public Module base

        <ExportAPI("list")>
        <Description("Creates a R# object.")>
        Public Function list(args As IEnumerable(Of NamedValue(Of Object))) As Object
            Dim obj As Dictionary(Of String, Object) = args _
                .SafeQuery _
                .ToDictionary(Function(x) x.Name,
                              Function(x) x.Value)
            Return obj
        End Function
    End Module
End Namespace