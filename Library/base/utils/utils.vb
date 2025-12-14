#Region "Microsoft.VisualBasic::faeaa818b2f03f2beb75bc57a3a270b5, Library\base\utils\utils.vb"

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

    '   Total Lines: 655
    '    Code Lines: 444 (67.79%)
    ' Comment Lines: 141 (21.53%)
    '    - Xml Docs: 87.23%
    ' 
    '   Blank Lines: 70 (10.69%)
    '     File Size: 27.76 KB


    ' Module utils
    ' 
    '     Function: ensureRowNames, loadCsv, MeasureGenericType, parseRData, printRawTable
    '               read_csv, read_feather, saveGeneric, saveTextFile, setRowNames
    '               write_csv, write_feather
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Data.Framework.DATA
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Data.Framework.IO.CSVFile
Imports Microsoft.VisualBasic.Data.Framework.StorageProvider.Reflection
Imports Microsoft.VisualBasic.DataStorage
Imports Microsoft.VisualBasic.DataStorage.FeatherFormat
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Model
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.RDataSet
Imports SMRUCC.Rsharp.RDataSet.Convertor
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Serialize
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports any = Microsoft.VisualBasic.Scripting
Imports csv = Microsoft.VisualBasic.Data.Framework.IO.File
Imports csvData = Microsoft.VisualBasic.Data.Framework.IO.DataFrameResolver
Imports file = Microsoft.VisualBasic.Data.Framework.IO.File
Imports fileStream = System.IO.Stream
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal
Imports RPrinter = SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports Rsharp = SMRUCC.Rsharp
Imports textStream = System.IO.StreamReader

