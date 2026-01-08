Imports System.Text
Imports Microsoft.VisualBasic.My
Imports Microsoft.VisualBasic.Text

''' <summary>
''' A helper module for convert the Rscript as execute script file on linux,
''' by wrap a bash script helper
''' </summary>
Module InstallRscript

    Public Function Install(rscript As String, installLinuxRoot As Boolean) As Boolean
        ' XXXX.R -> XXXX
        '
        ' $ # alias of R# XXXX.R in linux bash
        ' $ XXXX
        Dim bash$ = If(installLinuxRoot, "/usr/local/bin/", rscript.ParentPath) & "/" & rscript.BaseName
        Dim utf8 As Encoding = Encodings.UTF8WithoutBOM.CodePage
        Dim dirHelper As String = UNIX.GetLocationHelper
        Dim script As String = dirHelper & vbLf & "
app=""$DIR/{script}""
cli=""$@""

# the R# may in other location that without PATH environment
dotnet ""{rscript}"" ""$app"" $cli" _
            .Replace("{script}", rscript.FileName) _
            .Replace("{rscript}", $"{App.HOME}/R#.dll")

        script = script.LineTokens.JoinBy(vbLf)

        Return script.SaveTo(bash, utf8)
    End Function

End Module
