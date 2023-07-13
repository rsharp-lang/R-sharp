#Region "Microsoft.VisualBasic::9e5de220b6c646a782d98f3e9d79990c, G:/GCModeller/src/R-sharp/studio/Rsharp_kit/devkit//documents/RFileHeader.vb"

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

    '   Total Lines: 43
    '    Code Lines: 31
    ' Comment Lines: 5
    '   Blank Lines: 7
    '     File Size: 1.52 KB


    ' Module RFileHeader
    ' 
    '     Function: Summary
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Text

Module RFileHeader

    Const RFuncDeclare$ = "\S+\s*(([<][-])|[=])\s*function\([^;]*?\)\s*\{"

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="r">脚本的文件路径</param>
    ''' <returns></returns>
    Public Function Summary(r$, relativePath$) As String
        Dim Rscript As String = r _
            .ReadAllText _
            .StringReplace("#Region ""Microsoft\.ROpen.+?"".+?#End Region", "", RegexOptions.Singleline) _
            .Trim(" "c, ASCII.TAB, ASCII.CR, ASCII.LF)
        Dim funcs = Rscript.Matches(RFuncDeclare, RegexICSng) _
                     .Select(Function(f)
                                 Return f.LineTokens.JoinBy(" ").StringReplace("\s{2,}", "") & "..."
                             End Function) _
                     .ToArray
        Dim md5$ = Rscript.MD5
        Dim header As New StringBuilder

        Call header.AppendLine($"#Region ""Microsoft.ROpen::{md5}, {relativePath}""")
        Call header.AppendLine()
        Call header.AppendLine($"    # Summaries:")
        Call header.AppendLine()

        For Each func As String In funcs
            Call header.AppendLine($"    # {func}")
        Next

        Call header.AppendLine()
        Call header.AppendLine($"#End Region")
        Call header.AppendLine()
        Call header.AppendLine(Rscript)

        Return header.ToString
    End Function
End Module
