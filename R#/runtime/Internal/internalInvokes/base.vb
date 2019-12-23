#Region "Microsoft.VisualBasic::dd2783a55f8243d5d171b3885d21e363, R#\Runtime\Internal\internalInvokes\base.vb"

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
'         Function: [get], [stop], all, any, cat
'                   createDotNetExceptionMessage, createMessageInternal, doCall, doPrintInternal, getEnvironmentStack
'                   getOption, globalenv, isEmpty, lapply, length
'                   names, neg, options, print, sapply
'                   source, str, warning
' 
'         Sub: cls, q, quit
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.System.Configuration
Imports devtools = Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' 在这个模块之中仅包含有最基本的数据操作函数
    ''' </summary>
    Public Module base

        <ExportAPI("console.cls")>
        Public Sub cls()
            Call Console.Clear()
        End Sub

        <ExportAPI("do.call")>
        Public Function doCall(what As Object, calls$, envir As Environment) As Object
            If what Is Nothing OrElse calls.StringEmpty Then
                Return Internal.stop("Nothing to call!", envir)
            End If

            Dim targetType As Type = what.GetType

            If targetType Is GetType(vbObject) Then
                Dim member = DirectCast(what, vbObject).getByName(name:=calls)

                If member.GetType Is GetType(RMethodInfo) Then
                    Return DirectCast(member, RMethodInfo).Invoke(envir, {})
                Else
                    Return member
                End If
            Else
                Return Internal.stop(New NotImplementedException(targetType.FullName), envir)
            End If
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

        <ExportAPI("is.empty")>
        Friend Function isEmpty(<RRawVectorArgument> o As Object) As Object
            If o Is Nothing Then
                Return True
            End If

            Dim type As Type = o.GetType

            If type Is GetType(String) Then
                Return DirectCast(o, String).StringEmpty(False)
            ElseIf type Is GetType(String()) Then
                With DirectCast(o, String())
                    If .Length > 1 Then
                        Return False
                    ElseIf .Length = 0 OrElse .First.StringEmpty(False) Then
                        Return True
                    Else
                        Return False
                    End If
                End With
            ElseIf type.IsArray Then
                With DirectCast(o, Array)
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
                Return DirectCast(o, RIndex).length = 0
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Get vector length
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("length")>
        Public Function length(<RRawVectorArgument> x As Object) As Integer
            If x Is Nothing Then
                Return 0
            ElseIf x.GetType.IsArray Then
                Return DirectCast(x, Array).Length
            ElseIf x.GetType.ImplementInterface(GetType(RIndex)) Then
                Return DirectCast(x, RIndex).length
            Else
                Return 1
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("any")>
        Public Function any(<RRawVectorArgument> test As Object) As Object
            Return Runtime.asLogical(test).Any(Function(b) b = True)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("all")>
        Public Function all(<RRawVectorArgument> test As Object) As Object
            Return Runtime.asLogical(test).All(Function(b) b = True)
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
                .ToArray
            Dim R As RInterpreter = envir.globalEnvironment.Rscript

            Return R.Source(path, args)
        End Function

        <ExportAPI("getOption")>
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

            If opts.GetType Is GetType(String()) Then
                values = New list With {
                    .slots = DirectCast(opts, String()) _
                        .ToDictionary(Function(key) key,
                                      Function(key)
                                          Return CObj(configs.getOption(key, ""))
                                      End Function)
                }
            Else
                values = DirectCast(opts, list)

                For Each value As KeyValuePair(Of String, Object) In values.slots
                    Try
                        configs.setOption(value.Key, value.Value)
                    Catch ex As Exception
                        Return Internal.stop(ex, envir)
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
        Public Function names([object] As Object, Optional namelist As Array = Nothing, Optional envir As Environment = Nothing) As Object
            If namelist Is Nothing OrElse namelist.Length = 0 Then
                Return Internal.names.getNames([object], envir)
            Else
                Return Internal.names.setNames([object], namelist, envir)
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
        Public Function rownames([object] As Object, Optional namelist As Array = Nothing, Optional envir As Environment = Nothing) As Object
            If namelist Is Nothing OrElse namelist.Length = 0 Then
                Return Internal.names.getNames([object], envir)
            Else
                Return Internal.names.setNames([object], namelist, envir)
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
        Public Function colnames([object] As Object, Optional namelist As Array = Nothing, Optional envir As Environment = Nothing) As Object
            If namelist Is Nothing OrElse namelist.Length = 0 Then
                Return Internal.names.getNames([object], envir)
            Else
                Return Internal.names.setNames([object], namelist, envir)
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
                If debugMode Then
                    Throw DirectCast(message, Exception)
                Else
                    Return DirectCast(message, Exception).createDotNetExceptionMessage(envir)
                End If
            Else
                If debugMode Then
                    Throw New Exception(Runtime.asVector(Of Object)(message) _
                       .AsObjectEnumerator _
                       .SafeQuery _
                       .Select(Function(o) Scripting.ToString(o, "NULL")) _
                       .JoinBy("; ")
                    )
                Else
                    Return base.createMessageInternal(message, envir, level:=MSG_TYPES.ERR)
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
                frames += New StackFrame With {
                    .Method = New Method With {
                        .Method = parent.stackTag
                    }
                }
                parent = parent.parent
            Loop

            Return frames
        End Function

        Private Function createMessageInternal(messages As Object, envir As Environment, level As MSG_TYPES) As Message
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

        <ExportAPI("warning")>
        <DebuggerStepThrough>
        Public Function warning(<RRawVectorArgument> message As Object, Optional envir As Environment = Nothing) As Message
            Return createMessageInternal(message, envir, level:=MSG_TYPES.WRN)
        End Function

        <ExportAPI("cat")>
        Public Function cat(<RRawVectorArgument> values As Object,
                            Optional file$ = Nothing,
                            Optional sep$ = " ") As Object

            Dim strs = Runtime.asVector(Of Object)(values) _
                .AsObjectEnumerator _
                .Select(Function(o) Scripting.ToString(o, "")) _
                .JoinBy(sep) _
                .DoCall(AddressOf sprintf)

            If Not file.StringEmpty Then
                Call strs.SaveTo(file)
            Else
                Call Console.Write(strs)
            End If

            Return strs
        End Function

        <ExportAPI("str")>
        Public Function str(<RRawVectorArgument> x As Object, Optional envir As Environment = Nothing) As Object
            Dim print As String = classPrinter.printClass(x)
            Console.WriteLine(print)
            Return print
        End Function

        Dim markdown As MarkdownRender = MarkdownRender.DefaultStyleRender

        <ExportAPI("print")>
        Public Function print(<RRawVectorArgument> x As Object, envir As Environment) As Object
            If x Is Nothing Then
                Call Console.WriteLine("NULL")

                ' just returns nothing literal
                Return Nothing
            Else
                Return doPrintInternal(x, x.GetType, envir)
            End If
        End Function

        Private Function doPrintInternal(x As Object, type As Type, envir As Environment) As Object
            Dim globalEnv As GlobalEnvironment = envir.globalEnvironment
            Dim maxPrint% = globalEnv.options.maxPrint

            If type Is GetType(RMethodInfo) Then
                Call globalEnv _
                    .packages _
                    .packageDocs _
                    .PrintHelp(x)
            ElseIf type.ImplementInterface(GetType(RPrint)) Then
                Try
                    Call markdown.DoPrint(DirectCast(x, RPrint).GetPrintContent, 0)
                Catch ex As Exception
                    Return Internal.stop(ex, envir)
                End Try
            ElseIf type Is GetType(Message) Then
                Return x
            ElseIf type Is GetType(list) Then
                Call DirectCast(x, list) _
                    .slots _
                    .DoCall(Sub(list)
                                printer.printInternal(list, "", maxPrint, globalEnv)
                            End Sub)
            Else
                Call printer.printInternal(x, "", maxPrint, globalEnv)
            End If

            Return x
        End Function

        <ExportAPI("lapply")>
        Public Function lapply(<RRawVectorArgument> sequence As Object, doApply As Object, envir As Environment) As Object
            If doApply Is Nothing Then
                Return Internal.stop({"Missing apply function!"}, envir)
            ElseIf Not doApply.GetType.ImplementInterface(GetType(RFunction)) Then
                Return Internal.stop({"Target is not a function!"}, envir)
            End If

            If Program.isException(sequence) Then
                Return sequence
            ElseIf Program.isException(doApply) Then
                Return doApply
            End If

            Dim apply As RFunction = doApply
            Dim list As Dictionary(Of String, Object)

            If sequence.GetType Is GetType(Dictionary(Of String, Object)) Then
                list = DirectCast(sequence, Dictionary(Of String, Object)) _
                    .ToDictionary(Function(d) d.Key,
                                  Function(d)
                                      Return apply.Invoke(envir, {d.Value})
                                  End Function)
            Else
                list = Runtime.asVector(Of Object)(sequence) _
                    .AsObjectEnumerator _
                    .SeqIterator _
                    .ToDictionary(Function(i) $"[[{i.i}]]",
                                  Function(d)
                                      Return apply.Invoke(envir, {d.value})
                                  End Function)
            End If

            Return New list With {.slots = list}
        End Function

        <ExportAPI("sapply")>
        Public Function sapply(<RRawVectorArgument> sequence As Object, doApply As Object, envir As Environment) As Object
            If doApply Is Nothing Then
                Return Internal.stop({"Missing apply function!"}, envir)
            ElseIf Not doApply.GetType.ImplementInterface(GetType(RFunction)) Then
                Return Internal.stop({"Target is not a function!"}, envir)
            End If

            If Program.isException(sequence) Then
                Return sequence
            ElseIf Program.isException(doApply) Then
                Return doApply
            End If

            Dim apply As RFunction = doApply

            If sequence.GetType Is GetType(Dictionary(Of String, Object)) Then
                Dim list = DirectCast(sequence, Dictionary(Of String, Object))
                Dim names = list.Keys.ToArray
                Dim seq As Array = names _
                    .Select(Function(key)
                                Return apply.Invoke(envir, {list(key)})
                            End Function) _
                    .ToArray

                Return New vector(names, seq, envir)
            Else
                Dim seq = Runtime.asVector(Of Object)(sequence) _
                    .AsObjectEnumerator _
                    .Select(Function(d)
                                Return apply.Invoke(envir, {d})
                            End Function) _
                    .ToArray

                Return New vector With {.data = seq}
            End If
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

            Else

            End If

            If runLast Then
                Dim last = envir.FindSymbol(".Last")

                If Not last Is Nothing Then
                    Call DirectCast(last, RFunction).Invoke(envir, {})
                End If
            End If

            Call App.Exit(status)
        End Sub
    End Module
End Namespace
