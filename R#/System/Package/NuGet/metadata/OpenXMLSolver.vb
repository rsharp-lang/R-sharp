#Region "Microsoft.VisualBasic::d09fba4850192161a3f7a12108d78a04, R-sharp\R#\System\Package\NuGet\metadata\OpenXMLSolver.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 28
    '    Code Lines: 23
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 1.43 KB


    '     Module OpenXMLSolver
    ' 
    '         Function: DefaultContentTypes, defaultNugetFiles, defaultRpkgFiles
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
