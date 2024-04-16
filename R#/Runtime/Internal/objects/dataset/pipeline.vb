#Region "Microsoft.VisualBasic::0ce14e736a33bf5748efb19b40faa7eb, D:/GCModeller/src/R-sharp/R#//Runtime/Internal/objects/dataset/pipeline.vb"

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

'   Total Lines: 308
'    Code Lines: 200
' Comment Lines: 64
'   Blank Lines: 44
'     File Size: 13.52 KB


'     Class pipeline
' 
'         Properties: [pipeFinalize], isError, isMessage
' 
'         Constructor: (+3 Overloads) Sub New
'         Function: CreateFromPopulator, createVector, fromVector, getError, populates
'                   ToString, TryCastGroupStream, TryCastObjectVector, TryCreatePipeline
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.[Interface]
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Object

    ''' <summary>
    ''' The R# data pipeline
    ''' 
    ''' this model object is a kind of wrapper of the .net clr <see cref="IEnumerable"/> interface
    ''' </summary>
    Public Class pipeline : Inherits RsharpDataObject
        Implements RPipeline

        Friend ReadOnly pipeline As IEnumerable

        ''' <summary>
        ''' 在抛出数据的时候所遇到的第一个错误消息
        ''' </summary>
        Dim populatorFirstErr As Message

        ''' <summary>
        ''' The action will be called after finish loop on the sequence. 
        ''' Null value of this property will be ignored
        ''' </summary>
        ''' <returns></returns>
        Public Property [pipeFinalize] As Action

        ''' <summary>
        ''' contains an error message in the pipeline populator or 
        ''' the pipeline data is an error message. You can get the
        ''' error message via function <see cref="getError()"/> when
        ''' this property value is TRUE
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isError As Boolean Implements RPipeline.isError
            Get
                If Not populatorFirstErr Is Nothing Then
                    Return True
                End If

                Return TypeOf pipeline Is Message AndAlso DirectCast(pipeline, Message).level = MSG_TYPES.ERR
            End Get
        End Property

        ''' <summary>
        ''' current data pipeline is a data message?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isMessage As Boolean
            Get
                Return populatorFirstErr IsNot Nothing AndAlso
                    pipeline Is Nothing AndAlso
                    GetType(Message) Is elementType
            End Get
        End Property

        Sub New(input As IEnumerable, type As Type)
            pipeline = input
            elementType = RType.GetRSharpType(type)
        End Sub

        Sub New(message As Message)
            elementType = RType.GetRSharpType(GetType(Message))
            populatorFirstErr = message
        End Sub

        Sub New(input As IEnumerable, type As RType)
            pipeline = input
            elementType = type
        End Sub

        ''' <summary>
        ''' Gets the R# error message data
        ''' </summary>
        ''' <returns></returns>
        Public Function getError() As Message Implements RPipeline.getError
            If Not populatorFirstErr Is Nothing Then
                Return populatorFirstErr
            ElseIf isError Then
                Return DirectCast(CObj(pipeline), Message)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' Only suite for small set of data
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' If the data in current pipeline is super large, then
        ''' the program will be crashed.
        ''' </remarks>
        Public Function createVector(env As Environment) As vector
            Return New vector(elementType, populates(Of Object)(env), env)
        End Function

        ''' <summary>
        ''' populate the data element from the pipeline stream
        ''' </summary>
        ''' <typeparam name="T">the .net clr generic type constraint</typeparam>
        ''' <param name="env"></param>
        ''' <returns>direct cast</returns>
        Public Iterator Function populates(Of T)(env As Environment) As IEnumerable(Of T)
            Dim cast As T
            Dim objWrapper As Boolean = GetType(T) Is GetType(vbObject)

            For Each obj As Object In pipeline
                If TypeOf obj Is Message Then
                    populatorFirstErr = DirectCast(obj, Message)
                Else
                    Try
                        If (Not objWrapper) AndAlso TypeOf obj Is vbObject Then
                            obj = DirectCast(obj, vbObject).target
                        End If

                        cast = Nothing
                        cast = DirectCast(obj, T)
                    Catch ex As Exception
                        Dim warnings As String() = {
                            "the given pipeline is early stop due to an unexpected error message was generated from upstream."
                        }.JoinIterates(populatorFirstErr.message.Select(Function(msg) $"Err: " & msg)) _
                         .ToArray

                        populatorFirstErr = Internal.debug.stop(ex, env)
                        env.AddMessage(warnings, MSG_TYPES.WRN)

                        Exit For
                    End Try

                    Yield cast
                End If
            Next

            If Not _pipeFinalize Is Nothing Then
                Call _pipeFinalize()
            End If
        End Function

        Public Overrides Function ToString() As String
            If isError Then
                Return DirectCast(pipeline, Message).ToString
            Else
                Return $"pipeline[{elementType.ToString}]"
            End If
        End Function

        ''' <summary>
        ''' Create pipeline object from the given upstream data
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="upstream">the upstream data</param>
        ''' <param name="finalize"></param>
        ''' <returns></returns>
        <DebuggerStepThrough>
        Public Shared Function CreateFromPopulator(Of T)(upstream As IEnumerable(Of T), Optional finalize As Action = Nothing) As pipeline
            Return New pipeline(upstream, GetType(T)) With {
                .pipeFinalize = finalize
            }
        End Function

        Private Shared Function fromVector(Of T)(upstream As vector, env As Environment, suppress As Boolean) As pipeline
            If upstream.length = 0 Then
                env.AddMessage("the input vector is in empty size!", MSG_TYPES.WRN)
                Return CreateFromPopulator(New T() {})
            End If

            If upstream.elementType Like GetType(T) Then
                Return CreateFromPopulator(upstream.data.AsObjectEnumerator(Of T))
            ElseIf GetType(T) Is GetType(Object) Then
                Return CreateFromPopulator(upstream.data.AsObjectEnumerator)
            ElseIf GetType(T).IsInterface AndAlso upstream.elementType.raw.ImplementInterface(Of T) Then
                Return CreateFromPopulator(upstream.data.AsObjectEnumerator.Select(Function(o) CType(o, T)))
            ElseIf upstream.elementType.raw.IsInheritsFrom(GetType(T)) Then
                Return CreateFromPopulator(upstream.data.AsObjectEnumerator.Select(Function(o) CType(o, T)))
            Else
                Return TryCastObjectVector(Of T)(upstream.data.AsObjectEnumerator.ToArray, env, suppress)
            End If
        End Function

        ''' <summary>
        ''' try create populator with specific type constraint
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="upstream"></param>
        ''' <param name="env"></param>
        ''' <param name="suppress"></param>
        ''' <param name="callerFrameName">debug used only</param>
        ''' <returns>
        ''' the required data sequence or an error message if the 
        ''' upstream element type is not matched of the required 
        ''' target type.
        ''' 
        ''' this function also will returns a null reference error
        ''' message if the given <paramref name="upstream"/> data is
        ''' ``Nothing``.
        ''' </returns>
        Public Shared Function TryCreatePipeline(Of T)(upstream As Object, env As Environment,
                                                       Optional suppress As Boolean = False,
                                                       <CallerMemberName>
                                                       Optional callerFrameName$ = Nothing) As pipeline

            If TypeOf upstream Is Dictionary(Of String, Object).ValueCollection Then
                upstream = DirectCast(upstream, Dictionary(Of String, Object).ValueCollection).ToArray
            End If
            If TypeOf upstream Is vbObject Then
                upstream = DirectCast(upstream, vbObject).target
            End If

            If upstream Is Nothing Then
                Return Internal.debug.stop("the upstream data can not be nothing!", env, suppress_log:=suppress)
            End If

            Dim upstream_type As Type = upstream.GetType

            If TypeOf upstream Is pipeline Then
                If DirectCast(upstream, pipeline).elementType Like GetType(T) Then
                    Return upstream
                ElseIf GetType(T).IsInterface AndAlso DirectCast(upstream, pipeline).elementType.raw.ImplementInterface(Of T) Then
                    Return upstream
                Else
                    Return Message.InCompatibleType(GetType(T), DirectCast(upstream, pipeline).elementType.raw, env,
                                                    suppress:=suppress,
                                                    suppress_log:=suppress)
                End If

            ElseIf TypeOf upstream Is T() Then
                Return CreateFromPopulator(Of T)(DirectCast(upstream, T()))

            ElseIf (Not GetType(T) Is GetType(Object)) AndAlso TypeOf upstream Is T Then
                Return CreateFromPopulator(Of T)({DirectCast(upstream, T)})

            ElseIf TypeOf upstream Is IEnumerable(Of T) Then
                Return CreateFromPopulator(Of T)(DirectCast(upstream, IEnumerable(Of T)))

            ElseIf TypeOf upstream Is vector Then
                Return fromVector(Of T)(DirectCast(upstream, vector), env, suppress)

            ElseIf TypeOf upstream Is Object() Then
                Return TryCastObjectVector(Of T)(DirectCast(upstream, Object()), env, suppress)

            ElseIf TypeOf upstream Is list Then
                ' unlist
                Return DirectCast(upstream, list).data _
                    .DoCall(Function(ls)
                                Return TryCastObjectVector(Of T)(ls.ToArray, env, suppress)
                            End Function)

            ElseIf TypeOf upstream Is Group Then
                Return TryCastGroupStream(Of T)(DirectCast(upstream, Group), env, callerFrameName, suppress)

            ElseIf GetType(T) Is GetType(Object) Then
                If upstream_type.IsArray Then
                    Return CreateFromPopulator(Of T)(From x In DirectCast(upstream, Array) Select x)
                Else
                    Return CreateFromPopulator(Of T)({upstream})
                End If
            ElseIf upstream_type.IsArray Then
                If GetType(T).IsInterface AndAlso upstream_type.GetElementType.ImplementInterface(Of T) Then
                    Return CreateFromPopulator(DirectCast(upstream, Array).AsObjectEnumerator.Select(Function(o) DirectCast(o, T)))
                Else
                    Return Message.InCompatibleType(GetType(T), upstream_type, env, suppress:=suppress, suppress_log:=suppress)
                End If
            ElseIf DataFramework.IsNumericType(upstream_type) AndAlso DataFramework.IsNumericType(GetType(T)) Then
                Dim vec As Array = Array.CreateInstance(GetType(T), 1)
                vec(0) = Conversion.CTypeDynamic(upstream, GetType(T))
                Return CreateFromPopulator(DirectCast(vec, T()))
            ElseIf DataFramework.IsNumericCollection(upstream_type) AndAlso DataFramework.IsNumericType(GetType(T)) Then
                Dim pull As Func(Of IEnumerable(Of T)) =
                    Iterator Function() As IEnumerable(Of T)
                        For Each el As Object In DirectCast(upstream, IEnumerable)
                            Yield Conversion.CTypeDynamic(el, GetType(T))
                        Next
                    End Function
                Return CreateFromPopulator(pull())
            Else
                Return Message.InCompatibleType(GetType(T), upstream_type, env, suppress:=suppress, suppress_log:=suppress)
            End If
        End Function

        Private Shared Function TryCastGroupStream(Of T)(pipGroup As Group, env As Environment, callerFrameName$, suppress As Boolean) As pipeline
            If pipGroup.length = 0 Then
                Call env.AddMessage({
                    $"the given group is empty!",
                    $"caller: {callerFrameName}"
                }, MSG_TYPES.WRN)

                Return [Object].pipeline.CreateFromPopulator(Of T)({})
            Else
                Dim seq As Object() = pipGroup.group _
                    .AsObjectEnumerator(Of Object) _
                    .ToArray

                Return TryCastObjectVector(Of T)(seq, env, suppress)
            End If
        End Function

        Private Shared Function TryCastObjectVector(Of T)(objs As Object(), env As Environment, suppress As Boolean) As pipeline
            Dim target As Type = GetType(T)
            Dim requireAbstractType As Boolean = target.IsAbstract OrElse target.IsInterface
            Dim types As Type() = MeasureVectorTypes(
                array:=objs,
                unique:=False,
                excludeInterfaceOrAbstract:=Not requireAbstractType
            ).ToArray
            Dim type As Type = MeasureVectorType(types)

            If types.Length = 0 Then
                Return New pipeline(objs, RType.GetRSharpType(GetType(T)))
            End If

            If type Is GetType(T) OrElse GetType(T) Is GetType(Object) Then
                Return New pipeline(objs, RType.GetRSharpType(type))
            ElseIf type.IsInheritsFrom(GetType(T), strict:=False) Then
                Return New pipeline(objs, RType.GetRSharpType(type))
            ElseIf types.Distinct.All(Function(subtype) subtype.IsInheritsFrom(GetType(T), strict:=False)) Then
                Return New pipeline(objs, RType.GetRSharpType(GetType(T)))
            Else
                Return Message.InCompatibleType(GetType(T), type, env, suppress:=suppress, suppress_log:=suppress)
            End If
        End Function

        Public Shared Widening Operator CType([error] As Message) As pipeline
            Return New pipeline([error])
        End Operator

    End Class
End Namespace
