#Region "Microsoft.VisualBasic::a87044891f32a3a843f417369993e079, F:/GCModeller/src/R-sharp/Library/base//utils/dataframe.vb"

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

    '   Total Lines: 700
    '    Code Lines: 526
    ' Comment Lines: 88
    '   Blank Lines: 86
    '     File Size: 26.88 KB


    ' Module dataframe
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: appendCells, appendRow, AsDataframeRaw, asIndexList, cells
    '               colnames, column, createEntityRow, CreateRowObject, dataframeTable
    '               deserialize, measureColumnVector, openCsv, parseDataframe, parseRow
    '               printRowVector, printTable, project, rawToDataFrame, readCsvRaw
    '               readDataSet, rows, rowToString, RowToString, stripCommentRows
    '               transpose, vector
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports any = Microsoft.VisualBasic.Scripting
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File
Imports Idataframe = Microsoft.VisualBasic.Data.csv.IO.DataFrame
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RPrinter = SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports Rsharp = SMRUCC.Rsharp
Imports vec = SMRUCC.Rsharp.Runtime.Internal.Object.vector

''' <summary>
''' The sciBASIC.NET dataframe api
''' </summary>
<Package("dataframe", Category:=APICategories.UtilityTools, Publisher:="xie.guigang@gcmodeller.org")>
Module dataframe

    Sub New()
        Call RPrinter.AttachConsoleFormatter(Of DataSet)(AddressOf RowToString)
        Call RPrinter.AttachConsoleFormatter(Of DataSet())(AddressOf printTable)
        Call RPrinter.AttachConsoleFormatter(Of EntityObject())(AddressOf printTable)
        ' Call RPrinter.AttachConsoleFormatter(Of csv)(AddressOf printTable)
        Call RPrinter.AttachConsoleFormatter(Of RowObject)(AddressOf printRowVector)

        Call makeDataframe.addHandler(GetType(DataSet()), AddressOf dataframeTable(Of Double, DataSet))
        Call makeDataframe.addHandler(GetType(EntityObject()), AddressOf dataframeTable(Of String, EntityObject))
    End Sub

    Private Function printRowVector(r As RowObject) As String
        Dim cellsDataPreviews As String

        If r.NumbersOfColumn > 6 Then
            cellsDataPreviews = r _
                .Take(6) _
                .Select(Function(str) $"""{str}""") _
                .JoinBy(vbTab) & vbTab & "..."
        Else
            cellsDataPreviews = r.Select(Function(str) $"""{str}""").JoinBy(vbTab)
        End If

        Return $"row, char [1:{r.NumbersOfColumn}] {cellsDataPreviews}"
    End Function

    Private Function dataframeTable(Of T, DataSet As {INamedValue, DynamicPropertyBase(Of T)})(data As DataSet(), args As list, env As Environment) As Rdataframe
        Dim names As String() = data.Keys.ToArray
        Dim table As New Rdataframe With {
            .rownames = names,
            .columns = New Dictionary(Of String, Array)
        }

        For Each colName As String In data.PropertyNames
            table.columns(colName) = data _
                .Select(Function(d) d(colName)) _
                .ToArray
        Next

        Return table
    End Function

    Private Function printTable(obj As Object) As String
        Dim matrix As csv

        Select Case obj.GetType
            Case GetType(DataSet())
                matrix = DirectCast(obj, DataSet()).ToCsvDoc
            Case GetType(EntityObject())
                matrix = DirectCast(obj, EntityObject()).ToCsvDoc
                ' Case GetType(csv)
                ' matrix = DirectCast(obj, csv)
            Case Else
                Throw New NotImplementedException
        End Select

        Return matrix _
            .AsMatrix _
            .Select(Function(r) r.ToArray) _
            .Print(False)
    End Function

    ''' <summary>
    ''' show data set summary
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Private Function RowToString(x As Object) As String
        Dim id$, length%
        Dim keys$()

        If x.GetType Is GetType(DataSet) Then
            With DirectCast(x, DataSet)
                id = .ID
                length = .Properties.Count
                keys = .Properties _
                       .Keys _
                       .ToArray
            End With
        Else
            With DirectCast(x, EntityObject)
                id = .ID
                length = .Properties.Count
                keys = .Properties _
                       .Keys _
                       .ToArray
            End With
        End If

        Return $"${id} {length} slots {{{keys.Take(3).JoinBy(", ")}..."
    End Function

    ''' <summary>
    ''' Load .NET objects from a given dataframe data object.
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="type">
    ''' the object type value, and it should be one of the:
    ''' 
    ''' 1. class name from the <see cref="RTypeExportAttribute"/>
    ''' 2. .NET CLR <see cref="Type"/> value
    ''' 3. R-sharp <see cref="RType"/> value
    ''' 4. R-sharp primitive <see cref="TypeCodes"/> value
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("as.objects")>
    <RApiReturn(GetType(vec))>
    Public Function deserialize(data As csv, type As Object,
                                Optional strict As Boolean = False,
                                Optional metaBlank$ = "",
                                Optional silent As Boolean = True,
                                Optional env As Environment = Nothing) As Object

        Dim dataframe As Idataframe = Idataframe.CreateObject(data)
        Dim schema As RType = env.globalEnvironment.GetType(type)
        Dim result As IEnumerable(Of Object) = dataframe.LoadDataToObject(
            type:=schema.raw,
            strict:=strict,
            metaBlank:=metaBlank,
            silent:=silent
        )
        Dim vector As New vec(schema, result, env)

        Return vector
    End Function

    <Extension>
    Private Function measureColumnVector(col As String(), check_modes As Boolean) As Array
        If check_modes Then
            Return col _
                .Skip(1) _
                .ToArray _
                .DoCall(AddressOf TypeCast.DataImports.ParseVector)
        Else
            Return DirectCast(col.Skip(1).ToArray, Array)
        End If
    End Function

    ''' <summary>
    ''' filter out all data rows that line start with the <paramref name="comment_char"/>, default is ``#`` symbol.
    ''' </summary>
    ''' <param name="raw"></param>
    ''' <param name="comment_char"></param>
    ''' <returns></returns>
    <Extension>
    Private Function stripCommentRows(raw As csv, comment_char As Char) As csv
        If comment_char = ASCII.NUL Then
            Return raw
        End If

        Dim data As New List(Of RowObject)

        For Each row As RowObject In raw
            If row.NumbersOfColumn = 0 Then
                Continue For
            ElseIf Not row.First.StringEmpty Then
                If row.First.First = comment_char Then
                    Continue For
                End If
            End If

            Call data.Add(row)
        Next

        Return New csv(data)
    End Function

    ''' <summary>
    ''' convert the raw csv table object to R dataframe object.
    ''' </summary>
    ''' <param name="raw"></param>
    ''' <returns></returns>
    <ExportAPI("rawToDataFrame")>
    <Extension>
    <RApiReturn(GetType(Rdataframe))>
    Public Function rawToDataFrame(raw As csv,
                                   <RRawVectorArgument>
                                   Optional row_names As Object = Nothing,
                                   Optional check_names As Boolean = True,
                                   Optional check_modes As Boolean = True,
                                   Optional comment_char As Char = "#"c,
                                   Optional skip_rows As Integer = -1,
                                   Optional env As Environment = Nothing) As Object

        Dim cols() = raw _
            .DoCall(Function(table)
                        If skip_rows > 0 Then
                            Return New csv(table.Skip(skip_rows))
                        Else
                            Return table
                        End If
                    End Function) _
            .stripCommentRows(comment_char).Columns _
            .ToArray
        Dim colNames As String() = cols _
            .Select(Function(col) col(Scan0)) _
            .ToArray

        If check_names Then
            colNames = Internal.Invokes.base.makeNames(colNames, unique:=True)
        Else
            ' the underlying base type is dictionary
            ' for columns, this required of the column
            ' name as dictionary key should be unique!
            colNames = colNames.uniqueNames
        End If

        Dim dataframe As New Rdataframe() With {
            .columns = cols _
                .SeqIterator _
                .AsParallel _
                .Select(Function(i)
                            Return (i.i, colVec:=i.value.measureColumnVector(check_modes))
                        End Function) _
                .OrderBy(Function(i) i.i) _
                .ToDictionary(Function(col) colNames(col.i),
                              Function(col)
                                  Return col.colVec
                              End Function)
        }

        If Not row_names Is Nothing Then
            Dim err As New Value(Of Message)

            If TypeOf row_names Is Boolean AndAlso Not DirectCast(row_names, Boolean) Then
                GoTo ReturnTable
            End If

            row_names = ensureRowNames(row_names, env)

            If Program.isException(row_names) Then
                Return row_names
            End If
            If Not err = dataframe.setRowNames(row_names, env) Is Nothing Then
                Return err.Value
            End If
        End If

