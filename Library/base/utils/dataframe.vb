#Region "Microsoft.VisualBasic::32a00b0c12f469e85f67b432142fa67f, Library\base\utils\dataframe.vb"

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

    '   Total Lines: 1050
    '    Code Lines: 713 (67.90%)
    ' Comment Lines: 203 (19.33%)
    '    - Xml Docs: 90.64%
    ' 
    '   Blank Lines: 134 (12.76%)
    '     File Size: 41.41 KB


    ' Module dataframeTools
    ' 
    '     Function: anyToFeatureFrame, appendCells, appendRow, AsDataframeRaw, asIndexList
    '               cells, characterLoader, colnames, column, create_tableFrame
    '               createEntityRow, CreateRowObject, dataframeTable, deserialize, field_vector
    '               loadDataframe, measureColumnVector, numericLoader, openCsv, parseArffText
    '               parseDataframe, parseRow, printRowVector, printTable, project
    '               rawToDataFrame, read_arff_f, readCsvRaw, readDataSet, rows
    '               rowToString, RowToString, stripCommentRows, to_dataframe, transpose
    '               vector, write_arff, writeDataframe
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Data.Framework.IO.ArffFile
Imports Microsoft.VisualBasic.Data.Framework.IO.CSVFile
Imports Microsoft.VisualBasic.Data.Framework.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports any = Microsoft.VisualBasic.Scripting
Imports ASCII = Microsoft.VisualBasic.Text.ASCII
Imports csv = Microsoft.VisualBasic.Data.Framework.IO.File
Imports FeatureFrame = Microsoft.VisualBasic.Data.Framework.DataFrame
Imports Idataframe = Microsoft.VisualBasic.Data.Framework.StorageProvider.DataFrameResolver
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal
Imports RPrinter = SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports Rsharp = SMRUCC.Rsharp
Imports vec = SMRUCC.Rsharp.Runtime.Internal.Object.vector
Imports ParquetStore = Microsoft.VisualBasic.DataStorage.Parquet

