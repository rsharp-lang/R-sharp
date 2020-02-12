#Region "Microsoft.VisualBasic::02e99087d1358fd1f6b3cb2418e9c5c5, Library\R.base\utils\strings.vb"

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

    ' Module strings
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Levenshtein
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming.Levenshtein
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports RHtml = SMRUCC.Rsharp.Runtime.Internal.htmlPrinter

<Package("strings", Category:=APICategories.UtilityTools)>
Module strings

    Sub New()
        RHtml.AttachHtmlFormatter(Of DistResult)(AddressOf ResultVisualize.HTMLVisualize)
    End Sub

    <ExportAPI("levenshtein")>
    Public Function Levenshtein(x$, y$) As DistResult
        Return LevenshteinDistance.ComputeDistance(x, y)
    End Function
End Module
