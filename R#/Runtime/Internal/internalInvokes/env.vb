#Region "Microsoft.VisualBasic::6a43bd87fe1d5368b473f8afc664440b, R#\Runtime\Internal\internalInvokes\env.vb"

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

'     Module env
' 
'         Function: [get], CallInternal, doCall, environment, globalenv
'                   ls, objects
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.System.Package

Namespace Runtime.Internal.Invokes

    Module env

        ''' <summary>
        ''' # Return the Value of a Named Object
        ''' 
        ''' Search by name for an object (get) or zero or more objects (mget).
        ''' </summary>
        ''' <param name="x">For get, an object name (given as a character string).
        ''' For mget, a character vector of object names.</param>
        ''' <param name="envir">where to look for the object (see ‘Details’); if omitted search as if the name of the object appeared unquoted in an expression.</param>
        ''' <param name="inherits">
        ''' should the enclosing frames of the environment be searched?
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("get")>
        Public Function [get](x As Object, envir As Environment, Optional [inherits] As Boolean = True) As Object
            Dim name As String = Runtime.asVector(Of Object)(x) _
                .DoCall(Function(o)
                            Return Scripting.ToString(Runtime.getFirst(o), null:=Nothing)
                        End Function)

            If name.StringEmpty Then
                Return Internal.stop("NULL value provided for object name!", envir)
            End If

            Dim symbol As Variable = envir.FindSymbol(name, [inherits])

            If symbol Is Nothing Then
                Return Message.SymbolNotFound(envir, name, TypeCodes.generic)
            Else
                Return symbol.value
            End If
        End Function

        <ExportAPI("globalenv")>
        <DebuggerStepThrough>
        Private Function globalenv(env As Environment) As Object
            Return env.globalEnvironment
        End Function

        <ExportAPI("environment")>
        <DebuggerStepThrough>
        Private Function environment(env As Environment) As Object
            Return env
        End Function

        ''' <summary>
        ''' # List Objects
        ''' 
        ''' ``ls`` and ``objects`` return a vector of character strings giving 
        ''' the names of the objects in the specified environment. When invoked 
        ''' with no argument at the top level prompt, ls shows what data sets 
        ''' and functions a user has defined. When invoked with no argument inside 
        ''' a function, ls returns the names of the function's local variables: 
        ''' this is useful in conjunction with ``browser``.
        ''' </summary>
        ''' <param name="name">The package name, which environment to use in 
        ''' listing the available objects. Defaults to the current environment. 
        ''' Although called name for back compatibility, in fact this argument 
        ''' can specify the environment in any form.</param>
        ''' <param name="env">
        ''' an alternative argument to name for specifying the environment. 
        ''' Mostly there for back compatibility.
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("ls")>
        Private Function ls(<RSymbolTextArgument> Optional name$ = Nothing, Optional env As Environment = Nothing) As Object
            If name.StringEmpty Then
                Return env.variables.Keys.ToArray
            Else
                Dim globalEnv = env.globalEnvironment
                Dim package As Package = globalEnv.packages.FindPackage(name, Nothing)

                If package Is Nothing Then
                    Return {}
                Else
                    Return package.ls
                End If
            End If
        End Function

        <ExportAPI("objects")>
        Private Function objects(env As Environment) As Object
            Return env.variables.Keys.ToArray
        End Function

        ''' <summary>
        ''' # Execute a Function Call
        ''' 
        ''' ``do.call`` constructs and executes a function call from a name or 
        ''' a function and a list of arguments to be passed to it.
        ''' </summary>
        ''' <param name="what"></param>
        ''' <param name="calls">
        ''' either a function or a non-empty character string naming the function 
        ''' to be called.
        ''' </param>
        ''' <param name="args">
        ''' a list of arguments to the function call. The names attribute of 
        ''' args gives the argument names.
        ''' </param>
        ''' <param name="envir">
        ''' an environment within which to evaluate the call. This will be most 
        ''' useful if what is a character string and the arguments are symbols 
        ''' or quoted expressions.
        ''' </param>
        ''' <returns>The result of the (evaluated) function call.</returns>
        <ExportAPI("do.call")>
        Public Function doCall(what As Object, calls$,
                               <RListObjectArgument>
                               Optional args As Object = Nothing,
                               Optional envir As Environment = Nothing) As Object

            If what Is Nothing OrElse calls.StringEmpty Then
                Return Internal.stop("Nothing to call!", envir)
            ElseIf what.GetType Is GetType(String) Then
                ' call static api by name
                Return CallInternal(what, args, envir)
            End If

            Dim targetType As Type = what.GetType

            ' call api from an object instance 
            If targetType Is GetType(vbObject) Then
                Dim Robj As vbObject = DirectCast(what, vbObject)
                Dim member As Object

                If Not Robj.existsName(calls) Then
                    ' throw exception for invoke missing member from .NET object?
                    Return Internal.stop({$"Missing member '{calls}' in target {what}", "type: " & Robj.type.fullName, "member name: " & calls}, envir)
                Else
                    member = Robj.getByName(name:=calls)
                End If

                ' invoke .NET API / property getter
                If member.GetType Is GetType(RMethodInfo) Then
                    Dim arguments As InvokeParameter() = args
                    Dim api As RMethodInfo = DirectCast(member, RMethodInfo)

                    Return api.Invoke(envir, arguments)
                Else
                    Return member
                End If
            ElseIf targetType Is GetType(list) Then
                Throw New NotImplementedException
            Else
                Return Internal.stop(New NotImplementedException(targetType.FullName), envir)
            End If
        End Function

        Public Function CallInternal(call$, args As Object, envir As Environment) As Object
            Return Internal.stop(New NotImplementedException("Call internal functions"), envir)
        End Function
    End Module
End Namespace