''' <summary>
''' The R Utils Package 
''' </summary>
<Package("utils", Category:=APICategories.UtilityTools)>
<RTypeExport("entity", GetType(EntityObject))>
<RTypeExport("dataset", GetType(DataSet))>
Public Module utils

    Friend Sub Main()
        Call RPrinter.AttachConsoleFormatter(Of csv)(AddressOf printRawTable)
    End Sub

    ''' <summary>
    ''' read ``*.rda`` data file which is saved from R environment. 
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    <ExportAPI("readRData")>
    Public Function parseRData(<RRawVectorArgument> file As Object, Optional env As Environment = Nothing) As Object
        Dim is_filepath As Boolean = False
        Dim s = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env, is_filepath:=is_filepath)

        If s Like GetType(Message) Then
            Return s.TryCast(Of Message)
        End If

        Using buffer As Stream = s.TryCast(Of Stream)
            Dim obj As Struct.RData = Reader.ParseData(buffer)
            Dim symbols As list = ConvertToR.ToRObject(obj.object)

            Return symbols
        End Using
    End Function

    Private Function printRawTable(raw As csv) As String
        Dim matrix As New List(Of String())
        Dim i As i32 = 1

        Call {""} _
            .JoinIterates(SheetTable.GetColumnIndex.Take(raw.Width)) _
            .ToArray _
            .DoCall(AddressOf matrix.Add)

        For Each row As RowObject In raw.Rows
            Call {(++i).ToString} _
                .JoinIterates(row.AsEnumerable) _
                .ToArray _
                .DoCall(AddressOf matrix.Add)
        Next

        Return matrix.Print(False)
    End Function

    ''' <summary>
    ''' load csv as a .NET CLR object vector
    ''' </summary>
    ''' <param name="file"></param>
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
    <ExportAPI("load.csv")>
    Public Function loadCsv(<RRawVectorArgument> file As Object, type As Object,
                            Optional encoding As Object = "unknown",
                            Optional tsv As Boolean = False,
                            Optional env As Environment = Nothing) As Object

        Dim textEncoding As Encoding = Rsharp.GetEncoding(encoding)
        Dim typeinfo As Type = env.globalEnvironment.GetType([typeof]:=type)
        Dim buffer = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env)

        If buffer Like GetType(Message) Then
            Return buffer.TryCast(Of Message)
        End If

        ' read csv data
        Dim reader As csvData = csvData.Load(
            stream:=buffer.TryCast(Of Stream),
            encoding:=textEncoding,
            isTsv:=tsv
        )
        Dim seq As Array = reader _
            .LoadDataToObject(typeinfo, strict:=False, metaBlank:="", silent:=True) _
            .ToArray
        Dim generic As Array = REnv.TryCastGenericArray(seq, env)

        Return generic
    End Function

    ''' <summary>
    ''' read the feather as dataframe
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("read.feather")>
    Public Function read_feather(<RRawVectorArgument> file As Object, Optional env As Environment = Nothing) As Object
        Dim auto_close As Boolean = False
        Dim s = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env, is_filepath:=auto_close)

        If s Like GetType(Message) Then
            Return s.TryCast(Of Message)
        End If

        Dim df As New Rdataframe With {.columns = New Dictionary(Of String, Array)}

        Using untyped = FeatherReader.ReadFromStream(s.TryCast(Of Stream), BasisType.One)
            Dim data As Array
            Dim type As Type
            Dim value As FeatherFormat.Value

            For Each col As Column In untyped.AllColumns.AsEnumerable
                type = RType.GetUnderlyingType(col.Type)
                data = Array.CreateInstance(type, col.Length)

                For i As Integer = 1 To col.Length
                    value = col(i)
                    data.SetValue(value.TryCast(type), i - 1)
                Next

                If col.Name = "row.names" AndAlso type Is GetType(String) Then
                    df.rownames = data
                Else
                    df.add(col.Name, data)
                End If
            Next
        End Using

        If auto_close Then
            Try
                Call s.TryCast(Of Stream).Close()
                Call s.TryCast(Of Stream).Dispose()
            Catch ex As Exception

            End Try
        End If

        Return df
    End Function

    <ExportAPI("write.feather")>
    Public Function write_feather(x As Rdataframe, file As Object,
                                  Optional row_names As Boolean = True,
                                  Optional env As Environment = Nothing) As Object

        Dim auto_close As Boolean = False
        Dim s = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Write, env, is_filepath:=auto_close)
        Dim v As Array

        If s Like GetType(Message) Then
            Return s.TryCast(Of Message)
        ElseIf x Is Nothing Then
            Return RInternal.debug.stop("the required dataframe object should not be nothing!", env)
        End If

        Using writer As New FeatherWriter(s.TryCast(Of Stream), WriteMode.Eager)
            If row_names Then
                Call writer.AddColumn("row.names", x.getRowNames)
            End If

            For Each name As String In x.colnames
                v = x.getVector(name, fullSize:=False)
                v = REnv.UnsafeTryCastGenericArray(v)
                writer.AddColumn(name, v)
            Next
        End Using

        If auto_close Then
            Try
                Call s.TryCast(Of Stream).Flush()
                Call s.TryCast(Of Stream).Close()
                Call s.TryCast(Of Stream).Dispose()
            Catch ex As Exception

            End Try
        End If

        Return True
    End Function

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
    ''' 
    ''' use <see cref="Encoding.Default"/> by default.
    ''' </param>
    ''' <param name="row_names">
    ''' a vector of row names. This can be a vector giving the actual row names, 
    ''' or a single number giving the column of the table which contains the 
    ''' row names, or character string giving the name of the table column 
    ''' containing the row names.
    '''
    ''' If there Is a header And the first row contains one fewer field than the 
    ''' number Of columns, the first column In the input Is used For the row 
    ''' names. Otherwise If row.names Is missing, the rows are numbered.
    '''
    ''' Using row.names = NULL forces row numbering. Missing Or NULL row.names 
    ''' generate row names that are considered To be 'automatic’ (and not 
    ''' preserved by as.matrix).
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("read.csv")>
    <RApiReturn(GetType(Rdataframe))>
    Public Function read_csv(file As Object,
                             Optional header As Boolean = True,
                             <RRawVectorArgument>
                             Optional row_names As Object = Nothing,
                             Optional check_names As Boolean = True,
                             Optional check_modes As Boolean = True,
                             Optional encoding As Object = "unknown",
                             Optional comment_char As String = "#",
                             Optional tsv As Boolean = False,
                             Optional skip_rows As Integer = -1,
                             Optional env As Environment = Nothing) As Object

        Dim datafile As Object
        Dim textEncoding As Encoding = Rsharp.GetEncoding(encoding)

        If file Is Nothing Then
            If env.strictOption Then
                Return RInternal.debug.stop("the required dataframe file source should not be nothing!", env)
            Else
                Call "the required dataframe file source is nothing, null value will be returns as the dataframe result value.".warning
            End If

            Return Nothing
        End If

        If TypeOf file Is String Then
            datafile = REnv _
                .TryCatch(runScript:=Function()
                                         If tsv Then
                                             Return IO.File.LoadTsv(file, encoding:=textEncoding)
                                         Else
                                             Return IO.File.Load(file, encoding:=textEncoding, mute:=Not env.verboseOption)
                                         End If
                                     End Function,
                          debug:=env.globalEnvironment.debugMode
                )
        ElseIf TypeOf file Is fileStream Then
            Using reader As New textStream(DirectCast(file, fileStream), textEncoding)
                datafile = reader.ReadToEnd _
                    .LineTokens _
                    .DoCall(Function(lines) FileLoader.Load(lines, False, Nothing, isTsv:=tsv)) _
                    .DoCall(Function(ls)
                                Return New file(ls)
                            End Function)
            End Using
        ElseIf TypeOf file Is textBuffer Then
            datafile = DirectCast(file, textBuffer).text _
                .LineTokens _
                .DoCall(Function(lines) FileLoader.Load(lines, False, Nothing, isTsv:=tsv)) _
                .DoCall(Function(ls)
                            Return New file(ls)
                        End Function)
        Else
            Return RInternal.debug.stop({
                "invalid file clr object content type!",
                "clr_type: " & file.GetType.FullName
            }, env)
        End If

        If Not TypeOf datafile Is file Then
            Return RInternal.debug.stop(datafile, env)
        Else
            Return DirectCast(datafile, csv).rawToDataFrame(
                row_names:=row_names,
                check_names:=check_names,
                check_modes:=check_modes,
                comment_char:=comment_char,
                skip_rows:=skip_rows,
                env:=env,
                header:=header
            )
        End If
    End Function

    Const NA As Object = Nothing

    ''' <summary>
    ''' ## Data Input
    ''' 
    ''' Reads a file in table format and creates a data frame from it, with cases corresponding to lines and variables to fields in the file.
    ''' </summary>
    ''' <param name="file">the name Of the file which the data are To be read from. Each row Of the table appears As one line Of the file.
    ''' If it does Not contain an absolute path, the file name Is relative To the current working directory, getwd(). Tilde-expansion Is 
    ''' performed where supported. This can be a compressed file (see file).
    ''' Alternatively, file can be a readable text-mode connection (which will be opened for reading if necessary, And if so closed 
    ''' (And hence destroyed) at the end of the function call). (If stdin() Is used, the prompts for lines may be somewhat confusing. 
    ''' Terminate input with a blank line Or an EOF signal, Ctrl-D on Unix And Ctrl-Z on Windows. Any pushback on stdin() will be cleared 
    ''' before return.)
    ''' file can also be a complete URL. (For the supported URL schemes, see the 'URLs’ section of the help for url.)</param>
    ''' <param name="header">a logical value indicating whether the file contains the names Of the variables As its first line. If missing,
    ''' the value Is determined from the file format: header Is set to TRUE if And only if the first row contains one fewer field than the 
    ''' number of columns.</param>
    ''' <param name="sep">the field separator character. Values On Each line Of the file are separated by this character. If sep = "" 
    ''' (the Default For read.table) the separator Is 'white space’, that is one or more spaces, tabs, newlines or carriage returns.</param>
    ''' <param name="quote">the set of quoting characters. To disable quoting altogether, use quote = "". See scan for the behaviour on quotes 
    ''' embedded in quotes. Quoting Is only considered for columns read as character, which Is all of them unless colClasses Is specified.</param>
    ''' <param name="dec">the character used In the file For Decimal points.</param>
    ''' <param name="numerals">string indicating how To convert numbers whose conversion To Double precision would lose accuracy, see type.convert.
    ''' Can be abbreviated. (Applies also To complex-number inputs.)</param>
    ''' <param name="row_names">a vector of row names. This can be a vector giving the actual row names, or a single number giving the column 
    ''' of the table which contains the row names, or character string giving the name of the table column containing the row names.
    ''' If there Is a header And the first row contains one fewer field than the number Of columns, the first column In the input Is used 
    ''' For the row names. Otherwise If row.names Is missing, the rows are numbered.
    ''' using row.names = NULL forces row numbering. Missing Or NULL row.names generate row names that are considered To be 'automatic’ 
    ''' (and not preserved by as.matrix).</param>
    ''' <param name="col_names">a vector Of Optional names For the variables. The Default Is To use "V" followed by the column number.</param>
    ''' <param name="as_is">controls conversion Of character variables (insofar As they are Not converted To logical, numeric Or complex) 
    ''' To factors, If Not otherwise specified by colClasses. Its value Is either a vector Of logicals (values are recycled If necessary), 
    ''' Or a vector Of numeric Or character indices which specify which columns should Not be converted To factors.
    ''' Note: to suppress all conversions including those of numeric columns, set colClasses = "character".
    ''' Note that As.Is Is specified per column (Not per variable) And so includes the column of row names (if any) And any columns to be skipped.</param>
    ''' <param name="tryLogical">a logical determining If columns consisting entirely Of "F", "T", "FALSE", And "TRUE" should be converted To logical; 
    ''' passed To type.convert, True by Default.</param>
    ''' <param name="na_strings">a character vector of strings which are to be interpreted as NA values. Blank fields are also considered 
    ''' to be missing values in logical, integer, numeric and complex fields. Note that the test happens after white space is stripped from 
    ''' the input (if enabled), so na.strings values may need their own white space stripped in advance.</param>
    ''' <param name="colClasses">character. A vector of classes to be assumed for the columns. If unnamed, recycled as necessary. 
    ''' If named, names are matched with unspecified values being taken to be NA.
    ''' Possible values are NA (the Default, When type.convert Is used), "NULL" (When the column Is skipped), one Of the atomic vector 
    ''' classes (logical, Integer, numeric, complex, character, raw), Or "factor", "Date" Or "POSIXct". Otherwise there needs To be an As
    ''' method (from package methods) For conversion from "character" To the specified formal Class.
    ''' Note that colClasses Is specified per column (Not per variable) And so includes the column Of row names (If any).</param>
    ''' <param name="nrows">Integer: the maximum number Of rows To read In. Negative And other invalid values are ignored.</param>
    ''' <param name="skip">integer: the number of lines of the data file to skip before beginning to read data.</param>
    ''' <param name="check_names">logical. If TRUE then the names of the variables in the data frame are checked to ensure that they are 
    ''' syntactically valid variable names. If necessary they are adjusted (by make.names) so that they are, And also to ensure that 
    ''' there are no duplicates.</param>
    ''' <param name="fill">logical. If TRUE then in case the rows have unequal length, blank fields are implicitly added. See ‘Details’.</param>
    ''' <param name="strip_white">logical. Used only when sep has been specified, And allows the stripping of leading And trailing white 
    ''' space from unquoted character fields (numeric fields are always stripped). See scan for further details (including the exact 
    ''' meaning of 'white space’), remembering that the columns may include the row names.</param>
    ''' <param name="blank_lines_skip">logical: if TRUE blank lines in the input are ignored.</param>
    ''' <param name="comment_char">character: a character vector of length one containing a single character or an empty string. 
    ''' Use "" to turn off the interpretation of comments altogether.</param>
    ''' <param name="allowEscapes">logical. Should C-style escapes such as '⁠\n⁠’ be processed or read verbatim (the default)? 
    ''' Note that if not within quotes these could be interpreted as a delimiter (but not as a comment character). 
    ''' For more details see scan.</param>
    ''' <param name="flush">logical: If True, scan will flush To the End Of the line after reading the last Of the fields requested. 
    ''' This allows putting comments after the last field.</param>
    ''' <param name="stringsAsFactors">logical: should character vectors be converted to factors? Note that this is overridden 
    ''' by as.is and colClasses, both of which allow finer control.</param>
    ''' <param name="fileEncoding">character string: if non-empty declares the encoding used on a file when given as a character 
    ''' string (not on an existing connection) so the character data can be re-encoded. See the ‘Encoding’ section of the help for file,
    ''' the ‘R Data Import/Export’ manual and ‘Note’.</param>
    ''' <param name="encoding">encoding to be assumed for input strings. It Is used to mark character strings as known to be in 
    ''' Latin-1 Or UTF-8 (see Encoding) it Is Not used to re-encode the input, but allows R to handle encoded strings in their 
    ''' native encoding (if one of those two). See 'Value’ and ‘Note’.</param>
    ''' <param name="text">character string: if file is not supplied and this is, then data are read from the value of text via 
    ''' a text connection. Notice that a literal string can be used to include (small) data sets within R code.</param>
    ''' <param name="skipNul">logical: should NULs be skipped?</param>
    ''' <param name="env"></param>
    ''' <returns>A data frame (data.frame) containing a representation of the data in the file.
    ''' Empty input Is an Error unless col.names Is specified, When a 0-row data frame Is returned: similarly giving just a header line 
    ''' If header = True results In a 0-row data frame. Note that In either Case the columns will be logical unless colClasses was supplied.
    ''' Character strings In the result (including factor levels) will have a declared encoding If encoding Is "latin1" Or "UTF-8".</returns>
    ''' <remarks>
    ''' This function is the principal means of reading tabular data into R.
    ''' Unless colClasses Is specified, all columns are read As character columns And Then converted Using type.convert To logical, 
    ''' Integer, numeric, complex Or (depending On As.Is) factor As appropriate. Quotes are (by Default) interpreted In all fields,
    ''' so a column Of values Like "42" will result In an Integer column.
    ''' A field Or line Is 'blank’ if it contains nothing (except whitespace if no separator is specified) before a comment character 
    ''' or the end of the field or line.
    ''' If row.names Is Not specified And the header line has one less entry than the number Of columns, the first column Is taken
    ''' To be the row names. This allows data frames To be read In from the format In which they are printed. If row.names Is specified
    ''' And does Not refer To the first column, that column Is discarded from such files.
    ''' The number Of data columns Is determined by looking at the first five lines Of input (Or the whole input If it has less than five lines), 
    ''' Or from the length Of col.names If it Is specified And Is longer. This could conceivably be wrong If fill Or blank.lines.skip are True,
    ''' so specify col.names If necessary (As In the 'Examples’).
    ''' read.csv And read.csv2 are identical to read.table except for the defaults. They are intended for reading 'comma separated value’ files 
    ''' (‘.csv’) or (read.csv2) the variant used in countries that use a comma as decimal point and a semicolon as field separator. Similarly,
    ''' read.delim and read.delim2 are for reading delimited files, defaulting to the TAB character for the delimiter. Notice that 
    ''' header = TRUE and fill = TRUE in these variants, and that the comment character is disabled.
    ''' The rest Of the line after a comment character Is skipped; quotes are Not processed In comments. Complete comment lines are allowed 
    ''' provided blank.lines.skip = True; however, comment lines prior To the header must have the comment character In the first non-blank 
    ''' column.
    ''' Quoted fields With embedded newlines are supported except after a comment character. Embedded NULs are unsupported: skipping 
    ''' them(with skipNul = TRUE) may work.
    ''' 
    ''' The columns referred to in as.is and colClasses include the column of row names (if any).
    ''' There are two approaches For reading input that Is Not In the local encoding. If the input Is known To be UTF-8 Or Latin1, 
    ''' use the encoding argument To Declare that. If the input Is In some other encoding, Then it may be translated On input. 
    ''' The fileEncoding argument achieves this by setting up a connection To Do the re-encoding into the current locale. 
    ''' Note that On Windows Or other systems Not running In a UTF-8 locale, this may Not be possible.
    ''' </remarks>
    <ExportAPI("read.table")>
    Public Function read_table(file As Object, Optional header As Boolean = False, Optional sep As String = "", Optional quote As String = """'",
       Optional dec As String = ".", <RRawVectorArgument(TypeCodes.string)> Optional numerals As Object = "allow.loss|warn.loss|no.loss",
        <RRawVectorArgument> Optional row_names As Object = Nothing, <RRawVectorArgument> Optional col_names As Object = Nothing, <RLazyExpression> Optional as_is As Object = "!stringsAsFactors", Optional tryLogical As Boolean = True,
          Optional na_strings As String = "NA", Optional colClasses As Object = NA, Optional nrows As Integer = -1,
           Optional skip As Integer = 0, Optional check_names As Boolean = True, <RLazyExpression> Optional fill As Object = "!blank.lines.skip",
          Optional strip_white As Boolean = False, Optional blank_lines_skip As Boolean = True,
          Optional comment_char As Char = "#"c,
        Optional allowEscapes As Boolean = False, Optional flush As Boolean = False,
          Optional stringsAsFactors As Boolean = False,
          Optional fileEncoding As String = "", Optional encoding As String = "unknown", Optional text As Object = Nothing, Optional skipNul As Boolean = False, Optional env As Environment = Nothing) As Object

    End Function

    Public Function ensureRowNames(row_names As Object, env As Environment) As Object
        Dim indexType As RType = RType.TypeOf(row_names)

        If Not TypeOf row_names Is vector Then
            If indexType.mode = TypeCodes.string OrElse indexType.mode = TypeCodes.double Then
                row_names = New vector(GetType(String), CLRVector.asCharacter(row_names), env)
            ElseIf indexType.mode = TypeCodes.integer Then
                row_names = New vector(GetType(Integer), CLRVector.asInteger(row_names), env)
            Else
                Return RInternal.debug.stop({
                    "invalid data type for set row names!",
                    "given: " & RType.TypeOf(row_names).ToString
                }, env)
            End If
        End If

        Return row_names
    End Function

    <Extension>
    Friend Function setRowNames(dataframe As Rdataframe, row_names As vector, env As Environment) As Message
        Select Case row_names.elementType.mode
            Case TypeCodes.string, TypeCodes.double
                If row_names.length = 1 Then
                    Dim key = any.ToString(row_names.data.GetValue(Scan0))

                    If dataframe.columns.ContainsKey(key) Then
                        dataframe.rownames = CLRVector.asCharacter(dataframe.columns(key))
                        dataframe.columns.Remove(key)
                    ElseIf key = "" Then
                        ' 20210912
                        ' unsure for the NULL literal bug in build R package
                        Return Nothing
                    Else
                        Return RInternal.debug.stop({
                            "undefined column was selected as row names!",
                            "given: " & key,
                            "you can check your table file format andalso check the parameters: check.names, comment.char, skip_rows"
                        }, env)
                    End If
                Else
                    dataframe.rownames = row_names.data _
                        .AsObjectEnumerator(Of Object) _
                        .Select(AddressOf any.ToString) _
                        .ToArray
                End If
            Case TypeCodes.integer
                If row_names.length = 1 Then
                    Dim i As Integer = CInt(row_names.data.GetValue(Scan0)) - 1
                    Dim project = dataframe.columns.Keys(i)

                    dataframe.rownames = dataframe.columns(project) _
                        .AsObjectEnumerator _
                        .Select(Function(x) any.ToString(x)) _
                        .ToArray
                    dataframe.columns.Remove(project)
                Else

                End If
            Case Else
                Return RInternal.debug.stop("invalid row names data type! required character or numeric!", env)
        End Select

        Return Nothing
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
    ''' <param name="meta_blank">
    ''' set the cell value for represents the missing value of the metadata 
    ''' column when do save of the clr object array.
    ''' </param>
    ''' <returns></returns>
    ''' <remarks>
    ''' this function will create an empty table file if the given data
    ''' table object <paramref name="x"/> is nothing.
    ''' </remarks>
    <ExportAPI("write.csv")>
    <RApiReturn(GetType(Boolean))>
    Public Function write_csv(<RRawVectorArgument> x As Object,
                              Optional file As Object = Nothing,
                              <RRawVectorArgument>
                              Optional row_names As Object = True,
                              Optional fileEncoding As Object = "",
                              Optional tsv As Boolean = False,
                              <RDefaultExpression>
                              Optional number_format As Object = Nothing,
                              Optional meta_blank As String = "",
                              Optional env As Environment = Nothing) As Object

        If TypeOf file Is dataframeBuffer Then
            ' just wrap a buffer object for web service session
            DirectCast(file, dataframeBuffer).dataframe = x
            DirectCast(file, dataframeBuffer).tsv = tsv
            Return file
        ElseIf TypeOf file Is textBuffer Then
            ' transfer as plant text file
            Dim document = DirectCast(x, Rdataframe).DataFrameRows(row_names, formatNumber:=Nothing, env)
            Dim ms As New MemoryStream
            Dim text As String

            If document Like GetType(Message) Then
                Return document.TryCast(Of Message)
            End If

            StreamIO.SaveDataFrame(document.TryCast(Of csv).Rows, ms, Encoding.UTF8, tsv:=tsv, silent:=True)
            ms.Flush()
            text = Encoding.UTF8.GetString(ms.ToArray)
            ms.Dispose()

            With DirectCast(file, textBuffer)
                .text = text
                .mime = "text/csv"
            End With

            Return file
        ElseIf TypeOf file Is Stream Then
            ' save table with given stream
            Dim document = DirectCast(x, Rdataframe).DataFrameRows(row_names, formatNumber:=Nothing, env)

            If document Like GetType(Message) Then
                Return document.TryCast(Of Message)
            End If

            StreamIO.SaveDataFrame(document.TryCast(Of csv).Rows, DirectCast(file, Stream), Encoding.UTF8,
                                   tsv:=tsv,
                                   silent:=True,
                                   autoCloseFile:=False)
            Return True
        ElseIf file Is Nothing OrElse TypeOf file Is String Then
            ' save table with specific file path
            Return env.saveTextFile(x, file, row_names, fileEncoding, tsv,
                                    meta_blank:=meta_blank,
                                    number_format:=number_format)
        Else
            Return Message.InCompatibleType(GetType(String), file.GetType, env)
        End If
    End Function

    ''' <summary>
    ''' Save the data table to a local csv file
    ''' </summary>
    ''' <param name="env"></param>
    ''' <param name="x">any clr object/R# object that could be cast to dataframe</param>
    ''' <param name="file">the file path of the local file</param>
    ''' <param name="row_names"></param>
    ''' <param name="fileEncoding"></param>
    ''' <param name="tsv"></param>
    ''' <param name="number_format"></param>
    ''' <returns></returns>
    <Extension>
    Private Function saveTextFile(env As Environment,
                                  x As Object,
                                  file$,
                                  row_names As Object,
                                  fileEncoding As Object,
                                  tsv As Boolean,
                                  meta_blank As String,
                                  number_format As Object) As Object
        If x Is Nothing Then
            Call env.AddMessage("Empty dataframe object!", MSG_TYPES.WRN)
            Return "".SaveTo(file)
        ElseIf Not file.StringEmpty Then
            ' test if the target table file is not locked by excel
            Dim err As Object = REnv.TryCatch(Function() "".SaveTo(file), debug:=env.globalEnvironment.debugMode)

            If Not TypeOf err Is Boolean Then
                Return RInternal.debug.stop(err, env)
            End If
        End If

        Dim type As Type = x.GetType
        Dim encoding As Encodings = TextEncodings.GetEncodings(Rsharp.GetEncoding(fileEncoding))
        ' format is nothing, means apply the 
        ' format of .net clr default
        Dim formatNumber As String = CLRVector.asCharacter(number_format).ElementAtOrDefault(Scan0, [default]:=Nothing)

        If type Is GetType(Rdataframe) Then
            x = DirectCast(x, Rdataframe).CheckDimension(env)

            If TypeOf x Is Message Then
                Return x
            End If

            Dim value = DirectCast(x, Rdataframe).DataFrameRows(row_names, formatNumber, env)

            If value Like GetType(Message) Then
                Return value.TryCast(Of Message)
            Else
                Return value.TryCast(Of csv).Save(
                    path:=file,
                    encoding:=encoding,
                    silent:=True,
                    tsv:=tsv
                )
            End If
        ElseIf type Is GetType(file) Then
            Return DirectCast(x, file).Save(path:=file, encoding:=encoding, silent:=True)
        ElseIf type Is GetType(DataFrameResolver) Then
            Return DirectCast(x, DataFrameResolver).csv.Save(path:=file, encoding:=encoding, silent:=True)
        ElseIf REnv.isVector(Of EntityObject)(x) Then
#Disable Warning
            Return DirectCast(REnv.asVector(Of EntityObject)(x), EntityObject()).SaveDataSet(path:=file, encoding:=encoding, silent:=True)
        ElseIf REnv.isVector(Of DataSet)(x) Then
            Return DirectCast(REnv.asVector(Of DataSet)(x), DataSet()) _
                .SaveTo(
                    path:=file,
                    encoding:=encoding.CodePage,
                    silent:=True,
                    metaBlank:=0.0
                )
        ElseIf x.GetType.ImplementInterface(Of MatrixProvider) Then
            Return DirectCast(x, MatrixProvider).GetMatrix.SaveTo(
                path:=file,
                encoding:=encoding.CodePage,
                silent:=True,
                metaBlank:=0.0
            )
#Enable Warning
        ElseIf type.IsArray OrElse type Is GetType(vector) Then
            Return saveGeneric(x, type, file, meta_blank, encoding.CodePage, env)
        Else
            Dim stream As pipeline = pipeline.TryCreatePipeline(Of EntityObject)(x, env)

            If stream.isError Then
                stream = pipeline.TryCreatePipeline(Of DataSet)(x, env)

                If stream.isError Then
                    Return Message.InCompatibleType(GetType(file), type, env)
                Else
                    Return stream.populates(Of DataSet)(env) _
                        .SaveTo(
                            path:=file,
                            encoding:=encoding.CodePage,
                            silent:=True,
                            metaBlank:=0
                        )
                End If
            Else
                Return stream.populates(Of EntityObject)(env).SaveDataSet(path:=file, encoding:=encoding, silent:=True)
            End If
        End If
    End Function

    Friend Function MeasureGenericType(x As Object, ByRef type As Type) As Array
        If type Is GetType(vector) Then
            With DirectCast(x, vector)
                x = DirectCast(x, vector).data
                type = .elementType?.raw
            End With

            If type Is Nothing Then
                type = x.GetType.GetElementType
            End If
        Else
            type = type.GetElementType
        End If

        Return DirectCast(x, Array)
    End Function

    ''' <summary>
    ''' save a generic data collection as csv data file
    ''' </summary>
    ''' <param name="x">should be an array to save as csv file</param>
    ''' <param name="type">the element type of the items in the target array <paramref name="x"/></param>
    ''' <param name="file">the file path string</param>
    ''' <param name="encoding">text encoding of the csv text file</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Function saveGeneric(x As Object, type As Type, file$, meta_blank As String, encoding As Encoding, env As Environment) As Boolean
        Dim castGeneric As Array = MeasureGenericType(x, type)

        Call $"save clr object({type.Name}) array as dataframe table file.".info

        Return castGeneric.SaveTable(file, encoding, type,
            meta_blank:=meta_blank,
            silent:=True)
    End Function
End Module
