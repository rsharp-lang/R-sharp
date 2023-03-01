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
