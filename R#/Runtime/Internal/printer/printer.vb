﻿#Region "Microsoft.VisualBasic::43afd28db7e04be14dbae2d04aca8565, R#\Runtime\Internal\printer\printer.vb"

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

    '   Total Lines: 479
    '    Code Lines: 340 (70.98%)
    ' Comment Lines: 73 (15.24%)
    '    - Xml Docs: 83.56%
    ' 
    '   Blank Lines: 66 (13.78%)
    '     File Size: 19.95 KB


    '     Delegate Function
    ' 
    ' 
    '     Module printer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: DateToString, f64_InternalToString, getMaxColumns, getStrings, printComplex
    '                   printInternal, printList, printStream, ToString, toStringArray
    '                   ValueToString
    ' 
    '         Sub: AttachConsoleFormatter, AttachInternalConsoleFormatter, printArray, printContentArray
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization
Imports SMRUCC.Rsharp.Development.CodeAnalysis
Imports SMRUCC.Rsharp.Development.Components
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.base
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.math
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting
Imports collectionSet = Microsoft.VisualBasic.ComponentModel.DataStructures.Set
Imports complex = System.Numerics.Complex
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.ConsolePrinter

    ''' <summary>
    ''' A delegate function for produce the data handler for build print string 
    ''' </summary>
    ''' <param name="printContent"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    Public Delegate Function InternalToString(printContent As Boolean, env As GlobalEnvironment) As IStringBuilder

    ''' <summary>
    ''' R# console nice print supports.
    ''' </summary>
    Public Module printer

        Friend ReadOnly RtoString As New Dictionary(Of Type, IStringBuilder)
        Friend ReadOnly RInternalToString As New Dictionary(Of Type, InternalToString)

        Sub New()
            RtoString(GetType(Color)) = Function(c) DirectCast(c, Color).ToHtmlColor.ToLower
            RtoString(GetType(Byte)) = Function(b) "&H" & CInt(DirectCast(b, Byte)).ToHexString
            RtoString(GetType(vbObject)) = Function(o) DirectCast(o, vbObject).ToString
            RtoString(GetType(pipeline)) = Function(o) DirectCast(o, pipeline).ToString
            RtoString(GetType(RType)) = Function(o) DirectCast(o, RType).ToString
            RtoString(GetType(DateTime)) = AddressOf DateToString
            RtoString(GetType(ExceptionData)) = AddressOf debug.PrintRExceptionStackTrace
            RtoString(GetType(Environment)) = Function(o) DirectCast(o, Environment).ToString
            RtoString(GetType(GlobalEnvironment)) = Function(o) DirectCast(o, GlobalEnvironment).ToString
            RtoString(GetType(LogEntry)) = Function(o) DirectCast(o, LogEntry).ToString
            RtoString(GetType(unit)) = Function(o) DirectCast(o, unit).ToString
            RtoString(GetType(RSessionInfo)) = Function(o) o.ToString
            RtoString(GetType(MemoryStream)) = AddressOf printStream
            RtoString(GetType(FormulaExpression)) = Function(f) f.ToString
            RtoString(GetType(complex)) = Function(c) printComplex(DirectCast(c, complex))
            RtoString(GetType(corTestResult)) = Function(o) o.ToString
            RtoString(GetType(NoInspector)) = Function(o) o.ToString
            RtoString(GetType(SymbolTypeDefine)) = Function(o) o.ToString
            RtoString(GetType(FileReference)) = Function(o) o.ToString
            RtoString(GetType(collectionSet)) = Function(o) o.ToString

            ' Rscript expression to string
            RtoString(GetType(Literal)) = Function(o) DirectCast(o, Literal).ToString
            RtoString(GetType(VectorLiteral)) = Function(o) DirectCast(o, VectorLiteral).ToString

            RInternalToString(GetType(Double)) = AddressOf printer.f64_InternalToString
        End Sub

        Private Function printComplex(c As complex) As String
            If c.Imaginary < 0 Then
                Return $"{c.Real}-{c.Imaginary}i"
            Else
                Return $"{c.Real}+{c.Imaginary}i"
            End If
        End Function

        Private Function printStream(buffer As MemoryStream) As String
            Dim bytes As Byte() = buffer.ToArray
            Dim hex As String() = bytes.Take(10).Select(Function(i) CInt(i).ToHexString).ToArray

            Return $"[size={buffer.Length}] {hex.JoinBy(" ")}..."
        End Function

        Private Function DateToString(x As Date) As String
            Dim yy = x.Year.ToString.FormatZero("0000")
            Dim mm = x.Month.ToString.FormatZero("00")
            Dim dd = x.Day.ToString.FormatZero("00")
            Dim h = x.Hour.ToString.FormatZero("00")
            Dim m = x.Minute.ToString.FormatZero("00")
            Dim s = x.Second.ToString.FormatZero("00")

            Return $"#{yy}-{mm}-{dd} {h}:{m}:{s}#"
        End Function

        ''' <summary>
        ''' get formatter function for double to string
        ''' </summary>
        ''' <param name="printContent"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Private Function f64_InternalToString(printContent As Boolean, env As GlobalEnvironment) As IStringBuilder
            Dim opts As Options = env.globalEnvironment.options
            Dim format As String = $"{opts.f64Format}{opts.digits}"

            Return Function(d)
                       Dim val As Double = 0

                       If d Is Nothing Then
                           val = 0
                       ElseIf TypeOf d Is String Then
                           val = Conversion.Val(d)
                       Else
                           val = CType(d, Double)
                       End If

                       If val = 0.0 Then
                           ' 20201009
                           ' 因为下面的代码有一个将0字符进行TrimEnd的操作
                           ' 所以val等于零的时候，会变为空字符串
                           ' 在这里直接返回零来避免这个格式化问题
                           Return "0"
                       End If

                       Dim str As String = val.ToString(format)

                       If str.IndexOf("."c) > -1 Then
                           str = str.TrimEnd("0"c)

                           If str.Last = "."c Then
                               str = str.TrimEnd("."c)
                           End If
                       End If

                       If printContent AndAlso val > 0 Then
                           str = " " & str
                       End If

                       Return str
                   End Function
        End Function

        ''' <summary>
        ''' <see cref="Object"/> -> <see cref="String"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="formatter"></param>
        Public Sub AttachConsoleFormatter(Of T)(formatter As IStringBuilder)
            RtoString(GetType(T)) = formatter
        End Sub

        ''' <summary>
        ''' <see cref="Object"/> -> <see cref="String"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="formatter"></param>
        Public Sub AttachInternalConsoleFormatter(Of T)(formatter As InternalToString)
            RInternalToString(GetType(T)) = formatter
        End Sub

        ''' <summary>
        ''' Print the internal R# runtime object
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="listPrefix"></param>
        ''' <param name="opts"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Friend Function printInternal(x As Object, listPrefix As String, opts As PrinterOptions, env As GlobalEnvironment) As Message
            Dim valueType As Type
            Dim output As RContentOutput = env.stdout

            If x Is Nothing Then
                Call output.WriteLine("NULL")
                Return Nothing
            Else
                valueType = x.GetType
            End If

            If RtoString.ContainsKey(valueType) Then
                Call output.WriteLine(RtoString(valueType)(x))
            ElseIf valueType.IsInheritsFrom(GetType(Array)) Then
                With DirectCast(x, Array)
                    If .Length > 1 Then
                        Call .printArray(opts.maxPrint, env)
                    ElseIf .Length = 0 Then
                        Call output.WriteLine("NULL")
                    Else
                        x = .GetValue(Scan0)
                        ' get the first value and then print its
                        ' text value onto console
                        GoTo printSingleElement
                    End If
                End With
            ElseIf valueType Is GetType(vector) Then
                Dim vec As vector = DirectCast(x, vector)

                Call vec.data.printArray(opts.maxPrint, env)

                If Not vec.unit Is Nothing Then
                    Call output.WriteLine($"unit: {vec.unit}")
                End If

            ElseIf valueType.ImplementInterface(GetType(IDictionary)) Then
                Dim err As Message = DirectCast(x, IDictionary).printList(listPrefix, opts, env)

                If Not err Is Nothing Then
                    Call output.Flush()
                    Return err
                End If
            ElseIf valueType Is GetType(list) Then
                Dim err As Message = DirectCast(x, list) _
                    .slots _
                    .DoCall(Function(list)
                                Return DirectCast(list, IDictionary).printList(listPrefix, opts, env)
                            End Function)

                If Not err Is Nothing Then
                    Call output.Flush()
                    Return err
                End If
            ElseIf valueType Is GetType(dataframe) Then
                Dim dataframe As dataframe = DirectCast(x, dataframe)

                If Not opts.fields.IsNullOrEmpty Then
                    Dim result = dataframe.projectByColumn(opts.fields, env:=env)

                    If TypeOf result Is Message Then
                        Return result
                    Else
                        dataframe = result
                    End If
                End If

                Dim err As Message = tablePrinter.PrintTable(dataframe, opts.maxPrint, opts.maxWidth, output, env)

                If Not err Is Nothing Then
                    Call output.Flush()
                    Return err
                End If
            ElseIf valueType Is GetType(vbObject) Then
                Call DirectCast(x, vbObject).ToString.DoCall(AddressOf output.WriteLine)
            Else
