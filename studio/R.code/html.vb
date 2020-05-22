#Region "Microsoft.VisualBasic::9509e69ef9cb953c717eb0d6f151f844, studio\R.code\html.vb"

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

    ' Module codeHtml
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: html
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal

<Package("devkit.code")>
Public Module codeHtml

    Sub New()
        htmlPrinter.AttachHtmlFormatter(Of String())(Function(code) html(DirectCast(code, String()).FirstOrDefault))
    End Sub

    ''' <summary>
    ''' generate R code highlights
    ''' </summary>
    ''' <param name="scriptText"></param>
    ''' <param name="debug"></param>
    ''' <returns></returns>
    <ExportAPI("R.highlights")>
    Public Function html(scriptText As String, Optional debug As Boolean = False) As String
        Dim Rscript As Rscript = Rscript.AutoHandleScript(scriptText)
        Dim [error] As String = Nothing
        Dim program As Program = Program.CreateProgram(Rscript, debug:=debug, [error]:=[error])

        Return program.toHtml
    End Function
End Module
