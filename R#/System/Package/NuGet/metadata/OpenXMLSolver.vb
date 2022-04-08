Imports Microsoft.VisualBasic.Text.Xml.OpenXml

Namespace Development.Package.NuGet.metadata

    Public Module OpenXMLSolver

        Private Iterator Function defaultNugetFiles() As IEnumerable(Of Type)
            Yield New Type With {.Extension = "rels", .ContentType = "application/vnd.openxmlformats-package.relationships+xml"}
            Yield New Type With {.Extension = "psmdcp", .ContentType = "application/vnd.openxmlformats-package.core-properties+xml"}
            Yield New Type With {.Extension = "dll", .ContentType = "application/octet"}
            Yield New Type With {.Extension = "xml", .ContentType = "application/octet"}
            Yield New Type With {.Extension = "png", .ContentType = "application/octet"}
            Yield New Type With {.Extension = "nuspec", .ContentType = "application/octet"}
        End Function

        Private Iterator Function defaultRpkgFiles() As IEnumerable(Of Type)
            Yield New Type With {.Extension = "map", .ContentType = "visualstudio/sourcemap"}
            Yield New Type With {.Extension = "1", .ContentType = "text/unix-manpage"}
        End Function

        Public Function DefaultContentTypes() As ContentTypes
            Return New ContentTypes With {
                .[Default] = defaultNugetFiles.ToArray,
                .[Overrides] = defaultRpkgFiles.AsList
            }
        End Function
    End Module
End Namespace