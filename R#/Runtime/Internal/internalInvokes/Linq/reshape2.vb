#Region "Microsoft.VisualBasic::e43f9ab95ff60497d81256f13839ffed, F:/GCModeller/src/R-sharp/R#//Runtime/Internal/internalInvokes/Linq/reshape2.vb"

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

'   Total Lines: 113
'    Code Lines: 79
' Comment Lines: 19
'   Blank Lines: 15
'     File Size: 5.05 KB


'     Module reshape2
' 
'         Function: melt, melt_array, melt_dataframe, melt_list
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Internal.[Object].Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Invokes.LinqPipeline

    Public Module reshape2

        ''' <summary>
        ''' Aggregate two or more sequence
        ''' </summary>
        ''' <param name="zip"></param>
        ''' <param name="args"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("zip_tuple")>
        Public Function tuple(Optional zip As Object = Nothing,
                              <RListObjectArgument>
                              Optional args As list = Nothing,
                              Optional env As Environment = Nothing) As Object

            Dim symbols = args.getNames _
                .Where(Function(s) s <> "zip" AndAlso s <> "args" AndAlso s <> "env") _
                .ToArray
            Dim seqs As New Dictionary(Of String, GetVectorElement)

            For Each var As String In symbols
                Dim tmp = args.getByName(var)
                Dim getter = GetVectorElement.CreateAny(tmp)

                Call seqs.Add(var, getter)
            Next

            Dim multiple As Integer() = seqs.Values _
                .Select(Function(a) a.size) _
                .Where(Function(a) a > 1) _
                .ToArray
            Dim len As Integer = If(multiple.Length > 0,
                multiple.Min,
                seqs.Values.Select(Function(a) a.size).Max)
            Dim getters As Dictionary(Of String, Func(Of Integer, Object)) = seqs _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  Return a.Value.Getter
                              End Function)

            If zip Is Nothing Then
                Return getters.zip(len).ToArray
            Else
                Dim check = applys.checkInternal(Nothing, zip, env)

                If Program.isException(check) Then
                    Return check
                Else
                    Return getters.aggregate(lambda:=zip, len, New Environment(env, "zip_tuples.internal_loops"))
                End If
            End If
        End Function

        <Extension>
        Private Function aggregate(getters As Dictionary(Of String, Func(Of Integer, Object)),
                                   lambda As RFunction,
                                   len As Integer,
                                   env As Environment) As Object

            Dim getterVec = getters.ToArray
            Dim argv As InvokeParameter() = New InvokeParameter(getters.Count - 1) {}
            Dim result As Object
            Dim zipList As New List(Of Object)

            For i As Integer = 0 To len - 1
                For j As Integer = 0 To argv.Length - 1
                    argv(j) = New InvokeParameter(getterVec(j).Key, getterVec(j).Value(i), i)
                Next

                result = lambda.Invoke(env, argv)

                If Program.isException(result) Then
                    Return result
                Else
                    Call zipList.Add(result)
                End If
            Next

            Return REnv.TryCastGenericArray(zipList.ToArray, env)
        End Function

        <Extension>
        Private Iterator Function zip(getters As Dictionary(Of String, Func(Of Integer, Object)), len As Integer) As IEnumerable(Of list)
            ' just create the tuple list
            For i As Integer = 0 To len - 1
                Dim li As New list With {.slots = New Dictionary(Of String, Object)}

                For Each item In getters
                    Call li.add(item.Key, getters(item.Key)(i))
                Next

                Yield li
            Next
        End Function

        ''' <summary>
        ''' melt: Convert an object into a molten data frame.
        ''' 
        ''' This the generic melt function. See the following functions 
        ''' for the details about different data structures
        ''' </summary>
        ''' <param name="data">Data set to melt</param>
        ''' <param name="na_rm">Should NA values be removed from the data set? 
        ''' This will convert explicit missings to implicit missings.</param>
        ''' <param name="value_name">name of variable used to store values</param>
        ''' <param name="args">
        ''' further arguments passed To Or from other methods.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' 1. melt.data.frame for data.frames
        ''' 2. melt.array for arrays, matrices And tables
        ''' 3. melt.list for lists
        ''' </returns>
        <ExportAPI("melt")>
        Public Function melt(<RRawVectorArgument> data As Object,
                             Optional na_rm As Boolean = False,
                             Optional value_name As String = "value",
                             <RListObjectArgument>
                             Optional args As list = Nothing,
                             Optional env As Environment = Nothing) As Object

            If TypeOf data Is dataframe Then
                Return melt_dataframe(data, na_rm, value_name, args, env)
            ElseIf TypeOf data Is list Then
                Return melt_list(data, na_rm, value_name, args, env)
            Else
                Return melt_array(REnv.asVector(Of Object)(data), na_rm, value_name, args, env)
            End If
        End Function

        <ExportAPI("melt.data.frame")>
        Public Function melt_dataframe(<RRawVectorArgument> data As dataframe,
                                       Optional na_rm As Boolean = False,
                                       Optional value_name As String = "value",
                                       <RListObjectArgument>
                                       Optional args As list = Nothing,
                                       Optional env As Environment = Nothing) As Object

            Dim df As New dataframe With {.columns = New Dictionary(Of String, Array)}
            Dim Name As New List(Of String)
            Dim variable As New List(Of String)
            Dim value As New List(Of Double)
            Dim idVars As String = CLRVector.asCharacter(args.getByName("id.vars")).FirstOrDefault
            Dim vname As String()

            If idVars Is Nothing Then
                vname = data.getRowNames

                For Each var As String In data.colnames
                    Call Name.AddRange(vname)
                    Call variable.AddRange(var.Replicate(data.nrows))
                    Call value.AddRange(CLRVector.asNumeric(data(var)))
                Next

                idVars = "X"
            Else
                vname = CLRVector.asCharacter(data(idVars))

                For Each var As String In data.colnames
                    If var = idVars Then
                        Continue For
                    End If

                    Call Name.AddRange(vname)
                    Call variable.AddRange(var.Replicate(data.nrows))
                    Call value.AddRange(CLRVector.asNumeric(data(var)))
                Next
            End If

            Call df.add(idVars, Name)
            Call df.add("variable", variable)
            Call df.add(value_name, value)

            Return df
        End Function

        <ExportAPI("melt.array")>
        Public Function melt_array(<RRawVectorArgument> data As Object,
                                   Optional na_rm As Boolean = False,
                                   Optional value_name As String = "value",
                                   <RListObjectArgument>
                                   Optional args As list = Nothing,
                                   Optional env As Environment = Nothing) As Object
            Throw New NotImplementedException
        End Function

        <ExportAPI("melt.list")>
        Public Function melt_list(<RRawVectorArgument> data As list,
                                  Optional na_rm As Boolean = False,
                                  Optional value_name As String = "value",
                                  <RListObjectArgument>
                                  Optional args As list = Nothing,
                                  Optional env As Environment = Nothing) As Object
            Throw New NotImplementedException
        End Function

        ''' <summary>
        ''' ### shift: Fast lead/lag for vectors and lists
        ''' 
        ''' lead or lag vectors, lists, data.frames or data.tables implemented in VisualBasic for speed.
        ''' 
        ''' shift accepts vectors, lists, data.frames or data.tables. It always 
        ''' returns a list except when the input is a vector and length(n) == 1 
        ''' in which case a vector is returned, for convenience. This is so that 
        ''' it can be used conveniently within data.table's syntax. For example, 
        ''' DT[, (cols) := shift(.SD, 1L), by=id] would lag every column of .SD by
        ''' 1 for each group and DT[, newcol := colA + shift(colB)] would assign 
        ''' the sum of two vectors to newcol.
        '''
        ''' Argument n allows multiple values. For example, DT[, (cols) := shift(.SD, 1:2), by=id] 
        ''' would lag every column of .SD by 1 And 2 for each group. If .SD contained
        ''' four columns, the first two elements of the list would correspond to 
        ''' lag=1 And lag=2 for the first column of .SD, the next two for second 
        ''' column of .SD And so on. Please see examples for more.
        '''
        ''' shift Is designed mainly for use in data.tables along with := Or set. 
        ''' Therefore, it returns an unnamed list by default as assigning names for 
        ''' each group over And over can be quite time consuming with many groups. 
        ''' It may be useful to set names automatically in other cases, which can 
        ''' be done by setting give.names to TRUE.
        ''' </summary>
        ''' <param name="x">
        ''' A vector, list, data.frame Or data.table.</param>
        ''' <param name="n">integer vector denoting the offset by which 
        ''' to lead or lag the input. To create multiple lead/lag vectors, 
        ''' provide multiple values to n; negative values of n will "flip" 
        ''' the value of type, i.e., n=-1 and type='lead' is the same as 
        ''' n=1 and type='lag'.</param>
        ''' <param name="fill">
        ''' Value to use for padding when the window goes beyond the input 
        ''' length.
        ''' </param>
        ''' <param name="type">default is "lag" (look "backwards"). The other 
        ''' possible values "lead" (look "forwards") and "shift" (behave same 
        ''' as "lag" except given names).</param>
        ''' <param name="give_names">default is FALSE which returns an unnamed
        ''' list. When TRUE, names are automatically generated corresponding 
        ''' to type and n. If answer is an atomic vector, then the argument 
        ''' is ignored.</param>
        ''' <returns>
        ''' A list containing the lead/lag of input x.
        ''' </returns>
        ''' <remarks>
        ''' The function behavior is different at here when compare with the 
        ''' ``shift`` function of the ``data.table`` package from the original
        ''' R language: the shift function from R language not allow the <paramref name="fill"/>
        ''' data be nothing, but the ``shift`` function in R# language will 
        ''' behavior a different result: when the <paramref name="fill"/> value
        ''' is nothing at here, this function will becomes skip for <paramref name="type"/>
        ''' is ``lag`` or ``shift`` and this function will becomes take for <paramref name="type"/>
        ''' is ``lead``.
        ''' </remarks>
        <ExportAPI("shift")>
        Public Function shift(<RRawVectorArgument> x As Object,
                              Optional n As Integer = 1L,
                              Optional fill As Object = "NA",
                              <RRawVectorArgument(GetType(String))>
                              Optional type As Object = "lag|lead|shift",
                              Optional give_names As Boolean = False,
                              Optional env As Environment = Nothing) As Object

            Dim flag As String = CLRVector.asCharacter(type).ElementAtOrDefault(Scan0, "lag")

            If flag <> "lag" AndAlso flag <> "lead" AndAlso flag <> "shift" Then
                Return Internal.debug.stop("", env)
            End If

            If TypeOf x Is list Then
                Return Internal.debug.stop(New NotImplementedException, env)
            ElseIf TypeOf x Is dataframe Then
                Return Internal.debug.stop(New NotImplementedException, env)
            Else
                ' is array/vector
                If fill Is Nothing Then
                    If flag = "lag" Then
                        Return linq.skip(x, n, env)
                    ElseIf flag = "lead" Then
                        Return linq.take(x, n, env)
                    Else
                        Return linq.skip(x, n, env)
                    End If
                Else
                    Dim vec As Array = TryCastGenericArray(x, env)
                    Dim vec_type As RType = RType.GetRSharpType(MeasureRealElementType(vec))
                    Dim shift_vec As Array = Array.CreateInstance(Runtime.GetType(vec_type.mode, elementType:=True), vec.Length)

                    If RCType.IsNALiteralValue(fill) Then
                        fill = RCType.NADefault(RType.GetRSharpType(shift_vec.GetType.GetElementType))
                    End If

                    fill = RCType.CTypeDynamic(fill, vec_type.raw, env)

                    If flag = "lag" OrElse flag = "shift" Then
                        For i As Integer = 0 To n - 1
                            Call shift_vec.SetValue(fill, i)
                        Next

                        Call Array.ConstrainedCopy(vec, Scan0, shift_vec, n, vec.Length - n)
                    Else
                        For i As Integer = vec.Length - n To vec.Length - 1
                            Call shift_vec.SetValue(fill, i)
                        Next

                        Call Array.ConstrainedCopy(vec, n, shift_vec, vec.Length - n, vec.Length - n)
                    End If

                    Return shift_vec
                End If
            End If
        End Function
    End Module
End Namespace
