#Region "Microsoft.VisualBasic::f6f7f67197fd83fcd0b1887696fd57ce, R#\LegacyRHelper.vb"

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

    '   Total Lines: 44
    '    Code Lines: 35 (79.55%)
    ' Comment Lines: 2 (4.55%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (15.91%)
    '     File Size: 1.84 KB


    ' Module LegacyRHelper
    ' 
    '     Function: RemovesInvalidEscapes
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My
Imports r = System.Text.RegularExpressions.Regex

Public Module LegacyRHelper

    Public Function RemovesInvalidEscapes(xmlDoc As String) As String
        Dim unescapesInName$() = r.Matches(xmlDoc, "[<]\d+", RegexICSng).ToArray
        ' biodeep包所生成的xml文档里面应该是没有<数字>标签的
        Dim document As New StringBuilder(xmlDoc)

        For Each unescape In unescapesInName
            document.Replace(unescape, "&lt;" & unescape.Trim("<"c))
        Next

        Call document.Replace("&plusmn;", "±")
        Call document.Replace("&alpha;", "alpha")
        Call document.Replace("&beta;", "beta")
        Call document.Replace("&Delta;", "delta")
        Call document.Replace("&delta;", "delta")
        Call document.Replace("&gamma;", "gamma")
        Call document.Replace("&Gamma;", "Gamma")
        Call document.Replace("&omega;", "omega")
        Call document.Replace("&Omega;", "Omega")
        Call document.Replace("&mu", "mu")
        Call document.Replace("&alpha", "-alpha")
        Call document.Replace("&beta", "-beta")
        Call document.Replace("&prime;", "prime")
        Call document.Replace("&prime", "prime")
        Call document.Replace("&ouml;", "ouml")
        Call document.Replace("&ouml", "ouml")
        Call document.Replace("&A7", "-A7")

        ' 处理non-printable character在xml之中的转义
        Dim nonprints = r.Matches(document.ToString, "[<]U[+]\d+[>]", RegexICSng).ToArray

        For Each bin As String In nonprints
            Call document.Replace(bin, "")
        Next

        Return RlangInterop.ProcessingRUniCode(document.ToString).DoCall(AddressOf RlangInterop.ProcessingRRawUniCode)
    End Function
End Module
