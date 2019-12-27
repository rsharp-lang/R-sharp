#Region "Microsoft.VisualBasic::0393de5a5772b8098d8c13b8beeb526a, Library\R.base\utils\utils.vb"

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
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.dataframe

''' <summary>
''' The R Utils Package 
''' </summary>
<Package("utils", Category:=APICategories.UtilityTools)>
Public Module utils

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
    ''' 
    ''' </summary>
    ''' <param name="x">
    ''' the object to be written, preferably a matrix or data frame. If not, it is attempted to coerce x to a data frame.
    ''' </param>
    ''' <param name="file">
    ''' either a character string naming a file or a connection open for writing. "" indicates output to the console.
    ''' </param>
    ''' <param name="envir"></param>
    ''' <returns></returns>
    <ExportAPI("write.csv")>
    Public Function write_csv(<RRawVectorArgument> x As Object, file$, envir As Environment) As Object
        If x Is Nothing Then
            Return Internal.debug.stop("Empty dataframe object!", envir)
        End If

        Dim type As Type = x.GetType

        If type Is GetType(Rdataframe) Then
            Dim matrix As String()() = x.GetTable
            Dim rows = matrix.Select(Function(r) New RowObject(r))
            Dim dataframe As New File(rows)

            Return dataframe.Save(path:=file)
        ElseIf type Is GetType(File) Then
            Return DirectCast(x, File).Save(path:=file)
        ElseIf type Is GetType(IO.DataFrame) Then
            Return DirectCast(x, IO.DataFrame).Save(path:=file)
        ElseIf Runtime.isVector(Of EntityObject)(x) Then
            Return DirectCast(Runtime.asVector(Of EntityObject)(x), EntityObject()).SaveTo(path:=file, silent:=True)
        ElseIf Runtime.isVector(Of DataSet)(x) Then
            Return DirectCast(Runtime.asVector(Of DataSet)(x), DataSet()).SaveTo(path:=file, silent:=True)
        Else
            Return Message.InCompatibleType(GetType(File), type, envir)
        End If
    End Function
End Module
