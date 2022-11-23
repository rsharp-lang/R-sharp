Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime

''' <summary>
''' backend for the http help documents
''' </summary>
<Package("help")>
Public Module httpHelp

    Dim lib_renv As String

    <ExportAPI("http_load")>
    Public Function load(Optional env As Environment = Nothing) As Object
        lib_renv = env.globalEnvironment.options.lib_loc.GetDirectoryFullPath

        Return True
    End Function

    ''' <summary>
    ''' generate html page for keyword query
    ''' </summary>
    ''' <param name="term"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("search")>
    Public Function handleSearch(term As String, Optional env As Environment = Nothing) As Object


        Return ""
    End Function

    <ExportAPI("vignettes")>
    Public Function vignettes(ref As String, Optional context As String = Nothing, Optional env As Environment = Nothing) As Object
        If context.StringEmpty Then
            ' pkg$dll$index
            Dim t = ref.DecodeBase64.LoadJSON(Of String())
            Dim path = $"{lib_renv}/{t(0)}/package\vignettes/{t(1)}/{t(2)}.html"

            Return path.ReadAllText
        Else
            Dim t = context.DecodeBase64.LoadJSON(Of String())
            Dim path = $"{lib_renv}/{t(0)}/package\vignettes/{t(1)}/{ref}"

            Return path.ReadAllText
        End If
    End Function

    <ExportAPI("browse")>
    Public Function handlePackage(pkg As String, Optional env As Environment = Nothing) As Object
        Dim libmeta As String = $"{lib_renv}/{pkg}\package\index.json"
        Dim mods As String() = $"{lib_renv}/{pkg}/package\vignettes".ListDirectory.ToArray
        Dim meta As DESCRIPTION = libmeta.LoadJSON(Of DESCRIPTION)
        Dim innerTempl =
            <html>
                <head>
                    <title>{$title}</title>
                </head>
                <body>
                    <h1>
                        {$title}
                    </h1>
                    <hr/>
                    <p>
                        Documentation for package ‘{$pkg}’ version {$ver}
                    </p>
                    <p>
                        {$desc}
                    </p>
                    <table>
                        {$modules}
                    </table>
                </body>
            </html>
        Dim html As New ScriptBuilder(innerTempl)
        Dim modules As New StringBuilder

        For Each dir As String In mods
            Dim subIndex = dir.EnumerateFiles("*.html").ToArray
            Dim modLinks = subIndex.Select(Function(index) $"<a href='/vignettes?q={New String() {pkg, dir.BaseName, index.BaseName}.GetJson.Base64String}'>{index.BaseName}</a>").JoinBy("; ")
            Dim row As XElement =
                <tr>
                    <td><%= dir.BaseName %></td>
                    <td>
                        %s
                    </td>
                </tr>

            Call modules.AppendLine(sprintf(row, modLinks))
        Next

        With html
            !title = meta.Title
            !pkg = meta.Package
            !ver = meta.Version
            !desc = meta.Description
            !modules = modules.ToString
        End With

        Return html.ToString
    End Function

    <ExportAPI("index")>
    Public Function getHelpIndex(Optional env As Environment = Nothing) As Object
        Dim dirs As String() = lib_renv _
            .ListDirectory _
            .Select(Function(pkg) $"{pkg}\package\index.json") _
            .ToArray
        Dim html As New StringBuilder

        html.AppendLine($"<table>")

        For Each pkg As String In dirs
            Dim info As DESCRIPTION = pkg.LoadJsonFile(Of DESCRIPTION)
            Dim line As String = $"<tr>
<td><a href='/browse?pkg={info.Package}'>{info.Package}</a></td>
<td>{info.Title}</td>
</tr>"
            Call html.AppendLine(line)
        Next

        Return html.AppendLine("</table>").ToString
    End Function

End Module
