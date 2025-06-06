﻿#Region "Microsoft.VisualBasic::a44aeb1472ee97bdf0e0aaafb0148baf, R#\Runtime\Internal\internalInvokes\base.vb"

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

    '   Total Lines: 3706
    '    Code Lines: 1620 (43.71%)
    ' Comment Lines: 1781 (48.06%)
    '    - Xml Docs: 83.49%
    ' 
    '   Blank Lines: 305 (8.23%)
    '     File Size: 166.31 KB


    '     Module base
    ' 
    '         Function: __empty, __invisible, [date], [stop], allocate
    '                   append, appendFinal, appendOfList, appendOfVector, (+3 Overloads) argumentList
    '                   as_array, attachPackageFile, autoDispose, c, cat
    '                   cbind, character, checkDimensionsAgree, colnames, columnVector
    '                   commandArgs, doPrintInternal, dQuote, factor, factors
    '                   getOption, getPosition, ifelse, ifelseScalar, ifelseVector
    '                   ints, is_array, isDataframe, isEmpty, isEmptyArray
    '                   isFALSE, isFunction, isList, isNA, isNull
    '                   isRVector, isTRUE, join_data, length, library
    '                   logical, makeNames, names, ncol, neg
    '                   nrow, numeric, objectAddInvoke, options, options_flush
    '                   print, range, raws, rbind, Rdataframe
    '                   rep, rep_int, rep_len, replace, Rlist
    '                   Robj_dimension, rownames, rowwise, seq, setOptionsParameters
    '                   sink, sink_number, source, sQuote, str
    '                   summary, t, uniqueNames, unitOfT, ValueAt
    '                   warning, year
    ' 
    '         Sub: println_message, warnings
    '         Class PrinterOptions
    ' 
    '             Properties: fields, maxPrint, maxWidth, quot, syntax
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.ValueTypes
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.CodeAnalysis
Imports SMRUCC.Rsharp.Development.Components
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.[Object].baseOp.dataframeOp
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Utils
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports any = Microsoft.VisualBasic.Scripting
Imports collectionSet = Microsoft.VisualBasic.ComponentModel.DataStructures.Set
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RObj = SMRUCC.Rsharp.Runtime.Internal.Object
Imports std = System.Math
Imports vector = SMRUCC.Rsharp.Runtime.Internal.Object.vector

