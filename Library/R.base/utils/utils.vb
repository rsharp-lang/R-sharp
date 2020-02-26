#Region "Microsoft.VisualBasic::b1917a6a07b4ea42c5ef1afbaf57a1a1, Library\R.base\utils\utils.vb"

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
    '     Function: read_csv, saveGeneric, write_csv
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports SMRUCC
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe

''' <summary>
''' The R Utils Package 
''' </summary>
<Package("utils", Category:=APICategories.UtilityTools)>
Public Module utils

    ''' <summary>
    ''' ### Data Input
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
    ''' <param name="encoding">
    ''' encoding to be assumed for input strings. It is used to mark character 
    ''' strings as known to be in Latin-1 or UTF-8 (see Encoding): it is not 
    ''' used to re-encode the input, but allows R to handle encoded strings in 
    ''' their native encoding (if one of those two). 
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("read.csv")>
    Public Function read_csv(file$, Optional encoding As Object = "unknown") As Rdataframe
        Dim datafile As File = IO.File.Load(file, encoding:=Rsharp.GetEncoding(encoding))
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
    ''' ### Data Output
    ''' 
    ''' prints its required argument ``x`` (after converting it to a 
    ''' data frame if it is not one nor a matrix) to a file or 
    ''' connection.
    ''' </summary>
    ''' <param name="x">
    ''' the object to be written, preferably a matrix or data frame. 
    ''' If not, it is attempted to coerce x to a data frame.
    ''' </param>
    ''' <param name="file">
    ''' either a character string naming a file or a connection open 
    ''' for writing. "" indicates output to the console.
    ''' </param>
    ''' <param name="fileEncoding">
    ''' character string: if non-empty declares the encoding to be 
    ''' used on a file (not a connection) so the character data can 
    ''' be re-encoded as they are written.
    ''' </param>
    ''' <param name="env"></param>
    ''' <param name="row_names">
    ''' either a logical value indicating whether the row names of 
    ''' x are to be written along with x, or a character vector of 
    ''' row names to be written.
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("write.csv")>
    <RApiReturn(GetType(Boolean))>
    Public Function write_csv(<RRawVectorArgument> x As Object, file$,
                              Optional row_names As Boolean = True,
                              Optional fileEncoding As Object = "",
                              Optional env As Environment = Nothing) As Object

        If x Is Nothing Then
            Return Internal.debug.stop("Empty dataframe object!", env)
        End If

        Dim type As Type = x.GetType

        If type Is GetType(Rdataframe) Then
            Dim matrix As String()() = DirectCast(x, Rdataframe) _
                .GetTable(
                    env:=env.globalEnvironment,
                    printContent:=False,
                    showRowNames:=row_names
                )
            Dim rows As IEnumerable(Of RowObject) = matrix _
                .Select(Function(r) New RowObject(r)) _
                .ToArray
            Dim dataframe As New File(rows)

            Return dataframe.Save(path:=file, encoding:=Encodings.UTF8WithoutBOM, silent:=True)
        ElseIf type Is GetType(File) Then
            Return DirectCast(x, File).Save(path:=file, encoding:=Encodings.UTF8WithoutBOM, silent:=True)
        ElseIf type Is GetType(IO.DataFrame) Then
            Return DirectCast(x, IO.DataFrame).Save(path:=file, encoding:=Encodings.UTF8WithoutBOM, silent:=True)
        ElseIf Runtime.isVector(Of EntityObject)(x) Then
            Return DirectCast(Runtime.asVector(Of EntityObject)(x), EntityObject()).SaveTo(path:=file, encoding:=Encodings.UTF8WithoutBOM.CodePage, silent:=True)
        ElseIf Runtime.isVector(Of DataSet)(x) Then
            Return DirectCast(Runtime.asVector(Of DataSet)(x), DataSet()).SaveTo(path:=file, encoding:=Encodings.UTF8WithoutBOM.CodePage, silent:=True)
        ElseIf type.IsArray OrElse type Is GetType(vector) Then
            Return saveGeneric(x, type, file, env)
        Else
            Return Message.InCompatibleType(GetType(File), type, env)
        End If
    End Function

    Private Function saveGeneric(x As Object, type As Type, file$, env As Environment) As Boolean
        Dim encoding As Encoding = Encodings.UTF8WithoutBOM.CodePage

        If type Is GetType(vector) Then
            x = DirectCast(x, vector).data
            type = x.GetType
        End If

        type = type.GetElementType

        Return DirectCast(x, Array).SaveTable(file, encoding, type, silent:=True)
    End Function
End Module
