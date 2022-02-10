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
