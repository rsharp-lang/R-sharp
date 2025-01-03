﻿#Region "Microsoft.VisualBasic::69ed35ef4ea0ad5de2c1a77179c7b77f, R#\Runtime\Internal\internalInvokes\Linq\keyIndex.vb"

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

    '   Total Lines: 116
    '    Code Lines: 91 (78.45%)
    ' Comment Lines: 5 (4.31%)
    '    - Xml Docs: 60.00%
    ' 
    '   Blank Lines: 20 (17.24%)
    '     File Size: 4.90 KB


    '     Module keyIndex
    ' 
    '         Function: indexName, InternalKeyTypeExtractor, (+2 Overloads) keyNameAuto
    '         Class funcEvalKey
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: GetName
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports any0 = Microsoft.VisualBasic.Scripting

Namespace Runtime.Internal.Invokes.LinqPipeline

    ''' <summary>
    ''' get key name index for the lapply/parLapply
    ''' </summary>
    Module keyIndex

        Friend Function keyNameAuto(type As Type) As Func(Of Object, String)
            Static cache As New Dictionary(Of Type, Func(Of Object, String))

            Return cache.ComputeIfAbsent(
                key:=type,
                lazyValue:=Function(key As Type)
                               Return InternalKeyTypeExtractor(key)
                           End Function)
        End Function

        Private Function InternalKeyTypeExtractor(key As Type) As Func(Of Object, String)
            If key.ImplementInterface(GetType(INamedValue)) Then
                Return Function(a) DirectCast(a, INamedValue).Key
            ElseIf key.ImplementInterface(GetType(IReadOnlyId)) Then
                Return Function(a) DirectCast(a, IReadOnlyId).Identity
            ElseIf key.ImplementInterface(GetType(IKeyedEntity(Of String))) Then
                Return Function(a) DirectCast(a, IKeyedEntity(Of String)).Key
            ElseIf key Is GetType(Group) Then
                Return Function(g) any0.ToString(DirectCast(g, Group).key)
            ElseIf key Is GetType(KeyValuePair(Of String, Object)) Then
                Return Function(t) DirectCast(t, KeyValuePair(Of String, Object)).Key
            ElseIf key.Name = "KeyValuePair" AndAlso key.GetGenericArguments.FirstOrDefault Is GetType(String) Then
                Dim getKey = key.GetProperties(PublicProperty) _
                    .Where(Function(p) p.Name = NameOf(KeyValuePair(Of String, Object).Key)) _
                    .FirstOrDefault

                ' value could be direct cast to string
                Return Function(o) CStr(getKey.GetValue(o))
            ElseIf key.Name = "KeyValuePair" Then
                Dim getKey = key.GetProperties(PublicProperty) _
                   .Where(Function(p) p.Name = NameOf(KeyValuePair(Of String, Object).Key)) _
                   .FirstOrDefault

                ' value should be do type cast to string
                Return Function(o) any0.ToString(getKey.GetValue(o))
            Else
                Return Function() Nothing
            End If
        End Function

        Friend Function indexName(i As SeqValue(Of Object)) As [Variant](Of String, Message)
            Dim name As String = Nothing

            If Not i.value Is Nothing Then
                name = keyNameAuto(i.value.GetType)(i.value)
            End If

            If name Is Nothing Then
                name = $"[[{i.i + 1}]]"
            End If

            Return name
        End Function

        Private Class funcEvalKey

            ReadOnly func As RFunction
            ReadOnly env As Environment

            Sub New(func As RFunction, env As Environment)
                Me.env = env
                Me.func = func
            End Sub

            Public Function GetName(i As SeqValue(Of Object)) As [Variant](Of String, Message)
                Dim nameVals = func.Invoke(env, invokeArgument(i.value))
                Dim namesVec As Object

                If TypeOf nameVals Is Message Then
                    Return DirectCast(nameVals, Message)
                Else
                    namesVec = RConversion.asCharacters(nameVals)
                End If

                Return CStr(getFirst(namesVec))
            End Function
        End Class

        Public Function keyNameAuto(names As Object, env As Environment) As Func(Of SeqValue(Of Object), [Variant](Of String, Message))
            If names Is Nothing Then
                Return AddressOf indexName
            ElseIf names.GetType.ImplementInterface(Of RFunction) Then
                Dim func As RFunction = DirectCast(names, RFunction)
                Dim eval As New funcEvalKey(func, env)

                Return AddressOf eval.GetName
            Else
                Dim vec As String() = CLRVector.asCharacter(names)

                Return Function(i)
                           Return vec(i)
                       End Function
            End If
        End Function
    End Module
End Namespace
