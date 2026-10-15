Namespace Language

    <HideModuleName>
    Public Module Keywords

        ''' <summary>
        ''' A list of the keywords inside the R# language.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property KeywordSet As New Dictionary(Of String, String) From {
            {"let", "Declare a new mutable symbol/variable in the runtime (optionally with a type constraint and an initial value)."},
            {"declare", "Declare a new constant/locked symbol binding in the runtime (read-only once assigned)."},
            {"function", "Declare an anonymous function or define a named function closure."},
            {"return", "Return a value from a function or closure body."},
            {"as", "Keyword for type constraint / type annotation, e.g. 'let x as integer'."},
            {"integer", "Type constraint keyword for a 32-bit integer value."},
            {"double", "Type constraint keyword for a double-precision floating-point number."},
            {"boolean", "Type constraint keyword for a logical TRUE/FALSE value."},
            {"string", "Type constraint keyword for a character/string value."},
            {"const", "Declare a new constant/locked symbol binding in the runtime (read-only, cannot be reassigned)."},
            {"imports", "Import a package/module into the current runtime."},
            {"require", "Load and require a package/library into the current runtime."},
            {"library", "Load a package/library into the current runtime."},
            {"if", "Conditional branching keyword."},
            {"else", "Alternative branch of an if / conditional expression."},
            {"for", "For-loop iteration keyword (e.g. for i in 1:10 step 2)."},
            {"loop", "Built-in keyword/function for enumerating/looping over collection elements and creating an iteration populator used in pipelines."},
            {"while", "While-loop iteration keyword (repeat a block while a condition is TRUE)."},
            {"repeat", "Repeat-loop keyword (unconditional loop that runs until a break is hit)."},
            {"step", "Step size for generating a sequential value vector in a for-loop."},
            {"break", "Break out of the enclosing loop (for/while/repeat)."},
            {"next", "Skip to the next iteration of the enclosing loop (continue)."},
            {"between", "Operator to test whether a value is within a specific min and max range."},
            {"in", "Operator to access an element from a collection or test membership in a set."},
            {"like", "Operator to test whether a given string matches a specific regular expression pattern."},
            {"from", "R# language LINQ keyword: source of a query."},
            {"where", "R# language LINQ keyword: filter elements."},
            {"order", "R# language LINQ keyword: sort elements."},
            {"by", "R# language LINQ keyword: grouping or ordering key."},
            {"distinct", "R# language LINQ keyword: remove duplicate elements."},
            {"select", "R# language LINQ keyword: project elements into a new shape."},
            {"take", "R# language LINQ keyword: take the first N elements."},
            {"skip", "R# language LINQ keyword: skip the first N elements."},
            {"into", "R# language LINQ keyword: continue a query (e.g. group/let ... into)."},
            {"aggregate", "R# language LINQ keyword: aggregate elements into a single value."},
            {"join", "R# language LINQ keyword: join two data sources."},
            {"on", "R# language LINQ keyword: join key condition."},
            {"ascending", "R# language LINQ keyword: ascending sort order."},
            {"descending", "R# language LINQ keyword: descending sort order."},
            {"suppress", "Suppress the warning/error message of a single syntax token and continue evaluation (similar to VB's On Error Resume Next but per-token)."},
            {"typeof", "Get the underlying .NET CLR type of an object/value."},
            {"modeof", "Get the internal R# runtime data type/mode of an object."},
            {"valueof", "Get the actual .NET CLR type information of a target .NET function."},
            {"using", "Declare a symbol whose resource is automatically disposed (IDisposable) at the end of the block."},
            {"new", "Create a new S4 object or instantiate a new object from a specific exported .NET CLR type."},
            {"try", "Try-catch error handling keyword."},
            {"switch", "Switch / multi-branch control keyword."},
            {"of", "Loop through the elements of a collection, e.g. for x of collection."},
            {"yield", "Yield a value from a generator/iterator function."}
        }
    End Module
End Namespace