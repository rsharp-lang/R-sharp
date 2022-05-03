#Region "Microsoft.VisualBasic::dcc93a6927f898408b0e5268956af07c, R-sharp\R#\Runtime\Internal\objects\dataset\pipeline.vb"

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

'   Total Lines: 251
'    Code Lines: 169
' Comment Lines: 51
'   Blank Lines: 31
'     File Size: 10.89 KB


'     Class pipeline
' 
'         Properties: [pipeFinalize], isError, isMessage
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: CreateFromPopulator, createVector, fromVector, getError, populates
'                   ToString, TryCastGroupStream, TryCastObjectVector, TryCreatePipeline
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Object

    ''' <summary>
    ''' The R# pipeline
    ''' </summary>
    Public Class pipeline : Inherits RsharpDataObject

        Friend ReadOnly pipeline As IEnumerable

        ''' <summary>
        ''' 在抛出数据的时候所遇到的第一个错误消息
        ''' </summary>
        Dim populatorFirstErr As Message

        Public Property [pipeFinalize] As Action

        ''' <summary>
        ''' contains an error message in the pipeline populator or 
        ''' the pipeline data is an error message
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isError As Boolean
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
                Return TypeOf pipeline Is Message AndAlso GetType(Message) Is elementType
            End Get
        End Property

        Sub New(input As IEnumerable, type As Type)
            pipeline = input
            elementType = RType.GetRSharpType(type)
        End Sub

        Sub New(input As IEnumerable, type As RType)
            pipeline = input
            elementType = type
        End Sub

        Public Function getError() As Message
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
        ''' <typeparam name="T"></typeparam>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Public Iterator Function populates(Of T)(env As Environment) As IEnumerable(Of T)
            Dim cast As T

            For Each obj As Object In pipeline
                If TypeOf obj Is Message Then
                    populatorFirstErr = DirectCast(obj, Message)
                Else
                    Try
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
        ''' </returns>
        Public Shared Function TryCreatePipeline(Of T)(upstream As Object, env As Environment,
                                                       Optional suppress As Boolean = False,
                                                       <CallerMemberName>
                                                       Optional callerFrameName$ = Nothing) As pipeline

            If TypeOf upstream Is Dictionary(Of String, Object).ValueCollection Then
                upstream = DirectCast(upstream, Dictionary(Of String, Object).ValueCollection).ToArray
            End If

            If upstream Is Nothing Then
                Return Internal.debug.stop("the upstream data can not be nothing!", env)

            ElseIf TypeOf upstream Is pipeline Then
                If DirectCast(upstream, pipeline).elementType Like GetType(T) Then
                    Return upstream
                ElseIf GetType(T).IsInterface AndAlso DirectCast(upstream, pipeline).elementType.raw.ImplementInterface(Of T) Then
                    Return upstream
                Else
                    Return Message.InCompatibleType(GetType(T), DirectCast(upstream, pipeline).elementType.raw, env, suppress:=suppress)
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
                Return CreateFromPopulator(Of T)({upstream})

            Else
                Return Message.InCompatibleType(GetType(T), upstream.GetType, env, suppress:=suppress)
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
            Dim type As Type = MeasureRealElementType(objs)

            If type Is GetType(T) OrElse GetType(T) Is GetType(Object) Then
                Return New pipeline(objs, RType.GetRSharpType(type))
            Else
                Return Message.InCompatibleType(GetType(T), type, env, suppress:=suppress)
            End If
        End Function

        Public Shared Widening Operator CType([error] As Message) As pipeline
            Return New pipeline([error], GetType(Message))
        End Operator

    End Class
End Namespace
