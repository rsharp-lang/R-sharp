#Region "Microsoft.VisualBasic::4de23a1560b1320c85729b8143327dd1, F:/GCModeller/src/R-sharp/R#//Runtime/Internal/internalInvokes/Linq/lambda.vb"

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

    '   Total Lines: 65
    '    Code Lines: 53
    ' Comment Lines: 2
    '   Blank Lines: 10
    '     File Size: 2.95 KB


    '     Module lambda
    ' 
    '         Function: CreatePreidcateLambda, CreateProjectLambda
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts

Namespace Runtime.Internal.Invokes.LinqPipeline

    Module lambda

        Public Function CreatePreidcateLambda(func As Object, env As Environment) As Func(Of Object, Integer, Boolean)
            Return CreateProjectLambda(Of Boolean)(func, env)
        End Function

        Public Function CreateProjectLambda(Of Out)(func As Object, env As Environment) As Func(Of Object, Integer, Out)
            If func.GetType.ImplementInterface(GetType(RFunction)) Then
                Dim declares As RFunction = DirectCast(func, RFunction)

                Return Function(x, i)
                           Return RCType.CTypeDynamic(declares.Invoke(env, InvokeParameter.CreateLiterals(x, i)), GetType(Out), env)
                       End Function
            ElseIf TypeOf func Is Func(Of Object, Out) Then
                Dim lambda As Func(Of Object, Out) = func

                Return Function(x, i)
                           Return lambda(x)
                       End Function
            ElseIf TypeOf func Is Func(Of Object, Integer, Out) Then
                Return func
            ElseIf TypeOf func Is Func(Of Out) Then
                Dim lambda As Func(Of Out) = func

                Return Function(x, i)
                           Return lambda()
                       End Function
            ElseIf TypeOf func Is MethodInfo Then
                Dim declares As MethodInfo = DirectCast(func, MethodInfo)

                If declares.ReturnType Is GetType(Out) AndAlso declares.GetParameters.Length <= 2 AndAlso Not declares.IsGenericMethod Then
                    Dim args = declares.GetParameters

                    If args.Length = 0 Then
                        Return Function(x, i)
                                   Return declares.Invoke(Nothing, {})
                               End Function
                    ElseIf args.Length = 1 AndAlso args(Scan0).ParameterType Is GetType(Object) Then
                        Return Function(x, i)
                                   Return declares.Invoke(Nothing, {x})
                               End Function
                    ElseIf args.Length = 2 AndAlso args(Scan0).ParameterType Is GetType(Object) AndAlso args(1).ParameterType Is GetType(Integer) Then
                        Return Function(x, i)
                                   Return declares.Invoke(Nothing, {x, i})
                               End Function
                    Else
                        ' do nothing
                    End If
                Else
                    ' do nothing
                End If
            End If

            Return Nothing
        End Function
    End Module
End Namespace
