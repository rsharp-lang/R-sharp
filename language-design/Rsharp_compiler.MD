By default, one ``R#`` script equivalent to a small program in function **main**, like this example R# script file:

```R
# program.R

let s as string <- "Hello world!";
let expand as function() {
    s <- `${s} (Message from R#)`;
}

expand();
println(s);
# Hello world! (Message from R#)
```

The R# code that show above is equivalent to the VisualBasic program:

```vbnet
' program.vb

Module Program

    Sub Main()
        Dim s As String = "Hello world!"
        Dim expand = Function()
                         s = $"{s} message from VisualBasic"
                     End Function
        ' Hello world! message from VisualBasic
        Call expand()
        Call println(s)
    End Sub
End Module
```

Then you can using the ``R#`` compiler for compile you R script as a standard .NET program, using commandline:

```bash
"/home/xieguigang/Rsharp/rc.exe" /compile --file "/home/xieguigang/test/program.R" --out "./hello.exe"

./hello.exe
# Hello world! (Message from R#)
```

## How it works?

```
R# script -> Syntax Tree -> VisualBasic CodeDOM -> .NET assembly
```

### Solving name conflicts

### Solving closure value

# The R# executable file format

The R# executable file is based on the netCDF file, with file structure looks like:

+ ``globalAttributes`` for store assembly information/meta data, just like ``AssemblyInfo.vb`` file it does in a VisualBasic project.
+ ``data`` section consist with code lines for execute the R# program

## ``data`` section

Each line in ``data`` section is an expression model in R# interpreter, the line number is the data variable name. And the data variable consist with data structure like:

+ ``name`` code line number
+ ``attributes`` stores the expression type, the next code line number for jumping to
+ ``data`` stores the expression content value in byte array, the byte array in format like:
    
    ```
    i16          i32      i16          i32
    content_type codeline content_type codeline
    ```

The R# runtime load expression content by reading such data and rebuild the expression object based on the expression type given in attribute value and then finally create a code line for execute in R# interpreter