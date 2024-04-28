#Region "Microsoft.VisualBasic::d43a9ca3c4ea2f2e004390f74fb1a08d, E:/GCModeller/src/R-sharp/R#//Runtime/Internal/internalInvokes/reflections.vb"

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

    '   Total Lines: 337
    '    Code Lines: 137
    ' Comment Lines: 171
    '   Blank Lines: 29
    '     File Size: 15.67 KB


    '     Module reflections
    ' 
    '         Function: attr, attributes, eval, formals, getClass
    '                   parse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language.Syntax
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Invokes

    <Package("reflections")>
    Module reflections

        ''' <summary>
        ''' ### Object Attributes
        ''' 
        ''' Get or set specific attributes of an object.
        ''' </summary>
        ''' <param name="x">an object whose attributes are to be accessed.</param>
        ''' <param name="which">a non-empty character string specifying which attribute is to be accessed.</param>
        ''' <param name="exact">logical: should which be matched exactly?</param>
        ''' <param name="value">an object, the new value of the attribute, or NULL to remove the attribute.</param>
        ''' <param name="env"></param>
        ''' <returns>For the extractor, the value of the attribute matched, or NULL if no exact 
        ''' match is found and no or more than one partial match is found.</returns>
        ''' <remarks>
        ''' These functions provide access to a single attribute of an object. The replacement 
        ''' form causes the named attribute to take the value specified (or create a new attribute
        ''' with the value given).
        ''' 
        ''' The extraction Function first looks For an exact match To which amongst the attributes 
        ''' Of x, Then (unless exact = True) a unique Partial match. (Setting options(warnPartialMatchAttr = True) 
        ''' causes Partial matches To give warnings.)
        ''' 
        ''' The replacement Function only uses exact matches.
        ''' 
        ''' Note that some attributes (namely Class, comment, Dim, dimnames, names, row.names And tsp) 
        ''' are treated specially And have restrictions On the values which can be Set. (Note that 
        ''' this Is Not True Of levels which should be Set For factors via the levels replacement 
        ''' Function.)
        ''' 
        ''' The extractor Function allows (And does Not match) empty And missing values Of which: the 
        ''' replacement Function does Not.
        ''' 
        ''' NULL objects cannot have attributes And attempting To assign one by attr gives an Error.
        ''' 
        ''' Both are primitive functions.
        ''' </remarks>
        ''' <example>
        ''' # create a 2 by 5 matrix
        ''' x &lt;- 110
        ''' attr(x,"dim") &lt;- c(2, 5)
        ''' </example>
        <ExportAPI("attr")>
        Public Function attr(<RRawVectorArgument> x As Object, which As String,
                             Optional exact As Boolean = False,
                             <RByRefValueAssign> <RRawVectorArgument>
                             Optional value As Object = Nothing,
                             Optional env As Environment = Nothing) As Object

            If Not TypeOf x Is RsharpDataObject Then
                Return Nothing
            End If

            If value Is Nothing Then
                ' get attribute
                Return DirectCast(x, RsharpDataObject).getAttribute(which)
            Else
                ' set attribute value
                DirectCast(x, RsharpDataObject).setAttribute(which, value)
            End If

            Return value
        End Function

        ''' <summary>
        ''' ### Object Attribute Lists
        ''' 
        ''' These functions access an object's attributes. The first form 
        ''' below returns the object's attribute list. The replacement 
        ''' forms uses the list on the right-hand side of the assignment 
        ''' as the object's attributes (if appropriate).
        ''' </summary>
        ''' <param name="x">any R object</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Unlike attr it is not an error to set attributes on a NULL object:
        ''' it will first be coerced to an empty list.
        ''' 
        ''' Note that some attributes (namely Class, comment, Dim, dimnames, 
        ''' names, row.names And tsp) are treated specially And have restrictions
        ''' On the values which can be Set. (Note that this Is Not True Of 
        ''' levels which should be Set For factors via the levels replacement 
        ''' Function.)
        ''' 
        ''' Attributes are Not stored internally As a list And should be thought
        ''' Of As a Set And Not a vector, i.e, the order Of the elements Of 
        ''' attributes() does Not matter. This Is also reflected by identical()'s
        ''' behaviour with the default argument attrib.as.set = TRUE. Attributes 
        ''' must have unique names (and NA is taken as "NA", not a missing value).
        ''' 
        ''' Assigning attributes first removes all attributes, Then sets any Dim 
        ''' attribute And Then the remaining attributes In the order given: this
        ''' ensures that setting a Dim attribute always precedes the dimnames 
        ''' attribute.
        ''' 
        ''' The mostattributes assignment takes special care For the Dim, names And
        ''' dimnames attributes, And assigns them only When known To be valid 
        ''' whereas an attributes assignment would give an Error If any are Not. 
        ''' It Is principally intended For arrays, And should be used With care
        ''' On classed objects. For example, it does Not check that row.names
        ''' are assigned correctly For data frames.
        ''' 
        ''' The names Of a pairlist are Not stored As attributes, but are reported 
        ''' As If they were (And can be Set by the replacement form Of attributes).
        ''' 
        ''' NULL objects cannot have attributes And attempts To assign them will
        ''' promote the Object To an empty list. Both assignment And replacement
        ''' forms Of attributes are primitive functions.
        ''' </remarks>
        <ExportAPI("attributes")>
        Public Function attributes(x As Object, Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return Nothing
            ElseIf Not TypeOf x Is SymbolExpression Then
                If TypeOf x Is RsharpDataObject Then
                    Dim attrs As list = list.empty
                    Dim obj As RsharpDataObject = x

                    For Each name As String In obj.getAttributeNames
                        Call attrs.add(name, obj.getAttribute(name))
                    Next

                    If TypeOf x Is dataframe Then
                        Dim df As dataframe = x

                        attrs.slots("names") = df.colnames
                        attrs.slots("class") = "data.frame"
                        attrs.slots("row.names") = df.getRowNames
                    ElseIf TypeOf x Is list Then
                        Dim li As list = x

                        attrs.slots("names") = li.getNames
                        attrs.slots("length") = li.length
                    End If

                    Return attrs
                End If

                Return Nothing
            Else
                Dim symbol As SymbolExpression = x
                Dim attrs As New list With {.slots = New Dictionary(Of String, Object)}

                For Each name As String In symbol.GetAttributeNames
                    Call attrs.add(name, symbol.GetAttributeValue(name).ToArray)
                Next

                Return attrs
            End If
        End Function

        ''' <summary>
        ''' ### Object Classes
        ''' 
        ''' R possesses a simple generic function mechanism which 
        ''' can be used for an object-oriented style of programming. 
        ''' Method dispatch takes place based on the class of the 
        ''' first argument to the generic function.
        ''' </summary>
        ''' <param name="x">a R object</param>
        ''' <returns></returns>
        <ExportAPI("class")>
        Public Function getClass(x As Object) As Object
            If x Is Nothing Then
                Return Nothing
            Else
                Return RType.TypeOf(x).mode
            End If
        End Function

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
            Dim exprList As pipeline = pipeline.TryCreatePipeline(Of Expression)(expr, env)
            Dim result As Object = Nothing

            If exprList.isError Then
                Return exprList.getError
            End If

            For Each expression As Expression In exprList.populates(Of Expression)(env)
                result = expression.Evaluate(env)
            Next

            Return result
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
        <RApiReturn(GetType(Expression))>
        Public Function parse(text As String, Optional env As Environment = Nothing) As Object
            Dim opts As New SyntaxBuilderOptions(AddressOf Expression.CreateExpression, Function(c, s) New Scanner(c, s)) With {
                .source = Rscript.AutoHandleScript(text)
            }
            Dim result As Expression() = opts.source _
                .GetExpressions(opts) _
                .ToArray

            If result.Length > 1 Then
                Call env.AddMessage({
                   "the given script text input contains multiple expression, this function will just returns the first expression..."
                }.JoinIterates(result.Select(Function(exp, i) $"[{i + 1}] {exp.ToString}")) _
                 .ToArray, MSG_TYPES.WRN)
            End If

            If opts.haveSyntaxErr Then
                Return Internal.debug.stop(opts.error, env)
            Else
                Return result.FirstOrDefault
            End If
        End Function
    End Module
End Namespace
