#Region "Microsoft.VisualBasic::3182ba7b93070c6c9afcda0562933fd9, Library\base\utils\dataframe.vb"

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

    '   Total Lines: 1040
    '    Code Lines: 705 (67.79%)
    ' Comment Lines: 203 (19.52%)
    '    - Xml Docs: 90.64%
    ' 
    '   Blank Lines: 132 (12.69%)
    '     File Size: 41.04 KB


    ' Module dataframe
    ' 
    '     Function: anyToFeatureFrame, appendCells, appendRow, AsDataframeRaw, asIndexList
    '               cells, characterLoader, colnames, column, create_tableFrame
    '               createEntityRow, CreateRowObject, dataframeTable, deserialize, field_vector
    '               loadDataframe, measureColumnVector, numericLoader, openCsv, parseArffText
    '               parseDataframe, parseRow, printRowVector, printTable, project
    '               rawToDataFrame, readCsvRaw, readDataSet, rows, rowToString
    '               RowToString, stripCommentRows, to_dataframe, transpose, vector
    '               write_arff, writeDataframe
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
Imports Idataframe = Microsoft.VisualBasic.Data.Framework.IO.DataFrameResolver
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal
Imports RPrinter = SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports Rsharp = SMRUCC.Rsharp
Imports vec = SMRUCC.Rsharp.Runtime.Internal.Object.vector

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
                    dataframe = FrameReader.ReadFeatures(file).toDataframe(list.empty, env)
                End If
        End Select

        Return dataframe
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
