#Region "Microsoft.VisualBasic::745b42aa6e4ee860bada2f170c98c440, studio\Rsharp_kit\webKit\FTP.vb"

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

    '   Total Lines: 78
    '    Code Lines: 48 (61.54%)
    ' Comment Lines: 21 (26.92%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (11.54%)
    '     File Size: 2.86 KB


    ' Module FTP
    ' 
    '     Function: ftpget, list_ftpdirs
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Net.FTP
Imports Microsoft.VisualBasic.Net.WebClient
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

''' <summary>
''' ftp modules
''' </summary>
<Package("ftp", Category:=APICategories.UtilityTools)>
<RTypeExport("ftp", GetType(FtpContext))>
Module FTP

    ''' <summary>
    ''' list file names in the given ftp directory.
    ''' </summary>
    ''' <param name="ftp"></param>
    ''' <param name="dir"></param>
    ''' <param name="throwEx"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("list.ftp_dirs")>
    Public Function list_ftpdirs(ftp As FtpContext, dir As String,
                                 Optional throwEx As Boolean = False,
                                 Optional env As Environment = Nothing) As Object

        Try
            Using client As FtpClient = ftp.CreateFtpClient()
                Dim dirname As String = dir.Trim("/"c).Split("/"c).Last

                Dim entries As String() = client.ListDirectoryAsync(dir).GetAwaiter().GetResult()

                Return entries _
                    .Select(Function(a) a.Replace($"{dirname}/", "")) _
                    .ToArray
            End Using
        Catch ex As Exception
            If throwEx Then
                Return RInternal.debug.stop(New Exception(dir, ex), env)
            Else
                env.AddMessage({ex.Message, "dir: " & dir}, MSG_TYPES.WRN)
                Return {}
            End If
        End Try
    End Function

    ''' <summary>
    ''' download file via ftp
    ''' </summary>
    ''' <param name="ftp">the ftp server</param>
    ''' <param name="file">the remote file on ftp server</param>
    ''' <param name="save">
    ''' a directory path or actual file path for save the given file that download from the remote ftp server.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("ftp.get")>
    Public Function ftpget(ftp As FtpContext, file As String, Optional save As String = "./", Optional env As Environment = Nothing) As Object
        Dim filepath As String

        If save.StringEmpty OrElse save.Last = "/" Then
            filepath = $"{save}/{file.FileName}"
        Else
            filepath = save
        End If

        Using client As FtpClient = ftp.CreateFtpClient()
            client.DownloadFileAsync(file, filepath, overwrite:=True).GetAwaiter().GetResult()
        End Using

        Return True
    End Function
End Module
