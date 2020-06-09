#Region "Microsoft.VisualBasic::dbee26b69ac0f5c8d218d9006b50bfc2, R#\Runtime\Internal\internalInvokes\applys.vb"

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

'     Module applys
' 
'         Function: apply, lapply, sapply
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RObj = SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Internal.Invokes

    Module applys

        <ExportAPI("apply")>
        Public Function apply(x As Object, margin As margins, FUN As Object, Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return x
            ElseIf TypeOf x Is dataframe Then
                Return doApply.apply(DirectCast(x, dataframe), margin, FUN, env)
            Else
                Return debug.stop(New InvalidProgramException, env)
            End If
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
        <RApiReturn(GetType(vector))>
        Public Function sapply(<RRawVectorArgument> X As Object, FUN As Object, envir As Environment) As Object
            If FUN Is Nothing Then
                Return Internal.debug.stop({"Missing apply function!"}, envir)
            ElseIf Not FUN.GetType.ImplementInterface(GetType(RFunction)) Then
                Return Internal.debug.stop({"Target is not a function!"}, envir)
            End If

            If Program.isException(X) Then
                Return X
            ElseIf Program.isException(FUN) Then
                Return FUN
            ElseIf X Is Nothing Then
                Return Nothing
            End If

            Dim apply As RFunction = FUN

            If X.GetType Is GetType(list) Then
                X = DirectCast(X, list).slots
            End If

            If X.GetType.ImplementInterface(GetType(IDictionary)) Then
                Dim list = DirectCast(X, IDictionary)
                Dim seq As New List(Of Object)
                Dim names As New List(Of String)
                Dim value As Object

                For Each key As Object In list.Keys
                    value = apply.Invoke(envir, invokeArgument(list(key)))

                    If Program.isException(value) Then
                        Return value
                    End If

                    seq.Add(Runtime.single(value))
                    names.Add(Scripting.ToString(key))
                Next

                Return New RObj.vector(names, seq.ToArray, envir)
            Else
                Dim seq As New List(Of Object)
                Dim value As Object

                For Each d In Runtime.asVector(Of Object)(X) _
                    .AsObjectEnumerator

                    value = apply.Invoke(envir, invokeArgument(d))

                    If Program.isException(value) Then
                        Return value
                    Else
                        seq.Add(Runtime.single(value))
                    End If
                Next

                Return New RObj.vector With {.data = seq.ToArray}
            End If
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
                Return Internal.debug.stop({"Missing apply function!"}, envir)
            ElseIf Not FUN.GetType.ImplementInterface(GetType(RFunction)) Then
                Return Internal.debug.stop({"Target is not a function!"}, envir)
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
                Dim getName As Func(Of SeqValue(Of Object), String) = keyNameAuto(names, envir)
                Dim keyName$
                Dim value As Object

                For Each d As SeqValue(Of Object) In REnv.asVector(Of Object)(X) _
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

        Private Function keyNameAuto(type As Type) As Func(Of Object, String)
            Static cache As New Dictionary(Of Type, Func(Of Object, String))

            Return cache.ComputeIfAbsent(
                key:=type,
                lazyValue:=Function(key As Type)
                               If key.ImplementInterface(GetType(INamedValue)) Then
                                   Return Function(a) DirectCast(a, INamedValue).Key
                               ElseIf key.ImplementInterface(GetType(IReadOnlyId)) Then
                                   Return Function(a) DirectCast(a, IReadOnlyId).Identity
                               ElseIf key.ImplementInterface(GetType(IKeyedEntity(Of String))) Then
                                   Return Function(a) DirectCast(a, IKeyedEntity(Of String)).Key
                               Else
                                   Return Function() Nothing
                               End If
                           End Function)
        End Function

        Public Function keyNameAuto(names As RFunction, env As Environment) As Func(Of SeqValue(Of Object), String)
            If names Is Nothing Then
                Return Function(i)
                           Dim name As String = Nothing

                           If Not i.value Is Nothing Then
                               name = keyNameAuto(i.value.GetType)(i.value)
                           End If

                           If name Is Nothing Then
                               name = $"[[{i.i + 1}]]"
                           End If

                           Return name
                       End Function
            Else
                Return Function(i)
                           Dim nameVals = names.Invoke(env, invokeArgument(i.value))
                           Dim namesVec = RConversion.asCharacters(nameVals)

                           Return getFirst(namesVec)
                       End Function
            End If
        End Function
    End Module
End Namespace