#If NET48 Then
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
#Else
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
#End If

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' 在这个模块之中仅包含有最基本的数据操作函数
    ''' </summary>
    ''' 
    <Package("base")>
    Public Module base

        ''' <summary>
        ''' ### Logical Operators
        ''' 
        ''' isTRUE(x) is the same as { is.logical(x) &amp;&amp; length(x) == 1 &amp;&amp; !is.na(x) &amp;&amp; x }; 
        ''' isFALSE() is defined analogously. Consequently, if(isTRUE(cond)) may be preferable to if(cond) because 
        ''' of NAs.
        ''' 
        ''' In earlier R versions, isTRUE &lt;- function(x) identical(x, TRUE), had the drawback to be false e.g., 
        ''' for x &lt;- c(val = TRUE).
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <example>
        ''' # TRUE
        ''' isTRUE(TRUE);
        ''' 
        ''' # FALSE
        ''' isTRUE("TRUE");
        ''' 
        ''' # FALSE
        ''' isTRUE(FALSE);
        ''' 
        ''' # FALSE
        ''' isTRUE([TRUE TRUE]);
        ''' </example>
        <ExportAPI("isTRUE")>
        Public Function isTRUE(<RRawVectorArgument> x As Object) As Boolean
            Dim vx = REnv.asVector(Of Object)(x)

            If vx.IsNullOrEmpty OrElse vx.Length > 1 Then
                Return False
            Else
                x = vx.GetValue(0)

                If TypeOf x Is Boolean Then
                    Return x
                Else
                    Return False
                End If
            End If
        End Function

        ''' <summary>
        ''' ### Logical Operators
        ''' 
        ''' isTRUE(x) is the same as { is.logical(x) &amp;&amp; length(x) == 1 &amp;&amp; !is.na(x) &amp;&amp; x }; 
        ''' isFALSE() is defined analogously. Consequently, if(isTRUE(cond)) may be preferable to if(cond) because 
        ''' of NAs.
        ''' 
        ''' In earlier R versions, isTRUE &lt;- function(x) identical(x, TRUE), had the drawback to be false e.g., 
        ''' for x &lt;- c(val = TRUE).
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <ExportAPI("isFALSE")>
        Public Function isFALSE(<RRawVectorArgument> x As Object) As Boolean
            Dim vx = REnv.asVector(Of Object)(x)

            If vx.IsNullOrEmpty OrElse vx.Length > 1 Then
                Return False
            Else
                x = vx.GetValue(0)

                If TypeOf x Is Boolean Then
                    Return Not CBool(x)
                Else
                    Return False
                End If
            End If
        End Function

        ''' <summary>
        ''' ### Numeric Vectors
        ''' 
        ''' Creates or coerces objects of type "numeric". is.numeric 
        ''' is a more general test of an object being interpretable 
        ''' as numbers.
        ''' </summary>
        ''' <param name="length">A non-negative integer specifying 
        ''' the desired length. Double values will be coerced to integer: 
        ''' supplying an argument of length other than one is an error.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' numeric is identical to double. It creates a double-precision 
        ''' vector of the specified length with each element equal to 0.
        ''' </remarks>
        <ExportAPI("numeric")>
        Public Function numeric(Optional length As Integer = 0, Optional env As Environment = Nothing) As Object
            Return __empty(Of Double)(length, env)
        End Function

        Private Function __empty(Of T)(length As Integer, env As Environment) As Object
            If length < 0 Then
                Return Internal.debug.stop("invalid 'length' argument value, should be positive or zero!", env)
            End If

            Return New T(length - 1) {}
        End Function

        <ExportAPI("logical")>
        Public Function logical(Optional length As Integer = 0, Optional env As Environment = Nothing) As Object
            Return __empty(Of Boolean)(length, env)
        End Function

        <ExportAPI("character")>
        Public Function character(Optional length As Integer = 0, Optional env As Environment = Nothing) As Object
            Return __empty(Of String)(length, env)
        End Function

        <ExportAPI("integer")>
        Public Function ints(Optional length As Integer = 0, Optional env As Environment = Nothing) As Object
            Return __empty(Of Long)(length, env)
        End Function

        ''' <summary>
        ''' Create the raw bytes vector
        ''' </summary>
        ''' <param name="length"></param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' A vector of the raw bytes data if the given <paramref name="length"/>
        ''' greater than zero or a .net clr <see cref="MemoryStream"/> object that 
        ''' could be used for the file write connection if used the default <paramref name="length"/>
        ''' value: zero.
        ''' </returns>
        <ExportAPI("raw")>
        <RApiReturn(TypeCodes.raw)>
        Public Function raws(Optional length As Integer = 0, Optional env As Environment = Nothing) As Object
            If length <= 0 Then
                Return New MemoryStream
            Else
                Return __empty(Of Byte)(length, env)
            End If
        End Function

        ''' <summary>
        ''' ### Argument List of a Function
        ''' 
        ''' Displays the argument names and corresponding default values of a 
        ''' (non-primitive or primitive) function.
        ''' </summary>
        ''' <param name="name">
        ''' a function (a primitive or a closure, i.e., “non-primitive”). If name 
        ''' is a character string then the function with that name is found and 
        ''' used.
        ''' </param>
        ''' <returns>
        ''' + For a closure, a closure with identical formal argument list but an empty (NULL) body.
        ''' + For a primitive (function), a closure with the documented usage And NULL body. 
        '''    Note that some primitives do Not make use of named arguments And match by position 
        '''    rather than name.
        ''' + NULL in case of a non-function.
        ''' </returns>
        ''' <remarks>
        ''' This function is mainly used interactively to print the argument list 
        ''' of a function. For programming, consider using formals instead.
        ''' </remarks>
        <ExportAPI("args")>
        Public Function argumentList(name As Object, Optional env As Environment = Nothing) As Object
            If name Is Nothing Then
                Return Nothing
            End If

            If name.GetType.ImplementInterface(Of RFunction) Then
                Return DirectCast(name, RFunction).argumentList
            ElseIf TypeOf name Is MethodInfo Then
                Return DirectCast(name, MethodInfo).argumentList
            ElseIf TypeOf name Is String Then
                Dim func = env.FindFunction(name)?.value

                If func Is Nothing Then
                    Return Nothing
                Else
                    Return argumentList(func, env)
                End If
            Else
                Return Nothing
            End If
        End Function

        <Extension>
        Private Function argumentList(func As MethodInfo) As Object
            Return New SymbolTypeDefine(func)
        End Function

        <Extension>
        Private Function argumentList(func As RFunction) As Object
            If TypeOf func Is DeclareNewFunction Then
                Return New SymbolTypeDefine(DirectCast(func, DeclareNewFunction))
            ElseIf TypeOf func Is DeclareLambdaFunction Then
                Return New SymbolTypeDefine(DirectCast(func, DeclareLambdaFunction))
            ElseIf TypeOf func Is RMethodInfo Then
                Return New SymbolTypeDefine(DirectCast(func, RMethodInfo))
            Else
                Throw New NotImplementedException(func.GetType.FullName)
            End If
        End Function

        ''' <summary>
        ''' ##### commandArgs: Extract Command Line Arguments
        ''' 
        ''' Provides access to a copy of the command line arguments supplied when this R session was invoked.
        ''' </summary>
        ''' <param name="trailingOnly">
        ''' logical. Should only arguments after --args be returned?
        ''' </param>
        ''' <returns>
        ''' A character vector containing the name of the executable and the user-supplied command 
        ''' line arguments. The first element is the name of the executable by which R was invoked. 
        ''' The exact form of this element is platform dependent: it may be the fully qualified name,
        ''' or simply the last component (or basename) of the application, or for an embedded R it 
        ''' can be anything the programmer supplied.
        ''' 
        ''' if ``trailingOnly = TRUE``, a character vector Of those arguments (If any) supplied 
        ''' after ``--args``.
        ''' </returns>
        ''' <remarks>
        ''' These arguments are captured before the standard R command line processing takes place. 
        ''' This means that they are the unmodified values. This is especially useful with the --args 
        ''' command-line flag to R, as all of the command line after that flag is skipped.
        ''' </remarks>
        <ExportAPI("commandArgs")>
        Public Function commandArgs(Optional trailingOnly As Boolean = False, Optional parse_args As Boolean = False) As Object
            Dim args = App.CommandLine

            If parse_args Then
                Return New list With {
                    .slots = New Dictionary(Of String, Object) From {
                        {"command", args.Name},
                        {"commandLine", App.Command},
                        {"opts", New list With {.slots = args.ToArgumentVector.ToDictionary(Function(a) a.Name, Function(a) CObj(a.Value))}}
                    }
                }
            Else
                Return args.Tokens
            End If
        End Function

        ''' <summary>
        ''' ## Range of Values
        ''' 
        ''' range returns a vector containing the minimum and maximum 
        ''' of all the given arguments.
        ''' </summary>
        ''' <param name="x">
        ''' any numeric or character objects.
        ''' </param>
        ''' <param name="na_rm">
        ''' logical, indicating if NA's should be omitted.
        ''' </param>
        ''' <param name="finite">
        ''' logical, indicating if all non-finite elements should be 
        ''' omitted.
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' range is a generic function: methods can be defined for it 
        ''' directly or via the Summary group generic. For this to work 
        ''' properly, the arguments ... should be unnamed, and dispatch 
        ''' is on the first argument.
        ''' 
        ''' If na.rm Is False, NA And NaN values In any Of the arguments 
        ''' will cause NA values To be returned, otherwise NA values are 
        ''' ignored.
        ''' If finite Is True, the minimum And maximum Of all finite 
        ''' values Is computed, i.e., finite = True includes na.rm = True.
        ''' 
        ''' A special situation occurs When there Is no (after omission 
        ''' Of NAs) nonempty argument left, see min.
        ''' </remarks>
        ''' <example>
        ''' # [2] 1 6
        ''' range(1:6);
        ''' </example>
        <ExportAPI("range")>
        <RApiReturn(GetType(Double))>
        Public Function range(<RRawVectorArgument>
                              x As Object,
                              Optional na_rm As Object = False,
                              Optional finite As Boolean = False,
                              Optional env As Environment = Nothing) As Object

            If RType.TypeOf(x).mode.IsNumeric AndAlso RType.TypeOf(na_rm).mode.IsNumeric Then
                ' range(min, max) for in operator
                ' example as:   x in range(1, 5)
                ' the given vector x element in numeric range 1..5
                Dim minMax As Double() = CLRVector.asNumeric(base.append(x, na_rm, env))
                Dim rangeVal As New DoubleRange(minMax)

                Return rangeVal
            End If

            Dim data As Double() = CLRVector.asNumeric(x)

            If na_rm Then
                data = data.Where(Function(d) Not Double.IsNaN(d)).ToArray
            End If
            If finite Then
                data = data.Where(Function(d) Not d.IsNaNImaginary).ToArray
            End If

            If data.Length = 0 Then
                Return {Double.NaN, Double.NaN}
            ElseIf data.Length = 1 Then
                Return {data(0), data(0)}
            Else
                Dim min As Double = data.Min
                Dim max As Double = data.Max

                Return {min, max}
            End If
        End Function

        ''' <summary>
        ''' ### Factors
        ''' 
        ''' The function factor is used to encode a vector as a factor 
        ''' (the terms ‘category’ and ‘enumerated type’ are also used 
        ''' for factors).
        ''' </summary>
        ''' <param name="x">
        ''' a vector of data, usually taking a small number of distinct 
        ''' values.
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("factor")>
        Public Function factor(x As String()) As Integer()
            Dim uniqIndex As Index(Of String) = x.Distinct.Indexing
            Dim i As Integer() = x _
                .Select(Function(factorStr) uniqIndex.IndexOf(factorStr) + 1) _
                .ToArray

            Return i
        End Function

        ''' <summary>
        ''' ### Sequence Generation
        ''' 
        ''' Generate regular sequences. seq is a standard generic with a default method. 
        ''' seq.int is a primitive which can be much faster but has a few restrictions. 
        ''' seq_along and seq_len are very fast primitives for two common cases.
        ''' </summary>
        ''' <param name="from">the starting And (maximal) End values Of the sequence. 
        ''' Of length 1 unless just from Is supplied As an unnamed argument.</param>
        ''' <param name="[to]">the starting And (maximal) End values Of the sequence. 
        ''' Of length 1 unless just from Is supplied As an unnamed argument.</param>
        ''' <param name="by">number: increment of the sequence.</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Numerical inputs should all be finite (that is, not infinite, NaN or NA).
        ''' The interpretation Of the unnamed arguments Of seq And seq.int Is Not standard, 
        ''' And it Is recommended always To name the arguments When programming.
        ''' 
        ''' seq Is generic, And only the default method Is described here. Note that it 
        ''' dispatches on the class of the first argument irrespective of argument names. 
        ''' This can have unintended consequences if it Is called with just one argument 
        ''' intending this to be taken as along.with it Is much better to use seq_along 
        ''' in that case.
        ''' 
        ''' seq.int Is an internal generic which dispatches on methods for "seq" based on 
        ''' the class of the first supplied argument (before argument matching).
        ''' 
        ''' Typical usages are
        ''' 
        ''' + seq(from, to)
        ''' + seq(from, to, by= )
        ''' + seq(from, to, length.out= )
        ''' + seq(along.with= )
        ''' + seq(from)
        ''' + seq(length.out= )
        ''' 
        ''' The first form generates the sequence from, from+/-1, ..., To (identical To 
        ''' from:to).
        '''
        ''' The second form generates from, from+by, ..., up To the sequence value less 
        ''' than Or equal To To. Specifying To - from And by Of opposite signs Is an Error. 
        ''' Note that the computed final value can go just beyond To To allow For rounding 
        ''' Error, but Is truncated To To. ('Just beyond’ is by up to 1e-10 times
        ''' abs(from - to).)
        '''
        ''' The third generates a sequence Of length.out equally spaced values from from To 
        ''' To. (length.out Is usually abbreviated To length Or len, And seq_len Is much 
        ''' faster.)
        '''
        ''' The fourth form generates the Integer sequence 1, 2, ..., length(along.With). 
        ''' (along.With Is usually abbreviated To along, And seq_along Is much faster.)
        '''
        ''' The fifth form generates the sequence 1, 2, ..., length(from) (As If argument 
        ''' along.With had been specified), unless the argument Is numeric Of length 1 When 
        ''' it Is interpreted As 1:from (even for seq(0) for compatibility with S). Using 
        ''' either seq_along Or seq_len Is much preferred (unless strict S compatibility 
        ''' Is essential).
        '''
        ''' The final form generates the Integer sequence 1, 2, ..., length.out unless 
        ''' length.out = 0, When it generates Integer(0).
        '''
        ''' Very small sequences (With from - To Of the order Of 10^{-14} times the larger 
        ''' Of the ends) will Return from.
        '''
        ''' For seq(only), up To two Of from, To And by can be supplied As complex values 
        ''' provided length.out Or along.With Is specified. More generally, the Default 
        ''' method Of seq will handle classed objects With methods For the Math, Ops 
        ''' And Summary group generics.
        '''
        ''' seq.int, seq_along And seq_len are primitive.
        ''' </remarks>
        ''' <example>
        ''' seq(1, 5);
        ''' # is equals to
        ''' 1:5;
        ''' 
        ''' seq(1, 5, by = 0.1);
        ''' # is equals to
        ''' 1:5 step 0.1;
        ''' </example>
        <ExportAPI("seq")>
        Public Function seq(from As Object, [to] As Object,
                            Optional by As Double = Double.NaN,
                            Optional length_out As Integer = -1,
                            Optional env As Environment = Nothing) As Object

            Dim t1 As RType = RType.TypeOf(from)
            Dim t2 As RType = RType.TypeOf([to])

            If t1.mode.IsNumeric AndAlso t2.mode.IsNumeric Then
                Dim from_d As Double = CDbl(from)
                Dim to_d As Double = CDbl([to])

                If length_out > 0 AndAlso by.IsNaNImaginary Then
                    by = ((to_d - from_d) / (length_out - 1))
                End If
                If by.IsNaNImaginary Then
                    by = 1
                End If

                Return Microsoft.VisualBasic.Math _
                    .seq(from_d, to_d, by) _
                    .ToArray
            ElseIf t1.mode = TypeCodes.string AndAlso t2.mode = TypeCodes.string Then
                ' length_out is not working when make
                ' ascii char sequence
                Dim ascFrom As Double = Asc(any.ToString(from).First)
                Dim ascTo As Double = Asc(any.ToString([to]).First)
                Dim ascii As Integer() = Microsoft.VisualBasic.Math _
                    .seq(ascFrom, ascTo, by) _
                    .Select(Function(d) CInt(d)) _
                    .ToArray
                Dim chrs As Char() = (From i As Integer
                                      In ascii
                                      Select ChrW(i)).ToArray

                Return chrs
            Else
                Return Message.InCompatibleType(GetType(Double), from.GetType, env)
            End If
        End Function

        ''' <summary>
        ''' ### System Date and Time
        ''' </summary>
        ''' <param name="str">
        ''' this parameter also can accept the unix timestamp.
        ''' </param>
        ''' <param name="format">
        ''' specific the date string parser format, example as ``yyyyMMdd`` will be 
        ''' used for parse the given string ``20220101`` as ``#2022-01-01#``.
        ''' </param>
        ''' <returns>
        ''' Returns a character string of the current system date and time.
        ''' </returns>
        ''' <remarks>
        ''' the given number string will not be treated as unix timestamp
        ''' if the format string has been specific.
        ''' </remarks>
        <ExportAPI("date")>
        <RApiReturn(GetType(Date))>
        Public Function [date](Optional str As String() = Nothing,
                               Optional format As String = Nothing,
                               Optional safe As Boolean = True) As Object

            If str.IsNullOrEmpty Then
                Return DateTime.Now
            Else
                Dim has_format = Not format.StringEmpty(, True)

                Return str _
                    .Select(Function(s)
                                If s.IsSimpleNumber AndAlso Not has_format Then
                                    Return DateTimeHelper.FromUnixTimeStamp(Val(s))
                                ElseIf has_format Then
                                    Dim pull As Date = Nothing
                                    Dim check = DateTime.TryParseExact(s, format,
                                                                       Globalization.CultureInfo.InvariantCulture,
                                                                       Globalization.DateTimeStyles.None, pull)

                                    If Not check Then
                                        Dim msg = $"string '{s}' was not recognized as a valid DateTime."

                                        If safe Then
                                            pull = Nothing
                                        Else
                                            Throw New InvalidDataException(msg)
                                        End If

                                        Call msg.Warning
                                    End If

                                    Return pull
                                Else
                                    Return Date.Parse(s)
                                End If
                            End Function) _
                    .ToArray
            End If
        End Function

        <ExportAPI("year")>
        <RApiReturn(GetType(Integer))>
        Public Function year(Optional dates As Date() = Nothing) As Object
            If dates.IsNullOrEmpty Then
                Return DateTime.Now.Year
            Else
                Return dates.Select(Function(d) d.Year).ToArray
            End If
        End Function

        ''' <summary>
        ''' ### Combine Values into a Vector or List
        ''' 
        ''' This is a generic function which combines its arguments.
        ''' The Default method combines its arguments To form a vector. 
        ''' All arguments are coerced To a common type which Is the 
        ''' type Of the returned value, And all attributes except 
        ''' names are removed.
        ''' </summary>
        ''' <param name="values">objects to be concatenated.</param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' NULL or an expression or a vector of an appropriate mode. 
        ''' (With no arguments the value is NULL.)
        ''' </returns>
        ''' <example>
        ''' # for the vector literal, use the syntax of [] literal is better:
        ''' 
        ''' # use c() function
        ''' c(1,2,3,4,5,6);
        ''' # is equals to the vector literal syntax
        ''' [1 2 3 4 5 6];
        ''' 
        ''' # create string vector
        ''' c("a","b","c","d");
        ''' 
        ''' # create logical vector
        ''' c(TRUE, TRUE, FALSE);
        ''' </example>
        <ExportAPI("c")>
        Public Function c(<RListObjectArgument> values As Object, Optional env As Environment = Nothing) As Object
            Dim list As New List(Of Object)
            Dim val As Object

            For Each par As InvokeParameter In DirectCast(values, InvokeParameter())
                If TypeOf par.value Is ValueAssignExpression Then
                    val = DirectCast(par.value, ValueAssignExpression).value.Evaluate(env)
                Else
                    val = par.value.Evaluate(env)
                End If

                If Program.isException(val) Then
                    Return val
                End If

                For Each x As Object In REnv.asVector(Of Object)(val)
                    Call list.Add(x)
                Next
            Next

            Return REnv.TryCastGenericArray(list.ToArray, env)
        End Function

        ''' <summary>
        ''' ### Conditional Element Selection
        ''' 
        ''' ifelse returns a value with the same shape as test which is filled with 
        ''' elements selected from either yes or no depending on whether the 
        ''' element of test is ``TRUE`` or ``FALSE``.
        ''' </summary>
        ''' <param name="test">an object which can be coerced to logical mode.</param>
        ''' <param name="yes">return values for true elements of test.</param>
        ''' <param name="no">return values for false elements of test.</param>
        ''' <returns>
        ''' A vector of the same length and attributes (including dimensions and "class") 
        ''' as test and data values from the values of yes or no. The mode of the answer 
        ''' will be coerced from logical to accommodate first any values taken from yes 
        ''' and then any values taken from no.
        ''' </returns>
        ''' <remarks>
        ''' If yes or no are too short, their elements are recycled. yes will 
        ''' be evaluated if and only if any element of test is true, and 
        ''' analogously for no.
        ''' 
        ''' Missing values In test give missing values In the result.
        ''' </remarks>
        <ExportAPI("ifelse")>
        Public Function ifelse(<RRawVectorArgument> <RLazyExpression> test As Object,
                               <RRawVectorArgument> <RLazyExpression> yes As Object,
                               <RRawVectorArgument> <RLazyExpression> no As Object,
                               Optional env As Environment = Nothing) As Object

            Dim testVals As Object = Nothing

            If TypeOf test Is Expression Then
                testVals = DirectCast(test, Expression).Evaluate(env)

                If Program.isException(testVals) Then
                    Return testVals
                Else
                    testVals = CLRVector.asLogical(testVals)
                End If
            Else
                testVals = CLRVector.asLogical(test)
            End If

            If DirectCast(testVals, Boolean()).Length = 0 Then
                Return {}
            ElseIf DirectCast(testVals, Boolean()).Length = 1 Then
                Return ifelseScalar(DirectCast(testVals, Boolean())(Scan0), yes, no, env)
            Else
                Return ifelseVector(DirectCast(testVals, Boolean()), yes, no, env)
            End If
        End Function

        Private Function ifelseVector(test As Boolean(), yes As Object, no As Object, env As Environment) As Object
            Dim result As New List(Of Object)

            If TypeOf yes Is Expression Then
                yes = DirectCast(yes, Expression).Evaluate(env)

                If Program.isException(yes) Then
                    Return yes
                Else
                    yes = REnv.asVector(Of Object)(yes)
                End If
            End If
            If TypeOf no Is Expression Then
                no = DirectCast(no, Expression).Evaluate(env)

                If Program.isException(no) Then
                    Return no
                Else
                    no = REnv.asVector(Of Object)(no)
                End If
            End If

            Dim getYes As Func(Of Integer, Object) = New GetVectorElement(yes, GetType(Object)).Getter
            Dim getNo As Func(Of Integer, Object) = New GetVectorElement(no, GetType(Object)).Getter

            For i As Integer = 0 To test.Length - 1
                If test(i) Then
                    result.Add(getYes(i))
                Else
                    result.Add(getNo(i))
                End If
            Next

            Return REnv.TryCastGenericArray(result, env)
        End Function

        Private Function ifelseScalar(flag As Boolean, yes As Expression, no As Expression, env As Environment) As Object
            If flag Then
                If TypeOf yes Is Expression Then
                    Return DirectCast(yes, Expression).Evaluate(env)
                Else
                    Return yes
                End If
            Else
                If TypeOf no Is Expression Then
                    Return DirectCast(no, Expression).Evaluate(env)
                Else
                    Return no
                End If
            End If
        End Function

        ''' <summary>
        ''' ### Dimensions of an Object
        ''' 
        ''' Retrieve or set the dimension of an object.
        ''' </summary>
        ''' <param name="x">an R Object, For example a matrix, array Or data frame.</param>
        ''' <returns>
        ''' For the default method, either NULL or a numeric vector, which is 
        ''' coerced to integer (by truncation).
        ''' 
        ''' For an array (and hence in particular, for a matrix) dim retrieves 
        ''' the dim attribute of the object. It is ``NULL`` or a vector of mode 
        ''' integer.
        ''' 
        ''' The replacement method changes the "dim" attribute (provided the New 
        ''' value Is compatible) And removes any "dimnames" And "names" 
        ''' attributes.
        ''' </returns>
        ''' <remarks>
        ''' The functions dim and ``dim&lt;-`` are internal generic primitive functions.
        ''' Dim has a method For ``data.frames``, which returns the lengths Of the 
        ''' ``row.names`` attribute Of x And Of x (As the numbers Of rows And columns 
        ''' respectively).
        ''' </remarks>
        <ExportAPI("dim")>
        Public Function Robj_dimension(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return Nothing
            ElseIf TypeOf x Is dataframe Then
                With DirectCast(x, dataframe)
                    Return { .nrows, .ncols}
                End With
            ElseIf TypeOf x Is vector Then
                Return DirectCast(x, vector).length
            ElseIf TypeOf x Is Array Then
                Return DirectCast(x, Array).Length
            ElseIf TypeOf x Is Image OrElse TypeOf x Is Bitmap Then
                Dim img As Image = CType(x, Image)
                Dim dims As Integer() = New Integer() {img.Width, img.Height}

                Return dims
            ElseIf TypeOf x Is BitmapReader Then
                With DirectCast(x, BitmapReader)
                    Return New Integer() { .ImageWidth, .ImageHeight}
                End With
            ElseIf Internal.generic.exists("dim") Then
                Dim dim_func = generic.getGenericCallable(x, x.GetType, "dim", env)

                If Not dim_func Like GetType(Message) Then
                    Return dim_func.TryCast(Of GenericFunction)()(x, Internal.Object.list.empty, env)
                Else
                    Return 1
                End If
            Else
                Return 1
            End If
        End Function

        ''' <summary>
        ''' ### Replicate Elements of Vectors and Lists
        ''' 
        ''' rep replicates the values in x. It is a generic function, 
        ''' and the (internal) default method is described here.
        ''' </summary>
        ''' <param name="x">
        ''' a vector (of any mode including a list) or a factor or (for rep only) 
        ''' a POSIXct or POSIXlt or Date object; or an S4 object containing such 
        ''' an object.
        ''' </param>
        ''' <param name="each">
        ''' non-negative integer. Each element of x is repeated each times. Other 
        ''' inputs will be coerced to an integer or double vector and the first 
        ''' element taken. Treated as 1 if NA or invalid.
        ''' </param>
        ''' <param name="times">an integer-valued vector giving the (non-negative) 
        ''' number of times to repeat each element if of length length(x), or to 
        ''' repeat the whole vector if of length 1. Negative or NA values are an 
        ''' error. A double vector is accepted, other inputs being coerced to an 
        ''' integer or double vector.</param>
        ''' <returns></returns>
        <ExportAPI("rep")>
        Public Function rep(<RRawVectorArgument>
                            x As Object,
                            Optional times As Integer? = Nothing,
                            Optional [each] As Integer? = Nothing,
                            Optional env As Environment = Nothing) As Object
#Disable Warning
            Dim out As New List(Of Object)
            Dim vx = REnv.asVector(Of Object)(x).AsObjectEnumerator.ToArray
#Enable Warning
            If Not times Is Nothing Then
                For i As Integer = 1 To times
                    Call out.AddRange(vx)
                Next
            ElseIf Not [each] Is Nothing Then
                For Each item As Object In vx
                    Call out.AddRange(Repeats(item, times:=[each]))
                Next
            Else
                Return Internal.debug.stop("the repeats `times` or `each` element repeats times parameter must be specific!", env)
            End If

            Return REnv.TryCastGenericArray(out.ToArray, env)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="times">an integer-valued vector giving the (non-negative) 
        ''' number of times to repeat each element if of length length(x), or to
        ''' repeat the whole vector if of length 1. Negative or NA values are an 
        ''' error. A double vector is accepted, other inputs being coerced to an
        ''' integer or double vector.</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("rep.int")>
        Public Function rep_int(<RRawVectorArgument>
                                x As Object,
                                times As Integer,
                                Optional env As Environment = Nothing) As Object

            Return rep(x, times, , env)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="length_out">non-negative integer. The desired 
        ''' length of the output vector. Other inputs will be coerced to 
        ''' a double vector and the first element taken. Ignored if NA 
        ''' or invalid.</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("rep_len")>
        Public Function rep_len(<RRawVectorArgument>
                                x As Object,
                                length_out As Integer,
                                Optional env As Environment = Nothing) As Object

            Dim out As New List(Of Object)
            Dim vx = REnv.asVector(Of Object)(x)

            For i As Integer = 1 To Integer.MaxValue
                out.AddRange(vx.AsObjectEnumerator)

                If out.Count >= length_out Then
                    Exit For
                End If
            Next

            Return REnv.TryCastGenericArray(out.Take(length_out).ToArray, env)
        End Function

        ''' <summary>
        ''' ### Combine R Objects by Rows or Columns
        ''' 
        ''' Take a sequence of vector, matrix or data-frame arguments and combine 
        ''' by columns or rows, respectively. These are generic functions with 
        ''' methods for other R classes.
        ''' </summary>
        ''' <param name="d">should be a dataframe object</param>
        ''' <param name="row">should be another dataframe object, or a vector 
        ''' for combine a row or a tuple list for combine as a row.</param>
        ''' <param name="env"></param>
        ''' <param name="safe">
        ''' Merge the dataframe safely?
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("rbind")>
        <RApiReturn(GetType(dataframe))>
        Public Function rbind(d As dataframe, <RRawVectorArgument> row As Object,
                              Optional safe As Boolean = False,
                              Optional env As Environment = Nothing) As Object

            If d Is Nothing OrElse d.empty Then
                ' deal with the situation when
                ' target is null or empty
                If row Is Nothing OrElse TypeOf row Is dataframe Then
                    Return row
                ElseIf Not TypeOf row Is list Then
                    Dim tbl As New dataframe With {
                        .columns = New Dictionary(Of String, Array)
                    }
                    Dim v As Array = REnv.asVector(Of Object)(row)

                    ' create new dataframe from a vector
                    ' and this new dataframe just contains one element
                    For i As Integer = 0 To v.Length - 1
                        tbl.columns($"v{i + 1}") = {v.GetValue(i)}
                    Next

                    Return tbl
                Else
                    Dim rowList As list = DirectCast(row, list)
                    Dim tbl As New dataframe With {
                        .columns = New Dictionary(Of String, Array)
                    }
                    Dim vobj As Object

                    For Each name As String In rowList.getNames
                        vobj = rowList(name)
                        vobj = REnv.asVector(Of Object)(vobj)
                        tbl.columns(name) = REnv.UnsafeTryCastGenericArray(vobj)
                    Next

                    Return tbl
                End If
            ElseIf row Is Nothing Then
                Return d
            ElseIf TypeOf row Is dataframe Then
                If DirectCast(row, dataframe).empty Then
                    Return d
                End If

                ' row bind of two dataframe object
                If safe Then
                    Return rbindOp.safeRowBindDataFrame(d, row, env)
                Else
                    Return rbindOp.rowBindDataFrame(d, row, env)
                End If
            Else
                ' dataframe rbind with a vector row
                Dim v As Array = REnv.asVector(Of Object)(row)
                Dim colnames As String() = d.colnames
                Dim nrow As Integer = d.nrows

                If v.Length <> colnames.Length Then
                    ' the vector is append as a row of the dataframe, so the vector
                    ' length should be equals to the columns in the dataframe
                    Return Internal.debug.stop({
                        $"mis-matched column size between dataframe and row vector size!",
                        $"({v.Length}) columns: {CLRVector.asCharacter(v).GetJson}",
                        $"({colnames.Length}) columns: {d.colnames.GetJson}"
                    }, env)
                End If

                For i As Integer = 0 To colnames.Length - 1
                    ' for each col, append one element
                    ' so no nrow-1 for create vec
                    Dim vec = New Object(nrow) {}
                    Dim v2 = d.columns(colnames(i))
                    Dim getter = GetVectorElement.CreateAny(v2)

                    For j As Integer = 0 To vec.Length - 1
                        vec(j) = getter(j)
                    Next

                    vec(vec.Length - 1) = v(i)
                    d.columns(colnames(i)) = REnv.TryCastGenericArray(vec, env)
                Next

                Return d
            End If
        End Function

        ''' <summary>
        ''' ### Combine R Objects by Rows or Columns
        ''' 
        ''' Take a sequence of vector, matrix or data-frame arguments 
        ''' and combine by columns or rows, respectively. These are 
        ''' generic functions with methods for other R classes.
        ''' </summary>
        ''' <param name="x">
        ''' (generalized) vectors Or matrices. These can be given as 
        ''' named arguments. Other R objects may be coerced as appropriate, 
        ''' Or S4 methods may be used: see sections 'Details’ and 
        ''' ‘Value’. (For the "data.frame" method of cbind these can 
        ''' be further arguments to data.frame such as stringsAsFactors.)
        ''' </param>
        ''' <param name="strict">
        ''' this option affects the column merge speed. When this option is 
        ''' set to TRUE, then cbind function will check the row order at first
        ''' and then do row re-ordering for column appends.
        ''' When this option is set to FALSE, then the cbind function will
        ''' merge the dataframe directly without any row order checking. So
        ''' you must ensure that the row order is keeps the same when doing
        ''' non-strict cbind invoke.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("cbind")>
        <RApiReturn(GetType(dataframe))>
        Public Function cbind(<RListObjectArgument> x As list,
                              Optional strict As Boolean = True,
                              Optional [default] As Object = Nothing,
                              Optional env As Environment = Nothing) As Object

            Dim nameList As String() = x.getNames
            Dim d As dataframe = Nothing

            For Each nameKey As String In nameList
                Dim col As Object = x.getByName(nameKey)
                Dim result = d.cbind(nameKey, col, strict, [default], env)

                If result Like GetType(Message) Then
                    Return result.TryCast(Of Message)
                ElseIf result Is Nothing Then
                    ' just do nothing
                Else
                    d = result.TryCast(Of dataframe)
                End If
            Next

            Return d
        End Function

        ''' <summary>
        ''' dataframe to rows
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <ExportAPI("rowwise")>
        Public Function rowwise(x As dataframe,
                                Optional clr_dynamic As Boolean = False,
                                Optional env As Environment = Nothing) As Object
            If clr_dynamic Then
                Return x.rowViews.ToArray
            Else
                Return asList(x, New list(slot("byrow") = True), env)
            End If
        End Function

        ''' <summary>
        ''' ### matrix transpose
        ''' 
        ''' Given a matrix or data.frame x, t returns the transpose of x.
        ''' </summary>
        ''' <param name="x">a matrix Or data frame, typically.</param>
        ''' <returns>
        ''' A matrix, with dim and dimnames constructed appropriately from 
        ''' those of x, and other attributes except names copied across.
        ''' </returns>
        ''' <remarks>
        ''' This is a generic function for which methods can be written. 
        ''' The description here applies to the default and "data.frame" 
        ''' methods.
        ''' 
        ''' A data frame Is first coerced To a matrix: see as.matrix. 
        ''' When x Is a vector, it Is treated as a column, i.e., the 
        ''' result Is a 1-row matrix.
        ''' </remarks>
        <ExportAPI("t")>
        Public Function t(x As dataframe, Optional generic As Boolean = True, Optional env As Environment = Nothing) As Object
            Dim rownames As String() = x.getRowNames
            Dim colnames As String() = x.columns.Keys.ToArray
            Dim mat As Object()() = colnames _
                .Select(Function(k)
                            Return x.columns(k).AsObjectEnumerator(Of Object).ToArray
                        End Function) _
                .MatrixTranspose _
                .ToArray
            Dim d As New dataframe With {
                .rownames = colnames,
                .columns = New Dictionary(Of String, Array)
            }
            Dim colVector As Array
            Dim genericVal As Object

            For i As Integer = 0 To rownames.Length - 1
                colVector = mat(i)

                If generic Then
                    genericVal = REnv.TryCastGenericArray(colVector, env)

                    If TypeOf genericVal Is Message Then
                        Return genericVal
                    Else
                        colVector = genericVal
                    End If
                End If

                d.columns(rownames(i)) = colVector
            Next

            Return d
        End Function

        ''' <summary>
        ''' create an empty vector with specific count of null value filled
        ''' </summary>
        ''' <param name="size"></param>
        ''' <returns></returns>
        <ExportAPI("allocate")>
        Public Function allocate(size As Integer) As vector
            Dim a As Object() = New Object(size - 1) {}
            Dim v As New vector With {
                .data = a
            }

            Return v
        End Function

        ''' <summary>
        ''' ### Function to Create a Unit Object
        ''' 
        ''' get or set unit to a given vector. This function creates a unit object — a vector of unit values. 
        ''' A unit value is typically just a single numeric value with an associated unit.
        ''' </summary>
        ''' <param name="x">A numeric vector. For is.unit, any R object.</param>
        ''' <param name="units">A character vector specifying the units for the corresponding numeric values.</param>
        ''' <param name="data">
        ''' This argument is used to supply extra information for special unit types.
        ''' </param>
        ''' <returns>
        ''' a numeric vector data that tagged with the given specific unit value, or returns the 
        ''' units name of the given vector data if the <paramref name="units"/> parameter has 
        ''' not been assigned.
        ''' </returns>
        ''' <remarks>
        ''' Unit objects allow the user to specify locations and dimensions in a large number of different
        ''' coordinate systems. All drawing occurs relative to a viewport and the units specifies what
        ''' coordinate system to use within that viewport.
        ''' 
        ''' Possible units (coordinate systems) are:
        ''' 
        ''' "npc"
        ''' Normalised Parent Coordinates (the default). The origin of the viewport is (0, 0) and the 
        ''' viewport has a width and height of 1 unit. For example, (0.5, 0.5) is the centre of the 
        ''' viewport.
        ''' 
        ''' "cm"
        ''' Centimetres.
        ''' 
        ''' "inches"
        ''' Inches. 1 in = 2.54 cm.
        ''' 
        ''' "mm"
        ''' Millimetres. 10 mm = 1 cm.
        ''' 
        ''' "points"
        ''' Points. 72.27 pt = 1 in.
        ''' 
        ''' "picas"
        ''' Picas. 1 pc = 12 pt.
        ''' 
        ''' "bigpts"
        ''' Big Points. 72 bp = 1 in.
        ''' 
        ''' "dida"
        ''' Dida. 1157 dd = 1238 pt.
        ''' 
        ''' "cicero"
        ''' Cicero. 1 cc = 12 dd.
        ''' 
        ''' "scaledpts"
        ''' Scaled Points. 65536 sp = 1 pt.
        ''' 
        ''' "lines"
        ''' Lines of text. Locations and dimensions are in terms of multiples of the default text size of the
        ''' viewport (as specified by the viewport's fontsize and lineheight).
        ''' 
        ''' "char"
        ''' Multiples of nominal font height of the viewport (as specified by the viewport's fontsize).
        ''' 
        ''' "native"
        ''' Locations and dimensions are relative to the viewport's xscale and yscale.
        ''' 
        ''' "snpc"
        ''' Square Normalised Parent Coordinates. Same as Normalised Parent Coordinates, except gives the same
        ''' answer for horizontal and vertical locations/dimensions. It uses the lesser of npc-width and npc-height. 
        ''' This is useful for making things which are a proportion of the viewport, but have to be square 
        ''' (or have a fixed aspect ratio).
        ''' 
        ''' "strwidth"
        ''' Multiples of the width of the string specified in the data argument. The font size is determined by
        ''' the pointsize of the viewport.
        ''' 
        ''' "strheight"
        ''' Multiples of the height of the string specified in the data argument. The font size is determined by 
        ''' the pointsize of the viewport.
        ''' 
        ''' "grobwidth"
        ''' Multiples of the width of the grob specified in the data argument.
        ''' 
        ''' "grobheight"
        ''' Multiples of the height of the grob specified in the data argument.
        ''' 
        ''' A number of variations are also allowed for the most common units. For example, it is possible to 
        ''' use "in" or "inch" instead of "inches" and "centimetre" or "centimeter" instead of "cm".
        ''' 
        ''' A special units value of "null" is also allowed, but only makes sense when used in specifying widths of
        ''' columns or heights of rows in grid layouts (see grid.layout).
        ''' 
        ''' The data argument must be a list when the unit.length() is greater than 1. For example,
        ''' 
        ''' ```r
        ''' unit(rep(1, 3), c("npc", "strwidth", "inches"),
        ''' data = list(NULL, "my string", NULL))
        ''' ```
        ''' 
        ''' It is possible to subset unit objects in the normal way and to perform subassignment (see the examples),
        ''' but a special function unit.c is provided for combining unit objects.
        ''' 
        ''' Certain arithmetic and summary operations are defined for unit objects. In particular, it is possible 
        ''' to add and subtract unit objects (e.g., unit(1, "npc") - unit(1, "inches")), and to specify the minimum 
        ''' or maximum of a list of unit objects (e.g., min(unit(0.5, "npc"), unit(1, "inches"))).
        ''' 
        ''' There is a format method for units, which should respond to the arguments for the default format method, 
        ''' e.g., digits to control the number of significant digits printed for numeric values.
        ''' 
        ''' The is.unit() function is a convenience for checking whether x inherits from the "unit" class.
        ''' </remarks>
        <ExportAPI("unit")>
        Public Function unitOfT(<RRawVectorArgument> x As Object,
                                <RByRefValueAssign>
                                Optional units As Object = Nothing,
                                Optional data As Object = Nothing,
                                Optional env As Environment = Nothing) As Object

            If units Is Nothing Then
                If TypeOf x Is vector Then
                    Return DirectCast(x, vector).unit
                Else
                    Return Nothing
                End If
            Else
                If TypeOf units Is vbObject Then
                    units = DirectCast(units, vbObject).target
                End If
                If TypeOf units Is String Then
                    units = New unit With {.name = units}
                End If

                If Not x Is Nothing AndAlso Not TypeOf x Is vector Then
                    With asVector(Of Object)(x)
                        x = New vector(.DoCall(AddressOf MeasureRealElementType), .ByRef, env)
                    End With

                    Call env.AddMessage({
                        "value x is not a vector",
                        "it will be convert to vector automatically..."
                    }, MSG_TYPES.WRN)
                End If
            End If

            If x Is Nothing Then
                x = New vector With {.data = {}, .unit = units}
            Else
                DirectCast(x, vector).unit = units
            End If

            Return x
        End Function

        <ExportAPI("replace")>
        Public Function replace(x As Array, find As Object, [as] As Object) As Object
            Dim type As Type = x.GetType.GetElementType

            If type Is GetType(Object) Then
                type = Runtime.MeasureArrayElementType(x)
            End If

            find = Conversion.CTypeDynamic(find, type)
            [as] = Conversion.CTypeDynamic([as], type)

            Dim copy As Array = Array.CreateInstance(type, x.Length)
            Dim xi As Object

            For i As Integer = 0 To x.Length - 1
                xi = x.GetValue(i)

                If xi.Equals(find) Then
                    copy.SetValue([as], i)
                Else
                    copy.SetValue(xi, i)
                End If
            Next

            Return copy
        End Function

        ''' <summary>
        ''' # Change the Print Mode to Invisible
        ''' 
        ''' Return a (temporarily) invisible copy of an object.
        ''' </summary>
        ''' <param name="x">an arbitrary R object.</param>
        ''' <returns>
        ''' This function can be useful when it is desired to have functions 
        ''' return values which can be assigned, but which do not print when 
        ''' they are not assigned.
        ''' </returns>
        <ExportAPI("invisible")>
        Public Function __invisible(x As Object) As invisible
            Return New invisible With {.value = x}
        End Function

        ''' <summary>
        ''' A function to make the numeric value negative
        ''' </summary>
        ''' <param name="o"></param>
        ''' <returns></returns>
        <ExportAPI("neg")>
        Public Function neg(<RRawVectorArgument> o As Object) As Object
            If o Is Nothing Then
                Return Nothing
            Else
                Return CLRVector.asNumeric(o) _
                    .Select(Function(d) -CDbl(d)) _
                    .ToArray
            End If
        End Function

        <ExportAPI("join")>
        Public Function join_data(x As list, b As list, Optional env As Environment = Nothing) As Object
            Dim union As New list
            Dim joins = x.slots _
                .JoinIterates(b.slots) _
                .GroupBy(Function(a) a.Key) _
                .ToArray

            For Each tuple As IGrouping(Of String, KeyValuePair(Of String, Object)) In joins
                Dim vals As Object() = tuple.Values

                If vals.Length = 1 Then
                    Call union.add(tuple.Key, vals(0))
                Else
                    Dim append = base.append(vals(0), vals(1), env)

                    If TypeOf append Is Message Then
                        Return append
                    Else
                        Call union.add(tuple.Key, append)
                    End If
                End If
            Next

            Return union
        End Function

        ''' <summary>
        ''' ### Vector Merging
        ''' 
        ''' Add elements to a vector.
        ''' </summary>
        ''' <param name="x">the vector the values are to be appended to.</param>
        ''' <param name="values">to be included in the modified vector.</param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' A vector containing the values in x with the elements of values 
        ''' appended after the specified element of x.
        ''' </returns>
        ''' <remarks>
        ''' for append one tuple list into another tuple list, element value may be overrides 
        ''' if there is duplicated name between the two list. ``join`` function could be used 
        ''' for union the element value.
        ''' 
        ''' </remarks>
        ''' <example>
        ''' let a = list(a = 1, b = 2);
        ''' let b = list(a = 333, c = 5);
        ''' 
        ''' str(append(a,b));
        ''' # tuple key ``a`` in list a has been overrided by the 
        ''' # tuple key ``a`` from the list b
        ''' 
        ''' # List of 3
        ''' #  $ a : int 333
        ''' #  $ b : int 2
        ''' #  $ c : int 5
        ''' 
        ''' str(join(a, b));
        ''' # tuple value of ``a`` will be union in join function
        ''' 
        ''' # List of 3
        ''' #  $ a : int [1:2] 1 333
        ''' #  $ b : int 2
        ''' #  $ c : int 5
        ''' 
        ''' </example>
        <ExportAPI("append")>
        Public Function append(<RRawVectorArgument> x As Object,
                               <RRawVectorArgument> values As Object,
                               Optional env As Environment = Nothing) As Object

            If x Is Nothing Then
                Return values
            ElseIf values Is Nothing Then
                Return x
            End If

            If x.GetType.IsArray Then
                x = REnv.TryCastGenericArray(x, env)
            Else
                If RType.GetRSharpType(x.GetType).mode.IsPrimitive(includeComplexList:=False) Then
                    x = REnv.TryCastGenericArray({x}, env)
                End If
            End If
            If values.GetType.IsArray Then
                values = REnv.TryCastGenericArray(values, env)
            Else
                If RType.GetRSharpType(values.GetType).mode.IsPrimitive(includeComplexList:=False) Then
                    values = REnv.TryCastGenericArray({values}, env)
                End If
            End If

            If TypeOf x Is list Then
                Return DirectCast(x, list).appendOfList(values, env)
            ElseIf TypeOf x Is vector OrElse x.GetType.IsArray Then
                Dim vec As Array

                If TypeOf x Is vector Then
                    vec = DirectCast(x, vector).data
                Else
                    vec = x
                End If

                Return env.appendOfVector(vec, values)
            ElseIf x.GetType Is values.GetType Then
                Dim joinTwo As Array = Array.CreateInstance(x.GetType, 2)
                joinTwo.SetValue(x, Scan0)
                joinTwo.SetValue(values, 1)
                Return joinTwo
            ElseIf values.GetType.IsArray AndAlso x.GetType Is values.GetType.GetElementType Then
                Dim joinArray As Array = Array.CreateInstance(x.GetType, DirectCast(values, Array).Length + 1)
                joinArray.SetValue(x, 0)
                Array.ConstrainedCopy(values, Scan0, joinArray, 1, DirectCast(values, Array).Length)
                Return joinArray
            Else
                ' add method is also ok!
                Return env.objectAddInvoke(x, values)
            End If
        End Function

        <Extension>
        Private Function appendFinal(env As Environment, x As Object, values As Object) As Object
            Dim xType As Type = x.GetType

            If TypeOf values Is vector Then
                values = DirectCast(values, vector).data
            End If
            If values.GetType.IsArray Then
                If values.GetType.GetElementType Is xType Then
                    Dim vec As Array = Array.CreateInstance(xType, 1)
                    vec.SetValue(x, Scan0)
                    Return env.appendOfVector(vec, values)
                Else
                    Return Internal.debug.stop({"x didn't contains a Add method!", $"type of x: {xType.FullName}", $"type of values: {values.GetType.FullName}"}, env)
                End If
            Else
                If values.GetType Is xType Then
                    Dim array As Array = Array.CreateInstance(xType, 2)
                    array.SetValue(x, Scan0)
                    array.SetValue(values, 1)
                    Return New vector With {.data = array, .elementType = RType.GetRSharpType(xType)}
                Else
                    Return Internal.debug.stop({"x didn't contains a Add method!", $"type of x: {xType.FullName}", $"type of values: {values.GetType.FullName}"}, env)
                End If
            End If

            Return Internal.debug.stop({"x didn't contains a Add method!", $"type of x: {xType.FullName}", $"type of values: {values.GetType.FullName}"}, env)
        End Function

        ''' <summary>
        ''' implements ``append`` via the add method
        ''' </summary>
        ''' <param name="env"></param>
        ''' <param name="x"></param>
        ''' <param name="values"></param>
        ''' <returns></returns>
        <Extension>
        Private Function objectAddInvoke(env As Environment, x As Object, values As Object) As Object
            Dim xType As Type = x.GetType

            Static collectionCache As New Dictionary(Of Type, MethodInfo)
RE0:
            If collectionCache.ContainsKey(xType) Then
                If collectionCache(xType) Is Nothing Then
                    ' 之前已经被解析过了
                    ' 但是找不到Add方法
                    Return env.appendFinal(x, values)
                End If
            Else
                Dim handle = xType _
                    .GetMethods(PublicProperty) _
                    .Where(Function(f)
                               Return f.Name = "Add" AndAlso f.GetParameters.Length = 1
                           End Function) _
                    .FirstOrDefault

                SyncLock collectionCache
                    collectionCache(xType) = handle
                End SyncLock

                GoTo RE0
            End If

            Dim add As MethodInfo = collectionCache(x.GetType)
            Dim valueType As Type = add.GetParameters.First.ParameterType
            Dim valueData As Object = REnv.asVector(values, valueType, env)

            If Program.isException(valueData) Then
                Return valueData
            End If

            For Each item As Object In DirectCast(valueData, Array)
                Call add.Invoke(x, {item})
            Next

            Return x
        End Function

        <Extension>
        Private Function appendOfList(x As list, values As Object, env As Environment) As Object
            Dim listX As New list(x)

            If TypeOf values Is list Then
                For Each item In DirectCast(values, list).slots
                    listX.slots(item.Key) = item.Value
                Next
            Else
                Dim i As Integer = listX.length + 1

                For Each item As Object In REnv.asVector(Of Object)(values)
                    listX.slots($"X_{i}") = item
                    i += 1
                Next
            End If

            Return listX
        End Function

        <Extension>
        Private Function appendOfVector(env As Environment, vec As Array, values As Object) As Object
            Dim type As Type = vec.GetType.GetElementType
            Dim type2 As Type
            Dim vec2 As Array

            If TypeOf values Is vector Then
                type2 = DirectCast(values, vector).data.GetType.GetElementType
                vec2 = DirectCast(values, vector).data
            ElseIf values.GetType.IsArray Then
                type2 = values.GetType.GetElementType
                vec2 = values
            Else
                type2 = values.GetType
                vec2 = Array.CreateInstance(type2, 1)
                vec2.SetValue(values, 0)
            End If

            If Not type Is type2 Then
                type = GetType(Object)
            End If

            Dim union As Array = Array.CreateInstance(type, vec.Length + vec2.Length)

            For i As Integer = 0 To vec.Length - 1
                union.SetValue(Conversion.CTypeDynamic(vec.GetValue(i), type), i)
            Next
            For i As Integer = 0 To vec2.Length - 1
                union.SetValue(Conversion.CTypeDynamic(vec2.GetValue(i), type), vec.Length + i)
            Next

            Return New vector With {
                .data = union,
                .elementType = RType.GetRSharpType(type)
            }
        End Function

        ''' <summary>
        ''' #### Data Frames
        ''' 
        ''' The function ``data.frame()`` creates data frames, tightly coupled collections 
        ''' of variables which share many of the properties of matrices and of lists, 
        ''' used as the fundamental data structure by most of R's modeling software.
        ''' </summary>
        ''' <param name="columns">
        ''' these arguments are of either the form value or ``tag = value``. Component names 
        ''' are created based on the tag (if present) or the deparsed argument itself.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' A data frame, a matrix-like structure whose columns may be of differing types 
        ''' (``numeric``, ``logical``, ``factor`` and ``character`` and so on).
        '''
        ''' How the names Of the data frame are created Is complex, And the rest Of this 
        ''' paragraph Is only the basic story. If the arguments are all named And simple 
        ''' objects (Not lists, matrices Of data frames) Then the argument names give the 
        ''' column names. For an unnamed simple argument, a deparsed version Of the 
        ''' argument Is used As the name (With an enclosing ``I(...)`` removed). For a named 
        ''' ``matrix``/``list``/``dataframe`` argument With more than one named column, the names Of 
        ''' the columns are the name Of the argument followed by a dot And the column name 
        ''' inside the argument: If the argument Is unnamed, the argument's column names 
        ''' are used. For a named or unnamed matrix/list/data frame argument that contains 
        ''' a single column, the column name in the result is the column name in the 
        ''' argument. 
        ''' Finally, the names are adjusted to be unique and syntactically valid unless 
        ''' ``check.names = FALSE``.
        ''' </returns>
        <ExportAPI("data.frame")>
        <RApiReturn(GetType(dataframe))>
        Public Function Rdataframe(<RListObjectArgument> columns As Object, Optional env As Environment = Nothing) As Object
            ' data.frame(a = 1, b = ["g","h","eee"], c = T)
            Dim parameters As InvokeParameter() = columns
            Dim popEvals As IEnumerable(Of NamedValue(Of Object)) = parameters _
                .SeqIterator _
                .Select(Function(a)
                            Return columnVector(a, env)
                        End Function)
            Dim values As New List(Of NamedValue(Of Object))

            For Each item As NamedValue(Of Object) In popEvals
                If Program.isException(item.Value) Then
                    Return item.Value
                Else
                    values.Add(item)
                End If
            Next

            Return values.RDataframe(env)
        End Function

        ''' <summary>
        ''' Utils function for check for dataframe object construction
        ''' </summary>
        ''' <param name="columns"></param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' false means the data has some column fields that do not matched 
        ''' with others to create a valid dataframe object
        ''' </returns>
        <ExportAPI("check.dimensions_agree")>
        <RApiReturn(GetType(Boolean))>
        Public Function checkDimensionsAgree(<RListObjectArgument> columns As Object, Optional env As Environment = Nothing) As Object
            Dim parameters As InvokeParameter() = columns
            Dim popEvals As IEnumerable(Of NamedValue(Of Object)) = parameters _
                .SeqIterator _
                .Select(Function(a)
                            Return columnVector(a, env)
                        End Function)
            Dim values As New List(Of NamedValue(Of Array))

            For Each item As NamedValue(Of Object) In popEvals
                If Program.isException(item.Value) Then
                    Return item.Value
                Else
                    values.Add(New NamedValue(Of Array)(item.Name, REnv.asVector(Of Object)(item.Value)))
                End If
            Next

            If values.Count = 0 Then
                Return True
            End If

            Dim max As Integer = values.Select(Function(v) v.Value.Length).Max
            Dim noneAgree As String() = values _
                .Where(Function(col)
                           Return col.Value.Length <> max AndAlso col.Value.Length <> 1
                       End Function) _
                .Select(Function(col) col.Name) _
                .ToArray

            Return noneAgree.IsNullOrEmpty
        End Function

        Private Function columnVector(a As SeqValue(Of InvokeParameter), envir As Environment) As NamedValue(Of Object)
            Dim name$

            If a.value.haveSymbolName(hasObjectList:=True) Then
                name = a.value.name
            Else
                name = "X" & (a.i + 1)
            End If

            Return New NamedValue(Of Object) With {
                .Name = name,
                .Value = a.value.Evaluate(envir)
            }
        End Function

        ''' <summary>
        ''' The Number of Rows/Columns of an Array
        ''' 
        ''' nrow and ncol return the number of rows or columns present in x.
        ''' </summary>
        ''' <param name="x">a vector, array, data frame, or NULL.</param>
        ''' <param name="env"></param>
        ''' <returns>an integer of length 1 or NULL, the latter only for ncol and nrow.</returns>
        <ExportAPI("nrow")>
        <RApiReturn(GetType(Integer))>
        Public Function nrow(x As Object, Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return 0
            ElseIf x.GetType Is GetType(dataframe) Then
                Return DirectCast(x, dataframe).nrows
            Else
                Return Internal.debug.stop(RType.GetRSharpType(x.GetType).ToString & " is not a dataframe!", env)
            End If
        End Function

        ''' <summary>
        ''' The Number of Rows/Columns of an Array
        ''' 
        ''' nrow and ncol return the number of rows or columns present in x.
        ''' </summary>
        ''' <param name="x">a vector, array, data frame, or NULL.</param>
        ''' <param name="env"></param>
        ''' <returns>an integer of length 1 or NULL, the latter only for ncol and nrow.</returns>
        <ExportAPI("ncol")>
        <RApiReturn(GetType(Integer))>
        Public Function ncol(x As Object, Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return 0
            ElseIf x.GetType Is GetType(dataframe) Then
                Return DirectCast(x, dataframe).ncols
            Else
                Return Internal.debug.stop(RType.GetRSharpType(x.GetType).ToString & " is not a dataframe!", env)
            End If
        End Function

        ''' <summary>
        ''' # Lists – Generic and Dotted Pairs
        ''' 
        ''' Functions to construct, coerce and check for both kinds of ``R#`` lists.
        ''' </summary>
        ''' <param name="slots">objects, possibly named.</param>
        ''' <param name="envir"></param>
        ''' <returns>
        ''' 
        ''' </returns>
        <ExportAPI("list")>
        <RApiReturn(GetType(list))>
        Public Function Rlist(<RListObjectArgument, RRawVectorArgument>
                              slots As Object,
                              Optional envir As Environment = Nothing) As Object

            Dim list As New Dictionary(Of String, Object)
            Dim slot As InvokeParameter
            Dim key As String
            Dim value As Object
            Dim parameters As InvokeParameter() = slots
            Dim uniqKeys As New Dictionary(Of String, Integer)

            If parameters Is Nothing Then
                parameters = {}
            End If

            For i As Integer = 0 To parameters.Length - 1
                slot = parameters(i)

                If slot.haveSymbolName(hasObjectList:=True) Then
                    ' 不支持tuple
                    key = slot.name
                    value = slot.Evaluate(envir)
                Else
                    key = i + 1
                    key = $"[[{key}]]"
                    value = slot.Evaluate(envir)
                End If

                If Program.isException(value) Then
                    Return value
                Else
                    If uniqKeys.ContainsKey(key) Then
                        uniqKeys(key) += 1
                        key = $"{key}_{uniqKeys(key)}"
                    Else
                        uniqKeys(key) = 0
                    End If

                    Call list.Add(key, value)
                End If
            Next

            If uniqKeys.Where(Function(k) k.Value > 0).Any Then
                Dim duplicatedKeys As String() = uniqKeys _
                    .Where(Function(k) k.Value > 0) _
                    .Select(Function(k) k.Key) _
                    .ToArray

                Call envir.AddMessage({
                    $"There are {duplicatedKeys.Length} was found!",
                    $"duplicates: {duplicatedKeys.JoinBy(", ")}"
                }, MSG_TYPES.WRN)
            End If

            Return New list With {.slots = list}
        End Function

        ''' <summary>
        ''' # Object Summaries
        ''' 
        ''' summary is a generic function used to produce result summaries of 
        ''' the results of various model fitting functions. The function 
        ''' invokes particular methods which depend on the class of the first 
        ''' argument.
        ''' </summary>
        ''' <param name="object">
        ''' an object for which a summary is desired.
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("summary")>
        Public Function summary(<RRawVectorArgument> [object] As Object,
                                <RRawVectorArgument, RListObjectArgument> args As Object,
                                Optional env As Environment = Nothing) As Object

            Dim argumentsVal As Object = base.Rlist(args, env)

            If TypeOf [object] Is vector Then
                [object] = REnv.TryCastGenericArray(DirectCast([object], vector).data, env)
            ElseIf TypeOf [object] Is list AndAlso DirectCast([object], list).hasName("summary") Then
                Return base.cat(DirectCast([object], list).getByName("summary"), env:=env)
            End If

            If Program.isException(argumentsVal) Then
                Return argumentsVal
            ElseIf DataFramework.IsNumericCollection([object].GetType) Then
                Dim vals As Double() = CLRVector.asNumeric([object])
                Dim min As Double = vals.Min
                Dim q = vals.OrderBy(Function(xi) xi).ToArray
                Dim q1 As Integer() = getPosition(vals.Length, 1 / 4)
                Dim median As Integer() = getPosition(vals.Length, 1 / 2)
                Dim q3 As Integer() = getPosition(vals.Length, 3 / 4)
                Dim max As Double = vals.Max

                Return New list With {
                    .slots = New Dictionary(Of String, Object) From {
                        {"Min.", min},
                        {"1st Qu.", ValueAt(vals, q1)},
                        {"Median", ValueAt(vals, median)},
                        {"Mean", vals.Average},
                        {"3rd Qu.", ValueAt(vals, q3)},
                        {"Max.", max}
                    }
                }
            Else
                ' summary is similar to str or print function
                ' but summary just returns simple data summary information
                ' and str function returns the data structure information
                ' about the given dataset object.
                ' the print function is print the data details
                Return DirectCast(argumentsVal, list).invokeGeneric([object], env, funcName:="summary")
            End If
        End Function

        Private Function ValueAt(x As Double(), i As Integer()) As Double
            If i.Length = 1 Then
                Return x(i(Scan0))
            Else
                Return i.Select(Function(idx) x(idx)).Average
            End If
        End Function

        Private Function getPosition(len As Integer, q As Double) As Integer()
            Dim offset = len * q
            Dim m = len Mod offset

            If m < 0.00001 Then
                Return New Integer() {CInt(offset)}
            Else
                Return New Integer() {
                    std.Floor(offset) - 1,
                    std.Floor(offset) + 1
                }
            End If
        End Function

        ''' <summary>
        ''' This function returns a logical value to determine that the given object is empty or not?
        ''' </summary>
        ''' <param name="x">an object for which test for empty is desired.</param>
        ''' <returns></returns>
        <ExportAPI("is.empty")>
        Public Function isEmpty(<RRawVectorArgument> x As Object) As Boolean
            ' 20191224
            ' 这个函数虽然申明为object类型的返回值，
            ' 实际上为了统一api的申明，在这里返回的都是一个逻辑值
            If x Is Nothing Then
                Return True
            End If

            Dim type As Type = x.GetType

            If type Is GetType(dataframe) Then
                Return DirectCast(x, dataframe).empty
            End If
            If type Is GetType(String) Then
                Return DirectCast(x, String).StringEmpty(False)
            ElseIf type Is GetType(String()) Then
                With DirectCast(x, String())
                    If .Length > 1 Then
                        Return False
                    ElseIf .Length = 0 OrElse .First.StringEmpty(False) Then
                        Return True
                    Else
                        Return False
                    End If
                End With
            ElseIf type.IsArray Then
                Return isEmptyArray(x)
            ElseIf type.ImplementInterface(GetType(RIndex)) Then
                Return DirectCast(x, RIndex).length = 0
            Else
                Return False
            End If
        End Function

        Private Function isEmptyArray(x As Object) As Boolean
            With DirectCast(x, Array)
                If .Length = 0 Then
                    Return True
                ElseIf .Length = 1 Then
                    Dim first As Object = .GetValue(Scan0)

                    If first Is Nothing Then
                        Return True
                    ElseIf first.GetType Is GetType(String) Then
                        Return DirectCast(first, String).StringEmpty(False)
                    Else
                        Return False
                    End If
                Else
                    Return False
                End If
            End With
        End Function

        ''' <summary>
        ''' # The Null Object
        ''' 
        ''' ``NULL`` represents the null object in R: it is a reserved word. 
        ''' ``NULL`` is often returned by expressions and functions whose 
        ''' value is undefined.
        ''' 
        ''' ``is.null`` is a primitive function.
        ''' </summary>
        ''' <param name="x">an object to be tested or coerced.</param>
        ''' <param name="els">
        ''' does function test for the elements inside x, not the given object x?
        ''' </param>
        ''' <returns>is.null returns TRUE if its argument's value is NULL and FALSE otherwise.</returns>
        ''' <remarks>
        ''' ``NULL`` can be indexed (see Extract) in just about any syntactically 
        ''' legal way: whether it makes sense or not, the result is always ``NULL``. 
        ''' Objects with value ``NULL`` can be changed by replacement operators 
        ''' and will be coerced to the type of the right-hand side.
        ''' 
        ''' ``NULL`` Is also used as the empty pairlist: see the examples. Because 
        ''' pairlists are often promoted To lists, you may encounter ``NULL`` being 
        ''' promoted To an empty list.
        ''' 
        ''' Objects with value ``NULL`` cannot have attributes as there Is only 
        ''' one null object attempts to assign them are either an error (attr) 
        ''' Or promote the object to an empty list with attribute(s) (attributes 
        ''' And structure).
        ''' </remarks>
        <ExportAPI("is.null")>
        <RApiReturn(TypeCodes.boolean)>
        Public Function isNull(<RRawVectorArgument> x As Object,
                               Optional els As Boolean = False,
                               Optional env As Environment = Nothing) As Object

            If x Is Nothing Then
                Return True
            End If

            If els Then
                Return ObjectSet.GetObjectSet(x, env) _
                    .Select(Function(o) o Is Nothing) _
                    .ToArray
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' ### Coerce to a Data Frame
        ''' 
        ''' Functions to check if an object is a data frame, or coerce it 
        ''' if possible.
        ''' </summary>
        ''' <param name="x">any R object.</param>
        ''' <returns>
        ''' is.data.frame returns TRUE if its argument is a data frame 
        ''' (that is, has "data.frame" amongst its classes) and FALSE 
        ''' otherwise.
        ''' </returns>
        <ExportAPI("is.data.frame")>
        Public Function isDataframe(<RRawVectorArgument> x As Object) As Boolean
            Return TypeOf x Is dataframe
        End Function

        ''' <summary>
        ''' ### Lists – Generic and Dotted Pairs
        ''' 
        ''' Functions to construct, coerce and check for both kinds of R lists.
        ''' </summary>
        ''' <param name="x">
        ''' object to be coerced or tested.
        ''' </param>
        ''' <returns>
        ''' is.list returns TRUE if and only if its argument is a list or 
        ''' a pairlist of length > 0. is.pairlist returns TRUE if and only 
        ''' if the argument is a pairlist or NULL (see below).
        ''' </returns>
        <ExportAPI("is.list")>
        Public Function isList(<RRawVectorArgument> x As Object) As Boolean
            If x Is Nothing Then
                Return False
            End If

            Return TypeOf x Is list OrElse x.GetType.ImplementInterface(Of IDictionary)
        End Function

        ''' <summary>
        ''' ### Is an Object of Type (Primitive) Function?
        ''' 
        ''' Checks whether its argument is a (primitive) function.
        ''' </summary>
        ''' <param name="x">an R object.</param>
        ''' <returns>
        ''' TRUE if x is a (primitive) function, and FALSE otherwise.
        ''' </returns>
        ''' <remarks>
        ''' is.primitive(x) tests if x is a primitive function, i.e, if typeof(x) is either "builtin" or "special".
        ''' </remarks>
        <ExportAPI("is.function")>
        Public Function isFunction(x As Object) As Boolean
            If x Is Nothing Then
                Return False
            ElseIf x.GetType.ImplementInterface(Of RFunction) Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' ‘is.vector’ returns ‘TRUE’ if ‘x’ is a vector of the specified
        ''' mode having no attributes _other than names_.  It returns 'FALSE’
        ''' otherwise.
        ''' </summary>
        ''' <param name="x">an R object.</param>
        ''' <param name="mode">
        ''' character string naming an atomic mode or ‘"list"’ or
        ''' '"expression"’ or (except for ‘vector’) ‘"any"’.  Currently,
        ''' 'is.vector()’ allows any type (see ‘typeof’) for ‘mode’, and
        ''' when mode Is Not '"any"’, ‘is.vector(x, mode)’ is almost the
        ''' same as 'typeof(x) == mode’.
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' The atomic modes are ‘"logical"’, ‘"integer"’, ‘"numeric"’
        ''' (synonym '"double"’), ‘"complex"’, ‘"character"’ and ‘"raw"’.
        '''
        ''' If 'mode = "any"’, ‘is.vector’ may return ‘TRUE’ for the atomic
        ''' modes, 'list’ and ‘expression’.  For any ‘mode’, it will return
        ''' 'FALSE’ if ‘x’ has any attributes except names.  (This is
        ''' incompatible with S.)  On the other hand, 'as.vector’ removes
        ''' _all_ attributes including names For results Of atomic mode (but
        ''' Not those of mode '"list"’ nor ‘"expression"’).
        '''
        ''' Note that factors are _not_ vectors; 'is.vector’ returns ‘FALSE’
        ''' And 'as.vector’ converts a factor to a character vector for ‘mode
        ''' = "any"'.
        ''' </remarks>
        <ExportAPI("is.vector")>
        Public Function isRVector(<RRawVectorArgument> x As Object, Optional mode As Object = "any") As Boolean
            If x Is Nothing Then
                Return True
            End If

            Dim type As Type = x.GetType

            If TypeOf x Is vector Then
                x = DirectCast(x, vector).data
            End If

            If x.GetType.IsArray Then
                If mode Is Nothing OrElse mode = "any" Then
                    Return True
                Else
                    ' unsure for the type test
                    ' just returns true at here
                    Return True
                End If
            End If

            Return False
        End Function

        ''' <summary>
        ''' Check of the given object is clr array or not
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <ExportAPI("is.array")>
        Public Function is_array(<RRawVectorArgument> x As Object) As Boolean
            If x Is Nothing Then
                Return False
            Else
                Return x.GetType.IsArray
            End If
        End Function

        ''' <summary>
        ''' cast any R object collection as a generic type clr array
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <ExportAPI("as.array")>
        Public Function as_array(<RRawVectorArgument> x As Object) As Object
            Return REnv.UnsafeTryCastGenericArray(CLRVector.asObject(x))
        End Function

        ''' <summary>
        ''' ### ‘Not Available’ / Missing Values
        ''' 
        ''' NA is a logical constant of length 1 which contains a missing value indicator. 
        ''' NA can be coerced to any other vector type except raw. There are also constants 
        ''' NA_integer_, NA_real_, NA_complex_ and NA_character_ of the other atomic vector 
        ''' types which support missing values: all of these are reserved words in the R 
        ''' language.
        ''' 
        ''' The generic Function Is.na indicates which elements are missing.
        ''' The generic Function Is.na&lt;- sets elements To NA.
        ''' </summary>
        ''' <param name="x">an R object to be tested: the default method for is.na and anyNA 
        ''' handle atomic vectors, lists, pairlists, and NULL.</param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' The default method for is.na applied to an atomic vector returns a logical vector 
        ''' of the same length as its argument x, containing TRUE for those elements marked NA 
        ''' or, for numeric or complex vectors, NaN, and FALSE otherwise. (A complex value is 
        ''' regarded as NA if either its real or imaginary part is NA or NaN.) dim, dimnames 
        ''' and names attributes are copied to the result.
        '''
        ''' The Default methods also work For lists And pairlists
        ''' 
        ''' For Is.na, elementwise the result Is false unless that element Is a length-one atomic 
        ''' vector And the single element of that vector Is regarded as NA Or NaN (note that any 
        ''' Is.na method for the class of the element Is ignored).
        ''' 
        ''' anyNA(recursive = FALSE) works the same way as Is.na; anyNA(recursive = TRUE) applies 
        ''' anyNA (with method dispatch) to each element.
        '''
        ''' The data frame method For Is.na returns a logical matrix With the same dimensions As 
        ''' the data frame, And With dimnames taken from the row And column names Of the data 
        ''' frame.
        '''
        ''' anyNA(NULL) Is false; Is.na(NULL) Is logical(0) (no longer warning since R version 
        ''' 3.5.0).
        ''' </returns>
        <ExportAPI("is.na")>
        Public Function isNA(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
            Dim numerics As pipeline = pipeline.TryCreatePipeline(Of Double)(x, env, suppress:=True)

            If x Is Nothing Then
                Return False
            ElseIf numerics.isError Then
                If x Is GetType(Void) Then
                    Return True
                Else
                    Return False
                End If
            End If

            Return numerics _
                .populates(Of Double)(env) _
                .Select(Function(a) a.IsNaNImaginary) _
                .ToArray
        End Function

        ''' <summary>
        ''' Send R Output to a File
        ''' 
        ''' ``sink`` diverts R output to a connection (and stops such diversions).
        ''' </summary>
        ''' <param name="file">
        ''' a writable connection Or a character String naming 
        ''' the file To write To, Or NULL To Stop sink-ing.
        ''' </param>
        ''' <param name="append">
        ''' logical. If TRUE, output will be appended to file; 
        ''' otherwise, it will overwrite the contents of file.
        ''' </param>
        ''' <param name="split">
        ''' logical: if TRUE, output will be sent to the new sink 
        ''' and to the current output stream, like the Unix 
        ''' program ``tee``.
        ''' </param>
        ''' <returns>sink returns NULL.</returns>
        ''' <remarks>
        ''' sink diverts R output to a connection (and must be used 
        ''' again to finish such a diversion, see below!). If file 
        ''' is a character string, a file connection with that name 
        ''' will be established for the duration of the diversion.
        '''
        ''' Normal R output (To connection stdout) Is diverted by the 
        ''' Default type = "output". Only prompts And (most) messages 
        ''' Continue To appear On the console. Messages sent To 
        ''' stderr() (including those from message, warning And Stop) 
        ''' can be diverted by sink(type = "message") (see below).
        '''
        ''' sink() Or sink(file = NULL) ends the last diversion (of 
        ''' the specified type). There Is a stack of diversions for 
        ''' normal output, so output reverts to the previous diversion 
        ''' (if there was one). The stack Is of up to 21 connections 
        ''' (20 diversions).
        '''
        ''' If file Is a connection it will be opened If necessary 
        ''' (In "wt" mode) And closed once it Is removed from the 
        ''' stack Of diversions.
        '''
        ''' split = TRUE only splits R output (via Rvprintf) And the 
        ''' default output from writeLines: it does Not split all 
        ''' output that might be sent To stdout().
        '''
        ''' Sink-ing the messages stream should be done only with 
        ''' great care. For that stream file must be an already open 
        ''' connection, And there Is no stack of connections.
        '''
        ''' If file Is a character String, the file will be opened 
        ''' Using the current encoding. If you want a different 
        ''' encoding (e.g., To represent strings which have been 
        ''' stored In UTF-8), use a file connection — but some 
        ''' ways To produce R output will already have converted 
        ''' such strings To the current encoding.
        ''' </remarks>
        <ExportAPI("sink")>
        Public Function sink(Optional file$ = Nothing,
                             Optional append As Boolean = False,
                             Optional split As Boolean = True,
                             Optional unsafe As Boolean = True,
                             Optional env As Environment = Nothing) As Object

            Dim stdout As RContentOutput = env.globalEnvironment.stdout

            If Not file.StringEmpty Then
                ' 打开一个新的会话用于保存输出日志
                Call stdout.openSink(file, split, append)
            ElseIf stdout.isLogOpen Then
                ' 结束当前的日志会话
                Call stdout.closeSink()
            ElseIf unsafe Then
                Return Internal.debug.stop("log file is missing!", env)
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' ``sink.number()`` reports how many diversions are in use.
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <ExportAPI("sink.number")>
        Public Function sink_number(<RRawVectorArgument(TypeCodes.string)>
                                    Optional type As Object = "output|message",
                                    Optional env As Environment = Nothing) As Integer

            Dim stdout As RContentOutput = env.globalEnvironment.stdout

            If stdout.isLogOpen Then
                Return 1
            Else
                Return 0
            End If
        End Function

        ''' <summary>
        ''' # Length of an Object
        ''' 
        ''' Get or set the length of vectors (including lists) and factors, 
        ''' and of any other R object for which a method has been defined.
        ''' </summary>
        ''' <param name="x">an R object. For replacement, a vector or factor.</param>
        ''' <param name="newSize">
        ''' a non-negative integer or double (which will be rounded down).
        ''' </param>
        ''' <returns>
        ''' The default method for length currently returns a non-negative 
        ''' integer of length 1, except for vectors of more than 2^31 - 1 
        ''' elements, when it returns a double.
        '''
        ''' For vectors(including lists) And factors the length Is the 
        ''' number of elements. For an environment it Is the number of 
        ''' objects in the environment, And NULL has length 0. For expressions 
        ''' And pairlists (including language objects And dotlists) it Is the 
        ''' length of the pairlist chain. All other objects (including 
        ''' functions) have length one: note that For functions this differs 
        ''' from S.
        '''
        ''' The replacement form removes all the attributes Of x except its 
        ''' names, which are adjusted (And If necessary extended by "").
        ''' </returns>
        <ExportAPI("length")>
        Public Function length(<RRawVectorArgument> x As Object, <RByRefValueAssign> Optional newSize As Integer = -1) As Integer
            If x Is Nothing Then
                Return 0
            End If

            Dim type As Type = x.GetType

            If type.IsArray Then
                Return DirectCast(x, Array).Length
            ElseIf type.ImplementInterface(GetType(RIndex)) Then
                ' list/vector
                Return DirectCast(x, RIndex).length
            ElseIf type.ImplementInterface(GetType(IDictionary)) Then
                Return DirectCast(x, IDictionary).Count
            ElseIf type Is GetType(dataframe) Then
                Return DirectCast(x, dataframe).ncols
            ElseIf type Is GetType(Group) Then
                Return DirectCast(x, Group).length
            ElseIf type Is GetType(collectionSet) Then
                Return DirectCast(x, collectionSet).Length
            ElseIf type.ImplementInterface(Of IList) Then
                Return DirectCast(x, IList).Count
            Else
                Return 1
            End If
        End Function

        ''' <summary>
        ''' ## Run the external R# script. Read R Code from a File, a Connection or Expressions
        ''' 
        ''' causes R to accept its input from the named file or URL or connection or expressions directly. 
        ''' Input is read and parsed from that file until the end of the file is reached, then the parsed 
        ''' expressions are evaluated sequentially in the chosen environment.
        ''' </summary>
        ''' <param name="path">
        ''' a connection Or a character String giving the pathname Of the file Or URL To read from. 
        ''' "" indicates the connection ``stdin()``.</param>
        ''' <param name="envir"></param>
        ''' <returns>
        ''' The value of special ``last variable`` or the value returns by the ``return`` keyword. 
        ''' </returns>
        <ExportAPI("source")>
        Public Function source(path$,
                               <RListObjectArgument>
                               Optional arguments As Object = Nothing,
                               Optional envir As Environment = Nothing) As Object

            Dim args As NamedValue(Of Object)() = RListObjectArgumentAttribute _
                .getObjectList(arguments, envir) _
                .Where(Function(a) a.Name <> path) _
                .ToArray

            If args.Length = 1 AndAlso TypeOf arguments Is InvokeParameter() AndAlso TypeOf args(Scan0).Value Is list Then
                args = DirectCast(args(Scan0).Value, list).slots _
                    .Select(Function(tuple)
                                Return New NamedValue(Of Object) With {
                                    .Name = tuple.Key,
                                    .Value = tuple.Value
                                }
                            End Function) _
                    .ToArray
            End If

            Dim R As RInterpreter = envir.globalEnvironment.Rscript
            Dim result As Object = R.Source(path, args)

            Return New invisible() With {
                .value = result
            }
        End Function

        ''' <summary>
        ''' # Options Settings
        ''' 
        ''' Allow the user to set and examine a variety of global options which affect the way in 
        ''' which R computes and displays its results.
        ''' </summary>
        ''' <param name="x">a character string holding an option name.</param>
        ''' <param name="default">if the specified option is not set in the options list, this 
        ''' value is returned. This facilitates retrieving an option and checking whether it is 
        ''' set and setting it separately if not.</param>
        ''' <param name="envir"></param>
        ''' <returns>For getOption, the current value set for option x, or default (which defaults 
        ''' to NULL) if the option is unset.</returns>
        ''' <remarks>
        ''' Invoking options() with no arguments returns a list with the current values of the options. 
        ''' Note that not all options listed below are set initially. To access the value of a single 
        ''' option, one should use, e.g., getOption("width") rather than options("width") which is a 
        ''' list of length one.
        ''' </remarks>
        <ExportAPI("getOption")>
        <RApiReturn(GetType(String))>
        Public Function getOption(x$,
                                  Optional default$ = Nothing,
                                  Optional envir As Environment = Nothing) As Object

            If x.StringEmpty Then
                Return invoke.missingParameter(NameOf(getOption), "name", envir)
            Else
                Return envir.globalEnvironment _
                    .options _
                    .getOption(x, [default])
            End If
        End Function

        ''' <summary>
        ''' ###### Options Settings
        ''' 
        ''' Allow the user to set and examine a variety of global options which 
        ''' affect the way in which R computes and displays its results.
        ''' </summary>
        ''' <param name="opts">
        ''' any options can be defined, using name = value. However, only the 
        ''' ones below are used in base R.
        ''' 
        ''' Options can also be passed by giving a Single unnamed argument which 
        ''' Is a named list.
        ''' </param>
        ''' <param name="envir"></param>
        ''' <returns>
        ''' For options(), a list of all set options sorted by name. For options(name), a list of 
        ''' length one containing the set value, or NULL if it is unset. For uses setting one or more 
        ''' options, a list with the previous values of the options changed (returned invisibly).
        ''' </returns>
        ''' <remarks>
        ''' Options used in base R
        ''' 
        ''' + add.smooth: typically logical, defaulting to TRUE. Could also be set to an 
        ''' integer for specifying how many (simulated) smooths should be added. This 
        ''' is currently only used by plot.lm.
        ''' + askYesNo: a function (typically set by a front-end) to ask the user binary 
        ''' response functions in a consistent way, or a vector of strings used by 
        ''' askYesNo to use as default responses for such questions.
        '''
        ''' browserNLdisabled:
        ''' logical: whether newline is disabled as a synonym for "n" in the browser.
        '''
        ''' checkPackageLicense:
        ''' logical, not set by default. If true, loadNamespace asks a user to accept any 
        ''' non-standard license at first load of the package.
        '''
        ''' check.bounds:
        ''' logical, defaulting to FALSE. If true, a warning is produced whenever a vector 
        ''' (atomic or list) is extended, by something like x &lt;- 1:3; x[5] &lt;- 6.
        '''
        ''' CBoundsCheck:
        ''' logical, controlling whether .C and .Fortran make copies to check for array 
        ''' over-runs on the atomic vector arguments.
        '''
        ''' Initially set from value of the environment variable R_C_BOUNDS_CHECK (set to 
        ''' yes to enable).
        '''
        ''' conflicts.policy:
        ''' character string or list controlling handling of conflicts found in calls to 
        ''' library or require. See library for details.
        '''
        ''' continue:
        ''' a non-empty string setting the prompt used for lines which continue over one 
        ''' line.
        '''
        ''' defaultPackages:
        ''' the packages that are attached by default when R starts up. Initially set from 
        ''' value of the environment variable R_DEFAULT_PACKAGES, or if that is unset 
        ''' to c("datasets", "utils", "grDevices", "graphics", "stats", "methods"). (Set 
        ''' R_DEFAULT_PACKAGES to NULL or a comma-separated list of package names.) It 
        ''' will not work to set this in a ‘.Rprofile’ file, as its value is consulted 
        ''' before that file is read.
        '''
        ''' deparse.cutoff:
        ''' integer value controlling the printing of language constructs which are deparsed. 
        ''' Default 60.
        '''
        ''' deparse.max.lines:
        ''' controls the number of lines used when deparsing in traceback, browser, and 
        ''' upon entry to a function whose debugging flag is set. Initially unset, and 
        ''' only used if set to a positive integer.
        '''
        ''' digits:
        ''' controls the number of significant (see signif) digits to print when printing 
        ''' numeric values. It is a suggestion only. Valid values are 1...22 with default 
        ''' 7. See the note in print.default about values greater than 15.
        '''
        ''' digits.secs:
        ''' controls the maximum number of digits to print when formatting time values 
        ''' in seconds. Valid values are 0...6 with default 0. See strftime.
        '''
        ''' download.file.extra:
        ''' Extra command-line argument(s) for non-default methods: see download.file.
        '''
        ''' download.file.method:
        ''' Method to be used for download.file. Currently download methods "internal",
        ''' "wininet" (Windows only), "libcurl", "wget" and "curl" are available. If 
        ''' not set, method = "auto" is chosen: see download.file.
        '''
        ''' echo:
        ''' logical. Only used in non-interactive mode, when it controls whether input 
        ''' is echoed. Command-line option --slave sets this to FALSE, but otherwise 
        ''' it starts the session as TRUE.
        '''
        ''' encoding:
        ''' The name of an encoding, default "native.enc". See connections.
        '''
        ''' error:
        ''' either a function or an expression governing the handling of non-catastrophic 
        ''' errors such as those generated by stop as well as by signals and internally 
        ''' detected errors. If the option is a function, a call to that function, with
        ''' no arguments, is generated as the expression. By default the option is not
        ''' set: see stop for the behaviour in that case. The functions dump.frames 
        ''' and recover provide alternatives that allow post-mortem debugging. Note that 
        ''' these need to specified as e.g. options(error = utils::recover) in startup
        ''' files such as ‘.Rprofile’.
        '''
        ''' expressions:
        ''' sets a limit on the number of nested expressions that will be evaluated. Valid 
        ''' values are 25...500000 with default 5000. If you increase it, you may also 
        ''' want to start R with a larger protection stack; see --max-ppsize in Memory. 
        ''' Note too that you may cause a segfault from overflow of the C stack, and on 
        ''' OSes where it is possible you may want to increase that. Once the limit is 
        ''' reached an error is thrown. The current number under evaluation can be found 
        ''' by calling Cstack_info.
        '''
        ''' interrupt:
        ''' a function taking no arguments to be called on a user interrupt if the interrupt 
        ''' condition is not otherwise handled.
        '''
        ''' keep.parse.data:
        ''' When internally storing source code (keep.source is TRUE), also store parse data. 
        ''' Parse data can then be retrieved with getParseData() and used e.g. for spell
        ''' checking of string constants or syntax highlighting. The value has effect only
        ''' when internally storing source code (see keep.source). The default is TRUE.
        '''
        ''' keep.parse.data.pkgs:
        ''' As for keep.parse.data, used only when packages are installed. Defaults to FALSE 
        ''' unless the environment variable R_KEEP_PKG_PARSE_DATA is set to yes. The space 
        ''' overhead of parse data can be substantial even after compression and it causes 
        ''' performance overhead when loading packages.
        '''
        ''' keep.source:
        ''' When TRUE, the source code for functions (newly defined or loaded) is stored 
        ''' internally allowing comments to be kept in the right places. Retrieve the source 
        ''' by printing or using deparse(fn, control = "useSource").
        '''
        ''' The default is interactive(), i.e., TRUE for interactive use.
        '''
        ''' keep.source.pkgs:
        ''' As for keep.source, used only when packages are installed. Defaults to FALSE 
        ''' unless the environment variable R_KEEP_PKG_SOURCE is set to yes.
        '''
        ''' matprod:
        ''' a string selecting the implementation of the matrix products %*%, crossprod, 
        ''' and tcrossprod for double and complex vectors:
        '''
        ''' "internal"
        ''' uses an unoptimized 3-loop algorithm which correctly propagates NaN and Inf
        ''' values and is consistent in precision with other summation algorithms inside
        ''' R like sum or colSums (which now means that it uses a long double accumulator
        ''' for summation if available and enabled, see capabilities).
        '''
        ''' "default"
        ''' uses BLAS to speed up computation, but to ensure correct propagation of NaN 
        ''' and Inf values it uses an unoptimized 3-loop algorithm for inputs that may 
        ''' contain NaN or Inf values. When deemed beneficial for performance, "default"
        ''' may call the 3-loop algorithm unconditionally, i.e., without checking the 
        ''' input for NaN/Inf values. The 3-loop algorithm uses (only) a double accumulator
        ''' for summation, which is consistent with the reference BLAS implementation.
        '''
        ''' "blas"
        ''' uses BLAS unconditionally without any checks and should be used with extreme 
        ''' caution. BLAS libraries do not propagate NaN or Inf values correctly and 
        ''' for inputs with NaN/Inf values the results may be undefined.
        '''
        ''' "default.simd"
        ''' is experimental and will likely be removed in future versions of R. It 
        ''' provides the same behavior as "default", but the check whether the input 
        ''' contains NaN/Inf values is faster on some SIMD hardware. On older systems
        ''' it will run correctly, but may be much slower than "default".
        '''
        ''' max.print:
        ''' integer, defaulting to 99999. print or show methods can make use of this option, 
        ''' to limit the amount of information that is printed, to something in the order 
        ''' of (and typically slightly less than) max.print entries.
        '''
        ''' OutDec:
        ''' character string containing a single character. The preferred character to be 
        ''' used as the decimal point in output conversions, that is in printing, 
        ''' plotting, format and as.character but not when deparsing nor by sprintf nor
        ''' formatC (which are sometimes used prior to printing.)
        '''
        ''' pager:
        ''' the command used for displaying text files by file.show, details depending on
        ''' the platform:
        '''
        ''' On a unix-alike
        ''' defaults to ‘R_HOME/bin/pager’, which is a shell script running the command-line
        ''' specified by the environment variable PAGER whose default is set at 
        ''' configuration, usually to less.
        '''
        ''' On Windows
        ''' defaults to "internal", which uses a pager similar to the GUI console. 
        ''' Another possibility is "console" to use the console itself.
        '''
        ''' Can be a character string or an R function, in which case it needs to accept
        ''' the arguments (files, header, title, delete.file) corresponding to the 
        ''' first four arguments of file.show.
        '''
        ''' papersize:
        ''' the default paper format used by postscript; set by environment variable
        ''' R_PAPERSIZE when R is started: if that is unset or invalid it defaults 
        ''' platform dependently
        '''
        ''' on a unix-alike
        ''' to a value derived from the locale category LC_PAPER, or if that is unavailable 
        ''' to a default set when R was built.
        '''
        ''' on Windows
        ''' to "a4", or "letter" in US and Canadian locales.
        '''
        ''' PCRE_limit_recursion:
        ''' Logical: should grep(perl = TRUE) and similar limit the maximal recursion
        ''' allowed when matching? PCRE can be built not to use a recursion stack (see
        ''' pcre_config, but it is by default with a recursion limit of 10000000 which 
        ''' potentially needs a very large C stack: see the discussion at 
        ''' http://www.pcre.org/original/doc/html/pcrestack.html. If true, the limit is 
        ''' reduced using R's estimate of the C stack size available (if known), 
        ''' otherwise 10000. If NA, the limit is imposed only if any input string has 
        ''' 1000 or more bytes.
        '''
        ''' PCRE_study:
        ''' Logical or integer: should grep(perl = TRUE) and similar ‘study’ the patterns? 
        ''' Either logical or a numerical threshold for the minimum number of strings 
        ''' to be matched for the pattern to be studied (the default is 10)). Missing 
        ''' values and negative numbers are treated as false.
        '''
        ''' PCRE_use_JIT:
        ''' Logical: should grep(perl = TRUE), strsplit(perl = TRUE) and similar make use
        ''' of PCRE's Just-In-Time compiler for studied patterns, if available? Missing 
        ''' values are treated as false.
        '''
        ''' pdfviewer:
        ''' default PDF viewer. The default is set from the environment variable
        ''' R_PDFVIEWER, the default value of which
        '''
        ''' on a unix-alike
        ''' is set when R is configured, and
        '''
        ''' on Windows
        ''' is the full path to open.exe, a utility supplied with R.
        '''
        ''' printcmd:
        ''' the command used by postscript for printing; set by environment variable 
        ''' R_PRINTCMD when R is started. This should be a command that expects either 
        ''' input to be piped to ‘stdin’ or to be given a single filename argument. 
        ''' Usually set to "lpr" on a Unix-alike.
        '''
        ''' prompt:
        ''' a non-empty string to be used for R's prompt; should usually end in a 
        ''' blank (" ").
        '''
        ''' rl_word_breaks:
        ''' (Unix only:) Used for the readline-based terminal interface. Default 
        ''' value " \t\n\"\\'`>&lt;=%;,|&amp;{()}".
        '''
        ''' This is the set of characters use to break the input line into tokens for
        ''' object- and file-name completion. Those who do not use spaces around 
        ''' operators may prefer
        ''' ``" \t\n\"\\'`>&lt;=+-*%;,|&amp;{()}"``
        '''
        ''' save.defaults, save.image.defaults:
        ''' see save.
        '''
        ''' scipen:
        ''' integer. A penalty to be applied when deciding to print numeric values in 
        ''' fixed or exponential notation. Positive values bias towards fixed and
        ''' negative towards scientific notation: fixed notation will be preferred
        ''' unless it is more than scipen digits wider.
        '''
        ''' setWidthOnResize:
        ''' a logical. If set and TRUE, R run in a terminal using a recent readline
        ''' library will set the width option when the terminal is resized.
        '''
        ''' showWarnCalls, showErrorCalls:
        ''' a logical. Should warning and error messages show a summary of the call
        ''' stack? By default error calls are shown in non-interactive sessions.
        '''
        ''' showNCalls:
        ''' integer. Controls how long the sequence of calls must be (in bytes) before 
        ''' ellipses are used. Defaults to 40 and should be at least 30 and no more 
        ''' than 500.
        '''
        ''' show.error.locations:
        ''' Should source locations of errors be printed? If set to TRUE or "top", the
        ''' source location that is highest on the stack (the most recent call) will
        ''' be printed. "bottom" will print the location of the earliest call found 
        ''' on the stack.
        '''
        ''' Integer values can select other entries. The value 0 corresponds to "top"
        ''' and positive values count down the stack from there. The value -1 corresponds
        ''' to "bottom" and negative values count up from there.
        '''
        ''' show.error.messages:
        ''' a logical. Should error messages be printed? Intended for use with try or a 
        ''' user-installed error handler.
        '''
        ''' stringsAsFactors:
        ''' The default setting for arguments of data.frame and read.table.
        '''
        ''' texi2dvi:
        ''' used by functions texi2dvi and texi2pdf in package tools.
        '''
        ''' unix-alike only:
        ''' Set at startup from the environment variable R_TEXI2DVICMD, which defaults 
        ''' first to the value of environment variable TEXI2DVI, and then to a value 
        ''' set when R was installed (the full path to a texi2dvi script if one was 
        ''' found). If necessary, that environment variable can be set to "emulation".
        '''
        ''' timeout:
        ''' integer. The timeout for some Internet operations, in seconds. Default 60 
        ''' seconds. See download.file and connections.
        '''
        ''' topLevelEnvironment:
        ''' see topenv and sys.source.
        '''
        ''' url.method:
        ''' character string: the default method for url. Normally unset, which is 
        ''' equivalent to "default", which is "internal" except on Windows.
        '''
        ''' useFancyQuotes:
        ''' controls the use of directional quotes in sQuote, dQuote and in rendering 
        ''' text help (see Rd2txt in package tools). Can be TRUE, FALSE, "TeX" or 
        ''' "UTF-8".
        '''
        ''' verbose:
        ''' logical. Should R report extra information on progress? Set to TRUE by the
        ''' command-line option --verbose.
        '''
        ''' warn:
        ''' sets the handling of warning messages. If warn is negative all warnings are 
        ''' ignored. If warn is zero (the default) warnings are stored until the 
        ''' top–level function returns. If 10 or fewer warnings were signalled they 
        ''' will be printed otherwise a message saying how many were signalled. An object 
        ''' called last.warning is created and can be printed through the function
        ''' warnings. If warn is one, warnings are printed as they occur. If warn is
        ''' two (or larger, coercible to integer), all warnings are turned into errors.
        '''
        ''' warnPartialMatchArgs:
        ''' logical. If true, warns if partial matching is used in argument matching.
        '''
        ''' warnPartialMatchAttr:
        ''' logical. If true, warns if partial matching is used in extracting attributes
        ''' via attr.
        '''
        ''' warnPartialMatchDollar:
        ''' logical. If true, warns if partial matching is used for extraction by $.
        '''
        ''' warning.expression:
        ''' an R code expression to be called if a warning is generated, replacing the 
        ''' standard message. If non-null it is called irrespective of the value of 
        ''' option warn.
        '''
        ''' warning.length:
        ''' sets the truncation limit for error and warning messages. A non-negative 
        ''' integer, with allowed values 100...8170, default 1000.
        '''
        ''' nwarnings:
        ''' the limit for the number of warnings kept when warn = 0, default 50. This 
        ''' will discard messages if called whilst they are being collected. If 
        ''' you increase this limit, be aware that the current implementation pre-allocates 
        ''' the equivalent of a named list for them, i.e., do not increase it to more 
        ''' than say a million.
        '''
        ''' width:
        ''' controls the maximum number of columns on a line used in printing vectors,
        ''' matrices and arrays, and when filling by cat.
        '''
        ''' Columns are normally the same as characters except in East Asian languages.
        '''
        ''' You may want to change this if you re-size the window that R is running in. 
        ''' Valid values are 10...10000 with default normally 80. (The limits on valid
        ''' values are in file ‘Print.h’ and can be changed by re-compiling R.) Some 
        ''' R consoles automatically change the value when they are resized.
        '''
        ''' See the examples on Startup for one way to set this automatically from the 
        ''' terminal width when R is started.
        '''
        ''' The ‘factory-fresh’ default settings of some of these options are
        '''
        ''' add.smooth	TRUE
        ''' check.bounds	FALSE
        ''' continue	"+ "
        ''' digits	7
        ''' echo	TRUE
        ''' encoding	"native.enc"
        ''' error	NULL
        ''' expressions	5000
        ''' keep.source	interactive()
        ''' keep.source.pkgs	FALSE
        ''' max.print	99999
        ''' OutDec	"."
        ''' prompt	"> "
        ''' scipen	0
        ''' show.error.messages	TRUE
        ''' timeout	60
        ''' verbose	FALSE
        ''' warn	0
        ''' warning.length	1000
        ''' width	80
        ''' Others are set from environment variables or are platform-dependent.
        ''' </remarks>
        <ExportAPI("options")>
        Public Function options(<RListObjectArgument> opts As Object, envir As Environment) As Object
            Dim configs As Options = envir.globalEnvironment.options
            Dim values As list = Nothing
            Dim type As Type = opts.GetType

            If type Is GetType(String()) Then
                values = New list With {
                    .slots = DirectCast(opts, String()) _
                        .ToDictionary(Function(key) key,
                                      Function(key)
                                          Return CObj(configs.getOption(key, ""))
                                      End Function)
                }
            ElseIf type Is GetType(list) Then
                values = DirectCast(opts, list)

                For Each value As KeyValuePair(Of String, Object) In values.slots
                    Try
                        configs.setOption(value.Key, value.Value, envir)
                    Catch ex As Exception
                        Return Internal.debug.stop(ex, envir)
                    End Try
                Next
            ElseIf type.IsArray AndAlso DirectCast(opts, Array).Length = 0 Then
                ' get all options
                values = RConversion.asList(configs.getAllConfigs, New InvokeParameter() {}, envir)
            Else
                Dim err = setOptionsParameters(envir, values, DirectCast(opts, InvokeParameter()))

                If Not err Is Nothing Then
                    Return err
                End If
            End If

            Return values
        End Function

        Private Function setOptionsParameters(env As Environment, ByRef values As list, opts As InvokeParameter()) As Object
            Dim configs As Options = env.globalEnvironment.options

            If opts.TryCount = 1 AndAlso TypeOf opts(0).value Is SymbolReference Then
                Return base.options(opts(0).Evaluate(env), env)
            End If

            values = New list With {
                .slots = New Dictionary(Of String, Object)
            }

            ' invoke parameters
            For Each value As InvokeParameter In opts
                Dim name As String = value.name
                Dim cfgValue As Object = value.Evaluate(env)

                If Program.isException(cfgValue) Then
                    Return cfgValue
                End If

                Dim vec As Array = REnv.asVector(Of Object)(cfgValue)

                If vec.Length > 0 Then
                    cfgValue = vec.GetValue(Scan0)
                Else
                    cfgValue = Nothing
                End If

                Try
                    values.slots(name) = configs.setOption(name, any.ToString(cfgValue), env)
                Catch ex As Exception
                    Return Internal.debug.stop(ex, env)
                End Try
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' save the options configuration data to file
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("options_flush")>
        Public Function options_flush(Optional env As Environment = Nothing) As Object
            env.globalEnvironment.options.flush(save:=True)
            Return Nothing
        End Function

        ''' <summary>
        ''' # The Names of an Object
        ''' 
        ''' Functions to get or set the names of an object.
        ''' </summary>
        ''' <param name="object">an R object.</param>
        ''' <param name="namelist">
        ''' a character vector of up to the same length as ``x``, or ``NULL``.
        ''' </param>
        ''' <param name="envir"></param>
        ''' <returns>
        ''' For ``names``, ``NULL`` or a character vector of the same length as x. 
        ''' (NULL is given if the object has no names, including for objects of 
        ''' types which cannot have names.) For an environment, the length is the 
        ''' number of objects in the environment but the order of the names is 
        ''' arbitrary.
        ''' 
        ''' For ``names&lt;-``, the updated object. (Note that the value of 
        ''' ``names(x) &lt;- value`` Is that of the assignment, value, Not the 
        ''' return value from the left-hand side.)
        ''' </returns>
        ''' <example>
        ''' x = list(1,2,3,4,5,6)
        ''' print(names(x));
        ''' # NULL
        ''' names(x) = ["a", "b", "c", "d", "e", "f"];
        ''' print(names(x));
        ''' # [1] "a" "b" "c" "d" "e" "f"
        ''' </example>
        <ExportAPI("names")>
        Public Function names(<RRawVectorArgument> [object] As Object,
                              <RByRefValueAssign>
                              Optional namelist As Array = Nothing,
                              Optional envir As Environment = Nothing) As Object
            ' > names(NULL)
            ' NULL
            If [object] Is Nothing Then
                Return Nothing
            End If
            If namelist Is Nothing OrElse namelist.Length = 0 Then
                ' get element names
                Return RObj.names.getNames([object], envir)
            Else
                ' set element names
                Return RObj.names.setNames([object], namelist, envir)
            End If
        End Function

        ''' <summary>
        ''' ### Make Syntactically Valid Names
        ''' 
        ''' Make syntactically valid names out of character vectors.
        ''' 
        ''' 
        ''' </summary>
        ''' <param name="names">
        ''' character vector To be coerced To syntactically valid names. 
        ''' This Is coerced To character If necessary.
        ''' </param>
        ''' <param name="unique">
        ''' logical; if TRUE, the resulting elements are unique. This may 
        ''' be desired for, e.g., column names.
        ''' </param>
        ''' <param name="allow_">
        ''' logical. For compatibility with R prior to 1.9.0.
        ''' </param>
        ''' <remarks>
        ''' A syntactically valid name consists of letters, numbers and 
        ''' the dot or underline characters and starts with a letter or 
        ''' the dot not followed by a number. Names such as ".2way" are 
        ''' not valid, and neither are the reserved words.
        '''
        ''' The definition Of a letter depends On the current locale, but 
        ''' only ASCII digits are considered To be digits.
        '''
        ''' The character "X" Is prepended If necessary. All invalid 
        ''' characters are translated To ".". A missing value Is translated 
        ''' To "NA". Names which match R keywords have a dot appended To 
        ''' them. Duplicated values are altered by make.unique.
        ''' </remarks>
        ''' <returns>A character vector of same length as names with each 
        ''' changed to a syntactically valid name, in the current locale's 
        ''' encoding.</returns>
        <ExportAPI("make.names")>
        Public Function makeNames(<RRawVectorArgument(GetType(String))>
                                  names As Object,
                                  Optional unique As Boolean = False,
                                  Optional allow_ As Boolean = True) As String()

            Dim nameList As String() = CLRVector.asCharacter(names)
            Dim resultNames As String() = nameList.makeNames(unique, allow_)

            Return resultNames
        End Function

        ''' <summary>
        ''' make the given name string be unique
        ''' </summary>
        ''' <param name="names"></param>
        ''' <returns>this function by extends the numeric suffix to the duplictaed name for make the name string be uniqued!</returns>
        <ExportAPI("unique.names")>
        Public Function uniqueNames(<RRawVectorArgument(GetType(String))> names As Object) As String()
            Dim nameList As String() = CLRVector.asCharacter(names)
            Dim resultNames As String() = nameList.UniqueNames

            Return resultNames
        End Function

        ''' <summary>
        ''' # Row and Column Names
        ''' 
        ''' Retrieve or set the row or column names of a matrix-like object.
        ''' </summary>
        ''' <param name="[object]">a matrix-like R object, with at least two dimensions for colnames.</param>
        ''' <param name="namelist">a valid value for that component of ``dimnames(x)``. 
        ''' For a matrix or array this is either NULL or a character vector of non-zero 
        ''' length equal to the appropriate dimension.</param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' The extractor functions try to do something sensible for any matrix-like object x. 
        ''' If the object has dimnames the first component is used as the row names, and the 
        ''' second component (if any) is used for the column names. For a data frame, rownames 
        ''' and colnames eventually call row.names and names respectively, but the latter are 
        ''' preferred.
        ''' 
        ''' If do.NULL Is FALSE, a character vector (of length NROW(x) Or NCOL(x)) Is returned 
        ''' in any case, prepending prefix to simple numbers, if there are no dimnames Or the 
        ''' corresponding component of the dimnames Is NULL.
        ''' 
        ''' The replacement methods For arrays/matrices coerce vector And factor values Of value 
        ''' To character, but Do Not dispatch methods For As.character.
        ''' 
        ''' For a data frame, value for rownames should be a character vector of non-duplicated 
        ''' And non-missing names (this Is enforced), And for colnames a character vector of 
        ''' (preferably) unique syntactically-valid names. In both cases, value will be coerced 
        ''' by as.character, And setting colnames will convert the row names To character.
        ''' </remarks>
        <ExportAPI("rownames")>
        Public Function rownames([object] As Object,
                                 <RByRefValueAssign>
                                 Optional namelist As Array = Nothing,
                                 Optional envir As Environment = Nothing) As Object

            If [object] Is Nothing Then
                Return Nothing
            End If

            If namelist Is Nothing OrElse namelist.Length = 0 Then
                Return RObj.names.getRowNames([object], envir)
            Else
                Return RObj.names.setRowNames([object], CLRVector.asCharacter(namelist), envir)
            End If
        End Function

        ''' <summary>
        ''' # Row and Column Names
        ''' 
        ''' Retrieve or set the row or column names of a matrix-like object.
        ''' </summary>
        ''' <param name="x">a matrix-like R object, with at least two dimensions for colnames.</param>
        ''' <param name="namelist">a valid value for that component of ``dimnames(x)``. 
        ''' For a matrix or array this is either NULL or a character vector of non-zero 
        ''' length equal to the appropriate dimension.</param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' The extractor functions try to do something sensible for any matrix-like object x. 
        ''' If the object has dimnames the first component is used as the row names, and the 
        ''' second component (if any) is used for the column names. For a data frame, rownames 
        ''' and colnames eventually call row.names and names respectively, but the latter are 
        ''' preferred.
        ''' 
        ''' If do.NULL Is FALSE, a character vector (of length NROW(x) Or NCOL(x)) Is returned 
        ''' in any case, prepending prefix to simple numbers, if there are no dimnames Or the 
        ''' corresponding component of the dimnames Is NULL.
        ''' 
        ''' The replacement methods For arrays/matrices coerce vector And factor values Of value 
        ''' To character, but Do Not dispatch methods For As.character.
        ''' 
        ''' For a data frame, value for rownames should be a character vector of non-duplicated 
        ''' And non-missing names (this Is enforced), And for colnames a character vector of 
        ''' (preferably) unique syntactically-valid names. In both cases, value will be coerced 
        ''' by as.character, And setting colnames will convert the row names To character.
        ''' </remarks>
        <ExportAPI("colnames")>
        Public Function colnames(x As Object,
                                 <RByRefValueAssign>
                                 Optional namelist As Array = Nothing,
                                 Optional envir As Environment = Nothing) As Object

            If x Is Nothing Then
                Return Nothing
            End If

            If namelist Is Nothing OrElse namelist.Length = 0 Then
                Return RObj.names.getColNames(x, envir)
            Else
                Return RObj.names.setColNames(x, namelist, envir)
            End If
        End Function

        ''' <summary>
        ''' ### Stop Function Execution
        ''' 
        ''' ``stop`` stops execution of the current expression and executes an error action.
        ''' </summary>
        ''' <param name="message">
        ''' <see cref="String"/> array or <see cref="Exception"/>, zero Or more objects which 
        ''' can be coerced to character (And which are pasted together with no separator) Or 
        ''' a single condition object.
        ''' </param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' The error action is controlled by error handlers established within the executing 
        ''' code and by the current default error handler set by options(error=). The error 
        ''' is first signaled as if using signalCondition(). If there are no handlers or if 
        ''' all handlers return, then the error message is printed (if options("show.error.messages") 
        ''' is true) and the default error handler is used. The default behaviour (the NULL 
        ''' error-handler) in interactive use is to return to the top level prompt or the top 
        ''' level browser, and in non-interactive use to (effectively) call q("no", status = 1, 
        ''' runLast = FALSE). The default handler stores the error message in a buffer; it can 
        ''' be retrieved by geterrmessage(). It also stores a trace of the call stack that can 
        ''' be retrieved by traceback().
        ''' 
        ''' Errors will be truncated To getOption("warning.length") characters, Default 1000.
        ''' 
        ''' If a condition Object Is supplied it should be the only argument, And further arguments 
        ''' will be ignored, With a warning.
        ''' </remarks>
        <ExportAPI("stop")>
        Public Function [stop](<RRawVectorArgument>
                               Optional message As Object = "unexpected error",
                               Optional envir As Environment = Nothing) As Message

            Return debug.stop(message, envir)
        End Function

        ''' <summary>
        ''' ### Warning Messages
        ''' 
        ''' Generates a warning message that corresponds to 
        ''' its argument(s) and (optionally) the expression 
        ''' or function from which it was called.
        ''' </summary>
        ''' <param name="message">
        ''' zero Or more objects which can be coerced to 
        ''' character (And which are pasted together with 
        ''' no separator) Or a single condition object.
        ''' </param>
        ''' <param name="envir"></param>
        ''' <returns>
        ''' The warning message as character string, invisibly.
        ''' </returns>
        ''' <remarks>
        ''' Create a new wraning message in current environment frame
        ''' </remarks>
        <ExportAPI("warning")>
        <DebuggerStepThrough>
        Public Function warning(<RRawVectorArgument> message As Object, Optional envir As Environment = Nothing) As Message
            Dim msg As Message = debug.CreateMessageInternal(message, envir, level:=MSG_TYPES.WRN)
            envir.messages.Add(msg)
            App.LogFile.log(MSG_TYPES.WRN, msg.message.JoinBy(vbCrLf), "rsharp_warning")
            Return msg
        End Function

        ''' <summary>
        ''' ### Print Warning Messages
        ''' 
        ''' warnings and its print method print the variable last.warning in a pleasing form.
        ''' </summary>
        ''' <param name="all"></param>
        ''' <param name="env"></param>
        <ExportAPI("warnings")>
        Public Sub warnings(Optional all As Boolean = False, Optional env As GlobalEnvironment = Nothing)
            Call debug.PrintWarningMessages(env.messages, env, all)
        End Sub

        ''' <summary>
        ''' ### Quote Text
        ''' 
        ''' Single or double quote text by combining with appropriate 
        ''' single or double left and right quotation marks.
        ''' </summary>
        ''' <param name="x">an R object, to be coerced to a character vector.</param>
        ''' <param name="q">the kind of quotes to be used, see ‘Details’.</param>
        ''' <returns></returns>
        <ExportAPI("sQuote")>
        Public Function sQuote(x As String, <RDefaultExpression> Optional q As Object = "~getOption(""useFancyQuotes"")") As String
            Dim cl As String = "'"
            Dim cr As String = cl
            Dim qb As Boolean = CLRVector.asLogical(q).DefaultFirst(True)

            If qb Then

            End If

            Return {cl, x, cr}.JoinBy("")
        End Function

        ''' <summary>
        ''' ### Quote Text
        ''' 
        ''' Single or double quote text by combining with appropriate 
        ''' single or double left and right quotation marks.
        ''' </summary>
        ''' <param name="x">an R object, to be coerced to a character vector.</param>
        ''' <param name="q">the kind of quotes to be used, see ‘Details’.</param>
        ''' <returns></returns>
        <ExportAPI("dQuote")>
        Public Function dQuote(x As String, <RDefaultExpression> Optional q As Object = "~getOption(""useFancyQuotes"")") As String
            Dim cl As String = """"
            Dim cr As String = cl
            Dim qb As Boolean = CLRVector.asLogical(q).DefaultFirst(True)

            If qb Then

            End If

            Return {cl, x, cr}.JoinBy("")
        End Function

        ''' <summary>
        ''' ### Diagnostic Messages
        ''' 
        ''' Generate a diagnostic message from its arguments.
        ''' </summary>
        ''' <param name="x">zero or more objects which can be coerced 
        ''' to character (and which are pasted together with no separator) 
        ''' or (for message only) a single condition object.</param>
        <ExportAPI("message")>
        Public Sub println_message(<RListObjectArgument> x As list,
                                   Optional domain As Object = null,
                                   Optional appendLF As Boolean = True,
                                   Optional env As Environment = Nothing)

            x.slots.Remove(NameOf(domain))
            x.slots.Remove(NameOf(appendLF))
            x.slots.Remove(NameOf(env))

            Dim si As String() = CLRVector.asCharacter(x.slots.Values)
            Dim msg As String = si.JoinBy("")

            If appendLF Then
                msg &= vbLf
            End If

            Call env.globalEnvironment.stdout.Write(msg)
            Call env.globalEnvironment.stdout.Flush()
        End Sub

        ''' <summary>
        ''' # Concatenate and Print
        ''' 
        ''' Outputs the objects, concatenating the representations. 
        ''' ``cat`` performs much less conversion than ``print``.
        ''' </summary>
        ''' <param name="values">R objects (see ‘Details’ for the types of objects allowed).</param>
        ''' <param name="file">A connection, or a character string naming the file to print to. 
        ''' If "" (the default), cat prints to the standard output connection, the console 
        ''' unless redirected by ``sink``.</param>
        ''' <param name="sep">a character vector of strings to append after each element.</param>
        ''' <returns></returns>
        <ExportAPI("cat")>
        Public Function cat(<RRawVectorArgument> values As Object,
                            Optional file$ = Nothing,
                            Optional sep$ = " ",
                            Optional env As Environment = Nothing) As Object

            Dim vec As Object() = Runtime.asVector(Of Object)(values) _
                .AsObjectEnumerator _
                .ToArray
            Dim strs As String

            If vec.Length = 1 AndAlso TypeOf vec(Scan0) Is dataframe Then
                values = DirectCast(vec(Scan0), dataframe).CheckDimension(env)

                If TypeOf values Is Message Then
                    Return values
                End If

                sep = sprintf(sep)
                strs = TableFormatter _
                    .GetTable(DirectCast(vec(Scan0), dataframe), env.globalEnvironment, printContent:=False, True) _
                    .Select(Function(row)
                                Return row.JoinBy(sep)
                            End Function) _
                    .JoinBy(vbCrLf)
            Else
                strs = vec _
                    .Select(Function(o) any.ToString(o, "")) _
                    .JoinBy(sprintf(sep)) _
                    .DoCall(AddressOf sprintf)
            End If

            If Not file.StringEmpty Then
                Call strs.SaveTo(file)
            ElseIf env.globalEnvironment.stdout Is Nothing Then
                Call Console.Write(strs)
            Else
                Call env.globalEnvironment.stdout.Write(strs)
            End If

            Return strs
        End Function

        ''' <summary>
        ''' ### Compactly Display the Structure of an Arbitrary ``R#`` Object
        ''' 
        ''' Compactly display the internal structure of an R object, a diagnostic function 
        ''' and an alternative to summary (and to some extent, dput). Ideally, only one 
        ''' line for each ‘basic’ structure is displayed. It is especially well suited to 
        ''' compactly display the (abbreviated) contents of (possibly nested) lists. The 
        ''' idea is to give reasonable output for any R object. It calls args for 
        ''' (non-primitive) function objects.
        ''' 
        ''' ``strOptions()`` Is a convenience function for setting ``options(str = .)``, 
        ''' see the examples.
        ''' </summary>
        ''' <param name="object">any R object about which you want to have some information.</param>
        ''' <param name="list_len">
        ''' numeric; maximum number of list elements to display within a level.
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("str")>
        Public Function str(<RRawVectorArgument>
                            [object] As Object,
                            Optional list_len% = 99,
                            Optional env As Environment = Nothing) As Object

            Dim structure$ = reflector.GetStructure([object], env.globalEnvironment, " ", list_len)
            Call env.globalEnvironment.stdout.WriteLine([structure])
            Return Nothing
        End Function

        Dim markdown As MarkdownRender = MarkdownRender.DefaultStyleRender

        ''' <summary>
        ''' # Print Values
        ''' 
        ''' print prints its argument and returns it invisibly (via invisible(x)). 
        ''' It is a generic function which means that new printing methods can be 
        ''' easily added for new classes.
        ''' </summary>
        ''' <param name="x">an object used to select a method.</param>
        ''' <param name="args">
        ''' #### quote
        ''' logical, indicating whether Or Not strings (characters) should be 
        ''' printed with surrounding quotes.
        ''' 
        ''' #### max.print
        ''' integer, the max number of elements to print. this parameter value
        ''' will overrides the max.print options from the options function.
        ''' 
        ''' #### syntax.highlight
        ''' logical, indicates that show the R# runtime expression object code
        ''' with syntax highlights? default is TRUE.
        ''' 
        ''' #### max.width
        ''' integer, the max character number for display in a cell when apply for 
        ''' print of the dataframe contents. default config is max 200 character
        ''' in a cell. if the cell string char length is greater then this threshold
        ''' then the display string will be truncated.
        ''' 
        ''' #### select
        ''' character, a character vector for select the columns for display when 
        ''' do print of a dataframe object.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("print")>
        Public Function print(<RRawVectorArgument> x As Object,
                              <RListObjectArgument>
                              Optional args As list = Nothing,
                              Optional env As Environment = Nothing) As Object

            Dim globalEnv As GlobalEnvironment = env.globalEnvironment

            If args Is Nothing Then
                args = New list With {.slots = New Dictionary(Of String, Object)}
            End If
            If TypeOf x Is Message Then
                Return x
            End If

            Dim quot As Boolean = args.getValue("quot", env, True)
            Dim maxPrint As Integer = args.getValue("max.print", env, globalEnv.options.maxPrint)
            Dim fields As String() = args.getValue(Of String())("select", env, Nothing)
            Dim maxWidth As Integer = args.getValue("max.width", env, 200)
            ' display the syntax highlights of the R# runtime expression object?
            Dim highlights As Boolean = args.getValue("syntax.highlight", env, [default]:=True)

            ' keeps pretty print in multiple threading environment
            Static dummy As New Object

            ' 这个函数是由用户指定调用的，会忽略掉invisible属性值
            If x Is Nothing Then
                SyncLock dummy
                    Call globalEnv.stdout.WriteLine("NULL")
                    ' just returns nothing literal
                    Return Nothing
                End SyncLock
            Else
                SyncLock dummy
                    Return New PrinterOptions With {
                        .maxPrint = maxPrint,
                        .quot = quot,
                        .fields = fields,
                        .maxWidth = maxWidth,
                        .syntax = highlights
                    }.doPrintInternal(x, x.GetType, env)
                End SyncLock
            End If
        End Function

        Friend Class PrinterOptions

            ''' <summary>
            ''' quot string with symbol `"`?
            ''' </summary>
            ''' <returns></returns>
            Public Property quot As Boolean = True
            Public Property maxPrint As Integer
            ''' <summary>
            ''' apply for table cell text truncated
            ''' </summary>
            ''' <returns></returns>
            Public Property maxWidth As Integer
            ''' <summary>
            ''' fields for select data from the dataframe 
            ''' to print contents, nothing means print all
            ''' fields
            ''' </summary>
            ''' <returns></returns>
            Public Property fields As String()
            Public Property syntax As Boolean = True

        End Class

        <Extension>
        Private Function doPrintInternal(opts As PrinterOptions, x As Object, type As Type, env As Environment) As Object
            Dim globalEnv As GlobalEnvironment = env.globalEnvironment
            Dim maxPrint As Integer = opts.maxPrint

            If TypeOf x Is invisible Then
                x = DirectCast(x, invisible).value

                If x Is Nothing Then
                    Call globalEnv.stdout.WriteLine("NULL")
                    ' just returns nothing literal
                    Return Nothing
                End If
            End If

            If type Is GetType(RMethodInfo) Then
                Call globalEnv _
                    .packages _
                    .packageDocs _
                    .PrintHelp(x, env.globalEnvironment.stdout)
            ElseIf type Is GetType(DeclareNewFunction) Then
                Dim std_out = env.globalEnvironment.stdout

                ' print the runtime function code
                Call ConsoleMarkdownPrinter.printDocs(DirectCast(x, DeclareNewFunction))

                If std_out.env = OutputEnvironments.Console AndAlso opts.syntax Then
                    Call ConsoleSyntaxHighlightPrinter.PrintCode(vbCrLf & " " & x.ToString, std_out)
                Else
                    Call std_out.WriteLine(DirectCast(x, DeclareNewFunction).ToString)
                End If
            ElseIf type.ImplementInterface(GetType(RPrint)) Then
                Try
                    Call markdown.DoPrint(DirectCast(x, RPrint).GetPrintContent, 0)
                Catch ex As Exception
                    Return Internal.debug.stop(ex, env)
                End Try
            ElseIf type Is GetType(Message) Then
                Return x
            Else
                Dim err = printer.printInternal(x, "", opts, env:=globalEnv)

                If Not err Is Nothing Then
                    Return err
                End If
            End If

            Return x
        End Function

        <ExportAPI("factors")>
        Public Function factors(x As String()) As Integer()
            Dim index As New Index(Of String)(base:=1)
            Dim i As New List(Of Integer)

            For Each str As String In x
                If index.IndexOf(str) = -1 Then
                    Call index.Add(str)
                End If

                Call i.Add(index.IndexOf(str))
            Next

            Return i.ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x"><see cref="ISaveHandle"/> object or any other object.</param>
        ''' <param name="dispose">
        ''' the dispose handler, for type of parameter x is <see cref="ISaveHandle"/>,
        ''' this parameter value should be a file path. for other type, this parameter
        ''' value should be a function object.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("auto")>
        <RApiReturn(GetType(RDispose))>
        Public Function autoDispose(x As Object, Optional dispose As Object = Nothing, Optional env As Environment = Nothing) As Object
            ' using a as x :> auto(o -> ...) {
            '    ...
            ' }
            Dim final As Action(Of Object)

            If dispose Is Nothing Then
                final = Sub()
                            ' do nothing
                        End Sub
            ElseIf TypeOf dispose Is Action Then
                final = Sub() Call DirectCast(dispose, Action)()
            ElseIf TypeOf dispose Is Action(Of Object) Then
                final = DirectCast(dispose, Action(Of Object))
            ElseIf TypeOf dispose Is RMethodInfo Then
                final = Sub(obj)
                            Call DirectCast(dispose, RMethodInfo).Invoke(env, invokeArgument(obj))
                        End Sub
            ElseIf dispose.GetType.ImplementInterface(GetType(RFunction)) Then
                final = Sub(obj)
                            Call DirectCast(dispose, RFunction).Invoke(env, invokeArgument(obj))
                        End Sub
            ElseIf TypeOf dispose Is String Then
                If TypeOf x Is vbObject Then
                    x = DirectCast(x, vbObject).target
                End If

                If x.GetType.ImplementInterface(GetType(ISaveHandle)) Then
                    Return New AutoFileSave With {
                        .data = x,
                        .filePath = dispose
                    }
                Else
                    Return Internal.debug.stop(New InvalidProgramException(dispose.GetType.FullName), env)
                End If
            Else
                Return Internal.debug.stop(New InvalidProgramException(dispose.GetType.FullName), env)
            End If

            Return New RDispose(x, final)
        End Function

        ''' <summary>
        ''' # Loading/Attaching and Listing of Packages
        ''' 
        ''' library and require load and attach add-on packages.
        ''' </summary>
        ''' <param name="package">the name Of a package, given As a name Or 
        ''' literal character String, Or a character String, depending On 
        ''' whether character.only Is False (Default) Or True.</param>
        ''' <param name="env"></param>
        ''' <returns>Normally library returns (invisibly) the list of attached 
        ''' packages, but TRUE or FALSE if logical.return is TRUE. When 
        ''' called as library() it returns an object of class "libraryIQR", 
        ''' and for library(help=), one of class "packageInfo".
        ''' 
        ''' require returns(invisibly) a logical indicating whether the required 
        ''' package Is available.</returns>
        ''' <remarks>
        ''' library(package) and require(package) both load the namespace of 
        ''' the package with name package and attach it on the search list. 
        ''' require is designed for use inside other functions; it returns 
        ''' FALSE and gives a warning (rather than an error as library() does 
        ''' by default) if the package does not exist. Both functions check 
        ''' and update the list of currently attached packages and do not 
        ''' reload a namespace which is already loaded. (If you want to 
        ''' reload such a package, call detach(unload = TRUE) or unloadNamespace 
        ''' first.) If you want to load a package without attaching it on 
        ''' the search list, see requireNamespace.
        ''' 
        ''' To suppress messages during the loading of packages use 
        ''' suppressPackageStartupMessages: this will suppress all messages from 
        ''' R itself but Not necessarily all those from package authors.
        ''' 
        ''' If library Is called With no package Or help argument, it lists all 
        ''' available packages In the libraries specified by Lib.loc, And returns 
        ''' the corresponding information In an Object Of Class "libraryIQR". 
        ''' (The Structure Of this Class may change In future versions.) Use 
        ''' .packages(all = True) To obtain just the names Of all available packages, 
        ''' And installed.packages() For even more information.
        ''' 
        ''' library(help = somename) computes basic information about the package 
        ''' somename, And returns this in an object of class "packageInfo". 
        ''' (The structure of this class may change in future versions.) When 
        ''' used with the default value (NULL) for lib.loc, the attached packages 
        ''' are searched before the libraries.
        ''' </remarks>
        <ExportAPI("library")>
        Public Function library(package As String,
                                Optional quietly As Boolean = False,
                                Optional env As Environment = Nothing) As Object

            Dim require As Expression

            If package.DirectoryExists Then
                ' hot load of a package directory
                Dim meta As DESCRIPTION = Nothing
                Dim errMsg As Message = PackageLoader2.Hotload(package, env.globalEnvironment, meta)

                If Not errMsg Is Nothing Then
                    Return errMsg
                Else
                    ' this meta data object is required for
                    ' run package document build
                    Return meta
                End If
            ElseIf package.FileExists Then
                Return env.globalEnvironment.Rscript.attachPackageFile(zip:=package, quietly)
            ElseIf package.IndexOf(":"c) > -1 OrElse package.IndexOf("/"c) > -1 Then
                With package.StringSplit("[:/]+")
                    require = New [Imports](.Last, .First)
                End With
            Else
                require = New Require(package)
            End If

            ' require/imports ... from ...
            Dim err As Object = require.Evaluate(env)

            If Program.isException(err) Then
                Return err
            ElseIf RType.TypeOf(err).mode = TypeCodes.boolean AndAlso base.isFALSE(err) Then
                ' 20240816
                ' require returns false if load package error
                ' the error message was attached inside the logical vector
                ' with attribute name ``error``.
                err = TryCast(err, vector).getAttribute("error")
                Return err
            Else
                Return env.globalEnvironment _
                    .attachedNamespace _
                    .packageNames
            End If
        End Function

        ''' <summary>
        ''' attact a given zip archive file as package
        ''' </summary>
        ''' <param name="R"></param>
        ''' <param name="zip">the file path to the zip package</param>
        ''' <returns></returns>
        <Extension>
        Public Function attachPackageFile(R As RInterpreter, zip As String, quietly As Boolean) As Object
            Dim mount_zip_fs As New ZipStream(zip, is_readonly:=True)
            ' maybe in format like: packageName_version
            Dim guess_pkgName As String = zip.BaseName
            Dim err As Message = PackageLoader2.LoadPackage(mount_zip_fs, guess_pkgName,
                                                            quietly:=quietly,
                                                            env:=R.globalEnvir)
            Return err
        End Function
    End Module
End Namespace
