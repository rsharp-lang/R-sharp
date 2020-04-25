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
