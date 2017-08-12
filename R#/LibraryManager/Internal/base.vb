#Region "Microsoft.VisualBasic::eda580b9559e06efea432e4f8494cea1, ..\R-sharp\R#\LibraryManager\Internal\base.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

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
    <Description(base.describ)>
    Public Module base

        Friend Const describ$ = "This package contains the basic functions which let R function as a language: arithmetic, input/output, basic programming support, etc. 
                                 Its contents are available through inheritance from any environment.

                                 For a complete list of functions, use ``library(help = ""base"")``."

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
