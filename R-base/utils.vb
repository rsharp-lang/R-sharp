#Region "Microsoft.VisualBasic::759471025a51af6639a0b6807d077a31, ..\R-sharp\R-base\utils.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.ComponentModel
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File

<Package("utils")>
<Description("")>
Public Module utils

    ''' <summary>
    ''' Reads a file in table format and creates a data frame from it, with cases corresponding to lines and variables to fields in the file.
    ''' </summary>
    ''' <param name="file$">
    ''' the name of the file which the data are to be read from. Each row of the table appears as one line of the file. If it does not contain an absolute path, the file name is relative to the current working directory, getwd(). Tilde-expansion is performed where supported. This can be a compressed file (see file).
    ''' Alternatively, file can be a readable text-mode connection (which will be opened for reading if necessary, And if so closed (And hence destroyed) at the end of the function call). (If stdin() Is used, the prompts for lines may be somewhat confusing. Terminate input with a blank line Or an EOF signal, Ctrl-D on Unix And Ctrl-Z on Windows. Any pushback on stdin() will be cleared before return.)
    ''' file can also be a complete URL. (For the supported URL schemes, see the 'URLs’ section of the help for url.)
    ''' </param>
    ''' <param name="encoding$"></param>
    ''' <returns></returns>
    <ExportAPI("read.csv")>
    Public Function readcsv(file$, Optional encoding$ = "utf8") As DataFrame
        Dim codepage As Encoding = encoding _
            .ParseEncodingsName(Encodings.UTF8) _
            .CodePage
        Return DataFrame.CreateObject(csv.Load(file, encoding:=codepage))
    End Function
End Module
