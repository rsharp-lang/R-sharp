#Region "Microsoft.VisualBasic::51800e967f57ff46f58c93f99544a5b5, D:/GCModeller/src/R-sharp/R#//Runtime/Internal/objects/RConversion/conversion.vb"

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

    '   Total Lines: 1271
    '    Code Lines: 773
    ' Comment Lines: 367
    '   Blank Lines: 131
    '     File Size: 56.83 KB


    '     Module RConversion
    ' 
    '         Function: asCharacters, asDataframe, asDate, asDate2, asDouble
    '                   asInteger, asList, asLogicals, asNumeric, asObject
    '                   asPipeline, asRaw, asVector, castArrayOfGeneric, castArrayOfObject
    '                   castListMatrix, castListRows, castListRowsToDataframe, castListToDataframe, castType
    '                   checkList, checkNames, handleListFeatureProjections, handleUnsure, isCharacter
    '                   isDateTime, isLogical, populateNumeric, tryUnlistArray, unlist
    '                   unlistOfRList, unlistRecursive
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.ValueTypes
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Interop.[CType]
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Object.Converts

    Public Module RConversion

        ''' <summary>
        ''' parse string text content as date time values
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("as.Date")>
        Public Function asDate2(<RRawVectorArgument> obj As Object, Optional env As Environment = Nothing) As Date()
            ' in R language
            ' base::as.Date
            Return asDate(obj, env)
        End Function

        ''' <summary>
        ''' the given string is in datetime format?
        ''' </summary>
        ''' <param name="str"></param>
        ''' <returns></returns>
        <ExportAPI("is.date")>
        Public Function isDateTime(str As String) As Boolean
            Return DateTime.TryParse(str, Nothing)
        End Function

        ''' <summary>
        ''' parse string text content as date time values
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("as.date")>
        Public Function asDate(<RRawVectorArgument> obj As Object, Optional env As Environment = Nothing) As Date()
            ' in R# language
            Return ObjectSet _
                .GetObjectSet(obj, env) _
                .Select(Function(o)
                            If TypeOf o Is Date Then
                                Return CDate(o)
                            Else
                                Return Date.Parse(any.ToString(o))
                            End If
                        End Function) _
                .ToArray
        End Function

        ''' <summary>
        ''' Cast .NET object to R# object
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        <ExportAPI("as.object")>
        Public Function asObject(<RRawVectorArgument> obj As Object) As Object
            If obj Is Nothing Then
                Return Nothing
            Else
                Dim type As Type = obj.GetType

                Select Case type
                    Case GetType(vbObject), GetType(vector), GetType(list)
                        Return obj
                    Case GetType(RReturn)
                        Return asObject(DirectCast(obj, RReturn).Value)
                    Case Else
                        If type.IsArray Then
                            Return Runtime.asVector(Of Object)(obj) _
                                .AsObjectEnumerator _
                                .Select(Function(o) New vbObject(o)) _
                                .ToArray
                        Else
                            Return New vbObject(obj, type)
                        End If
                End Select
            End If
        End Function

        ''' <summary>
        ''' ### Flatten Lists
        ''' 
        ''' Given a list structure x, unlist simplifies it to produce a vector 
        ''' which contains all the atomic components which occur in <paramref name="x"/>.
        ''' </summary>
        ''' <param name="x">an R Object, typically a list Or vector.</param>
        ''' <param name="[typeof]">element type of the array</param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' NULL or an expression or a vector of an appropriate mode to hold 
        ''' the list components.
        '''
        ''' The output type Is determined from the highest type Of the components 
        ''' In the hierarchy ``NULL`` &lt; ``raw`` &lt; ``logical`` &lt; ``Integer`` 
        ''' &lt; ``Double`` &lt; ``complex`` &lt; ``character`` &lt; ``list`` &lt; 
        ''' ``expression``, after coercion Of pairlists To lists.
        ''' </returns>
        ''' <remarks>
        ''' unlist is generic: you can write methods to handle specific classes of 
        ''' objects, see InternalMethods, and note, e.g., relist with the unlist
        ''' method for relistable objects.
        '''
        ''' If recursive = False, the Function will Not recurse beyond the first level 
        ''' items In x.
        '''
        ''' Factors are treated specially. If all non-list elements Of x are factors 
        ''' (Or ordered factors) Then the result will be a factor With levels the union 
        ''' Of the level sets Of the elements, In the order the levels occur In the 
        ''' level sets Of the elements (which means that If all the elements have the 
        ''' same level Set, that Is the level Set Of the result).
        '''
        ''' x can be an atomic vector, but Then unlist does Nothing useful, Not even 
        ''' drop names.
        '''
        ''' By Default, unlist tries to retain the naming information present in x. If 
        ''' ``use.names = FALSE`` all naming information Is dropped.
        '''
        ''' Where possible the list elements are coerced To a common mode during the 
        ''' unlisting, And so the result often ends up As a character vector. Vectors 
        ''' will be coerced To the highest type Of the components In the hierarchy 
        ''' ``NULL`` &lt; ``raw`` &lt; ``logical`` &lt; ``Integer`` &lt; ``Double`` &lt;
        ''' ``complex`` &lt; ``character`` &lt; ``list`` &lt; ``expression``: pairlists 
        ''' are treated As lists.
        '''
        ''' A list Is a (generic) vector, And the simplified vector might still be a list 
        ''' (And might be unchanged). Non-vector elements Of the list (For example language 
        ''' elements such As names, formulas And calls) are Not coerced, And so a list 
        ''' containing one Or more Of these remains a list. (The effect Of unlisting an lm 
        ''' fit Is a list which has individual residuals As components.) Note that 
        ''' ``unlist(x)`` now returns x unchanged also For non-vector x, instead Of signaling 
        ''' an Error In that Case.
        ''' </remarks>
        <ExportAPI("unlist")>
        Public Function unlist(<RRawVectorArgument> x As Object,
                               Optional [typeof] As Object = Nothing,
                               Optional pipeline As Boolean = False,
                               Optional env As Environment = Nothing) As Object

            If x Is Nothing Then
                Return Nothing
            ElseIf TypeOf x Is NoInspector Then
                Return DirectCast(x, NoInspector).obj
            End If

            If Not [typeof] Is Nothing Then
                [typeof] = env.globalEnvironment.GetType([typeof])
            End If
            If TypeOf [typeof] Is Type Then
                [typeof] = RType.GetRSharpType(DirectCast([typeof], Type))
            End If

            Dim containsListNames As Value(Of Boolean) = False
            Dim result As IEnumerable = unlistRecursive(x, containsListNames)

            If pipeline Then
                Return New pipeline(result, TryCast([typeof], RType))
            Else
                If containsListNames.Value Then
                    Dim names As New List(Of String)
                    Dim values As New List(Of Object)

                    For Each obj In result
                        If obj Is Nothing Then
                            names.Add("")
                            values.Add(Nothing)
                        ElseIf TypeOf obj Is NamedValue(Of Object) Then
                            With DirectCast(obj, NamedValue(Of Object))
                                names.Add(.Name)
                                values.Add(.Value)
                            End With
                        Else
                            names.Add("")
                            values.Add(obj)
                        End If
                    Next

                    If [typeof] Is Nothing Then
                        Return New vector(names.ToArray, values.ToArray(Of Object), env)
                    Else
                        Return New vector(names.ToArray, values.ToArray(Of Object), [typeof], env)
                    End If
                Else
                    Dim objs As Array = result.ToArray(Of Object)

                    If [typeof] Is Nothing Then
                        Return objs.TryCastGenericArray(env)
                    Else
                        Return New vector(objs, [typeof])
                    End If
                End If
            End If
        End Function

        Private Function unlistRecursive(x As Object, containsListNames As Value(Of Boolean)) As IEnumerable
            If x Is Nothing Then
                Return New Object() {}
            Else
                Dim listType As Type = x.GetType

                If listType.IsArray Then
                    Return tryUnlistArray(DirectCast(x, Array), containsListNames)
                ElseIf listType Is GetType(vector) Then
                    Return tryUnlistArray(DirectCast(x, vector).data, containsListNames)
                ElseIf DataFramework.IsPrimitive(listType) Then
                    Return {x}
                ElseIf listType Is GetType(list) Then
                    Return DirectCast(x, list).unlistOfRList(containsListNames)
                ElseIf listType.ImplementInterface(GetType(IDictionary)) Then
                    Return New list(DirectCast(x, IDictionary)).unlistOfRList(containsListNames)
                ElseIf listType Is GetType(pipeline) Then
                    Return tryUnlistArray(DirectCast(x, pipeline).pipeline, containsListNames)
                Else
                    ' Return Internal.debug.stop(New InvalidCastException(list.GetType.FullName), env)
                    ' is a single uer defined .NET object 
                    Return {x}
                End If
            End If
        End Function

        <Extension>
        Private Iterator Function tryUnlistArray(data As IEnumerable, containsListNames As Value(Of Boolean)) As IEnumerable
            For Each obj As Object In data
                For Each item As Object In unlistRecursive(obj, containsListNames)
                    Yield item
                Next
            Next
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="rlist"></param>
        ''' <returns></returns>
        <Extension>
        Private Function unlistOfRList(rlist As list, containsListNames As Value(Of Boolean)) As IEnumerable
            Dim data As New List(Of NamedValue(Of Object))

            For Each name As String In rlist.getNames
                Dim a As Array = Runtime.asVector(Of Object)(rlist.slots(name)).tryUnlistArray(containsListNames).ToArray(Of Object)

                If a.Length = 1 Then
                    data.Add(New NamedValue(Of Object)(name, a.GetValue(Scan0)))
                Else
                    Dim i As i32 = 1

                    For Each item As Object In a
                        data.Add(New NamedValue(Of Object)($"{name}{++i}", item))
                    Next
                End If
            Next

            containsListNames.Value = True

            Return data
        End Function

        ''' <summary>
        ''' ### Coerce to a Data Frame
        ''' 
        ''' Functions to check if an object is a data frame, or coerce it if possible.
        ''' </summary>
        ''' <param name="x">any R object.</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("as.data.frame")>
        <RApiReturn(GetType(dataframe))>
        Public Function asDataframe(<RRawVectorArgument> x As Object,
                                    <RListObjectArgument> args As Object,
                                    Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return x
            ElseIf TypeOf x Is dataframe Then
                ' clone object
                Dim raw As dataframe = x
                Dim clone As New dataframe With {
                    .columns = raw.columns _
                        .ToDictionary(Function(k) k.Key,
                                      Function(k)
                                          Return k.Value
                                      End Function),
                    .rownames = If(raw.rownames.IsNullOrEmpty, Nothing, raw.rownames.ToArray)
                }

                Return clone
            ElseIf x.GetType.IsArray AndAlso DirectCast(x, Array).Length = 0 Then
                Return Nothing
            ElseIf TypeOf x Is list Then
                Return DirectCast(x, list).slots.castListToDataframe(env)

            ElseIf TypeOf x Is list() OrElse (TypeOf x Is Object() AndAlso DirectCast(x, Object()).All(Function(xi) TypeOf xi Is list)) Then
                Return DirectCast(x, Array).AsObjectEnumerator(Of list).castListRowsToDataframe(env)

            Else
                If TypeOf x Is vbObject Then
                    x = DirectCast(x, vbObject).target
                End If

                args = base.Rlist(args, env)

                If Program.isException(args) Then
                    Return args
                ElseIf DirectCast(args, list).hasName("features") Then
                    If TypeOf x Is list Then
                        x = DirectCast(x, list).data.ToArray
                    End If

                    Return pipeline _
                        .TryCreatePipeline(Of list)(x, env) _
                        .populates(Of list)(env) _
                        .handleListFeatureProjections(
                            features:=DirectCast(args, list).getValue(Of String())("features", env, Nothing),
                            env:=env,
                            rowName:=DirectCast(args, list).getValue(Of String)("row.names", env, Nothing)
                         )
                End If
            End If
RE0:
            Dim type As Type = x.GetType

            If type Is GetType(list) Then
                ' each slot elements in list should be 
                ' a field in dataframe table
                Return makeDataframe.fromList(DirectCast(x, list), env)
            ElseIf makeDataframe.is_ableConverts(type) Then
                ' generic overloads method for .NET object
                ' direct cast at here
                Return makeDataframe.createDataframe(type, x, args, env)
            ElseIf type.IsArray Then
                ' 20220505 try to handling the array bugs when
                ' target array contains single element
                If DirectCast(x, Array).Length = 1 AndAlso makeDataframe.is_ableConverts(type.GetElementType) Then
                    x = DirectCast(x, Array).GetValue(Scan0)
                    type = x.GetType

                    Return makeDataframe.createDataframe(type, x, args, env)
                Else
                    Dim err As Message = handleUnsure(x, type, env)

                    If Not err Is Nothing Then
                        Return err
                    Else
                        GoTo RE0
                    End If
                End If
            ElseIf type.ImplementInterface(Of ICTypeDataframe) Then
                Return DirectCast(x, ICTypeDataframe).toDataframe
            Else
                ' generic overloads method for .NET object
                ' <base type>
                type = makeDataframe.tryTypeLineage(type)

                If type Is Nothing Then
                    Dim err As Message = handleUnsure(x, type, env)

                    If Not err Is Nothing Then
                        Return err
                    Else
                        GoTo RE0
                    End If
                Else
                    Return makeDataframe.createDataframe(type, x, args, env)
                End If
            End If
        End Function

        ''' <summary>
        ''' cast row list data to a data table object.
        ''' </summary>
        ''' <param name="rows">
        ''' each elements list in this collection data is a row in the generated dataframe object. 
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <Extension>
        Private Function castListRowsToDataframe(rows As IEnumerable(Of list), env As Environment) As Object
            Dim matrix As list() = rows.ToArray
            Dim fieldNames As String() = matrix _
                .Select(Function(row) row.getNames) _
                .IteratesALL _
                .Distinct _
                .ToArray
            Dim table As New dataframe With {
                .columns = New Dictionary(Of String, Array)
            }
            Dim v As Array

            For Each name As String In fieldNames
                v = matrix.Select(Function(r) r(name)).ToArray
                v = REnv.TryCastGenericArray(v, env)

                Call table.add(name, v)
            Next

            Return table
        End Function

        Private Function checkNames(listData As Dictionary(Of String, Object), env As Environment) As [Variant](Of Message, String())
            Dim pullNames As New List(Of String)

            For Each d In listData
                ' 20230626 the empty string generated from the
                ' json decode of the null literal
                If d.Value Is Nothing Then
                    Continue For
                End If
                If TypeOf d.Value Is String AndAlso CStr(d.Value) = "" Then
                    Continue For
                ElseIf Not TypeOf d.Value Is list Then
                    Return Message.InCompatibleType(GetType(list), d.Value.GetType, env, $"required of data tuple list by given {d.Value.GetType.FullName} at row with key '{d.Key}'!")
                Else
                    pullNames.AddRange(DirectCast(d.Value, list).getNames)
                End If
            Next

            Return pullNames.Distinct.ToArray
        End Function

        ''' <summary>
        ''' each item in listdata is a row?
        ''' </summary>
        ''' <param name="rows"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Private Function castListMatrix(rows As KeyValuePair(Of String, Object)(), allNames As String(), env As Environment) As Object
            Dim table As New dataframe With {
                .columns = New Dictionary(Of String, Array)
            }
            Dim row As Func(Of String, Object)

            For Each name As String In allNames
                table.add(name, Array.CreateInstance(GetType(Object), rows.Length))
            Next

            For i As Integer = 0 To rows.Length - 1
                Dim rowObj = rows(i).Value

                If rowObj Is Nothing Then
                    row = Function() Nothing
                ElseIf TypeOf rowObj Is String AndAlso CStr(rowObj) = "" Then
                    row = Function() Nothing
                Else
                    With DirectCast(rowObj, list)
                        row = Function(name) .getByName(name)
                    End With
                End If

                For Each name As String In allNames
                    table.columns(name).SetValue(row(name), i)
                Next
            Next

            For Each name As String In allNames
                table.columns(name) = REnv.TryCastGenericArray(table.columns(name), env)
            Next

            Return table
        End Function

        <Extension>
        Private Function castListRows(listData As Dictionary(Of String, Object),
                                      hasNames As Boolean,
                                      allnames As String(),
                                      env As Environment) As Object

            Dim table As New dataframe With {
                .columns = New Dictionary(Of String, Array),
                .rownames = allnames
            }

            For Each field As String In listData.Keys
                Dim raw As Object = listData(field)
                Dim isNull As Boolean = raw Is Nothing
                Dim v As Array

                ' json decode of the null literal will be empty string?
                If Not isNull AndAlso TypeOf raw Is String AndAlso CStr(raw) = "" Then
                    isNull = True
                End If

                If hasNames Then
                    v = allnames _
                        .Select(Function(d)
                                    If isNull Then
                                        Return Nothing
                                    Else
                                        Return DirectCast(raw, list)(d)
                                    End If
                                End Function) _
                        .ToArray
                    v = REnv.TryCastGenericArray(v, env)
                Else
#Disable Warning
                    v = REnv.TryCastGenericArray(REnv.asVector(Of Object)(raw), env)
#Enable Warning
                End If

                Call table.add(field, v)
            Next

            Return table
        End Function

        Private Function checkList(o As Object) As Boolean
            If o Is Nothing OrElse TypeOf o Is list Then
                Return True
            ElseIf TypeOf o Is String AndAlso CStr(o) = "" Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' cast column list to dataframe
        ''' </summary>
        ''' <param name="listData"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <Extension>
        Private Function castListToDataframe(listData As Dictionary(Of String, Object), env As Environment) As Object
            Dim hasNames As Boolean
            Dim allNames As String() = Nothing

            If listData.IsNullOrEmpty Then
                Return Nothing
            Else
                hasNames = TypeOf listData.First.Value Is list
            End If

            Dim isListMatrix = listData.Values.All(AddressOf checkList)
            ' all of the element key name is integer
            ' or integer index key name
            Dim t As Boolean = listData.Keys.All(Function(k) k.IsPattern("\d+") OrElse k.IsPattern("\[+\d+\]+"))

            If hasNames Then
                Dim pullNames = checkNames(listData, env)

                If pullNames Like GetType(Message) Then
                    Return pullNames.TryCast(Of Message)
                Else
                    allNames = pullNames
                End If
            End If

            If isListMatrix AndAlso t Then
                Return listData _
                    .ToArray _
                    .castListMatrix(allNames, env)
            Else
                Return listData.castListRows(hasNames, allNames, env)
            End If
        End Function

        <Extension>
        Private Function handleListFeatureProjections(data As IEnumerable(Of list), features As String(), rowName As String, env As Environment) As Object
            If features.IsNullOrEmpty Then
                Return Internal.debug.stop("no list feature was selected!", env)
            ElseIf rowName.StringEmpty Then
                rowName = Nothing
            End If

            Dim df As New dataframe With {
                .rownames = Nothing,
                .columns = New Dictionary(Of String, Array)
            }
            Dim fields As New Dictionary(Of String, List(Of Object))
            Dim rowNames As New List(Of Object)

            For Each name As String In features
                fields(name) = New List(Of Object)
            Next

            For Each row As list In data
                For Each field In fields
                    Call field.Value.Add(row.getByName(name:=field.Key))
                Next

                If Not rowName Is Nothing Then
                    Call rowNames.Add(row.getByName(rowName))
                End If
            Next

            For Each name As String In features
                Call df.columns.Add(name, fields(name).ToArray)
            Next

            df.rownames = CLRVector.asCharacter(rowNames.ToArray)

            Return df
        End Function

        Private Function handleUnsure(ByRef x As Object, ByRef type As Type, env As Environment) As Message
            If TypeOf x Is vector Then
                x = DirectCast(x, vector).data
                type = MeasureRealElementType(x)

                If x.GetType.GetElementType Is Nothing OrElse x.GetType.GetElementType Is GetType(Object) Then
                    Dim list = Array.CreateInstance(type, DirectCast(x, Array).Length)

                    With DirectCast(x, Array)
                        For i As Integer = 0 To .Length - 1
                            x = .GetValue(i)

                            If TypeOf x Is vbObject Then
                                x = DirectCast(x, vbObject).target
                            End If

                            list.SetValue(x, i)
                        Next
                    End With

                    x = list
                End If

                Return Nothing
            End If

            If type Is Nothing Then
                Return Internal.debug.stop(New InvalidProgramException("unknown type handler..."), env)
            Else
                Return Internal.debug.stop(New InvalidProgramException("missing api for handle of data: " & type.FullName), env)
            End If
        End Function

        ''' <summary>
        ''' ### Vectors
        ''' 
        ''' vector produces a vector of the given length and mode.
        ''' 
        ''' as.vector, a generic, attempts to coerce its argument into a 
        ''' vector of mode mode (the default is to coerce to whichever 
        ''' vector mode is most convenient): if the result is atomic all 
        ''' attributes are removed.
        ''' </summary>
        ''' <param name="x">an R object.</param>
        ''' <param name="mode">
        ''' character string naming an atomic mode or "list" or "expression" 
        ''' or (except for vector) "any". Currently, is.vector() allows any 
        ''' type (see typeof) for mode, and when mode is not "any", 
        ''' ``is.vector(x, mode)`` is almost the same as ``typeof(x) == mode``.
        ''' </param>
        ''' <param name="env"></param>
        ''' <remarks>
        ''' The atomic modes are "logical", "integer", "numeric" (synonym 
        ''' "double"), "complex", "character" and "raw".
        '''
        ''' If mode = "any", Is.vector may Return True For the atomic modes, 
        ''' list And expression. For any mode, it will Return False If x has 
        ''' any attributes except names. (This Is incompatible With S.) On 
        ''' the other hand, As.vector removes all attributes including names 
        ''' For results Of atomic mode (but Not those Of mode "list" nor 
        ''' "expression").
        '''
        ''' Note that factors are Not vectors; Is.vector returns False And 
        ''' ``as.vector`` converts a factor To a character vector For 
        ''' ``mode = "any"``.
        ''' 
        ''' as.vector and is.vector are quite distinct from the meaning of the 
        ''' formal class "vector" in the methods package, and hence as(x, "vector") 
        ''' and is(x, "vector").
        '''
        ''' Note that ``as.vector(x)`` Is Not necessarily a null operation if 
        ''' ``is.vector(x)`` Is true: any names will be removed from an atomic 
        ''' vector.
        '''
        ''' Non-vector modes "symbol" (synonym "name") And "pairlist" are accepted 
        ''' but have long been undocumented: they are used To implement As.name And 
        ''' As.pairlist, And those functions should preferably be used directly. 
        ''' None Of the description here applies To those modes: see the help For 
        ''' the preferred forms.
        ''' </remarks>
        ''' <returns>
        ''' For vector, a vector of the given length and mode. Logical vector 
        ''' elements are initialized to FALSE, numeric vector elements to 0, 
        ''' character vector elements to "", raw vector elements to nul bytes and 
        ''' list/expression elements to NULL.
        '''
        ''' For as.vector, a vector (atomic Or of type list Or expression). All 
        ''' attributes are removed from the result if it Is of an atomic mode, but 
        ''' Not in general for a list result. The default method handles 24 input 
        ''' types And 12 values of type: the details Of most coercions are 
        ''' undocumented And subject To change.
        '''
        ''' For Is.vector, TRUE Or FALSE. Is.vector(x, mode = "numeric") can be 
        ''' true for vectors of types "integer" Or "double" whereas 
        ''' ``is.vector(x, mode = "double")`` can only be true for those of type 
        ''' "double".
        ''' </returns>
        <ExportAPI("as.vector")>
        Public Function asVector(<RRawVectorArgument> x As Object,
                                 Optional mode As Object = "any",
                                 Optional env As Environment = Nothing) As Object

            If x Is Nothing Then
                Return x
            End If

            Dim classType As RType = env.globalEnvironment.GetType(mode)

            If TypeOf x Is vector Then
                If classType.raw Is GetType(Object) Then
                    DirectCast(x, vector).elementType = classType
                Else
                    x = New vector With {
                        .data = REnv.asVector(DirectCast(x, vector).data, classType.raw, env),
                        .elementType = classType
                    }
                End If

                Return x
            ElseIf TypeOf x Is Rectangle Then
                With DirectCast(x, Rectangle)
                    Return New vector(New Double() { .X, .Y, .Width, .Height}, RType.GetRSharpType(GetType(Double)))
                End With
            ElseIf TypeOf x Is RectangleF Then
                With DirectCast(x, RectangleF)
                    Return New vector(New Double() { .X, .Y, .Width, .Height}, RType.GetRSharpType(GetType(Double)))
                End With
            ElseIf x.GetType.IsArray Then
                Return New vector With {
                    .data = REnv.asVector(x, classType.raw, env),
                    .elementType = classType
                }
            ElseIf TypeOf x Is pipeline Then
                Return DirectCast(x, pipeline).createVector(env)
            ElseIf TypeOf x Is Group Then
                Return New vector With {
                    .data = REnv.asVector(DirectCast(x, Group).group, classType.raw, env),
                    .elementType = classType
                }
            Else
                Dim interfaces = x.GetType.GetInterfaces

                ' array of <T>
                For Each type In interfaces
                    If type.GenericTypeArguments.Length > 0 AndAlso type.ImplementInterface(Of IEnumerable) Then
                        Return type.castArrayOfGeneric(x)
                    End If
                Next

                ' array of object
                For Each type In interfaces
                    If type Is GetType(IEnumerable) Then
                        Return castArrayOfObject(x, env)
                    ElseIf type Is GetType(IBucketVector) Then
                        Return castArrayOfObject(DirectCast(x, IBucketVector).GetVector, env)
                    End If
                Next

                ' obj is not a vector type
                env.AddMessage($"target object of '{x.GetType.FullName}' can not be convert to a vector.", MSG_TYPES.WRN)

                Return x
            End If
        End Function

        Private Function castArrayOfObject(obj As Object, env As Environment) As vector
            Dim buffer As New List(Of Object)

            For Each x As Object In DirectCast(obj, IEnumerable)
                Call buffer.Add(x)
            Next

            Return New vector With {.data = REnv.TryCastGenericArray(buffer.ToArray, env)}
        End Function

        <Extension>
        Private Function castArrayOfGeneric(type As Type, obj As Object) As vector
            Dim generic As Type = type.GenericTypeArguments(Scan0)
            Dim buffer As Object = Activator.CreateInstance(GetType(List(Of )).MakeGenericType(generic))
            Dim add As MethodInfo = buffer.GetType.GetMethod("Add", BindingFlags.Public Or BindingFlags.Instance)
            Dim source As IEnumerator = DirectCast(obj, IEnumerable).GetEnumerator

            source.MoveNext()
            source = source.Current

            ' get element count by list
            Do While source.MoveNext
                Call add.Invoke(buffer, {source.Current})
            Loop

            ' write buffered data to vector
            Dim vec As Array = Array.CreateInstance(generic, DirectCast(buffer, IList).Count)
            Dim i As i32 = Scan0

            For Each x As Object In DirectCast(buffer, IEnumerable)
                Call vec.SetValue(x, ++i)
            Next

            Return New vector With {.data = vec}
        End Function

        ''' <summary>
        ''' ### Lists – Generic and Dotted Pairs
        ''' 
        ''' Functions to construct, coerce and check for both kinds of R lists.
        ''' Cast the raw dictionary object to R# list object
        ''' </summary>
        ''' <param name="x">
        ''' object to be coerced or tested.
        ''' this function will make a data copy if the input 
        ''' data is already a <see cref="list"/>
        ''' </param>
        ''' <param name="args">
        ''' for dataframe type:
        ''' 
        ''' + ``byrow``: logical, default is FALSE, means cast dataframe to list directly by column hash table values
        ''' + ``names``: character, the column names that will be used as the list names
        ''' 
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this function supports the generic function calls
        ''' 
        ''' Almost all lists in R internally are Generic Vectors, whereas traditional 
        ''' dotted pair lists (as in LISP) remain available but rarely seen by users 
        ''' (except as formals of functions).
        ''' 
        ''' The arguments To list Or pairlist are Of the form value Or tag = value. The 
        ''' functions Return a list Or dotted pair list composed Of its arguments With
        ''' Each value either tagged Or untagged, depending On how the argument was 
        ''' specified.
        '''
        ''' alist handles its arguments as if they described function arguments. So the 
        ''' values are Not evaluated, And tagged arguments with no value are allowed 
        ''' whereas list simply ignores them. alist Is most often used in conjunction 
        ''' with formals.
        '''
        ''' as.list attempts to coerce its argument to a list. For functions, this returns 
        ''' the concatenation of the list of formal arguments And the function body. For 
        ''' expressions, the list of constituent elements Is returned. as.list Is generic, 
        ''' And as the default method calls as.vector(mode = "list") for a non-list, 
        ''' methods for as.vector may be invoked. as.list turns a factor into a list of 
        ''' one-element factors, keeping names. Other attributes may be dropped unless the 
        ''' argument already Is a list Or expression. (This Is inconsistent with functions 
        ''' such as as.character which always drop attributes, And Is for efficiency since 
        ''' lists can be expensive to copy.)
        '''
        ''' Is.list returns TRUE if And only if its argument Is a list Or a pairlist of 
        ''' length > 0>0. Is.pairlist returns TRUE if And only if the argument Is a pairlist 
        ''' Or NULL (see below).
        '''
        ''' The "environment" method for as.list copies the name-value pairs (for names Not 
        ''' beginning with a dot) from an environment to a named list. The user can request 
        ''' that all named objects are copied. Unless sorted = TRUE, the list Is in no 
        ''' particular order (the order depends on the order of creation of objects And whether 
        ''' the environment Is hashed). No enclosing environments are searched. (Objects 
        ''' copied are duplicated so this can be an expensive operation.) Note that there 
        ''' Is an inverse operation, the as.environment() method for list objects.
        '''
        ''' An empty pairlist, pairlist() Is the same As NULL. This Is different from list(): 
        ''' some but Not all operations will promote an empty pairlist To an empty list.
        '''
        ''' as.pairlist Is implemented as as.vector(x, "pairlist"), And hence will dispatch 
        ''' methods for the generic function as.vector. Lists are copied element-by-element 
        ''' into a pairlist And the names of the list used as tags for the pairlist the return 
        ''' value for other types of argument Is undocumented.
        '''
        ''' list, Is.list And Is.pairlist are primitive functions.
        ''' </remarks>
        <ExportAPI("as.list")>
        <RApiReturn(GetType(list))>
        Public Function asList(<RRawVectorArgument> x As Object,
                               <RListObjectArgument> args As Object,
                               Optional env As Environment = Nothing) As Object

            If TypeOf args Is InvokeParameter() Then
                args = base.Rlist(args, env)
            End If

            If x Is Nothing Then
                Return Nothing
            ElseIf TypeOf x Is list Then
                ' just make a list data copy
                Return New list(DirectCast(x, list).elementType) With {
                    .slots = New Dictionary(Of String, Object)(DirectCast(x, list).slots)
                }
            ElseIf x.GetType.ImplementInterface(Of IDictionary) Then
                Return DirectCast(x, IDictionary).dictionaryToRList(args, env)
            ElseIf TypeOf x Is Size OrElse TypeOf x Is SizeF Then
                Dim size As SizeF = If(TypeOf x Is Size, DirectCast(x, Size).SizeF, DirectCast(x, SizeF))
                Dim listdata As New list With {
                    .elementType = RType.GetRSharpType(GetType(Double)),
                    .slots = New Dictionary(Of String, Object) From {
                        {"w", size.Width},
                        {"h", size.Height}
                    }
                }

                Return listdata
            Else
                Return listInternal(x, args, env)
            End If
        End Function

        ''' <summary>
        ''' Cast the given vector or list to integer type
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        <ExportAPI("as.integer")>
        <RApiReturn(GetType(Long))>
        Public Function asInteger(<RRawVectorArgument> obj As Object, Optional env As Environment = Nothing) As Object
            If obj Is Nothing Then
                Return 0
            ElseIf obj.GetType.ImplementInterface(GetType(IDictionary)) Then
                Return REnv.CTypeOfList(Of Long)(obj, env)
            Else
                If TypeOf obj Is vector Then
                    obj = DirectCast(obj, vector).data
                End If
            End If

            If obj.GetType.IsArray Then
                Dim type As Type = MeasureRealElementType(obj)

                If type Is GetType(String) Then
                    ' 20200427 try to fix bugs on linux platform 
                    ' 
                    ' Error in <globalEnvironment> -> InitializeEnvironment -> str_pad -> str_pad -> as.integer -> as.integer
                    ' 1. TargetInvocationException: Exception has been thrown by the target of an invocation.
                    ' 2. DllNotFoundException: kernel32
                    ' 3. stackFrames: 
                    ' at System.Reflection.MonoMethod.Invoke (System.Object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x00083] in <47d423fd1d4342b9832b2fe1f5d431eb>:0 
                    ' at System.Reflection.MethodBase.Invoke (System.Object obj, System.Object[] parameters) [0x00000] in <47d423fd1d4342b9832b2fe1f5d431eb>:0 
                    ' at SMRUCC.Rsharp.Runtime.Interop.RMethodInfo.Invoke (System.Object[] parameters, SMRUCC.Rsharp.Runtime.Environment env) [0x00073] in <f2d41b7896b5443b8d9c40b31555c1b7>:0 

                    ' R# source: mapId <- Call str_pad(Call as.integer(Call /\d+/(&mapId)), 5, left, 0)

                    ' RConversion.R#_interop::.as.integer at REnv.dll:line <unknown>
                    ' SMRUCC/R#.call_function.as.integer at renderMap_CLI.R:line 17
                    ' stringr.R#_interop::.str_pad at REnv.dll:line <unknown>
                    ' SMRUCC/R#.call_function.str_pad at renderMap_CLI.R:line 17
                    ' SMRUCC/R#.n/a.InitializeEnvironment at renderMap_CLI.R:line 0
                    ' SMRUCC/R#.global.<globalEnvironment> at <globalEnvironment>:line n/a
                    Return CLRVector.asCharacter(obj) _
                        .Select(AddressOf Long.Parse) _
                        .ToArray
                ElseIf type Is GetType(Boolean) Then
                    Return CLRVector.asLogical(obj) _
                        .Select(Function(b) If(b, 1, 0)) _
                        .ToArray
                Else
                    Return CLRVector.asLong(obj)
                End If
            Else
                Return CLRVector.asLong(obj)
            End If
        End Function

        ''' <summary>
        ''' # Double-Precision Vectors
        ''' 
        ''' Create, coerce to or test for a double-precision vector.
        ''' </summary>
        ''' <param name="x">object to be coerced or tested.</param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' as.double attempts to coerce its argument to be of double
        ''' type: like as.vector it strips attributes including names. 
        ''' (To ensure that an object is of double type without 
        ''' stripping attributes, use storage.mode.) Character strings 
        ''' containing optional whitespace followed by either a 
        ''' decimal representation or a hexadecimal representation 
        ''' (starting with 0x or 0X) can be converted, as can special 
        ''' values such as "NA", "NaN", "Inf" and "infinity", 
        ''' irrespective of case.
        ''' </returns>
        ''' <remarks>
        ''' as.double is a generic function. It is identical to as.numeric. 
        ''' Methods should return an object of base type "double".
        ''' 
        ''' ### Double-precision values
        ''' All R platforms are required To work With values conforming To
        ''' the IEC 60559 (also known As IEEE 754) standard. This basically
        ''' works With a precision Of 53 bits, And represents To that 
        ''' precision a range Of absolute values from about 2E-308 To 2E+308.
        ''' It also has special values NaN (many Of them), plus And minus 
        ''' infinity And plus And minus zero (although R acts As If these 
        ''' are the same). There are also denormal(ized) (Or subnormal)
        ''' numbers With absolute values above Or below the range given
        ''' above but represented To less precision.
        ''' 
        ''' See .Machine for precise information on these limits. Note that
        ''' ultimately how double precision numbers are handled Is down to 
        ''' the CPU/FPU And compiler.
        ''' 
        ''' In IEEE 754-2008/IEC605592011 this Is called 'binary64’ format.
        ''' 
        ''' ### Note on names
        ''' It Is a historical anomaly that R has two names for its floating-
        ''' point vectors, double And numeric (And formerly had real).
        ''' 
        ''' Double Is the name Of the type. numeric Is the name Of the mode 
        ''' And also Of the implicit Class. As an S4 formal Class, use 
        ''' "numeric".
        ''' 
        ''' The potential confusion Is that R has used mode "numeric" To mean 
        ''' 'double or integer’, which conflicts with the S4 usage. Thus is.numeric
        ''' tests the mode, not the class, but as.numeric (which is identical
        ''' to as.double) coerces to the class.
        ''' </remarks>
        <ExportAPI("as.double")>
        <RApiReturn(GetType(Double))>
        Public Function asDouble(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
            Return REnv.asVector(x, GetType(Double), env)
        End Function

        ''' <summary>
        ''' # Numeric Vectors
        ''' 
        ''' Creates or coerces objects of type "numeric". is.numeric is a more 
        ''' general test of an object being interpretable as numbers.
        ''' </summary>
        ''' <param name="x">object to be coerced or tested.</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' ``NULL`` will be treated as ZERO based on the rule of VisualBasic 
        ''' runtime conversion, the ``as.numeric`` function makes an array value 
        ''' copy always.
        ''' </remarks>
        <ExportAPI("as.numeric")>
        <RApiReturn(GetType(Double))>
        Public Function asNumeric(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return 0
            End If

            If TypeOf x Is list Then
                x = DirectCast(x, list).slots
            End If

            If x.GetType.ImplementInterface(GetType(IDictionary)) Then
                Return REnv.CTypeOfList(Of Double)(x, env)
            ElseIf TypeOf x Is Double() Then
                Return New vector(x, RType.GetRSharpType(GetType(Double)))
            ElseIf TypeOf x Is Integer() OrElse TypeOf x Is Long() OrElse TypeOf x Is Single() OrElse TypeOf x Is Short() Then
                Return New vector(DirectCast(x, Array).AsObjectEnumerator.Select(Function(d) CDbl(d)).ToArray, RType.GetRSharpType(GetType(Double)))
            ElseIf TypeOf x Is vector AndAlso DirectCast(x, vector).elementType Like RType.floats Then
                Return New vector(DirectCast(x, vector))
            Else
                Dim data As Object() = pipeline _
                    .TryCreatePipeline(Of Object)(x, env) _
                    .populates(Of Object)(env) _
                    .ToArray
                Dim type As Type = REnv.MeasureRealElementType(data)

                If type Is GetType(String) Then
                    ' parse string to double
                    Return data _
                        .Select(Function(obj) CStr(obj).ParseDouble) _
                        .ToArray
                Else
                    Dim dbls As New List(Of Double)
                    Dim i As Integer = 0

                    For Each item As Object In data.populateNumeric(env)
                        If Program.isException(item) Then
                            Return item
                        Else
                            ' due to the reason of gdi+ size contains two number after the
                            ' ``populateNumeric`` data converson, so the original dbls size
                            ' will not equals to the result vector
                            ' change from the vector array to type list of double
                            Call dbls.Add(CDbl(item))
                        End If
                    Next

                    Return dbls.ToArray
                End If
            End If
        End Function

        <Extension>
        Private Iterator Function populateNumeric(data As IEnumerable(Of Object), env As Environment) As IEnumerable(Of Object)
            For Each item As Object In data
                If item Is Nothing Then
                    Yield 0.0
                ElseIf TypeOf item Is String Then
                    Yield Val(DirectCast(item, String))
                ElseIf TypeOf item Is Double Then
                    Yield DirectCast(item, Double)
                ElseIf TypeOf item Is TimeSpan Then
                    Yield DirectCast(item, TimeSpan).TotalMilliseconds
                ElseIf TypeOf item Is Date Then
                    Yield DirectCast(item, Date).UnixTimeStamp
                ElseIf TypeOf item Is Size Then
                    With DirectCast(item, Size)
                        Yield .Width
                        Yield .Height
                    End With
                ElseIf TypeOf item Is SizeF Then
                    With DirectCast(item, SizeF)
                        Yield .Width
                        Yield .Height
                    End With
                Else
                    Yield RCType.CTypeDynamic(item, GetType(Double), env)
                End If
            Next
        End Function

        ''' <summary>
        ''' ### Character Vectors
        ''' 
        ''' Create or test for objects of type "character".
        ''' </summary>
        ''' <param name="x">object to be coerced or tested.</param>
        ''' <param name="args">
        ''' the additional argument values for format values to string, 
        ''' example as ``format = "F4"``.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' as.character attempts to coerce its argument to character type; 
        ''' like as.vector it strips attributes including names. For lists 
        ''' and pairlists (including language objects such as calls) it deparses 
        ''' the elements individually, except that it extracts the first element 
        ''' of length-one character vectors.
        ''' </returns>
        ''' <remarks>
        ''' as.character and is.character are generic: you can write methods 
        ''' to handle specific classes of objects, see InternalMethods. 
        ''' Further, for as.character the default method calls as.vector, 
        ''' so dispatch is first on methods for as.character and then for methods 
        ''' for as.vector.
        '''
        ''' as.character represents real And complex numbers to 15 significant 
        ''' digits (technically the compiler's setting of the ISO C constant 
        ''' DBL_DIG, which will be 15 on machines supporting IEC60559 arithmetic 
        ''' according to the C99 standard). This ensures that all the digits in 
        ''' the result will be reliable (and not the result of representation 
        ''' error), but does mean that conversion to character and back to numeric 
        ''' may change the number. If you want to convert numbers to character 
        ''' with the maximum possible precision, use format.
        ''' </remarks>
        <ExportAPI("as.character")>
        <RApiReturn(GetType(String))>
        Public Function asCharacters(<RRawVectorArgument> x As Object,
                                     <RListObjectArgument>
                                     Optional args As list = Nothing,
                                     Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return Nothing
            ElseIf TypeOf x Is Message Then
                Return x
            ElseIf x.GetType.ImplementInterface(GetType(IDictionary)) Then
                Return REnv.CTypeOfList(Of String)(x, env)
            Else
                Dim vec As Array = REnv.TryCastGenericArray(REnv.asVector(Of Object)(x), env)
                Dim schema As Type = vec.GetType

                If schema.GetElementType IsNot Nothing AndAlso
                    schema.GetElementType Is GetType(Double) Then

                    If args.hasName("format") Then
                        Dim format As String = args.getValue(Of String)("format", env)

                        Return DirectCast(vec, Double()) _
                            .Select(Function(d) d.ToString(format)) _
                            .ToArray
                    End If
                End If

                Return CLRVector.asCharacter(vec)
            End If
        End Function

        ''' <summary>
        ''' ### Character Vectors
        ''' 
        ''' Create or test for objects of type "character".
        ''' </summary>
        ''' <param name="x">object to be coerced or tested.</param>
        ''' <returns>is.character returns TRUE or FALSE depending on 
        ''' whether its argument is of character type or not.
        ''' </returns>
        <ExportAPI("is.character")>
        Public Function isCharacter(<RRawVectorArgument> x As Object) As Boolean
            If x Is Nothing Then
                Return False
            ElseIf x.GetType Is GetType(vector) Then
                x = DirectCast(x, vector).data
            ElseIf x.GetType Is GetType(list) Then
                ' 只判断list的value
                x = DirectCast(x, list).slots.Values.ToArray
            ElseIf x.GetType.ImplementInterface(GetType(IDictionary)) Then
                x = DirectCast(x, IDictionary).Values.AsSet.ToArray
            End If

            If x.GetType Like RType.characters Then
                Return True
            ElseIf x.GetType.IsArray AndAlso DirectCast(x, Array) _
                .AsObjectEnumerator _
                .All(Function(xi)
                         Return xi.GetType Like RType.characters
                     End Function) Then

                Return True
            Else
                Return False
            End If
        End Function

        <ExportAPI("is.logical")>
        Public Function isLogical(<RRawVectorArgument> x As Object) As Boolean
            If x Is Nothing Then
                Return False
            ElseIf x.GetType Is GetType(vector) Then
                x = DirectCast(x, vector).data
            ElseIf x.GetType Is GetType(list) Then
                ' 只判断list的value
                x = DirectCast(x, list).slots.Values.ToArray
            ElseIf x.GetType.ImplementInterface(GetType(IDictionary)) Then
                x = DirectCast(x, IDictionary).Values.AsSet.ToArray
            End If

            If x.GetType Like RType.logicals Then
                Return True
            ElseIf x.GetType.IsArray AndAlso DirectCast(x, Array) _
                .AsObjectEnumerator _
                .All(Function(xi)
                         Return xi.GetType Like RType.logicals
                     End Function) Then

                Return True
            Else
                Return False
            End If
        End Function

        <ExportAPI("as.logical")>
        <RApiReturn(GetType(Boolean))>
        Public Function asLogicals(<RRawVectorArgument> obj As Object, Optional env As Environment = Nothing) As Object
            If obj Is Nothing Then
                Return Nothing
            ElseIf obj.GetType.ImplementInterface(GetType(IDictionary)) Then
                Return Runtime.CTypeOfList(Of Boolean)(obj, env)
            Else
                Return CLRVector.asLogical(obj)
            End If
        End Function

        ''' <summary>
        ''' cast any R# object to raw buffer
        ''' </summary>
        ''' <param name="obj">numeric values or character vector</param>
        ''' <param name="env"></param>
        ''' <returns>A stream object that contains raw bytes data</returns>
        <ExportAPI("as.raw")>
        <RApiReturn(GetType(Stream))>
        Public Function asRaw(<RRawVectorArgument> obj As Object,
                              Optional encoding As Encodings = Encodings.UTF8WithoutBOM,
                              Optional networkByteOrder As Boolean = True,
                              Optional env As Environment = Nothing) As Object

            If obj Is Nothing Then
                Return Nothing
            ElseIf TypeOf obj Is RawStream Then
                Return New MemoryStream(DirectCast(obj, RawStream).Serialize)
            Else
                Return env.castToRawRoutine(obj, encoding, networkByteOrder)
            End If
        End Function

        ''' <summary>
        ''' running pipeline function in linq pipeline mode
        ''' </summary>
        ''' <param name="seq">any kind of object sequence in R# environment</param>
        ''' <returns></returns>
        <ExportAPI("as.pipeline")>
        Public Function asPipeline(<RRawVectorArgument> seq As Object, Optional env As Environment = Nothing) As pipeline
            Dim type As RType = Nothing
            Dim sequence = ObjectSet.GetObjectSet(seq, env, elementType:=type)

            Return New pipeline(sequence, type)
        End Function

        ''' <summary>
        ''' try to cast type class of the given data sequence 
        ''' </summary>
        ''' <param name="any"></param>
        ''' <param name="type"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("ctype")>
        Public Function castType(<RRawVectorArgument> any As Object, type As String, Optional env As Environment = Nothing) As Object
            Dim [class] As [Variant](Of RType, Message) = DataSets.CreateObject.TryGetType(type, env)

            If [class] Like GetType(Message) Then
                Return [class].TryCast(Of Message)
            End If

            If any Is Nothing Then
                Return Nothing
            ElseIf TypeOf any Is pipeline Then
                Dim pip As pipeline = DirectCast(any, pipeline)
                pip.elementType = [class].TryCast(Of RType)
                Return pip
            ElseIf TypeOf any Is vector Then
                Dim vec As vector = DirectCast(any, vector)
                vec.elementType = [class].TryCast(Of RType)
                Return vec
            ElseIf TypeOf any Is Array Then
                Dim arr As Array = DirectCast(any, Array)
                Dim vec As New vector(arr, [class].TryCast(Of RType))
                Return vec
            Else
                Return Internal.debug.stop(Message.InCompatibleType(GetType(pipeline), any.GetType, env), env)
            End If
        End Function
    End Module
End Namespace
