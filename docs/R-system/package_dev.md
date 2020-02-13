# Development of a R# package

## What is a package

A package is a kind of api collection that used for implements a specific function. In R# language, a package is a ``module`` in VisualBasic program. Here is a hello world example of the R# package source code:

```vbnet
<Package("hello")> Module Hello

    <ExportApi("echo.word")>
    Public Function hello_world(words As String()) As String()
        Return (From word In words Select $"Hello {word}!").ToArray
    End Function

End Module
```

The example that show above is a very simple R# package demo. Here are some basic elements for create a R# package:

+ ``<Package("package-name")>`` for create a package entry point, with this custom attribute that tagged on your ``module`` object, then the R# package system can identify that which module should be loaded into the scripting environment.
+ ``<ExportApi("api-name")>`` for create a R# api entry point, with this custom attribute that tagged on your ``function`` object, then the R# package system can identify that which function should be imports from the loaded module object.
+ ``Function Object`` for implements the R# api for do something.
+ ``Module Object`` is a package container for group your api functions.   