ReturnTable:
        Return dataframe
    End Function

    ''' <summary>
    ''' raw dataframe to rows
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    <ExportAPI("rows")>
    Public Function rows(file As csv) As RowObject()
        Return file.ToArray
    End Function

    ''' <summary>
    ''' convert the csv document row object to a text row in the table text file.
    ''' </summary>
    ''' <param name="r"></param>
    ''' <param name="delimiter">
    ''' a character string that used as delimiter, meta data in C language like 
    ''' ``\t`` is supported.
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("rowToString")>
    Public Function rowToString(r As RowObject, Optional delimiter As String = ",") As String
        If r Is Nothing Then
            Return ""
        Else
            Return r.AsLine(CLangStringFormatProvider.ReplaceMetaChars(delimiter))
        End If
    End Function

    ''' <summary>
    ''' raw dataframe get column data by index
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="index"></param>
    ''' <returns></returns>
    <ExportAPI("column")>
    Public Function column(file As csv, index As Integer) As String()
        Return file.Column(index).ToArray
    End Function

    <ExportAPI("cells")>
    Public Function cells(x As Object, Optional env As Environment = Nothing) As Object
        Dim type As Type

        If x Is Nothing Then
            Return New String() {}
        Else
            type = x.GetType
        End If

        Select Case type
            Case GetType(RowObject)
                Return DirectCast(x, RowObject).ToArray
            Case GetType(DataSet)
                Return DirectCast(x, DataSet).Properties.Values.ToArray
            Case GetType(EntityObject)
                Return DirectCast(x, EntityObject).Properties.Values.ToArray
            Case Else
                Return Internal.debug.stop(New InvalidCastException(type.FullName), env)
        End Select
    End Function

    <ExportAPI("read.csv.raw")>
    Public Function readCsvRaw(file$, Optional encoding As Object = "") As csv
        Return csv.Load(file, Rsharp.GetEncoding(encoding))
    End Function

    <ExportAPI("open.csv")>
    Public Function openCsv(file$, Optional encoding As Object = "") As RDispose
        Dim table As New csv
        Dim textEncode As Encoding = Rsharp.GetEncoding(encoding)

        Return New RDispose(table, Sub() table.Save(file, textEncode))
    End Function

    <ExportAPI("as.dataframe.raw")>
    <RApiReturn(GetType(csv))>
    Public Function AsDataframeRaw(<RRawVectorArgument> rows As Object,
                                   <RRawVectorArgument>
                                   Optional colnames As Object = Nothing,
                                   Optional env As Environment = Nothing) As Object

        Dim rowCols As RowObject()
        Dim rowPipe As pipeline = pipeline.TryCreatePipeline(Of RowObject)(rows, env)

        If rowPipe.isError Then
            Return rowPipe.getError
        Else
            rowCols = rowPipe _
                .populates(Of RowObject)(env) _
                .ToArray
        End If

        If colnames Is Nothing Then
            Return New csv(rowCols)
        Else
            Dim names As String() = CLRVector.asCharacter(colnames)
            Dim headers As New RowObject(names)

            Return New csv().AppendLine(headers).AppendLines(rowCols)
        End If
    End Function

    <ExportAPI("row")>
    Public Function CreateRowObject(Optional vals As Array = Nothing) As RowObject
        Return New RowObject(vals.AsObjectEnumerator.Select(AddressOf any.ToString))
    End Function

    ''' <summary>
    ''' parse text line as csv row.
    ''' </summary>
    ''' <param name="line"></param>
    ''' <returns></returns>
    <ExportAPI("parseRow")>
    Public Function parseRow(line As String, Optional delimiter As Char = ","c, Optional quot As Char = ASCII.Quot) As String()
        Return Tokenizer.CharsParser(line, delimiter, quot).ToArray
    End Function

    ''' <summary>
    ''' parse the text data as the dataframe
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("parseDataframe")>
    Public Function parseDataframe(text As String,
                                   <RRawVectorArgument>
                                   Optional row_names As Object = Nothing,
                                   Optional check_names As Boolean = True,
                                   Optional check_modes As Boolean = True,
                                   Optional comment_char As String = "#",
                                   Optional tsv As Boolean = False,
                                   Optional skip_rows As Integer = -1,
                                   Optional env As Environment = Nothing) As Rdataframe

        Dim table As csv = csv.Parse(text, tsv:=tsv)
        Dim df = table.rawToDataFrame(
            row_names:=row_names,
            check_names:=check_names,
            check_modes:=check_modes,
            comment_char:=comment_char,
            skip_rows:=skip_rows,
            env:=env
        )

        Return df
    End Function

    <ExportAPI("append.cells")>
    Public Function appendCells(row As RowObject, cells As Array) As RowObject
        row.AddRange(cells.AsObjectEnumerator.Select(AddressOf any.ToString))
        Return row
    End Function

    <ExportAPI("append.row")>
    Public Function appendRow(table As csv, row As RowObject) As csv
        table.Add(row)
        Return table
    End Function

    ''' <summary>
    ''' Get/Set column names of the given <paramref name="dataset"/>
    ''' </summary>
    ''' <param name="dataset"></param>
    ''' <param name="values"></param>
    ''' <param name="envir"></param>
    ''' <returns></returns>
    <ExportAPI("dataset.colnames")>
    Public Function colnames(dataset As Array, <RByRefValueAssign> Optional values As Array = Nothing, Optional envir As Environment = Nothing) As Object
        Dim baseElement As Type = REnv.MeasureArrayElementType(dataset)

        If values Is Nothing OrElse values.Length = 0 Then
            If baseElement Is GetType(EntityObject) Then
                Return dataset.AsObjectEnumerator _
                    .Select(Function(d)
                                Return DirectCast(d, EntityObject)
                            End Function) _
                    .PropertyNames
            ElseIf baseElement Is GetType(DataSet) Then
                Return dataset.AsObjectEnumerator _
                    .Select(Function(d)
                                Return DirectCast(d, DataSet)
                            End Function) _
                    .PropertyNames
            Else
                Return Internal.debug.stop(New InvalidProgramException, envir)
            End If
        Else
            Dim names As String() = CLRVector.asCharacter(values)

            If baseElement Is GetType(EntityObject) Then
                Return dataset.AsObjectEnumerator(Of EntityObject) _
                    .ColRenames(names) _
                    .Select(Function(a)
                                Return DirectCast(a, EntityObject)
                            End Function) _
                    .ToArray
            Else
                Return dataset.AsObjectEnumerator(Of DataSet) _
                    .ColRenames(names) _
                    .Select(Function(a)
                                Return DirectCast(a, DataSet)
                            End Function) _
                    .ToArray
            End If
        End If
    End Function

    ''' <summary>
    ''' Get/set value of a given data column
    ''' </summary>
    ''' <param name="dataset"></param>
    ''' <param name="col$"></param>
    ''' <param name="values"></param>
    ''' <param name="envir"></param>
    ''' <returns></returns>
    <ExportAPI("dataset.vector")>
    Public Function vector(dataset As Array, col$, <RByRefValueAssign> Optional values As Array = Nothing, Optional envir As Environment = Nothing) As Object
        If dataset Is Nothing OrElse dataset.Length = 0 Then
            Return Nothing
        End If

        Dim baseElement As Type = REnv.MeasureArrayElementType(dataset)
        Dim vectors As New List(Of Object)()
        Dim isGetter As Boolean = False
        Dim getValue As Func(Of Object) = Nothing

        If values Is Nothing Then
            isGetter = True
        ElseIf values.Length = 1 Then
            Dim firstValue As Object = REnv.getFirst(values)
            getValue = Function() firstValue
        Else
            Dim populator As IEnumerator = values.GetEnumerator
            getValue = Function() As Object
                           populator.MoveNext()
                           Return populator.Current
                       End Function
        End If

        If Not isGetter Then
            vectors = New List(Of Object)(values.AsObjectEnumerator)
        End If