''' <summary>
''' The sciBASIC.NET dataframe api
''' </summary>
<Package("dataframe", Category:=APICategories.UtilityTools, Publisher:="xie.guigang@gcmodeller.org")>
Module dataframeTools

    Friend Sub Main()
        Call RPrinter.AttachConsoleFormatter(Of DataSet)(AddressOf RowToString)
        Call RPrinter.AttachConsoleFormatter(Of DataSet())(AddressOf printTable)
        Call RPrinter.AttachConsoleFormatter(Of EntityObject())(AddressOf printTable)
        ' Call RPrinter.AttachConsoleFormatter(Of csv)(AddressOf printTable)
        Call RPrinter.AttachConsoleFormatter(Of RowObject)(AddressOf printRowVector)

        Call makeDataframe.addHandler(GetType(DataSet()), AddressOf dataframeTable(Of Double, DataSet))
        Call makeDataframe.addHandler(GetType(EntityObject()), AddressOf dataframeTable(Of String, EntityObject))
        Call makeDataframe.addHandler(GetType(CharacterTable), AddressOf to_dataframe)
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

    <RGenericOverloads("as.data.frame")>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Function to_dataframe(chars As CharacterTable, args As list, env As Environment) As Rdataframe
        Return dataframeTable(Of String, EntityObject)(chars.AsEnumerable.ToArray, args, env)
    End Function

    <RGenericOverloads("as.data.frame")>
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
    Public Function deserialize(<RRawVectorArgument> data As Object, type As Object,
                                Optional strict As Boolean = False,
                                Optional metaBlank$ = "",
                                Optional silent As Boolean = True,
                                Optional env As Environment = Nothing) As Object

        Dim dataframe As Idataframe

        If data Is Nothing Then
            Return Nothing
        End If

        If TypeOf data Is csv Then
            dataframe = Idataframe.CreateObject(data)
        ElseIf TypeOf data Is Rdataframe Then
            With DirectCast(data, Rdataframe).DataFrameRows(True, "G7", env)
                If .GetUnderlyingType Is GetType(Message) Then
                    Return .TryCast(Of Message)
                Else
                    dataframe = Idataframe.CreateObject(.TryCast(Of csv))
                End If
            End With
        ElseIf TypeOf data Is Idataframe Then
            dataframe = data
        Else
            Return Message.InCompatibleType(GetType(Rdataframe), data.GetType, env)
        End If

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
    Private Function measureColumnVector(col As String(), check_modes As Boolean, header As Boolean) As Array
        ' if header then first row is used as headers
        ' skip first row, otherwise do nothing
        Dim offset As Integer = If(header, 1, 0)

        If check_modes Then
            Return col _
                .Skip(offset) _
                .ToArray _
                .DoCall(AddressOf TypeCast.DataImports.ParseVector)
        Else
            Return DirectCast(col.Skip(offset).ToArray, Array)
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
    ''' <returns>this function will returns nothing if the target csv data input is nothing or contains no rows data.</returns>
    <ExportAPI("rawToDataFrame")>
    <Extension>
    <RApiReturn(GetType(Rdataframe))>
    Public Function rawToDataFrame(raw As csv,
                                   <RRawVectorArgument>
                                   Optional row_names As Object = Nothing,
                                   Optional header As Boolean = True,
                                   Optional check_names As Boolean = True,
                                   Optional check_modes As Boolean = True,
                                   Optional comment_char As Char = "#"c,
                                   Optional skip_rows As Integer = -1,
                                   Optional env As Environment = Nothing) As Object

        Dim cols() = raw _
            .DoCall(Function(table)
                        If skip_rows > 0 Then
                            Return New csv(table.SafeQuery.Skip(skip_rows))
                        Else
                            Return table
                        End If
                    End Function) _
            .stripCommentRows(comment_char).Columns _
            .ToArray

        If cols.IsNullOrEmpty Then
            ' file is empty
            Call env.AddMessage("empty table file contents!")
            Return Nothing
        End If

        ' get first row as the column headers
        Dim colNames As String() = cols _
            .Select(Function(col) col(Scan0)) _
            .ToArray

        If header = False Then
            colNames = cols _
                .Select(Function(col, i) $"V{i + 1}") _
                .ToArray
        End If

        If check_names Then
            colNames = RInternal.Invokes.base.makeNames(colNames, unique:=True)
        Else
            ' the underlying base type is dictionary
            ' for columns, this required of the column
            ' name as dictionary key should be unique!
            colNames = colNames.UniqueNames
        End If

        Dim dataframe As New Rdataframe() With {
            .columns = cols _
                .SeqIterator _
                .AsParallel _
                .Select(Function(i)
                            Return (i.i, colVec:=i.value.measureColumnVector(check_modes, header))
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
                Return RInternal.debug.stop(New InvalidCastException(type.FullName), env)
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

    ''' <summary>
    ''' cast the given array data as a single <see cref="RowObject"/>
    ''' </summary>
    ''' <param name="vals"></param>
    ''' <returns></returns>
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

    ''' <summary>
    ''' parse the given arff data file as clr dataframe object
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    <ExportAPI("parse_arff")>
    Public Function parseArffText(text As String) As FeatureFrame
        Return ArffReader.LoadDataFrame(New MemoryStream(Encoding.UTF8.GetBytes(text)))
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
    Public Function colnames(dataset As Array,
                             <RByRefValueAssign>
                             Optional values As Array = Nothing,
                             Optional envir As Environment = Nothing) As Object

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
                Return RInternal.debug.stop(New InvalidProgramException, envir)
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
    Public Function vector(dataset As Array, col$,
                           <RByRefValueAssign>
                           Optional values As Array = Nothing,
                           Optional envir As Environment = Nothing) As Object

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
            Return RInternal.debug.stop(New InvalidProgramException, envir)
        End If
#Enable Warning

        Return vectors.ToArray
    End Function

    <ExportAPI("as.table_frame")>
    <RApiReturn(GetType(CharacterTable))>
    Public Function create_tableFrame(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
        Dim pull As pipeline = pipeline.TryCreatePipeline(Of EntityObject)(x, env)

        If pull.isError Then
            Return pull.getError
        End If

        Return New CharacterTable(pull.populates(Of EntityObject)(env))
    End Function

    ''' <summary>
    ''' get field string vector
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="colname"></param>
    ''' <returns></returns>
    <ExportAPI("field_vector")>
    Public Function field_vector(x As CharacterTable, colname As String) As String()
        Return x(colname)
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
            Return RInternal.debug.stop(New InvalidProgramException, envir)
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
            Return RInternal.debug.stop(New InvalidProgramException, env)
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
    ''' load clr object from R# dataframe object
    ''' </summary>
    ''' <param name="type">
    ''' the clr type description of the taarget clr object <see cref="Type"/>
    ''' </param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("load.dataframe")>
    Public Function loadDataframe(x As Rdataframe, type As Object, Optional env As Environment = Nothing) As Object
        Dim typeinfo As Type = env.globalEnvironment.GetType([typeof]:=type)
        Dim document = x.DataFrameRows(Nothing, formatNumber:=Nothing, env)

        If document Like GetType(Message) Then
            Return document.TryCast(Of Message)
        End If

        Dim rawdata As Idataframe = Idataframe.CreateObject(document.TryCast(Of csv))
        Dim seq As Array = rawdata _
            .LoadDataToObject(typeinfo, strict:=False, metaBlank:="", silent:=True) _
            .ToArray
        Dim generic As Array = REnv.TryCastGenericArray(seq, env)

        Return generic
    End Function

    <ExportAPI("read.arff")>
    Public Function read_arff_f(file As String, Optional env As Environment = Nothing) As Object
        Return FeatureFrame.read_arff(file).toDataframe(list.empty, env)
    End Function

    ''' <summary>
    ''' Writes the given data object to an ARFF (Attribute-Relation File Format) file or stream.
    ''' </summary>
    ''' <param name="x">The data object to write. Can be a FeatureFrame, Rdataframe, array, vector, or Nothing.</param>
    ''' <param name="file">Target file path or writable stream object. If a path is provided, the stream will be automatically closed after writing.</param>
    ''' <param name="env">The R runtime environment for error handling and stream operations.</param>
    ''' <returns>Returns True on success. Returns error Message object if file/stream creation or data conversion fails.</returns>
    ''' <example>
    ''' df = data.frame(x = 1:5, labels = ["A","B","C","D","E"]);
    ''' write.arff(df, file = "./data.arff");
    ''' </example>
    <ExportAPI("write.arff")>
    <RApiReturn(GetType(Boolean))>
    Public Function write_arff(x As Object, file As Object, Optional env As Environment = Nothing) As Object
        Dim auto_close As Boolean = False
        Dim s = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Write, env, is_filepath:=auto_close)
        Dim pull_df As Object = anyToFeatureFrame(x, env)

        If s Like GetType(Message) Then
            Return s.TryCast(Of Message)
        ElseIf TypeOf pull_df Is Message Then
            Return pull_df
        End If

        Call FeatureFrame.write_arff(DirectCast(pull_df, FeatureFrame), s)
        Call s.TryCast(Of Stream).Flush()

        If auto_close Then
            Call s.TryCast(Of Stream).Dispose()
        End If

        Return True
    End Function

    ''' <summary>
    ''' Converts various object types to a standardized FeatureFrame structure.
    ''' </summary>
    ''' <param name="x">Input object to convert. Supported types:
    ''' - Nothing: Creates empty FeatureFrame
    ''' - FeatureFrame: Direct cast
    ''' - Rdataframe: Converts using feature set conversion
    ''' - Array/Vector: Creates FeatureFrame from element type
    ''' - Other types: Returns type conversion error</param>
    ''' <param name="env">R environment for error handling during conversion.</param>
    ''' <returns>Returns FeatureFrame on success, or Message object containing conversion error details.</returns>
    Public Function anyToFeatureFrame(x As Object, env As Environment) As Object
        Dim frame As FeatureFrame

        If x Is Nothing Then
            frame = New FeatureFrame With {
                .features = New Dictionary(Of String, FeatureVector),
                .rownames = {}
            }
        ElseIf TypeOf x Is FeatureFrame Then
            frame = DirectCast(x, FeatureFrame)
        ElseIf TypeOf x Is Rdataframe Then
            Dim cast = MathDataSet.toFeatureSet(DirectCast(x, Rdataframe), env)

            If TypeOf cast Is Message Then
                Return cast
            Else
                frame = DirectCast(cast, FeatureFrame)
            End If
        ElseIf TypeOf x Is vec OrElse x.GetType.IsArray Then
            Dim source = CLRVector.asObject(x).TryCastGenericArray(env)

            If TypeOf source Is Message Then
                Return source
            End If

            Dim type As Type = source _
                .GetType _
                .GetElementType

            frame = CLRVector.asObject(source).StreamToFrame(type)
        Else
            Return RInternal.debug.stop($"unsure how to cast object with type '{x.GetType.FullName}' to dataframe", env)
        End If

        Return frame
    End Function

    ''' <summary>
    ''' Writes dataframe to file in either ARFF text format or SciBasic binary format.
    ''' </summary>
    ''' <param name="x">Input data object convertible to FeatureFrame (dataframe, array, etc.)</param>
    ''' <param name="file">Target file path or writable stream object.</param>
    ''' <param name="arff">Boolean flag indicating whether to use ARFF format (True) or binary format (False)</param>
    ''' <param name="env">R environment for stream operations and error reporting.</param>
    ''' <returns>True on success, error Message on failure.</returns>
    ''' <example>
    ''' require(dataframe);
    ''' 
    ''' df = data.frame(x = 1:5, b = ["aaa" "b233" "c444" "d55" "e5"]);
    ''' 
    ''' # write binary dataframe file
    ''' write.dataframe(df, file = "./test.dat");
    ''' 
    ''' # write arff dataframe text file
    ''' write.dataframe(df, file = "./test.arff", arff = TRUE);
    ''' # equals to the function
    ''' write.arff(df, file = "./test.arff");
    ''' </example>
    <ExportAPI("write.dataframe")>
    Public Function writeDataframe(<RRawVectorArgument> x As Object, file As Object,
                                   Optional arff As Boolean = False,
                                   Optional env As Environment = Nothing) As Object

        Dim auto_close As Boolean = False
        Dim s = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Write, env, is_filepath:=auto_close)
        Dim frame As FeatureFrame
        Dim pull_df As Object = anyToFeatureFrame(x, env)

        If s Like GetType(Message) Then
            Return s.TryCast(Of Message)
        ElseIf TypeOf pull_df Is Message Then
            Return pull_df
        End If

        frame = DirectCast(pull_df, FeatureFrame)

        If arff Then
            Call FeatureFrame.write_arff(frame, s)
        Else
            Call frame.WriteFrame(s)
        End If

        Call s.TryCast(Of Stream).Flush()

        If auto_close Then
            Call s.TryCast(Of Stream).Dispose()
        End If

        Return True
    End Function

    ''' <summary>
    ''' Reads dataframe from various file formats including ARFF, CSV, and SciBasic binary.
    ''' </summary>
    ''' <param name="file">Path to data file. Supported extensions: .arff, .csv, .dat</param>
    ''' <param name="mode">Data parsing mode:
    ''' - Numeric: Enforce numeric matrix conversion (CSV only)
    ''' - Character: Preserve text data (CSV only)
    ''' - Any: Auto-detect based on file content</param>
    ''' <param name="toRObj">Convert result to Rdataframe object when True (default), return raw DataSet otherwise</param>
    ''' <param name="silent">Suppress file format detection messages when True</param>
    ''' <param name="strict">Throw error instead of warning for missing files when True</param>
    ''' <param name="env">R environment for error handling and message reporting</param>
    ''' <returns>Rdataframe by default. Returns DataSet array when toRObj=False for numeric/character modes.</returns>
    ''' <example>
    ''' require(dataframe);
    ''' 
    ''' # read binary dataframe data
    ''' df = read.dataframe(file = "./data.dat");
    ''' # read csv file
    ''' df = read.dataframe(file = "./data.csv", mode = "any");
    ''' # read numeric matrix in csv file format
    ''' df = read.dataframe(file = "./data.csv", mode = "numeric");
    ''' # read arff dataframe file
    ''' # the field data type has already been defined inside the arff attributes elements
    ''' df = read.dataframe(file = "./data.arff");
    ''' </example>
    <ExportAPI("read.dataframe")>
    <RApiReturn(GetType(Rdataframe))>
    Public Function readDataSet(file$,
                                Optional mode As DataModes = DataModes.any,
                                Optional toRObj As Boolean = False,
                                Optional silent As Boolean = True,
                                Optional strict As Boolean = False,
                                Optional env As Environment = Nothing) As Object

        Dim dataframe As New Rdataframe

        If Not file.FileExists Then
            Dim msg = {$"the given data table file is not exists on your file system!", $"file: {file}"}

            If strict Then
                Return RInternal.debug.stop(msg, env)
            Else
                Call env.AddMessage(msg, MSG_TYPES.WRN)
            End If
        End If

        Select Case mode
            Case DataModes.numeric
                Dim parsed = numericLoader(file, toRObj, silent, dataframe)

                If Not toRObj Then
                    Return parsed
                End If
            Case DataModes.character
                Dim parsed = characterLoader(file, toRObj, silent, dataframe)

                If Not toRObj Then
                    Return parsed
                End If
            Case Else
                Dim is_txt As Boolean = False

                Using s As Stream = file.Open(FileMode.Open, doClear:=False, [readOnly]:=True, aggressive:=False)
                    ' check of the binary magic header
                    is_txt = s.CheckMagicNumber(FrameWriter.magic)
                End Using

                If is_txt Then
                    If file.ExtensionSuffix("arff") Then
                        dataframe = FeatureFrame _
                            .read_arff(file) _
                            .toDataframe(list.empty, env)
                    Else
                        dataframe = utils.read_csv(file)
                    End If
                Else
                    dataframe = FrameReader.ReadFrame(file).toDataframe(list.empty, env)
                End If
        End Select

        Return dataframe
    End Function

    ' =================================================================
    ' Apache Parquet file format
    '
    ' https://parquet.apache.org/
    ' =================================================================

    ''' <summary>
    ''' Reads a [Apache Parquet](https://parquet.apache.org/) data file as a R# dataframe object.
    ''' </summary>
    ''' <param name="file">
    ''' the parquet data file its file path, or a readable <see cref="Stream"/> object.
    ''' </param>
    ''' <param name="row_names">
    ''' restore the ``__index__`` column as the dataframe row names? Default is ``TRUE``.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' a dataframe object, or a <see cref="Message"/> object if the given file is 
    ''' not a valid parquet data file.
    ''' </returns>
    ''' <remarks>
    ''' #### Data type mapping
    ''' 
    ''' 1. ``INT32`` is read as integer vector and ``INT64`` is read as long integer vector.
    ''' 2. ``INT32``/``INT64`` column which contains null values will be **downgraded** 
    '''    to a numeric vector, and the missing value is represented by ``NaN``.
    ''' 3. ``FLOAT``/``DOUBLE`` is read as numeric vector, ``BOOLEAN`` is read as logical vector.
    ''' 4. ``BYTE_ARRAY`` with the ``STRING`` logical type is read as character vector.
    ''' 5. ``DATE``/``TIMESTAMP``/``INT96`` is read as date time vector.
    ''' 6. ``LIST``/``MAP`` and the nested ``struct`` column is read as a json text vector.
    ''' </remarks>
    ''' <example>
    ''' require(dataframe);
    ''' 
    ''' # read a parquet data file which is generated by pyarrow/pandas
    ''' df = read.parquet("./data.parquet");
    ''' 
    ''' # do not restore the row names from the ``__index__`` column
    ''' df = read.parquet("./data.parquet", row_names = FALSE);
    ''' </example>
    <ExportAPI("read.parquet")>
    <RApiReturn(GetType(Rdataframe))>
    Public Function readParquet(<RRawVectorArgument> file As Object,
                                Optional row_names As Boolean = True,
                                Optional env As Environment = Nothing) As Object

        Dim auto_close As Boolean = False
        Dim s = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env, is_filepath:=auto_close)

        If s Like GetType(Message) Then
            Return s.TryCast(Of Message)
        End If

        Dim stream As Stream = s.TryCast(Of Stream)
        Dim table As ParquetStore.ParquetTable

        Try
            table = ParquetStore.ParquetReader.ReadTable(stream)
        Catch ex As Exception
            Return RInternal.debug.stop(ex, env)
        Finally
            If auto_close Then
                Call closeStream(stream)
            End If
        End Try

        If table Is Nothing OrElse table.Schema Is Nothing Then
            Return New Rdataframe With {.columns = New Dictionary(Of String, Array)}
        End If

        ' the row names data is only available when the parquet file contains 
        ' the ``__index__`` column, which is generated by the ``write.parquet`` function.
        Dim hasIndexColumn As Boolean = table.Schema _
            .FindLeaf(ParquetStore.ParquetConstants.IndexColumnName) IsNot Nothing

        Dim model As FeatureFrame = ParquetStore.DataFrameMapping.ToDataFrame(
            table.Schema,
            table.Columns,
            CInt(table.NumRows),
            restoreRowNames:=row_names
        )
        Dim df As New Rdataframe With {
            .columns = New Dictionary(Of String, Array)
        }

        For Each col As FeatureVector In model.features.Values
            If col Is Nothing OrElse col.vector Is Nothing Then
                Continue For
            End If

            df.columns(col.name) = col.vector
        Next

        If row_names AndAlso hasIndexColumn Then
            df.rownames = model.rownames
        End If

        Return df
    End Function

    ''' <summary>
    ''' Writes a dataframe object to a [Apache Parquet](https://parquet.apache.org/) data file.
    ''' </summary>
    ''' <param name="x">
    ''' the dataframe object, or any other object which can be converted to a dataframe 
    ''' object, such as a clr object vector or a ``FeatureFrame`` object.
    ''' </param>
    ''' <param name="file">
    ''' the target parquet data file its file path, or a writable <see cref="Stream"/> object.
    ''' If this parameter is not given, then the raw parquet binary bytes will be returned.
    ''' </param>
    ''' <param name="compression">
    ''' the page compression codec: ``none``/``snappy``/``gzip``, default is ``snappy``.
    ''' </param>
    ''' <param name="row_group_size">
    ''' the max number of data rows in a single row group, default is ``1000000`` rows. 
    ''' The parquet writer creates only one single data page for each column in a row group, 
    ''' so you may needs to decrease this value if you are writing a very large table.
    ''' </param>
    ''' <param name="row_names">
    ''' writes the dataframe row names as the ``__index__`` column? Default is ``TRUE``.
    ''' </param>
    ''' <param name="statistics">
    ''' writes the ``min``/``max``/``null_count`` statistics data of each column? Default is ``TRUE``.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' ``TRUE`` if the data file is write success, or a raw <see cref="Byte"/> vector when the 
    ''' <paramref name="file"/> parameter is not given, or a <see cref="Message"/> object 
    ''' if the given object could not be cast to a dataframe object.
    ''' </returns>
    ''' <remarks>
    ''' #### Data type mapping
    ''' 
    ''' 1. integer/short/byte is mapped to ``INT32``, long is mapped to ``INT64``.
    ''' 2. single is mapped to ``FLOAT`` and double is mapped to ``DOUBLE``.
    ''' 3. logical is mapped to ``BOOLEAN``.
    ''' 4. character is mapped to ``BYTE_ARRAY`` with the ``STRING`` logical type.
    ''' 5. date time is mapped to ``INT64`` with the ``TIMESTAMP_MILLIS`` logical type.
    ''' 6. a column which contains values with different clr types will be cast to 
    '''    a character vector automatically.
    ''' 
    ''' #### Missing value
    ''' 
    ''' both of the ``NULL`` character value and the ``NaN``/``NA`` numeric value are written 
    ''' as the parquet **null** value (the definition level is lower than the max definition level), 
    ''' so ``is.na()`` still returns ``TRUE`` after reading the data file back.
    ''' </remarks>
    ''' <example>
    ''' require(dataframe);
    ''' 
    ''' df = data.frame(
    '''     x = 1:5, 
    '''     labels = ["A","B","C","D","E"], 
    '''     score = [0.1, 0.2, NA, 0.4, 0.5]
    ''' );
    ''' 
    ''' # write the dataframe object as a parquet data file
    ''' write.parquet(df, file = "./data.parquet");
    ''' 
    ''' # write with the gzip compression and without the row names index column
    ''' write.parquet(df, file = "./data.parquet", compression = "gzip", row_names = FALSE);
    ''' 
    ''' # write to a raw byte vector
    ''' let bytes = write.parquet(df);
    ''' </example>
    <ExportAPI("write.parquet")>
    <RApiReturn(GetType(Boolean), GetType(Byte()))>
    Public Function writeParquet(<RRawVectorArgument> x As Object,
                                 Optional file As Object = Nothing,
                                 Optional compression As String = "snappy",
                                 Optional row_group_size As Integer = 1000000,
                                 Optional row_names As Boolean = True,
                                 Optional statistics As Boolean = True,
                                 Optional env As Environment = Nothing) As Object

        Dim auto_close As Boolean = False
        Dim buffer As MemoryStream = Nothing
        Dim stream As Stream

        If file Is Nothing Then
            ' 没有给定目标文件时，返回 parquet 格式的原始字节数据
            buffer = New MemoryStream()
            stream = buffer
        Else
            Dim s = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Write, env, is_filepath:=auto_close)

            If s Like GetType(Message) Then
                Return s.TryCast(Of Message)
            End If

            stream = s.TryCast(Of Stream)
        End If

        Dim model As Object = toParquetFrame(x, env)

        If TypeOf model Is Message Then
            If auto_close Then
                Call closeStream(stream)
            End If

            Return model
        End If

        Dim options As New ParquetStore.ParquetOptions With {
            .Compression = parseCompression(compression, env),
            .RowGroupSize = If(row_group_size > 0, row_group_size, 1000000),
            .WriteIndexColumn = row_names,
            .WriteStatistics = statistics
        }

        Try
            Call ParquetStore.pq.write_table(DirectCast(model, FeatureFrame), stream, options)
            Call stream.Flush()
        Catch ex As Exception
            If auto_close Then
                Call closeStream(stream)
            End If

            Return RInternal.debug.stop(ex, env)
        End Try

        If buffer IsNot Nothing Then
            Return buffer.ToArray()
        End If

        If auto_close Then
            Call closeStream(stream)
        End If

        Return True
    End Function

    Private Sub closeStream(stream As Stream)
        If stream Is Nothing Then
            Return
        End If

        Try
            Call stream.Flush()
        Catch ex As Exception
            ' ignore the flush error on the read only stream
        End Try

        Try
            Call stream.Close()
            Call stream.Dispose()
        Catch ex As Exception
            ' ignore
        End Try
    End Sub

    ''' <summary>
    ''' parse the compression codec name that used for the parquet data page.
    ''' </summary>
    Private Function parseCompression(name As String, env As Environment) As ParquetStore.Format.CompressionCodec
        Select Case LCase(If(name, "").Trim)
            Case "", "snappy"
                Return ParquetStore.Format.CompressionCodec.SNAPPY
            Case "none", "raw", "uncompressed"
                Return ParquetStore.Format.CompressionCodec.UNCOMPRESSED
            Case "gzip", "gz"
                Return ParquetStore.Format.CompressionCodec.GZIP
            Case Else
                Call env.AddMessage(
                    $"the given compression codec '{name}' is not supported by the parquet writer, the default codec snappy will be used.",
                    MSG_TYPES.WRN
                )

                Return ParquetStore.Format.CompressionCodec.SNAPPY
        End Select
    End Function

    ''' <summary>
    ''' cast the given R# object to the ``FeatureFrame`` model which is required 
    ''' by the parquet data writer.
    ''' </summary>
    ''' <returns>
    ''' a <see cref="FeatureFrame"/> object for the parquet data writer, or a 
    ''' <see cref="Message"/> object if the given object could not be cast.
    ''' </returns>
    Private Function toParquetFrame(x As Object, env As Environment) As Object
        If x Is Nothing Then
            Return RInternal.debug.stop("the dataframe object for writing parquet data file could not be nothing!", env)
        End If

        If TypeOf x Is FeatureFrame Then
            Return DirectCast(x, FeatureFrame)
        ElseIf TypeOf x Is Rdataframe Then
            Return rDataframeToFeatureFrame(DirectCast(x, Rdataframe), env)
        Else
            ' try to cast the clr object vector to a dataframe object
            Return anyToFeatureFrame(x, env)
        End If
    End Function

    ''' <summary>
    ''' convert the R# dataframe object to a <see cref="FeatureFrame"/> object.
    ''' </summary>
    Private Function rDataframeToFeatureFrame(x As Rdataframe, env As Environment) As Object
        Dim features As New Dictionary(Of String, FeatureVector)

        For Each name As String In x.colnames
            Dim raw As Array = x.columns(name)
            Dim col As Array = x.getVector(name, fullSize:=True)
            Dim feature As Object = toFeatureVector(name, col, raw, env)

            If TypeOf feature Is Message Then
                Return feature
            End If

            features(name) = DirectCast(feature, FeatureVector)
        Next

        Return New FeatureFrame With {
            .features = features,
            .rownames = x.getRowNames
        }
    End Function

    ''' <summary>
    ''' cast a R# column vector to a <see cref="FeatureVector"/> object.
    ''' </summary>
    ''' <param name="name">the column name</param>
    ''' <param name="col">the column data in full size</param>
    ''' <param name="raw">
    ''' the original column data that reads from the ``dataframe.columns`` table, this 
    ''' parameter is used for probing the column element type of a empty column.
    ''' </param>
    ''' <param name="env"></param>
    Private Function toFeatureVector(name As String, col As Array, raw As Array, env As Environment) As Object
        If col Is Nothing OrElse raw Is Nothing Then
            Return New FeatureVector(name, New String() {})
        End If

        Dim elementType As Type = If(col.Length > 0, col.GetType.GetElementType, raw.GetType.GetElementType)

        Select Case elementType
            Case GetType(String)
                Return New FeatureVector(name, normalizeStrings(col))
            Case GetType(Integer), GetType(Short), GetType(Long), GetType(Single), GetType(Double), GetType(Boolean)
                If col.Length = 0 Then
                    Return FeatureVector.FromGeneral(name, Array.CreateInstance(elementType, 0))
                Else
                    Return FeatureVector.FromGeneral(name, col)
                End If
            Case GetType(Date)
                Return New FeatureVector(name, toDates(col))
            Case GetType(TimeSpan)
                Return New FeatureVector(name, toTimeSpans(col))
            Case GetType(Byte)
                Return New FeatureVector(name, DirectCast(col, Byte()).Select(Function(b) CInt(b)))
            Case Else
                Return castObjectColumn(name, col, env)
        End Select
    End Function

    ''' <summary>
    ''' cast the mixed clr type column data to a typed column.
    ''' </summary>
    ''' <remarks>
    ''' 1. all of the non-missing value is numeric: a numeric vector with ``NaN`` as missing value.
    ''' 2. all of the non-missing value is date time and no missing value: a date time vector.
    ''' 3. otherwise: a character vector, and the missing value is ``NULL``.
    ''' </remarks>
    Private Function castObjectColumn(name As String, col As Array, env As Environment) As Object
        Dim values As Object() = col.AsObjectEnumerator.ToArray
        Dim isNumeric As Boolean = True
        Dim isDate As Boolean = True
        Dim hasNa As Boolean = False

        For Each value As Object In values
            If isNaValue(value) Then
                hasNa = True
                Continue For
            End If

            If Not isNumericValue(value) Then
                isNumeric = False
            End If
            If Not TypeOf value Is Date Then
                isDate = False
            End If
        Next

        If isNumeric Then
            Dim data As Double() = New Double(values.Length - 1) {}

            For i As Integer = 0 To values.Length - 1
                If isNaValue(values(i)) Then
                    data(i) = Double.NaN
                Else
                    data(i) = CDbl(values(i))
                End If
            Next

            Return New FeatureVector(name, data)
        ElseIf isDate AndAlso Not hasNa Then
            Dim data As Date() = New Date(values.Length - 1) {}

            For i As Integer = 0 To values.Length - 1
                data(i) = CDate(values(i))
            Next

            Return New FeatureVector(name, data)
        Else
            If isDate Then
                Call env.AddMessage(
                    $"the date time column '{name}' contains missing value, it will be cast to a character vector.",
                    MSG_TYPES.WRN
                )
            End If

            Return New FeatureVector(name, normalizeStrings(col))
        End If
    End Function

    ''' <summary>
    ''' cast the column data to a character vector, and all of the missing value 
    ''' are cast to ``NULL`` value.
    ''' </summary>
    Private Function normalizeStrings(col As Array) As String()
        Dim values As Object() = col.AsObjectEnumerator.ToArray
        Dim data As String() = New String(values.Length - 1) {}

        For i As Integer = 0 To values.Length - 1
            If isNaValue(values(i)) Then
                data(i) = Nothing
            Else
                data(i) = any.ToString(values(i))
            End If
        Next

        Return data
    End Function

    Private Function toDates(col As Array) As Date()
        If col.GetType Is GetType(Date()) Then
            Return DirectCast(col, Date())
        Else
            Return col.AsObjectEnumerator.Select(Function(v) CDate(v)).ToArray
        End If
    End Function

    Private Function toTimeSpans(col As Array) As TimeSpan()
        If col.GetType Is GetType(TimeSpan()) Then
            Return DirectCast(col, TimeSpan())
        Else
            Return col.AsObjectEnumerator.Select(Function(v) CType(v, TimeSpan)).ToArray
        End If
    End Function

    ''' <summary>
    ''' is the given value a missing value in the R# language?
    ''' </summary>
    Private Function isNaValue(value As Object) As Boolean
        If value Is Nothing Then
            Return True
        ElseIf value Is GetType(Void) Then
            ' the R# NA literal value
            Return True
        ElseIf TypeOf value Is Double AndAlso Double.IsNaN(DirectCast(value, Double)) Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Function isNumericValue(value As Object) As Boolean
        Select Case value.GetType
            Case GetType(Integer), GetType(Short), GetType(Long),
                 GetType(Single), GetType(Double), GetType(Byte), GetType(Decimal)
                Return True
            Case Else
                Return False
        End Select
    End Function

    ' Private helper methods with internal documentation
    ''' <summary>
    ''' (Internal) Loads character data from file and converts to Rdataframe structure
    ''' </summary>
    Private Function characterLoader(file As String, toRObj As Boolean, silent As Boolean, ByRef dataframe As Rdataframe) As EntityObject()
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

        Return Nothing
    End Function

    ''' <summary>
    ''' (Internal) Loads numeric data from file and converts to Rdataframe structure
    ''' </summary>
    Private Function numericLoader(file As String, toRObj As Boolean, silent As Boolean, ByRef dataframe As Rdataframe) As DataSet()
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

        Return Nothing
    End Function

    ''' <summary>
    ''' create a new entity row object
    ''' </summary>
    ''' <param name="id"></param>
    ''' <param name="properties"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
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
