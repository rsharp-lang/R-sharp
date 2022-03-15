#Region "Microsoft.VisualBasic::b29d250cc226cb621c9c98db32098584, R-sharp\R#\System\Document\Document.vb"

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


     Code Statistics:

        Total Lines:   63
        Code Lines:    55
        Comment Lines: 0
        Blank Lines:   8
        File Size:     2.32 KB


    '     Class Document
    ' 
    '         Properties: author, declares, description, details, examples
    '                     keywords, parameters, returns, see_also, title
    ' 
    '         Function: ToString, UnixMan
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.Utility
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace Development

    Public Class Document

        Public Property title As String
        Public Property description As String
        Public Property parameters As NamedValue()
        Public Property returns As String
        Public Property author As String()
        Public Property details As String
        Public Property keywords As String()
        Public Property declares As FunctionDeclare
        Public Property examples As String
        Public Property see_also As String

        Public Overrides Function ToString() As String
            Return title
        End Function

        Public Function UnixMan() As UnixManPage
            Dim man As New UnixManPage With {
                .AUTHOR = author.SafeQuery.JoinBy(","),
                .DESCRIPTION = description,
                .DETAILS = details,
                .NAME = declares.name,
                .VALUE = returns,
                .OPTIONS = declares.parameters _
                    .SafeQuery _
                    .Select(Function(arg)
                                Dim info As NamedValue = parameters _
                                    .SafeQuery _
                                    .Where(Function(p) p.name = arg.name) _
                                    .FirstOrDefault

                                Return New NamedValue(Of String)(arg.name, arg.text) With {
                                    .Description = info?.text
                                }
                            End Function) _
                    .ToArray,
                .PROLOG = declares.ToString,
                .EXAMPLES = examples,
                .FILES = declares.sourceMap.ToString,
                .SEE_ALSO = see_also,
                .SYNOPSIS = declares.ToString,
                .index = New Index With {
                    .category = 1,
                    .keyword = keywords.JoinBy(", "),
                    .index = declares.name,
                    .[date] = Now,
                    .title = title
                }
            }

            Return man
        End Function

    End Class
End Namespace
