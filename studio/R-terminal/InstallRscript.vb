#Region "Microsoft.VisualBasic::ec1aa70249c8abb91bed1eb92d58e728, studio\R-terminal\InstallRscript.vb"

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

    '   Total Lines: 42
    '    Code Lines: 25 (59.52%)
    ' Comment Lines: 9 (21.43%)
    '    - Xml Docs: 44.44%
    ' 
    '   Blank Lines: 8 (19.05%)
    '     File Size: 1.38 KB


    ' Module InstallRscript
    ' 
    '     Function: Install
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.My
Imports Microsoft.VisualBasic.Text

''' <summary>
''' A helper module for convert the Rscript as execute script file on linux,
''' by wrap a bash script helper
''' </summary>
Module InstallRscript

    Const LinuxRoot As String = "/usr/local/bin/"

    Public Function Install(rscript As String, installLinuxRoot As Boolean) As Boolean
        ' XXXX.R -> XXXX
        '
        ' $ # alias of R# XXXX.R in linux bash
        ' $ XXXX
        Dim bash$ = If(installLinuxRoot, LinuxRoot, rscript.ParentPath) & "/" & rscript.BaseName
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

        If installLinuxRoot Then
            ' make copy of the script file
            Call rscript _
                .ReadAllLines _
                .FlushAllLines($"{LinuxRoot}/{rscript.FileName}")
        End If

        Return script.SaveTo(bash, utf8)
    End Function

End Module

