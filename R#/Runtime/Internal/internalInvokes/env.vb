#Region "Microsoft.VisualBasic::0b1d472af882c4934fb94da793286883, D:/GCModeller/src/R-sharp/R#//Runtime/Internal/internalInvokes/env.vb"

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

'   Total Lines: 523
'    Code Lines: 282
' Comment Lines: 189
'   Blank Lines: 52
'     File Size: 23.60 KB


'     Module env
' 
'         Function: [get], [gettype], [set], [typeof], CallClrMemberFunction
'                   CallInternal, CallMemberFunction, doCall, environment, exists
'                   getCallLambda, getCurrentTrace, getOutputDevice, globalenv, listOptionItems
'                   lockBinding, ls, objects, objectSize, traceback
'                   unlockBinding
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Invokes

    <Package("env")>
    Module env

        <ExportAPI("exists")>
        Public Function exists(name As String, Optional envir As Environment = Nothing) As Boolean
            Return envir.FindSymbol(name) IsNot Nothing
        End Function

        ''' <summary>
        ''' ### Assign a Value to a Name
        ''' 
        ''' Assign a value to a name in an environment.
        ''' </summary>
        ''' <param name="env">the environment To use. See 'Details’.</param>
        ''' <param name="name">
        ''' a variable name, given as a character string. No coercion is done, 
        ''' and the first element of a character vector of length greater than 
        ''' one will be used, with a warning.</param>
        ''' <param name="value">a value To be assigned To x.</param>
        ''' <returns>
        ''' This function is invoked for its side effect, which is assigning 
        ''' value to the variable x. If no envir is specified, then the assignment 
        ''' takes place in the currently active environment.
        ''' If inherits Is TRUE, enclosing environments of the supplied environment 
        ''' are searched until the variable x Is encountered. The value Is Then 
        ''' assigned In the environment In which the variable Is encountered 
        ''' (provided that the binding Is Not locked: see lockBinding : If it Is, 
        ''' an error Is signaled). If the symbol Is Not encountered Then assignment 
        ''' takes place In the user's workspace (the global environment).
        ''' If inherits Is FALSE, assignment takes place in the initial frame of 
        ''' envir, unless an existing binding Is locked Or there Is no existing 
        ''' binding And the environment Is locked (when an error Is signaled).
        ''' </returns>
        ''' <remarks>
        ''' There are no restrictions on the name given as x: it can be a 
        ''' non-syntactic name (see make.names).
        ''' The pos argument can specify the environment In which To assign 
        ''' the Object In any Of several ways: as -1 (the default), as a 
        ''' positive integer (the position in the search list); as the 
        ''' character string name of an element in the search list; Or as 
        ''' an environment (including using sys.frame to access the currently 
        ''' active function calls). The envir argument Is an alternative way 
        ''' to specify an environment, but Is primarily for back compatibility.
        ''' assign does Not dispatch assignment methods, so it cannot be used 
        ''' To Set elements Of vectors, names, attributes, etc.
        ''' Note that assignment To an attached list Or data frame changes the 
        ''' attached copy And Not the original Object: see attach And With.
        ''' </remarks>
        ''' <example>
        ''' set(globalenv(), "symbol_name") &lt;- value;
        ''' </example>
        <ExportAPI("set")>
        Public Function [set](env As Environment, name As String, <RByRefValueAssign> <RRawVectorArgument> value As Object) As Object
            Return env.AssignSymbol(name, value)
        End Function

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
            Dim name As String = REnv.asVector(Of Object)(x) _
                .DoCall(Function(o)
                            Return Scripting.ToString(REnv.getFirst(o), null:=Nothing)
                        End Function)

            If name.StringEmpty Then
                Return Internal.debug.stop("NULL value provided for object name!", envir)
            Else
                Return SymbolReference.GetReferenceObject(name, envir, [inherits])
            End If
        End Function

        ''' <summary>
        ''' Get global environment
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("globalenv")>
        <DebuggerStepThrough>
        Private Function globalenv(env As Environment) As Object
            Return env.globalEnvironment
        End Function

        ''' <summary>
        ''' Get current environment or envirnment of target function closure
        ''' </summary>
        ''' <param name="env"></param>
        ''' <param name="fun">
        ''' a Function, a formula, Or NULL, which Is the default.
        ''' </param>
        ''' <returns>
        ''' If fun is a function or a formula then environment(fun) returns 
        ''' the environment associated with that function or formula. If fun 
        ''' is NULL then the current evaluation environment is returned.
        ''' </returns>
        <ExportAPI("environment")>
        <DebuggerStepThrough>
        Private Function environment(Optional fun As DeclareNewFunction = Nothing, Optional env As Environment = Nothing) As Object
            If fun Is Nothing Then
                Return env
            Else
                Return fun.envir
            End If
        End Function

        ''' <summary>
        ''' Get the standard output device name string
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("output_device")>
        Private Function getOutputDevice(env As Environment) As String
            Return env.globalEnvironment.stdout.env.ToString.ToLower
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
        <RApiReturn(GetType(String))>
        Private Function ls(<RSymbolTextArgument> Optional name$ = Nothing, Optional env As Environment = Nothing) As Object
            Dim opt As NamedValue(Of String) = name.GetTagValue(":", trim:=True)
            Dim globalEnv As GlobalEnvironment = env.globalEnvironment
            Dim pkgMgr As PackageManager = globalEnv.packages

            If name.StringEmpty Then
                ' list all of the objects in current 
                ' R# runtime environment
                Return env.GetSymbolsNames.ToArray
            ElseIf opt.Name.StringEmpty Then
                Return opt.listOptionItems(name, env)
            End If

            Select Case opt.Name.ToLower
                Case "package"
                    ' list all of the function api names in current package
                    Dim package As Package = pkgMgr.FindPackage(opt.Value, Nothing)

                    If package Is Nothing Then
                        Return debug.stop({"missing required package for query...", "package: " & opt.Value}, env)
                    Else
                        Return package.ls
                    End If
                Case Else
                    Return debug.stop(New NotSupportedException(name), env)
            End Select
        End Function

        <Extension>
        Private Function listOptionItems(opt As NamedValue(Of String), name$, env As Environment)
            Dim globalEnv As GlobalEnvironment = env.globalEnvironment
            Dim pkgMgr As PackageManager = globalEnv.packages

            If opt.Value = "REnv" Then
                Return Internal.invoke.ls
            ElseIf opt.Value = "Activator" Then
                Dim names As Array = globalEnv.types.Keys.ToArray
                Dim fullName As Array = DirectCast(names, String()) _
                    .Select(Function(key)
                                Return globalEnv.types(key).fullName
                            End Function) _
                    .ToArray

                Return New dataframe With {
                    .columns = New Dictionary(Of String, Array) From {
                        {"name", names},
                        {"fullName", fullName}
                    },
                    .rownames = names
                }
            ElseIf opt.Value.DirectoryExists Then
                ' list dir?
                Return opt.Value _
                    .ListFiles _
                    .Select(Function(path) path.FileName) _
                    .ToArray
            ElseIf pkgMgr.hasLibFile(name.FileName) Then
                ' list all of the package names in current dll module
                Return PackageLoader _
                    .ParsePackages(dll:=name) _
                    .Select(Function(pkg) pkg.namespace) _
                    .ToArray
            Else
                Dim func = env.EnumerateAllFunctions _
                    .Where(Function(fun)
                               Return TypeOf fun.value Is RMethodInfo AndAlso DirectCast(fun.value, RMethodInfo).GetPackageInfo.namespace = name
                           End Function) _
                    .Select(Function(fun) DirectCast(fun.value, RMethodInfo).name) _
                    .ToArray

                If func.IsNullOrEmpty Then
                    Return debug.stop({"invalid query term!", "term: " & name}, env)
                Else
                    Return func
                End If
            End If
        End Function

        ''' <summary>
        ''' list all object symbols inside current environment frame
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("objects")>
        Private Function objects(env As Environment) As String()
            Return env.GetSymbolsNames.ToArray
        End Function

        ''' <summary>
        ''' # Report the Space Allocated for an Object
        ''' 
        ''' Provides an estimate of the memory that is being used to store 
        ''' an R# object.
        ''' 
        ''' Exactly which parts of the memory allocation should be attributed 
        ''' to which object is not clear-cut. This function merely provides 
        ''' a rough indication: it should be reasonably accurate for atomic 
        ''' vectors, but does not detect if elements of a list are shared, for 
        ''' example. (Sharing amongst elements of a character vector is taken 
        ''' into account, but not that between character vectors in a single 
        ''' object.)
        '''
        ''' The calculation Is Of the size Of the Object, And excludes the 
        ''' space needed To store its name In the symbol table.
        '''
        ''' Associated space(e.g., the environment of a function And what the 
        ''' pointer in a EXTPTRSXP points to) Is Not included in the 
        ''' calculation.
        '''
        ''' Object sizes are larger On 64-bit builds than 32-bit ones, but will 
        ''' very likely be the same On different platforms With the same word 
        ''' length And pointer size.
        ''' </summary>
        ''' <param name="x">an R# object.</param>
        ''' <returns>
        ''' An object of class "object_size" with a length-one double value, 
        ''' an estimate of the memory allocation attributable to the object 
        ''' in bytes.
        ''' </returns>
        <ExportAPI("object.size")>
        Public Function objectSize(<RRawVectorArgument> x As Object) As Long
            Return HeapSizeOf.MeasureSize(x)
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
        Public Function doCall(what As Object,
                               Optional calls$ = Nothing,
                               <RListObjectArgument>
                               Optional args As Object = Nothing,
                               Optional envir As Environment = Nothing) As Object

            If what Is Nothing AndAlso calls.StringEmpty Then
                Return Internal.debug.stop("Nothing to call!", envir)
            End If

            ' call a static R# function
            ' do.call("round", args = list(1.0008));
            '
            ' call a instance clr object member function
            ' do.call(clr_obj, "method_name", args = list(xxx));
            If TypeOf what Is String OrElse what.GetType.ImplementInterface(Of RFunction) Then
                ' call static api by name
                Return CallInternal(what, args, envir)
            Else
                Return CallMemberFunction(what, calls, args, envir)
            End If
        End Function

        <Extension>
        Public Function CallClrMemberFunction(what As vbObject, calls$, args As Object, envir As Environment) As Object
            Dim member As Object

            If Not what.existsName(calls) Then
                ' throw exception for invoke missing member from .NET object?
                Return Internal.debug.stop({
                    $"Missing member '{calls}' in target {what}",
                    $"type: {what.type.fullName}",
                    $"member name: {calls}"
                }, envir)
            Else
                member = what.getByName(name:=calls)
            End If

            ' invoke .NET CLR API / property getter
            If member.GetType Is GetType(RMethodInfo) Then
                Dim arguments As InvokeParameter() = args
                Dim api As RMethodInfo = DirectCast(member, RMethodInfo)

                Return api.Invoke(envir, arguments)
            Else
                Return member
            End If
        End Function

        Public Function CallMemberFunction(what As Object, calls$, args As Object, envir As Environment) As Object
            Dim targetType As Type = what.GetType

            ' call api from an object instance 
            If targetType Is GetType(vbObject) Then
                Return DirectCast(what, vbObject).CallClrMemberFunction(calls, args, envir)
            ElseIf targetType Is GetType(list) Then
                Throw New NotImplementedException
            Else
                Return Internal.debug.stop(New NotImplementedException(targetType.FullName), envir)
            End If
        End Function

        Private Function getCallLambda([call] As Object, envir As Environment) As Object
            If TypeOf [call] Is String Then
                Dim ref As NamedValue(Of String) = DirectCast([call], String).GetTagValue("::")
                Dim callName As String = ref.Value
                Dim [namespace] As String = ref.Name
                Dim func As Object = FunctionInvoke.GetFunctionVar(New Literal(callName), envir, [namespace]:=[namespace])

                Return func
            ElseIf [call].GetType.ImplementInterface(Of RFunction) Then
                Return [call]
            Else
                Return Message.InCompatibleType(GetType(RFunction), [call].GetType, envir)
            End If
        End Function

        Public Function CallInternal([call] As Object, args As Object, envir As Environment) As Object
            Dim func As Object = getCallLambda([call], envir)

            If Program.isException(func) Then
                Return func
            End If

            Dim invoke As RFunction = DirectCast(func, RFunction)
            Dim arguments As New List(Of InvokeParameter)

            If TypeOf args Is list Then
                Dim i As i32 = Scan0
                Dim name As String

                For Each item In DirectCast(args, list).slots
                    name = item.Key

                    If name.IsPattern("\d+") Then
                        name = $"${CInt(i)}"
                    End If

                    Call New InvokeParameter(name, item.Value, ++i).DoCall(AddressOf arguments.Add)
                Next
            ElseIf TypeOf args Is InvokeParameter() Then
                arguments.AddRange(DirectCast(args, InvokeParameter()))
            End If

            Dim result As Object = invoke.Invoke(envir, arguments.ToArray)

            Return FunctionInvoke.HandleResult(result, envir)
        End Function

        ''' <summary>
        ''' ### Get and Print Call Stacks
        ''' 
        ''' By default traceback() prints the call stack of the last uncaught 
        ''' error, i.e., the sequence of calls that lead to the error. This 
        ''' is useful when an error occurs with an unidentifiable error message. 
        ''' It can also be used to print the current stack or arbitrary lists 
        ''' of deparsed calls.
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("traceback")>
        <RApiReturn(GetType(ExceptionData))>
        Public Function traceback(Optional err As Object = Nothing, Optional env As Environment = Nothing) As Object
            If err Is Nothing Then
                Return getCurrentTrace(env)
            ElseIf TypeOf err Is Message Then
                Return DirectCast(err, Message).environmentStack _
                    .Select(Function(line) line.ToString) _
                    .ToArray
            ElseIf TypeOf err Is TryError Then
                Return DirectCast(err, TryError).traceback
            Else
                Return getCurrentTrace(env)
            End If
        End Function

        Private Function getCurrentTrace(env As Environment) As ExceptionData
            Dim exception As Message = env.globalEnvironment.lastException

            If exception Is Nothing Then
                ' 如果错误消息不存在
                ' 则返回当前的调用栈信息
                Return New ExceptionData With {
                    .StackTrace = debug.getEnvironmentStack(env),
                    .Message = {"n/a"},
                    .TypeFullName = "n/a",
                    .Source = ""
                }
            Else
                Return New ExceptionData With {
                    .StackTrace = exception.environmentStack,
                    .Message = exception.message,
                    .TypeFullName = GetType(Message).FullName,
                    .Source = exception.source.ToString
                }
            End If
        End Function

        ''' <summary>
        ''' Binding and Environment Locking, Active Bindings
        ''' </summary>
        ''' <param name="sym">a name object or character string.</param>
        ''' <param name="env">an environment.</param>
        ''' <returns></returns>
        <ExportAPI("lockBinding")>
        Public Function lockBinding(sym As String(), Optional env As Environment = Nothing) As Object
            Dim symbolObj As Symbol

            For Each name As String In sym
                symbolObj = env.FindSymbol(name)

                If symbolObj Is Nothing Then
                    Return Message.SymbolNotFound(env, name, TypeCodes.NA)
                Else
                    Call symbolObj.setMutable([readonly]:=True)
                End If
            Next

            Return Nothing
        End Function

        <ExportAPI("unlockBinding")>
        Public Function unlockBinding(sym As String(), Optional env As Environment = Nothing) As Object
            Dim symbolObj As Symbol

            For Each name As String In sym
                symbolObj = env.FindSymbol(name)

                If symbolObj Is Nothing Then
                    Return Message.SymbolNotFound(env, name, TypeCodes.NA)
                Else
                    Call symbolObj.setMutable([readonly]:=False)
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' get a .NET type model from a given VB.NET type full name
        ''' </summary>
        ''' <param name="fullName">.NET type name</param>
        ''' <returns></returns>
        <ExportAPI("export")>
        Public Function [typeof](fullName As String) As RType
            Return RType.GetRSharpType(Type.GetType(fullName))
        End Function

        ''' <summary>
        ''' get exported type by name
        ''' </summary>
        ''' <param name="name">export type name</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("type")>
        Public Function [gettype](name As String, Optional env As Environment = Nothing) As Type
            Return env.globalEnvironment.types.TryGetValue(name)
        End Function

        ''' <summary>
        ''' ## Remove Objects from a Specified Environment
        ''' 
        ''' remove and rm can be used to remove objects. These can be specified 
        ''' successively as character strings, or in the character vector list, 
        ''' or through a combination of both. All objects thus specified will be 
        ''' removed.
        '''
        ''' + If envir Is NULL Then the currently active environment Is searched first.
        ''' + If inherits Is TRUE Then parents Of the supplied directory are 
        '''   searched until a variable With the given name Is encountered. A warning 
        '''   Is printed For Each variable that Is Not found.
        '''
        ''' </summary>
        ''' <param name="x">the objects to be removed, as names (unquoted) or character strings (quoted).</param>
        ''' <param name="list">a character vector naming objects to be removed.</param>
        ''' <param name="inherits">should the enclosing frames of the environment be inspected?</param>
        ''' <param name="envir">the environment to use. See ‘details’.</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' The pos argument can specify the environment from which to remove the 
        ''' objects in any of several ways: as an integer (the position in the search
        ''' list); as the character string name of an element in the search list; 
        ''' or as an environment (including using sys.frame to access the currently
        ''' active function calls). The envir argument is an alternative way to specify
        ''' an environment, but is primarily there for back compatibility.
        '''
        ''' It Is Not allowed to remove variables from the base environment And base 
        ''' namespace, nor from any environment which Is locked (see lockEnvironment).
        '''
        ''' Earlier versions Of R incorrectly claimed that supplying a character 
        ''' vector In ... removed the objects named In the character vector, but it
        ''' removed the character vector. Use the list argument To specify objects 
        ''' via a character vector.
        ''' </remarks>
        <ExportAPI("rm")>
        Public Function rm(<RListObjectArgument> x As list,
                           <RRawVectorArgument>
                           Optional list As Object = Nothing,
                           Optional [inherits] As Boolean = False,
                           Optional envir As Environment = Nothing) As Object

            For Each name As String In CLRVector.asCharacter(list)
                Call envir.Delete(name, seekParent:=[inherits])
            Next

            For Each name As String In x.getNames
                If name <> "list" AndAlso name <> "inherits" AndAlso name <> "envir" Then
                    Call envir.Delete(name, seekParent:=[inherits])
                End If
            Next

            Return Nothing
        End Function
    End Module
End Namespace
