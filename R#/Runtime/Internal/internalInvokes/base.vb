#Region "Microsoft.VisualBasic::fac315e9169ab766b7637706131dc937, R#\Runtime\Internal\internalInvokes\base.vb"

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
'         Function: [stop], all, any, append, cat
'                   colnames, createDotNetExceptionMessage, CreateMessageInternal, doPrintInternal, getEnvironmentStack
'                   getOption, invisible, invokeArgument, isEmpty, lapply
'                   length, names, ncol, neg, nrow
'                   options, print, Rdataframe, rep, replace
'                   Rlist, rownames, sapply, source, str
'                   summary, warning
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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
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

        <ExportAPI("unit")>
        Public Function unitOfT(x As vector, unit As unit) As vector
            x.unit = unit
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
            Return x
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

        <ExportAPI("nrow")>
        <RApiReturn(GetType(Integer))>
        Public Function nrow(x As Object, Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return 0
            ElseIf x.GetType Is GetType(dataframe) Then
                Return DirectCast(x, dataframe).nrows
            Else
                Return Internal.stop(RType.GetRSharpType(x).ToString & " is not a dataframe!", env)
            End If
        End Function

        <ExportAPI("ncol")>
        <RApiReturn(GetType(Integer))>
        Public Function ncol(x As Object, Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return 0
            ElseIf x.GetType Is GetType(dataframe) Then
                Return DirectCast(x, dataframe).ncols
            Else
                Return Internal.stop(RType.GetRSharpType(x).ToString & " is not a dataframe!", env)
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

            ' summary is similar to str or print function
            ' but summary just returns simple data summary information
            ' and str function returns the data structure information
            ' about the given dataset object.
            ' the print function is print the data details
            Return DirectCast(Rlist(args, env), list).invokeGeneric([object], env)
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
            ElseIf x.GetType.IsArray Then
                Return DirectCast(x, Array).Length
            ElseIf x.GetType.ImplementInterface(GetType(RIndex)) Then
                Return DirectCast(x, RIndex).length
            ElseIf x.GetType.ImplementInterface(GetType(IDictionary)) Then
                Return DirectCast(x, IDictionary).Count
            ElseIf x.GetType Is GetType(dataframe) Then
                Return DirectCast(x, dataframe).ncols
            Else
                Return 1
            End If
        End Function

        ''' <summary>
        ''' # Are Some Values True?
        ''' 
        ''' Given a set of logical vectors, is at least one of the values true?
        ''' </summary>
        ''' <param name="test">
        ''' zero or more logical vectors. Other objects of zero length are ignored, 
        ''' and the rest are coerced to logical ignoring any class.
        ''' </param>
        ''' <param name="narm">
        ''' logical. If true NA values are removed before the result Is computed.
        ''' </param>
        ''' <returns>
        ''' The value is a logical vector of length one.
        '''
        ''' Let x denote the concatenation of all the logical vectors in ... 
        ''' (after coercion), after removing NAs if requested by na.rm = TRUE.
        ''' 
        ''' The value returned Is True If at least one Of the values In x Is True, 
        ''' And False If all Of the values In x are False (including If there are 
        ''' no values). Otherwise the value Is NA (which can only occur If 
        ''' na.rm = False And ... contains no True values And at least one NA 
        ''' value).
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("any")>
        Public Function any(<RRawVectorArgument> test As Object, Optional narm As Boolean = False) As Boolean
            Return Runtime.asLogical(test).Any(Function(b) b = True)
        End Function

        ''' <summary>
        ''' # Are All Values True?
        ''' 
        ''' Given a set of logical vectors, are all of the values true?
        ''' </summary>
        ''' <param name="test">zero or more logical vectors. Other objects of zero 
        ''' length are ignored, and the rest are coerced to logical ignoring any 
        ''' class.</param>
        ''' <param name="narm">
        ''' logical. If true NA values are removed before the result is computed.
        ''' </param>
        ''' <returns>
        ''' The value is a logical vector of length one.
        '''
        ''' Let x denote the concatenation of all the logical vectors in ... 
        ''' (after coercion), after removing NAs if requested by na.rm = TRUE.
        '''
        ''' The value returned Is True If all Of the values In x are True 
        ''' (including If there are no values), And False If at least one Of 
        ''' the values In x Is False. Otherwise the value Is NA (which can 
        ''' only occur If na.rm = False And ... contains no False values And 
        ''' at least one NA value).
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("all")>
        Public Function all(<RRawVectorArgument> test As Object, Optional narm As Boolean = False) As Boolean
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
                .Where(Function(a) a.Name <> path) _
                .ToArray
            Dim R As RInterpreter = envir.globalEnvironment.Rscript

            Return R.Source(path, args)
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
                        Return Internal.stop(ex, envir)
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
        Public Function names([object] As Object,
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
            Else
                Call Console.Write(strs)
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
            Call Console.WriteLine(reflector.GetStructure([object], env.globalEnvironment, " "))
            Return Nothing
        End Function

        Dim markdown As MarkdownRender = MarkdownRender.DefaultStyleRender

        <ExportAPI("head")>
        Public Function head(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return x
            ElseIf x.GetType.IsArray Then
                x = New vector With {.data = x}
            ElseIf x.GetType.ImplementInterface(GetType(IDictionary(Of String, Object))) Then
                x = New list With {.slots = x}
            End If

            Dim type As Type = x.GetType
            Dim length% = 6

            If type Is GetType(vector) Then
                Dim v As vector = DirectCast(x, vector)

                If v.length <= length Then
                    Return x
                Else
                    Dim data As Array = Array.CreateInstance(v.type.raw, length)

                    For i As Integer = 0 To data.Length - 1
                        data.SetValue(v.data.GetValue(i), i)
                    Next

                    Return New vector With {.data = data}
                End If
            ElseIf type Is GetType(list) Then
                Dim l As list = DirectCast(x, list)

                If l.length <= length Then
                    Return l
                Else
                    Return New list With {
                        .slots = l.slots.Keys _
                            .Take(length) _
                            .ToDictionary(Function(key) key,
                                          Function(key)
                                              Return l.slots(key)
                                          End Function)
                    }
                End If
            ElseIf type Is GetType(dataframe) Then
                Dim df As dataframe = DirectCast(x, dataframe)

                If df.nrows <= length Then
                    Return df
                Else
                    Dim data As New Dictionary(Of String, Array)
                    Dim colVal As Array
                    Dim colSubset As Array

                    For Each col In df.columns
                        If col.Value.Length = 1 Then
                            data.Add(col.Key, col.Value)
                        Else
                            colVal = col.Value
                            colSubset = Array.CreateInstance(colVal.GetType.GetElementType, length)

                            For i As Integer = 0 To length - 1
                                colSubset.SetValue(colVal.GetValue(i), i)
                            Next

                            data.Add(col.Key, colSubset)
                        End If
                    Next

                    Return New dataframe With {
                        .columns = data,
                        .rownames = df.rownames _
                            .SafeQuery _
                            .Take(length) _
                            .ToArray
                    }
                End If
            Else
                Return x
            End If
        End Function

        ''' <summary>
        ''' # Print Values
        ''' 
        ''' print prints its argument and returns it invisibly (via invisible(x)). 
        ''' It is a generic function which means that new printing methods can be 
        ''' easily added for new classes.
        ''' </summary>
        ''' <param name="x">an object used to select a method.</param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
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
            ElseIf type Is GetType(DeclareNewFunction) Then
                Call Console.WriteLine(x.ToString)
            ElseIf type.ImplementInterface(GetType(RPrint)) Then
                Try
                    Call markdown.DoPrint(DirectCast(x, RPrint).GetPrintContent, 0)
                Catch ex As Exception
                    Return Internal.stop(ex, envir)
                End Try
            ElseIf type Is GetType(Message) Then
                Return x
            Else
                Call printer.printInternal(x, "", maxPrint, globalEnv)
            End If

            Return x
        End Function

        ''' <summary>
        ''' # Apply a Function over a List or Vector
        ''' 
        ''' lapply returns a list of the same length as X, each element of 
        ''' which is the result of applying FUN to the corresponding 
        ''' element of X.
        ''' </summary>
        ''' <param name="X">
        ''' a vector (atomic or list) or an expression object. Other objects 
        ''' (including classed objects) will be coerced by ``base::as.list``.
        ''' </param>
        ''' <param name="FUN">
        ''' the Function to be applied To Each element Of X: see 'Details’. 
        ''' In the case of functions like +, %*%, the function name must be 
        ''' backquoted or quoted.
        ''' </param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        <ExportAPI("lapply")>
        Public Function lapply(<RRawVectorArgument> X As Object, FUN As Object,
                               Optional names As RFunction = Nothing,
                               Optional envir As Environment = Nothing) As Object

            If FUN Is Nothing Then
                Return Internal.stop({"Missing apply function!"}, envir)
            ElseIf Not FUN.GetType.ImplementInterface(GetType(RFunction)) Then
                Return Internal.stop({"Target is not a function!"}, envir)
            End If

            If Program.isException(X) Then
                Return X
            ElseIf Program.isException(FUN) Then
                Return FUN
            ElseIf X.GetType Is GetType(list) Then
                X = DirectCast(X, list).slots
            End If

            Dim apply As RFunction = FUN
            Dim list As New Dictionary(Of String, Object)

            If X.GetType Is GetType(Dictionary(Of String, Object)) Then
                For Each d In DirectCast(X, Dictionary(Of String, Object))
                    list(d.Key) = apply.Invoke(envir, invokeArgument(d.Value))

                    If Program.isException(list(d.Key)) Then
                        Return list(d.Key)
                    End If
                Next
            Else
                Dim getName As Func(Of SeqValue(Of Object), String)
                Dim keyName$
                Dim value As Object

                If names Is Nothing Then
                    getName = Function(i) $"[[{i.i + 1}]]"
                Else
                    getName = Function(i)
                                  Return getFirst(RConversion.asCharacters(names.Invoke(envir, invokeArgument(i.value))))
                              End Function
                End If

                For Each d In Runtime.asVector(Of Object)(X) _
                    .AsObjectEnumerator _
                    .SeqIterator

                    keyName = getName(d)
                    value = apply.Invoke(envir, invokeArgument(d.value))

                    If Program.isException(value) Then
                        Return value
                    Else
                        list(keyName) = value
                    End If
                Next
            End If

            Return New list With {.slots = list}
        End Function

        ''' <summary>
        ''' 主要是应用于单个参数的R运行时函数的调用
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' 
        <DebuggerStepThrough>
        Private Function invokeArgument(value As Object) As InvokeParameter()
            Return InvokeParameter.Create(value)
        End Function

        ''' <summary>
        ''' # Apply a Function over a List or Vector
        ''' 
        ''' sapply is a user-friendly version and wrapper of lapply by default 
        ''' returning a vector, matrix or, if simplify = "array", an array 
        ''' if appropriate, by applying simplify2array(). sapply(x, f, simplify 
        ''' = FALSE, USE.NAMES = FALSE) is the same as lapply(x, f).
        ''' </summary>
        ''' <param name="X">
        ''' a vector (atomic or list) or an expression object. Other objects 
        ''' (including classed objects) will be coerced by ``base::as.list``.
        ''' </param>
        ''' <param name="FUN">
        ''' the Function to be applied To Each element Of X: see 'Details’. 
        ''' In the case of functions like +, %*%, the function name must be 
        ''' backquoted or quoted.
        ''' </param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        <ExportAPI("sapply")>
        Public Function sapply(<RRawVectorArgument> X As Object, FUN As Object, envir As Environment) As Object
            If FUN Is Nothing Then
                Return Internal.stop({"Missing apply function!"}, envir)
            ElseIf Not FUN.GetType.ImplementInterface(GetType(RFunction)) Then
                Return Internal.stop({"Target is not a function!"}, envir)
            End If

            If Program.isException(X) Then
                Return X
            ElseIf Program.isException(FUN) Then
                Return FUN
            End If

            Dim apply As RFunction = FUN

            If X.GetType Is GetType(list) Then
                X = DirectCast(X, list).slots
            End If

            If X.GetType Is GetType(Dictionary(Of String, Object)) Then
                Dim list = DirectCast(X, Dictionary(Of String, Object))
                Dim names = list.Keys.ToArray
                Dim seq As Array = names _
                    .Select(Function(key)
                                Return Runtime.single(apply.Invoke(envir, invokeArgument(list(key))))
                            End Function) _
                    .ToArray

                Return New RObj.vector(names, seq, envir)
            Else
                Dim seq = Runtime.asVector(Of Object)(X) _
                    .AsObjectEnumerator _
                    .Select(Function(d)
                                Return Runtime.single(apply.Invoke(envir, invokeArgument(d)))
                            End Function) _
                    .ToArray

                Return New RObj.vector With {.data = seq}
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

        <ExportAPI("auto")>
        <RApiReturn(GetType(RDispose))>
        Public Function autoDispose(x As Object, dispose As Object, Optional env As Environment = Nothing) As Object
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
            Else
                Return Internal.stop(New InvalidProgramException(dispose.GetType.FullName), env)
            End If

            Return New RDispose(x, final)
        End Function
    End Module
End Namespace
