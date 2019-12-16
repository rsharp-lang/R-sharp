Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components

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
    End Module
End Namespace