printSingleElement:
                Call output.WriteLine("[1] " & printer.ValueToString(x, env))
            End If

            Call output.Flush()

            Return Nothing
        End Function

        <Extension>
        Private Function printList(list As IDictionary, listPrefix$, opts As PrinterOptions, env As GlobalEnvironment) As Message
            Dim output As RContentOutput = env.stdout
            Dim err As Message

            If list Is Nothing Then
                Return Nothing
            End If

            For Each objKey As Object In list.Keys
                Dim slotValue As Object = list(objKey)
                Dim key$ = objKey.ToString

                If key.IsPattern("\d+") Then
                    key = $"{listPrefix}[[{key}]]"
                Else
                    key = $"{listPrefix}${key}"
                End If

                Call output.WriteLine(key)
                err = printer.printInternal(slotValue, key, opts, env)
                Call output.WriteLine()

                If Not err Is Nothing Then
                    Return err
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' Debugger test api of <see cref="ToString"/>
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ValueToString(x As Object, env As GlobalEnvironment) As String
            If x Is Nothing Then
                Return "NULL"
            Else
                Dim to_str = printer.ToString(x.GetType, env, True)

                If to_str Is Nothing Then
                    Return any.ToString(x, "<invalid>")
                Else
                    Return to_str(x)
                End If
            End If
        End Function

        Private Function toStringArray(o As Object) As String
            If o Is Nothing Then
                Return "NULL"
            ElseIf Not o.GetType.IsArray Then
                Return any.ToString(o, "NULL", True)
            End If

            Return DirectCast(o, Array) _
                .AsObjectEnumerator _
                .Select(Function(obj)
                            Return any.ToString(obj, "NULL", True)
                        End Function) _
                .JoinBy(", ")
        End Function

        ''' <summary>
        ''' The external string formatter will overrides the internal formatter
        ''' </summary>
        ''' <param name="elementType"></param>
        ''' <returns></returns>
        <Extension>
        Friend Function ToString(elementType As Type,
                                 env As GlobalEnvironment,
                                 printContent As Boolean,
                                 Optional allowClassPrinter As Boolean = True) As IStringBuilder

            If RtoString.ContainsKey(elementType) Then
                Return RtoString(elementType)
            ElseIf RInternalToString.ContainsKey(elementType) Then
                Return RInternalToString(elementType)(printContent, env)
            ElseIf elementType Is GetType(String) Then
                Return Function(o) As String
                           If o Is Nothing Then
                               Return "NULL"
                           ElseIf printContent Then
                               Return $"""{o}"""
                           Else
                               Return CStr(o)
                           End If
                       End Function
            ElseIf elementType = GetType(Boolean) Then
                Return Function(b) b.ToString.ToUpper
            ElseIf elementType.IsEnum Then
                Return AddressOf enumPrinter.printEnumValue(elementType).Invoke
            ElseIf elementType.IsArray Then
                Return AddressOf toStringArray
            ElseIf elementType Is GetType(Void) OrElse elementType.FullName = "System.RuntimeType" Then
                Return Function(obj)
                           ' 20210119
                           ' handle NA string at here
                           ' system.void is the literal of NA
                           If obj Is GetType(Void) Then
                               Return "NA"
                           Else
                               Return any.ToString(obj, "NULL", True)
                           End If
                       End Function
            Else
                If allowClassPrinter Then
                    If Not (elementType.Namespace.StartsWith("System.") OrElse elementType.Namespace = "System") Then
                        Return AddressOf classPrinter.printClass
                    Else
                        Return Function(obj) any.ToString(obj, "NULL", True)
                    End If
                Else
                    Return Function(obj) any.ToString(obj, "NULL", True)
                End If
            End If
        End Function

        ''' <summary>
        ''' populate strings from the data array
        ''' </summary>
        ''' <param name="xVec"></param>
        ''' <param name="elementType"></param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' try to convert any kind of array as the string collection for do console print
        ''' </returns>
        Friend Function getStrings(xVec As Array, ByRef elementType As Type, env As GlobalEnvironment) As IEnumerable(Of String)
            Dim toString As IStringBuilder

            elementType = REnv.MeasureArrayElementType(xVec)
            toString = printer.ToString(elementType, env, True)

            Return From element As Object
                   In xVec.AsQueryable
                   Let str As String = toString(element)
                   Select str
        End Function

        ''' <summary>
        ''' Print vector elements
        ''' </summary>
        ''' <param name="xvec"></param>
        <Extension>
        Friend Sub printArray(xvec As Array, maxPrint%, env As GlobalEnvironment)
            Dim elementType As Type = Nothing
            Dim stringVec As IEnumerable(Of String) = getStrings(xvec, elementType, env)
            Dim contents As String() = stringVec.Take(maxPrint).ToArray
            Dim maxColumns As Integer = env.getMaxColumns
            Dim output As RContentOutput = env.globalEnvironment.stdout

            If contents.Length = 0 Then
                Call output.WriteLine($"{RType.GetRSharpType(elementType)}(0)")
            Else
                Call contents.printContentArray(Nothing, Nothing, maxColumns, output)

                If xvec.Length > maxPrint Then
                    Call env.stdout.WriteLine($"[ reached getOption(""max.print"") -- omitted {xvec.Length - contents.Length} entries ]")
                End If
            End If
        End Sub

        ''' <summary>
        ''' get max width to break the table
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <Extension>
        Friend Function getMaxColumns(env As Environment) As Integer
            If env.globalEnvironment.stdout.env = OutputEnvironments.Html Then
                Return 128
            Else
                Return If(App.IsConsoleApp, Console.WindowWidth, Integer.MaxValue) * 0.75
            End If
        End Function

        <Extension>
        Friend Sub printContentArray(contents$(), deli$, indentPrefix$, maxColumns%, output As TextWriter)
            ' maxsize / average size
            Dim unitWidth As Integer = contents.Max(Function(c) c.Length) + 1
            Dim divSize As Integer = maxColumns \ unitWidth - 3

            ' 20200319 fix the bugs for io redirect on the linux platform
            If divSize >= SByte.MaxValue Then
                divSize = SByte.MaxValue
            End If

            Dim i As i32
            Dim cell As String

            If divSize <= 0 Then
                divSize = 1
                i = 0
            Else
                i = 1 - divSize
            End If

            For Each row As String() In contents.Split(partitionSize:=divSize)
                If indentPrefix Is Nothing Then
                    Call output.Write($"[{i = CInt(i + divSize)}]{vbTab}")
                Else
                    Call output.Write(indentPrefix)
                End If

                For j As Integer = 0 To row.Length - 1
                    cell = row(j)

                    Call output.Write(cell)

                    If deli Is Nothing Then
                        Call output.Write(New String(" "c, unitWidth - cell.Length))
                    ElseIf Not j = row.Length - 1 Then
                        Call output.Write(deli)
                    End If
                Next

                Call output.WriteLine()
            Next
        End Sub
    End Module
End Namespace
