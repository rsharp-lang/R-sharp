﻿#Region "Microsoft.VisualBasic::bbe410991dfbe7b9080f54b839dbe052, Library\base\utils\xlsx.vb"

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

    '   Total Lines: 284
    '    Code Lines: 199 (70.07%)
    ' Comment Lines: 60 (21.13%)
    '    - Xml Docs: 86.67%
    ' 
    '   Blank Lines: 25 (8.80%)
    '     File Size: 11.65 KB


    ' Module xlsx
    ' 
    '     Function: coercesDataTable, createRow, createSheet, createWorkbook, getSheetNames
    '               openXlsx, readXlsx, writeXlsx
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Data.Framework.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLS.MsHtml
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports any = Microsoft.VisualBasic.Scripting
Imports csv = Microsoft.VisualBasic.Data.Framework.IO.File
Imports DataTable = Microsoft.VisualBasic.Data.Framework.IO.DataFrameResolver
Imports msXlsx = Microsoft.VisualBasic.MIME.Office.Excel.XLSX.File
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal
Imports Rsharp = SMRUCC.Rsharp
Imports Worksheet = Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer.Worksheet

''' <summary>
''' Xlsx file toolkit
''' </summary>
<Package("xlsx", Category:=APICategories.UtilityTools)>
Module xlsx

    ''' <summary>
    ''' read a sheet table in xlsx file as a ``dataframe`` object.
    ''' </summary>
    ''' <param name="sheetIndex">
    ''' the data sheet index or name for read data. the index value of 
    ''' parameter should be start from base 1 if the type is an integer 
    ''' index
    ''' </param>
    ''' <param name="check_modes">
    ''' check the data type of each column in the dataframe? 
    ''' this options is working if the ``raw`` parameter is 
    ''' set to ``FALSE``. 
    ''' </param>
    ''' <param name="check_names">
    ''' make valid R# symbol names of each column in the dataframe?
    ''' this options is working if the ``raw`` parameter is set
    ''' to ``FALSE``.
    ''' </param>
    ''' <param name="row_names">
    ''' A character vector for set row names of the generated dataframe, or 
    ''' a character name in the dataframe, or an integer value to indicate
    ''' that which column should be used as the row names of the 
    ''' dataframe object.
    ''' 
    ''' this options is working if the ``raw`` parameter is set
    ''' to ``FALSE``.
    ''' </param>
    ''' <param name="file">the file path of the target xlsx data file.</param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("read.xlsx")>
    <RApiReturn(GetType(Rdataframe), GetType(csv))>
    Public Function readXlsx(file As Object,
                             Optional sheetIndex As Object = "Sheet1",
                             <RRawVectorArgument>
                             Optional row_names As Object = Nothing,
                             Optional raw As Boolean = False,
                             Optional check_names As Boolean = True,
                             Optional check_modes As Boolean = True,
                             Optional comment_char As Char = "#"c,
                             Optional skip_rows As Integer = -1,
                             Optional env As Environment = Nothing) As Object
        Dim xlsx As msXlsx
        Dim table As csv

        If TypeOf file Is String Then
            xlsx = msXlsx.Open(DirectCast(file, String))
        ElseIf TypeOf file Is msXlsx Then
            xlsx = DirectCast(file, msXlsx)
        Else
            Return RInternal.debug.stop(Message.InCompatibleType(GetType(String), file.GetType, env), env)
        End If

        If sheetIndex Is Nothing Then
            Return RInternal.debug.stop("the sheet index can not be nothing!", env)
        ElseIf RType.TypeOf(sheetIndex).mode = TypeCodes.integer Then
            table = xlsx.GetTable(CInt(sheetIndex) - 1)
        Else
            table = xlsx.GetTable(sheetName:=any.ToString(sheetIndex))
        End If

        If raw Then
            Return table
        ElseIf table Is Nothing Then
            Return Nothing
        Else
            Return table.rawToDataFrame(
                row_names:=row_names,
                check_names:=check_names,
                check_modes:=check_modes,
                comment_char:=comment_char,
                skip_rows:=skip_rows,
                env:=env
            )
        End If
    End Function

    <ExportAPI("open.xlsx")>
    Public Function openXlsx(file As String) As Workbook
        Return New Workbook(file)
    End Function

    ''' <summary>
    ''' get all sheet names
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    <ExportAPI("sheetNames")>
    Public Function getSheetNames(file As msXlsx) As vector
        Return file.SheetNames.DoCall(AddressOf RInternal.[Object].vector.asVector)
    End Function

    <ExportAPI("write.xlsx")>
    <RApiReturn(GetType(Boolean))>
    Public Function writeXlsx(<RRawVectorArgument> x As Object, file$,
                              Optional sheetName$ = "Sheet1",
                              Optional row_names As Object = True,
                              Optional fileEncoding As Object = "",
                              <RDefaultExpression>
                              Optional number_format As Object = "~`${getOption('f64.format')}${getOption('digits')}`",
                              Optional env As Environment = Nothing) As Object
        If x Is Nothing Then
            Return RInternal.debug.stop("Empty dataframe object!", env)
        ElseIf Not file.StringEmpty Then
            ' test if the target table file is not locked by excel
            Dim err As Object = REnv.TryCatch(Function() "".SaveTo(file), debug:=env.globalEnvironment.debugMode)

            If Not TypeOf err Is Boolean Then
                Return RInternal.debug.stop(err, env)
            End If
        End If

        Dim encoding As Encodings = TextEncodings.GetEncodings(Rsharp.GetEncoding(fileEncoding))
        Dim formatNumber As String = CLRVector.asCharacter(number_format).ElementAtOrDefault(Scan0, [default]:="G6")
        Dim table As csv

        If x.GetType Is GetType(list) AndAlso file.ExtensionSuffix("xlsx") Then
            Dim tables As list = DirectCast(x, list)
            Dim sanitizeSheetName As Boolean = True
            ' Create New workbook
            Dim workbook As New Workbook(file, Nothing)

            ' save multiple sheet table
            For Each name As String In tables.getNames
                Dim temp = coercesDataTable(tables.getByName(name), row_names, formatNumber, env)

                If temp Like GetType(Message) Then
                    Return temp.TryCast(Of Message)
                ElseIf temp Is Nothing Then
                    Return Message.InCompatibleType(GetType(csv), x.GetType, env)
                Else
                    workbook.AddWorksheet(name, sanitizeSheetName)
                    sanitizeSheetName = False
                    workbook.WriteSheetTable(temp.TryCast(Of csv))
                End If
            Next

            Call workbook.Save()

            Return True
        Else
            Dim temp As [Variant](Of Message, csv) = coercesDataTable(x, row_names, formatNumber, env)

            If temp Like GetType(Message) Then
                Return temp.TryCast(Of Message)
            ElseIf Not temp Is Nothing Then
                table = temp
            Else
                Return Message.InCompatibleType(GetType(csv), x.GetType, env)
            End If
        End If

        If file.ExtensionSuffix("xls") Then
            Return table _
                .ToExcel(sheetName) _
                .SaveTo(file, encoding.CodePage, append:=False)
        Else
            ' save xlsx zip package
            Dim workbook As New Workbook(file, sheetName)
            workbook.WriteSheetTable(table)
            workbook.Save()
            Return True
        End If
    End Function

    Private Function coercesDataTable(x As Object, row_names As Object, formatNumber As String, env As Environment) As [Variant](Of Message, csv)
        Dim type As Type = x.GetType

        If type Is GetType(Rdataframe) Then
            x = DirectCast(x, Rdataframe).CheckDimension(env)

            If TypeOf x Is Message Then
                Return x
            End If

            Return DirectCast(x, Rdataframe).DataFrameRows(row_names, formatNumber, env)
        ElseIf type Is GetType(csv) Then
            Return DirectCast(x, csv)
        ElseIf type Is GetType(DataTable) Then
            Return DirectCast(x, DataTable).csv
        ElseIf REnv.isVector(Of EntityObject)(x) Then
#Disable Warning
            Return Reflector.GetsRowData(
                source:=DirectCast(REnv.asVector(Of EntityObject)(x), EntityObject()).Select(Function(d) CObj(d)),
                type:=GetType(EntityObject),
                strict:=False,
                maps:=Nothing,
                parallel:=False,
                metaBlank:="",
                reorderKeys:=0,
                layout:=Nothing
            ).DoCall(Function(rows) New csv(rows))
        ElseIf REnv.isVector(Of DataSet)(x) Then
            Return Reflector.GetsRowData(
                source:=DirectCast(REnv.asVector(Of DataSet)(x), DataSet()).Select(Function(d) CObj(d)),
                type:=GetType(DataSet),
                strict:=False,
                maps:=Nothing,
                parallel:=False,
                metaBlank:="",
                reorderKeys:=0,
                layout:=Nothing
            ).DoCall(Function(rows) New csv(rows))
#Enable Warning
        ElseIf type.IsArray OrElse type Is GetType(vector) Then
            Return Reflector.doSave(objSource:=utils.MeasureGenericType(x, type),
                typeDef:=type,
                strict:=False,
                schemaOut:=Nothing
            ).DoCall(Function(rows) New csv(rows))
        Else
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' ## Functions to manipulate Excel 2007 workbooks.
    ''' 
    ''' createWorkbook creates an empty workbook object.
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("createWorkbook")>
    Public Function createWorkbook() As Workbook
        Return New Workbook
    End Function

    ''' <summary>
    ''' ## Functions to manipulate worksheets.
    ''' 
    ''' Functions to manipulate worksheets. Create a new worksheet for a given xlsx file 
    ''' </summary>
    ''' <param name="wb">
    ''' a workbook object as returned by createWorksheet or loadWorksheet.
    ''' </param>
    ''' <param name="sheetName">
    ''' a character specifying the name of the worksheet to create, or remove.
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("createSheet")>
    Public Function createSheet(wb As Workbook, Optional sheetName$ = "Sheet1") As Worksheet
        Call wb.AddWorksheet(sheetName)
        Return wb.CurrentWorksheet
    End Function

    <ExportAPI("createRow")>
    Public Function createRow(sheet As Worksheet,
                              <RRawVectorArgument(TypeCodes.integer)>
                              Optional rowIndex As Object = "1,2,3,4,5")

    End Function
End Module
