Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Invokes

    Module reflections

        ''' <summary>
        ''' ### Access to and Manipulation of the Formal Arguments
        ''' 
        ''' Get or set the formal arguments of a function.
        ''' 
        ''' For the first form, fun can also be a character string naming 
        ''' the function to be manipulated, which is searched for from the 
        ''' parent frame. If it is not specified, the function calling 
        ''' formals is used.
        '''
        ''' Only closures have formals, Not primitive functions.
        ''' </summary>
        ''' <param name="fun">a Function, Or see 'Details’.</param>
        ''' <param name="env">environment in which the function should be defined.</param>
        ''' <returns>
        ''' formals returns the formal argument list of the function specified, 
        ''' as a pairlist, or NULL for a non-function or primitive.
        ''' 
        ''' The replacement form sets the formals Of a Function To the list/pairlist 
        ''' On the right hand side, And (potentially) resets the environment Of 
        ''' the Function.
        ''' </returns>
        <ExportAPI("formals")>
        Public Function formals(fun As Object, Optional env As Environment = Nothing) As Object
            If fun Is Nothing Then
                env.AddMessage({"In formals(fun) : argument is not a function", "the given value is nothing!"})
                Return Nothing
            ElseIf TypeOf fun Is String Then
                Dim funVal = env.FindSymbol(fun)?.value

                If funVal Is Nothing Then
                    Return Internal.debug.stop({$"object '{fun}' of mode 'function' was not found!"}, env)
                Else
                    fun = funVal
                End If
            End If

            If Not fun.GetType.ImplementInterface(GetType(RFunction)) Then
                Return Internal.debug.stop({
                    $"the given object is not a callable function object!",
                    $"given: {fun.GetType.FullName}"
                }, env)
            End If

            Dim callable As RFunction = DirectCast(fun, RFunction)
            Dim params As New list With {
                .slots = New Dictionary(Of String, Object)
            }

            For Each arg As NamedValue(Of Expression) In callable.getArguments
                params.slots.Add(arg.Name, arg.Value)
            Next

            Return params
        End Function

        ''' <summary>
        ''' ### Evaluate an (Unevaluated) Expression
        ''' 
        ''' Evaluate an R expression in a specified environment.
        ''' </summary>
        ''' <param name="expr">an object to be evaluated. See 'Details’.</param>
        ''' <param name="env">the environment In which expr Is To be evaluated. 
        ''' May also be NULL, a list, a data frame, a pairlist Or an Integer 
        ''' As specified To sys.Call.</param>
        ''' <returns>
        ''' The result of evaluating the object: for an expression vector this is 
        ''' the result of evaluating the last element.
        ''' </returns>
        ''' <remarks>
        ''' eval evaluates the expr argument in the environment specified by envir 
        ''' and returns the computed value. If envir is not specified, then the 
        ''' default is parent.frame() (the environment where the call to eval was 
        ''' made).
        ''' 
        ''' Objects to be evaluated can be of types call Or expression Or name 
        ''' (when the name Is looked up in the current scope And its binding Is 
        ''' evaluated), a promise Or any of the basic types such as vectors, 
        ''' functions And environments (which are returned unchanged).
        ''' </remarks>
        <ExportAPI("eval")>
        Public Function eval(<RRawVectorArgument> expr As Object, Optional env As Environment = Nothing) As Object

        End Function

        ''' <summary>
        ''' ### Parse Expressions
        ''' 
        ''' parse returns the parsed but unevaluated expressions in a list.
        ''' </summary>
        ''' <param name="text">
        ''' character vector. The text to parse. Elements are treated as if 
        ''' they were lines of a file. Other R objects will be coerced to 
        ''' character if possible.
        ''' </param>
        ''' <returns>An object of type "expression", with up to n elements if 
        ''' specified as a non-negative integer.
        ''' 
        ''' A syntax error (including an incomplete expression) will throw an error.
        ''' 
        ''' Character strings In the result will have a declared encoding 
        ''' If encoding Is "latin1" Or "UTF-8", Or If text Is supplied With 
        ''' every element Of known encoding In a Latin-1 Or UTF-8 locale.
        ''' </returns>
        ''' <remarks>
        ''' If text has length greater than zero (after coercion) it is used 
        ''' in preference to file.
        ''' 
        ''' All versions Of R accept input from a connection With End Of line
        ''' marked by LF (As used On Unix), CRLF (As used On DOS/Windows) Or 
        ''' CR (As used On classic Mac OS). The final line can be incomplete, 
        ''' that Is missing the final EOL marker.
        ''' </remarks>
        <ExportAPI("parse")>
        Public Function parse(text As String) As Object

        End Function
    End Module
End Namespace