#Disable Warning
        If baseElement Is GetType(EntityObject) Then
            For Each item As EntityObject In REnv.asVector(Of EntityObject)(dataset)
                If isGetter Then
                    vectors.Add(item(col))
                Else
                    item(col) = Scripting.ToString(getValue())
                End If
            Next
        ElseIf baseElement Is GetType(DataSet) Then
            For Each item As DataSet In REnv.asVector(Of DataSet)(dataset)
                If isGetter Then
                    vectors.Add(item(col))
                Else
                    item(col) = Conversion.CTypeDynamic(Of Double)(getValue())
                End If
            Next
        Else
            Return Internal.debug.stop(New InvalidProgramException, envir)
        End If
#Enable Warning

        Return vectors.ToArray
    End Function

    ''' <summary>
    ''' Subset of the given dataframe by columns
    ''' </summary>
    ''' <param name="dataset"></param>
    ''' <param name="cols">a character vector of the dataframe column names.</param>
    ''' <returns></returns>
    <ExportAPI("dataset.project")>
    Public Function project(dataset As Array, cols$(), Optional envir As Environment = Nothing) As Object
        Dim baseElement As Type = REnv.MeasureArrayElementType(dataset)

        If baseElement Is GetType(EntityObject) Then
            Return dataset.AsObjectEnumerator _
                .Select(Function(d)
                            Dim row As EntityObject = DirectCast(d, EntityObject)
                            Dim subset As New EntityObject With {
                                .ID = row.ID,
                                .Properties = row.Properties.Subset(cols)
                            }

                            Return subset
                        End Function) _
                .ToArray
        ElseIf baseElement Is GetType(DataSet) Then
            Return dataset.AsObjectEnumerator _
                .Select(Function(d)
                            Dim row As DataSet = DirectCast(d, DataSet)
                            Dim subset As New DataSet With {
                                .ID = row.ID,
                                .Properties = row.Properties.Subset(cols)
                            }

                            Return subset
                        End Function) _
                .ToArray
        Else
            Return Internal.debug.stop(New InvalidProgramException, envir)
        End If
    End Function

    <ExportAPI("dataset.transpose")>
    Public Function transpose(dataset As Array, Optional env As Environment = Nothing) As Object
        Dim baseElement As Type = REnv.MeasureArrayElementType(dataset)

        If baseElement Is GetType(EntityObject) Then
            Return dataset.AsObjectEnumerator(Of EntityObject).Transpose
        ElseIf baseElement Is GetType(DataSet) Then
            Return dataset.AsObjectEnumerator(Of DataSet).Transpose
        Else
            Return Internal.debug.stop(New InvalidProgramException, env)
        End If
    End Function

    <ExportAPI("as.index")>
    <RApiReturn(GetType(list))>
    Public Function asIndexList(<RRawVectorArgument> dataframe As Object, Optional env As Environment = Nothing) As Object
        Dim data As pipeline = pipeline.TryCreatePipeline(Of EntityObject)(dataframe, env, suppress:=True)

        If data.isError Then
            data = pipeline.TryCreatePipeline(Of DataSet)(dataframe, env)

            If data.isError Then
                Return data.getError
            End If

            Return data.populates(Of DataSet)(env) _
                .ToDictionary(Function(a) a.ID, Function(a) CObj(a)) _
                .DoCall(Function(index)
                            Return New list With {
                                .slots = index
                            }
                        End Function)
        Else
            Return data.populates(Of EntityObject)(env) _
                .ToDictionary(Function(a) a.ID, Function(a) CObj(a)) _
                .DoCall(Function(index)
                            Return New list With {
                                .slots = index
                            }
                        End Function)
        End If
    End Function

    ''' <summary>
    ''' Read dataframe
    ''' </summary>
    ''' <param name="file">the csv file</param>
    ''' <param name="mode"></param>
    ''' <returns></returns>
    <ExportAPI("read.dataframe")>
    Public Function readDataSet(file$,
                                Optional mode As DataModes = DataModes.numeric,
                                Optional toRObj As Boolean = False,
                                Optional silent As Boolean = True,
                                Optional strict As Boolean = False,
                                Optional env As Environment = Nothing) As Object

        Dim dataframe As New Rdataframe

        If Not file.FileExists Then
            Dim msg = {$"the given data table file is not exists on your file system!", $"file: {file}"}

            If strict Then
                Return Internal.debug.stop(msg, env)
            Else
                Call env.AddMessage(msg, MSG_TYPES.WRN)
            End If
        End If

        Select Case mode
            Case DataModes.numeric
                Dim data = DataSet.LoadDataSet(file, silent:=silent).ToArray

                If toRObj Then
                    Dim allKeys$() = data.PropertyNames

                    dataframe.rownames = data.Select(Function(r) r.ID).ToArray
                    dataframe.columns = allKeys _
                        .ToDictionary(Function(key) key,
                                      Function(key)
                                          Return DirectCast(data.Select(Function(r) r(key)).ToArray, Array)
                                      End Function)
                Else
                    Return data
                End If
            Case DataModes.character
                Dim data = EntityObject.LoadDataSet(file, silent:=silent).ToArray

                If toRObj Then
                    Dim allKeys$() = data.PropertyNames

                    dataframe.rownames = data.Select(Function(r) r.ID).ToArray
                    dataframe.columns = allKeys _
                        .ToDictionary(Function(key) key,
                                      Function(key)
                                          Return DirectCast(data.Select(Function(r) r(key)).ToArray, Array)
                                      End Function)
                Else
                    Return data
                End If
            Case Else
                dataframe = utils.read_csv(file)
        End Select

        Return dataframe
    End Function

    <ExportAPI("entityRow")>
    Public Function createEntityRow(id As String,
                                    <RListObjectArgument>
                                    properties As list,
                                    Optional env As Environment = Nothing) As EntityObject

        Dim metadata As Dictionary(Of String, String) = properties.AsGeneric(Of String)(env)
        Dim obj As New EntityObject With {
            .ID = id,
            .Properties = metadata
        }

        Return obj
    End Function
End Module
