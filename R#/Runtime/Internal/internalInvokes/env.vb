Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Invokes

    Module env

        <ExportAPI("get")>
        Public Function [get](x As Object, envir As Environment) As Object
            Dim name As String = Runtime.asVector(Of Object)(x) _
                .DoCall(Function(o)
                            Return Scripting.ToString(Runtime.getFirst(o), null:=Nothing)
                        End Function)

            If name.StringEmpty Then
                Return Internal.stop("NULL value provided for object name!", envir)
            End If

            Dim symbol As Variable = envir.FindSymbol(name)

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

        <ExportAPI("ls")>
        Private Function ls(env As Environment) As Object
            Return env.variables.Keys.ToArray
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
        Public Function doCall(what As Object, calls$, <RListObjectArgument> args As Object, envir As Environment) As Object
            If what Is Nothing OrElse calls.StringEmpty Then
                Return Internal.stop("Nothing to call!", envir)
            End If

            Dim targetType As Type = what.GetType

            If targetType Is GetType(vbObject) Then
                Dim member = DirectCast(what, vbObject).getByName(name:=calls)

                If member.GetType Is GetType(RMethodInfo) Then
                    Return DirectCast(member, RMethodInfo).Invoke(envir, {})
                Else
                    Return member
                End If
            Else
                Return Internal.stop(New NotImplementedException(targetType.FullName), envir)
            End If
        End Function
    End Module
End Namespace