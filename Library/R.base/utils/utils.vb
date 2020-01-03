#Region "Microsoft.VisualBasic::1cfa41805a2df318d115030184d92501, Library\R.base\utils\utils.vb"

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

' Module utils
' 
'     Function: read_csv, write_csv
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe

''' <summary>
''' The R Utils Package 
''' </summary>
<Package("utils", Category:=APICategories.UtilityTools)>
Public Module utils

    ''' <summary>
    ''' # Data Input
    ''' 
    ''' Reads a file in table format and creates a data frame from it, 
    ''' with cases corresponding to lines and variables to fields in 
    ''' the file.
    ''' </summary>
    ''' <param name="file">
    ''' the name of the file which the data are to be read from. Each 
    ''' row of the table appears as one line of the file. If it does 
    ''' not contain an absolute path, the file name is relative to the 
    ''' current working directory, getwd(). Tilde-expansion is performed 
    ''' where supported. This can be a compressed file (see file).
    ''' 
    ''' Alternatively, file can be a readable text-mode connection (which 
    ''' will be opened for reading if necessary, And if so closed (And 
    ''' hence destroyed) at the end of the function call). (If ``stdin()``
    ''' Is used, the prompts for lines may be somewhat confusing. 
    ''' Terminate input with a blank line Or an EOF signal, Ctrl-D on Unix 
    ''' And Ctrl-Z on Windows. Any pushback on stdin() will be cleared 
    ''' before return.)
    ''' 
    ''' file can also be a complete URL. (For the supported URL schemes, 
    ''' see the 'URLs’ section of the help for ``url``.)
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("read.csv")>
    Public Function read_csv(file As String) As Rdataframe
        Dim datafile As File = IO.File.Load(file)
        Dim cols = datafile.Columns.ToArray
        Dim dataframe As New Rdataframe() With {
            .columns = cols _
                .ToDictionary(Function(col) col(Scan0),
                              Function(col)
                                  Return DirectCast(col.Skip(1).ToArray, Array)
                              End Function)
        }

        Return dataframe
    End Function

    ''' <summary>
    ''' # Data Output
    ''' 
    ''' prints its required argument ``x`` (after converting it to a 
    ''' data frame if it is not one nor a matrix) to a file or 
    ''' connection.
    ''' </summary>
    ''' <param name="x">
    ''' the object to be written, preferably a matrix or data frame. If not, it is attempted to coerce x to a data frame.
    ''' </param>
    ''' <param name="file">
    ''' either a character string naming a file or a connection open for writing. "" indicates output to the console.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("write.csv")>
    Public Function write_csv(<RRawVectorArgument> x As Object, file$, env As Environment) As Object
        If x Is Nothing Then
            Return Internal.debug.stop("Empty dataframe object!", env)
        End If

        Dim type As Type = x.GetType

        If type Is GetType(Rdataframe) Then
            Dim matrix As String()() = DirectCast(x, Rdataframe).GetTable(env.globalEnvironment)
            Dim rows = matrix.Select(Function(r) New RowObject(r))
            Dim dataframe As New File(rows)

            Return dataframe.Save(path:=file, encoding:=Encodings.UTF8WithoutBOM, silent:=True)
        ElseIf type Is GetType(File) Then
            Return DirectCast(x, File).Save(path:=file, encoding:=Encodings.UTF8WithoutBOM, silent:=True)
        ElseIf type Is GetType(IO.DataFrame) Then
            Return DirectCast(x, IO.DataFrame).Save(path:=file, encoding:=Encodings.UTF8WithoutBOM, silent:=True)
        ElseIf Runtime.isVector(Of EntityObject)(x) Then
            Return DirectCast(Runtime.asVector(Of EntityObject)(x), EntityObject()).SaveTo(path:=file, silent:=True)
        ElseIf Runtime.isVector(Of DataSet)(x) Then
            Return DirectCast(Runtime.asVector(Of DataSet)(x), DataSet()).SaveTo(path:=file, silent:=True)
        Else
            Return Message.InCompatibleType(GetType(File), type, env)
        End If
    End Function
End Module
