#Region "Microsoft.VisualBasic::767cb5bb368244719a127d0c25cccc65, R#\Runtime\Internal\internalInvokes\base.vb"

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

'     Module base
' 
'         Function: [stop], allocate, append, autoDispose, cat
'                   cbind, colnames, createDotNetExceptionMessage, CreateMessageInternal, doPrintInternal
'                   getEnvironmentStack, getOption, head, invisible, isEmpty
'                   isNull, length, names, ncol, neg
'                   nrow, options, print, rbind, Rdataframe
'                   rep, replace, Rlist, rownames, sink
'                   source, str, summary, t, unitOfT
'                   warning
' 
'         Sub: q, quit
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.System.Components
Imports SMRUCC.Rsharp.System.Configuration
Imports devtools = Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports RObj = SMRUCC.Rsharp.Runtime.Internal.Object
Imports vector = SMRUCC.Rsharp.Runtime.Internal.Object.vector

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' 在这个模块之中仅包含有最基本的数据操作函数
    ''' </summary>
    Public Module base

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
        ''' <param name="times">an integer-valued vector giving the (non-negative) 
        ''' number of times to repeat each element if of length length(x), or to 
        ''' repeat the whole vector if of length 1. Negative or NA values are an 
        ''' error. A double vector is accepted, other inputs being coerced to an 
        ''' integer or double vector.</param>
        ''' <returns></returns>
        <ExportAPI("rep")>
        Public Function rep(x As Object, times As Integer) As Object
            Return Repeats(x, times)
        End Function

        <ExportAPI("rbind")>
        Public Function rbind(d As dataframe, <RRawVectorArgument> row As Object, env As Environment) As dataframe
            Throw New NotImplementedException
        End Function

        <ExportAPI("cbind")>
        Public Function cbind(d As dataframe, <RRawVectorArgument> col As Object, env As Environment) As dataframe
            If col Is Nothing Then
                Return d
            End If

            If TypeOf col Is list Then
                With DirectCast(col, list)
                    Dim value As Object

                    For Each name As String In .slots.Keys
                        value = .slots(name)

                        If Not value Is Nothing Then
                            If TypeOf value Is Array Then
                                d.columns.Add(name, DirectCast(value, Array))
                            Else
                                d.columns.Add(name, {value})
                            End If
                        End If
                    Next
                End With
            ElseIf TypeOf col Is Array Then
                d.columns.Add($"X{d.columns.Count + 2}", DirectCast(col, Array))
            ElseIf TypeOf col Is vector Then
                d.columns.Add($"X{d.columns.Count + 2}", DirectCast(col, vector).data)
            Else
                d.columns.Add($"X{d.columns.Count + 2}", {col})
            End If

            Return d
        End Function

        ''' <summary>
        ''' matrix transpose
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <ExportAPI("t")>
        Public Function t(x As dataframe) As Object
            Dim rownames = x.getRowNames
            Dim colnames = x.columns.Keys.ToArray
            Dim mat = colnames.Select(Function(k) x.columns(k).AsObjectEnumerator(Of Object).ToArray).MatrixTranspose.ToArray
            Dim d As New dataframe With {
                .rownames = colnames,
                .columns = New Dictionary(Of String, Array)
            }

            For i As Integer = 0 To rownames.Length - 1
                d.columns.Add(rownames(i), mat(i))
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
        ''' get or set unit to a given vector
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="unit"></param>
        ''' <returns></returns>
        <ExportAPI("unit")>
        Public Function unitOfT(<RRawVectorArgument> x As Object,
                                <RByRefValueAssign>
                                Optional unit As Object = Nothing,
                                Optional env As Environment = Nothing) As Object

            If unit Is Nothing Then
                If TypeOf x Is vector Then
                    Return DirectCast(x, vector).unit
                Else
                    Return Nothing
                End If
            Else
                If TypeOf unit Is vbObject Then
                    unit = DirectCast(unit, vbObject).target
                End If
                If TypeOf unit Is String Then
                    unit = New unit With {.name = unit}
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
                x = New vector With {.data = {}, .unit = unit}
            Else
                DirectCast(x, vector).unit = unit
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
        Public Function invisible(x As Object) As <RSuppressPrint> Object
            Return New invisible With {.value = x}
        End Function

        <ExportAPI("neg")>
        Public Function neg(<RRawVectorArgument> o As Object) As Object
            If o Is Nothing Then
                Return Nothing
            Else
                Return Runtime.asVector(Of Double)(o) _
                    .AsObjectEnumerator _
                    .Select(Function(d) -CDbl(d)) _
                    .ToArray
            End If
        End Function

        <ExportAPI("append")>
        Public Function append(<RRawVectorArgument> x As Object, <RRawVectorArgument> values As Object, Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return values
            ElseIf values Is Nothing Then
                Return values
            End If

            Throw New NotImplementedException
        End Function

        ''' <summary>
        ''' #### Data Frames
        ''' 
        ''' The function data.frame() creates data frames, tightly coupled collections 
        ''' of variables which share many of the properties of matrices and of lists, 
        ''' used as the fundamental data structure by most of R's modeling software.
        ''' </summary>
        ''' <param name="columns">
        ''' these arguments are of either the form value or tag = value. Component names 
        ''' are created based on the tag (if present) or the deparsed argument itself.
        ''' </param>
        ''' <param name="envir"></param>
        ''' <returns>
        ''' A data frame, a matrix-like structure whose columns may be of differing types 
        ''' (numeric, logical, factor and character and so on).
        '''
        ''' How the names Of the data frame are created Is complex, And the rest Of this 
        ''' paragraph Is only the basic story. If the arguments are all named And simple 
        ''' objects (Not lists, matrices Of data frames) Then the argument names give the 
        ''' column names. For an unnamed simple argument, a deparsed version Of the 
        ''' argument Is used As the name (With an enclosing I(...) removed). For a named 
        ''' matrix/list/data frame argument With more than one named column, the names Of 
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
        Public Function Rdataframe(<RListObjectArgument>
                                   <RRawVectorArgument>
                                   columns As Object, Optional envir As Environment = Nothing) As Object

            ' data.frame(a = 1, b = ["g","h","eee"], c = T)
            Dim parameters As InvokeParameter() = columns
            Dim values As IEnumerable(Of NamedValue(Of Object)) = parameters _
                .SeqIterator _
                .Select(Function(a)
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
                        End Function)

            Return values.RDataframe(envir)
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
        ''' <returns></returns>
        <ExportAPI("list")>
        <RApiReturn(GetType(list))>
        Public Function Rlist(<RListObjectArgument, RRawVectorArgument> slots As Object, Optional envir As Environment = Nothing) As Object
            Dim list As New Dictionary(Of String, Object)
            Dim slot As InvokeParameter
            Dim key As String
            Dim value As Object
            Dim parameters As InvokeParameter() = slots

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
                    value = slot.Evaluate(envir)
                End If

                If Program.isException(value) Then
                    Return value
                Else
                    list.Add(key, value)
                End If
            Next

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

            If Program.isException(argumentsVal) Then
                Return argumentsVal
            Else
                ' summary is similar to str or print function
                ' but summary just returns simple data summary information
                ' and str function returns the data structure information
                ' about the given dataset object.
                ' the print function is print the data details
                Return DirectCast(argumentsVal, list).invokeGeneric([object], env)
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
            ElseIf type.ImplementInterface(GetType(RIndex)) Then
                Return DirectCast(x, RIndex).length = 0
            Else
                Return False
            End If
        End Function

        <ExportAPI("is.null")>
        Public Function isNull(x As Object) As Boolean
            Return x Is Nothing
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
                             Optional env As Environment = Nothing) As Object

            Dim stdout As RContentOutput = env.globalEnvironment.stdout

            If Not file.StringEmpty Then
                ' 打开一个新的会话用于保存输出日志
                Call stdout.openSink(file, split, append)
            Else
                ' 结束当前的日志会话
                Call stdout.closeSink()
            End If

            Return Nothing
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
            Else
                Dim type As Type = x.GetType

                If type.IsArray Then
                    Return DirectCast(x, Array).Length
                ElseIf type.ImplementInterface(GetType(RIndex)) Then
                    Return DirectCast(x, RIndex).length
                ElseIf type.ImplementInterface(GetType(IDictionary)) Then
                    Return DirectCast(x, IDictionary).Count
                ElseIf type Is GetType(dataframe) Then
                    Return DirectCast(x, dataframe).ncols
                ElseIf type Is GetType(Group) Then
                    Return DirectCast(x, Group).length
                Else
                    Return 1
                End If
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
            Dim R As RInterpreter = envir.globalEnvironment.Rscript
            Dim result As Object = R.Source(path, args)

            Return New invisible() With {
                .value = result
            }
        End Function

        <ExportAPI("getOption")>
        <RApiReturn(GetType(String))>
        Public Function getOption(name$,
                                  Optional defaultVal$ = Nothing,
                                  Optional envir As Environment = Nothing) As Object

            If name.StringEmpty Then
                Return invoke.missingParameter(NameOf(getOption), "name", envir)
            Else
                Return envir.globalEnvironment _
                    .options _
                    .getOption(name, defaultVal)
            End If
        End Function

        ''' <summary>
        ''' ###### Options Settings
        ''' 
        ''' Allow the user to set and examine a variety of global options which 
        ''' affect the way in which R computes and displays its results.
        ''' </summary>
        ''' <param name="opts">
        ''' any options can be defined, using name = value. However, only the ones below are used in base R.
        ''' Options can also be passed by giving a Single unnamed argument which Is a named list.
        ''' </param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("options")>
        Public Function options(<RListObjectArgument> opts As Object, envir As Environment) As Object
            Dim configs As Options = envir.globalEnvironment.options
            Dim values As list
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
                        configs.setOption(value.Key, value.Value)
                    Catch ex As Exception
                        Return Internal.debug.stop(ex, envir)
                    End Try
                Next
            ElseIf type.IsArray AndAlso DirectCast(opts, Array).Length = 0 Then
                ' get all options
                values = RConversion.asList(configs.getAllConfigs, New InvokeParameter() {}, envir)
            Else
                values = New list With {
                    .slots = New Dictionary(Of String, Object)
                }

                ' invoke parameters
                For Each value As InvokeParameter In DirectCast(opts, InvokeParameter())
                    Dim name = value.name
                    Dim cfgValue As Object = value.Evaluate(envir)

                    Try
                        values.slots(name) = configs.setOption(name, Scripting.ToString(cfgValue))
                    Catch ex As Exception
                        Return Internal.debug.stop(ex, envir)
                    End Try
                Next
            End If

            Return values
        End Function

        ''' <summary>
        ''' # The Names of an Object
        ''' 
        ''' Functions to get or set the names of an object.
        ''' </summary>
        ''' <param name="[object]">an R object.</param>
        ''' <param name="namelist">a character vector of up to the same length as ``x``, or ``NULL``.</param>
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
        <ExportAPI("names")>
        Public Function names(<RRawVectorArgument> [object] As Object,
                              <RByRefValueAssign>
                              Optional namelist As Array = Nothing,
                              Optional envir As Environment = Nothing) As Object

            If namelist Is Nothing OrElse namelist.Length = 0 Then
                Return RObj.names.getNames([object], envir)
            Else
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
        Public Function makeNames(<RRawVectorArgument> names As Object, Optional unique As Boolean = False, Optional allow_ As Boolean = True) As Object
            Dim nameList As String() = asVector(Of String)(names)
            Dim nameUniques As New Dictionary(Of String, Counter)
            Dim nameAll As New List(Of String)

            For Each name As String In nameList
                name = name _
                    .Select(Function(c)
                                If c = "_"c AndAlso allow_ Then
                                    Return c
                                Else
                                    If c >= "a"c AndAlso c <= "z"c Then
                                        Return c
                                    ElseIf c >= "A"c AndAlso c <= "Z"c Then
                                        Return c
                                    ElseIf c >= "0"c AndAlso c <= "9"c Then
                                        Return c
                                    Else
                                        Return "."c
                                    End If
                                End If
                            End Function) _
                    .CharString
RE0:
                If unique AndAlso nameUniques.ContainsKey(name) Then
                    nameUniques(name).Hit()
                    name = name & nameUniques(name).Value
                    GoTo RE0
                ElseIf unique Then
                    nameUniques.Add(name, Scan0)
                Else
                    nameAll.Add(name)
                End If
            Next

            If unique Then
                Return nameUniques.Keys.ToArray
            Else
                Return nameAll.ToArray
            End If
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
                Return RObj.names.setRowNames([object], namelist, envir)
            End If
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
        <ExportAPI("colnames")>
        Public Function colnames([object] As Object,
                                 <RByRefValueAssign>
                                 Optional namelist As Array = Nothing,
                                 Optional envir As Environment = Nothing) As Object

            If [object] Is Nothing Then
                Return Nothing
            End If

            If namelist Is Nothing OrElse namelist.Length = 0 Then
                Return RObj.names.getColNames([object], envir)
            Else
                Return RObj.names.setColNames([object], namelist, envir)
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="message">
        ''' <see cref="String"/> array or <see cref="Exception"/>
        ''' </param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("stop")>
        Public Function [stop](<RRawVectorArgument> message As Object, Optional envir As Environment = Nothing) As Message
            Dim debugMode As Boolean = envir.globalEnvironment.debugMode

            If Not message Is Nothing AndAlso message.GetType.IsInheritsFrom(GetType(Exception), strict:=False) Then
                Call App.LogException(DirectCast(message, Exception), trace:=envir.getEnvironmentStack.JoinBy(vbCrLf))

                If debugMode Then
                    Throw DirectCast(message, Exception)
                Else
                    Return DirectCast(message, Exception).createDotNetExceptionMessage(envir)
                End If
            ElseIf message.GetType Is GetType(Message) Then
                If debugMode Then
                    Dim err As New Exception(DirectCast(message, Message).message.JoinBy("; "))
                    Call App.LogException(err)
                    Throw err
                Else
                    Return message
                End If
            Else
                If debugMode Then
                    Dim err As New Exception(Runtime.asVector(Of Object)(message) _
                       .AsObjectEnumerator _
                       .SafeQuery _
                       .Select(Function(o) Scripting.ToString(o, "NULL")) _
                       .JoinBy("; ")
                    )
                    Call App.LogException(err)
                    Throw err
                Else
                    Return base.CreateMessageInternal(message, envir, level:=MSG_TYPES.ERR)
                End If
            End If
        End Function

        <Extension>
        Private Function createDotNetExceptionMessage(ex As Exception, envir As Environment) As Message
            Dim messages As New List(Of String)
            Dim exception As Exception = ex

            Do While Not ex Is Nothing
                messages += ex.GetType.Name & ": " & ex.Message
                ex = ex.InnerException
            Loop

            ' add stack info for display
            If exception.StackTrace.StringEmpty Then
                messages += "stackFrames: none"
            Else
                messages += "stackFrames: " & vbCrLf & exception.StackTrace
            End If

            Return New Message With {
                .message = messages,
                .environmentStack = envir.getEnvironmentStack,
                .level = MSG_TYPES.ERR,
                .trace = devtools.ExceptionData.GetCurrentStackTrace
            }
        End Function

        <Extension>
        Friend Function getEnvironmentStack(parent As Environment) As StackFrame()
            Dim frames As New List(Of StackFrame)

            Do While Not parent Is Nothing
                frames += parent.stackFrame
                parent = parent.parent
            Loop

            Return frames
        End Function

        ''' <summary>
        ''' Create R# internal message
        ''' </summary>
        ''' <param name="messages"></param>
        ''' <param name="envir"></param>
        ''' <param name="level">The message level</param>
        ''' <returns></returns>
        Friend Function CreateMessageInternal(messages As Object, envir As Environment, level As MSG_TYPES) As Message
            Return New Message With {
                .message = Runtime.asVector(Of Object)(messages) _
                    .AsObjectEnumerator _
                    .SafeQuery _
                    .Select(Function(o) Scripting.ToString(o, "NULL")) _
                    .ToArray,
                .level = level,
                .environmentStack = envir.getEnvironmentStack,
                .trace = devtools.ExceptionData.GetCurrentStackTrace
            }
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
        <ExportAPI("warning")>
        <DebuggerStepThrough>
        Public Function warning(<RRawVectorArgument> message As Object, Optional envir As Environment = Nothing) As Message
            Dim msg As Message = CreateMessageInternal(message, envir, level:=MSG_TYPES.WRN)
            envir.messages.Add(msg)
            Return msg
        End Function

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
                sep = sprintf(sep)
                strs = DirectCast(vec(Scan0), dataframe) _
                    .GetTable(env.globalEnvironment, printContent:=False, True) _
                    .Select(Function(row)
                                Return row.JoinBy(sep)
                            End Function) _
                    .JoinBy(vbCrLf)
            Else
                strs = vec _
                    .Select(Function(o) Scripting.ToString(o, "")) _
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
        ''' # Compactly Display the Structure of an Arbitrary ``R#`` Object
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
        ''' <param name="[object]">any R object about which you want to have some information.</param>
        ''' <returns></returns>
        <ExportAPI("str")>
        Public Function str(<RRawVectorArgument> [object] As Object, Optional env As Environment = Nothing) As Object
            Dim structure$ = reflector.GetStructure([object], env.globalEnvironment, " ")
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
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("print")>
        Public Function print(<RRawVectorArgument> x As Object, env As Environment) As Object
            If x Is Nothing Then
                Call env.globalEnvironment.stdout.WriteLine("NULL")

                ' just returns nothing literal
                Return Nothing
            Else
                Return doPrintInternal(x, x.GetType, env)
            End If
        End Function

        Private Function doPrintInternal(x As Object, type As Type, env As Environment) As Object
            Dim globalEnv As GlobalEnvironment = env.globalEnvironment
            Dim maxPrint% = globalEnv.options.maxPrint

            If type Is GetType(RMethodInfo) Then
                Call globalEnv _
                    .packages _
                    .packageDocs _
                    .PrintHelp(x, env.globalEnvironment.stdout)
            ElseIf type Is GetType(DeclareNewFunction) Then
                Call env.globalEnvironment.stdout.WriteLine(x.ToString)
            ElseIf type.ImplementInterface(GetType(RPrint)) Then
                Try
                    Call markdown.DoPrint(DirectCast(x, RPrint).GetPrintContent, 0)
                Catch ex As Exception
                    Return Internal.debug.stop(ex, env)
                End Try
            ElseIf type Is GetType(Message) Then
                Return x
            Else
                Call printer.printInternal(x, "", maxPrint, globalEnv)
            End If

            Return x
        End Function

        ''' <summary>
        ''' # Terminate an R Session
        ''' 
        ''' The function ``quit`` or its alias ``q`` terminate the current R session.
        ''' </summary>
        ''' <param name="save">
        ''' a character string indicating whether the environment (workspace) should be saved, 
        ''' one of ``"no"``, ``"yes"``, ``"ask"`` or ``"default"``.
        ''' </param>
        ''' <param name="status">
        ''' the (numerical) error status to be returned to the operating system, where relevant. 
        ''' Conventionally 0 indicates successful completion.
        ''' </param>
        ''' <param name="runLast">
        ''' should ``.Last()`` be executed?
        ''' </param>
        ''' 
        <ExportAPI("quit")>
        Public Sub quit(Optional save$ = "default",
                        Optional status% = 0,
                        Optional runLast As Boolean = True,
                        Optional envir As Environment = Nothing)

            Call base.q(save, status, runLast, envir)
        End Sub

        ''' <summary>
        ''' # Terminate an R Session
        ''' 
        ''' The function ``quit`` or its alias ``q`` terminate the current R session.
        ''' </summary>
        ''' <param name="save">
        ''' a character string indicating whether the environment (workspace) should be saved, 
        ''' one of ``"no"``, ``"yes"``, ``"ask"`` or ``"default"``.
        ''' </param>
        ''' <param name="status">
        ''' the (numerical) error status to be returned to the operating system, where relevant. 
        ''' Conventionally 0 indicates successful completion.
        ''' </param>
        ''' <param name="runLast">
        ''' should ``.Last()`` be executed?
        ''' </param>
        ''' 
        <ExportAPI("q")>
        Public Sub q(Optional save$ = "default",
                     Optional status% = 0,
                     Optional runLast As Boolean = True,
                     Optional envir As Environment = Nothing)

            Call Console.Write("Save workspace image? [y/n/c]: ")

            Dim input As String = Console.ReadLine.Trim(ASCII.CR, ASCII.LF, " "c, ASCII.TAB)

            If input = "c" Then
                ' cancel
                Return
            End If

            If input = "y" Then
                ' save image for yes
                Dim saveImage As Symbol = envir.FindSymbol("save.image")

                If Not saveImage Is Nothing AndAlso TypeOf saveImage.value Is RMethodInfo Then
                    Call DirectCast(saveImage.value, RMethodInfo).Invoke(envir, {})
                End If
            Else
                ' do nothing for no
            End If

            If runLast Then
                Dim last = envir.FindSymbol(".Last")

                If Not last Is Nothing Then
                    Call DirectCast(last, RFunction).Invoke(envir, {})
                End If
            End If

            Call App.Exit(status)
        End Sub

        ''' <summary>
        ''' force quit of current R# session without confirm
        ''' </summary>
        ''' <param name="status"></param>
        ''' 
        <ExportAPI("exit")>
        Public Sub [exit](status As Integer)
            Call App.Exit(status)
        End Sub

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
    End Module
End Namespace
