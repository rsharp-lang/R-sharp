<!-- vscode-markdown-toc -->
* 1. [Add .NET Assembly Reference](#Add.NETAssemblyReference)
* 2. [Construct .NET object instance](#Construct.NETobjectinstance)
* 3. [Using methods in R#](#UsingmethodsinR)
	* 3.1. [Using instant method](#Usinginstantmethod)
	* 3.2. [Using shared method](#Usingsharedmethod)
	* 3.3. [Using extension method](#Usingextensionmethod)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Add.NETAssemblyReference'></a>Add .NET Assembly Reference

You can dynamics add the ``.NET`` assembly reference by using the ``imports`` operator

```R
# imports a dll file means dynamics load the this .NET assembly into the R# library manager
# But the assembly will not registered into the library
imports "Microsoft.VisualBasic.Data.visualize.Network.Visualizer.dll";
```

and then you can using the .NET API by using ``library`` API imports directly:

```R
library("Microsoft.VisualBasic.Data.visualize.Network.Visualizer");
library("Microsoft.VisualBasic.Imaging");
library("System.Drawing");

# pipene for R# language read a network graph model and then visualize as png image
Network::Load("./test.network/")
    |GraphModel
    |doForceLayout
    |visualize(bg="lightblue")
    |SaveTo("./test.png");
```

NOTE: 

1. If the .NET object that load by ``library`` function, is a namespace, that means just only imports .NET types in that namespace. So that if you want to using the API in a type, then you can using ``::`` delimiter for the shared method. Object instance method is not avaliable for access directly!
2. If the .NET object that load by ``library`` function, is a .NET type, then all of the shared method in the target will be imports as R function, so that you can directly using the function in R. Object instance method is not avaliable for access directly!
3. For using the instance method in a .NET type, you should access it by an object instance.

##  2. <a name='Construct.NETobjectinstance'></a>Construct .NET object instance

Construct a .NET object instance almost keeps the same as the VB.NET it does:

```R
var x <- new type(args) with {
    $property = ...;
}
``` 

```vbnet
Dim x As New type(args) With {
    .property = ...
}
```

But they are have slightly different:

1. In VB.NET you can reference the type by namespace path, like ``namespace1.namespace2.type``, but the R# can not. so that which it means R# should imports the namespace at first by using ``library`` function, and then you are able to using target type.
2. The .NET type in R# by ``library`` imports is not a package, so that it will not be registered into the R# library. If you want to using the .NET type in next time, you should imports the dll again. but the R# package will not as it have registered in the package manager, so that will not required install again.

##  3. <a name='UsingmethodsinR'></a>Using methods in R#

###  3.1. <a name='Usinginstantmethod'></a>Using instant method

Using the .NET object its instant method in R language is invoke by ``:`` operator:

```R
var display as string <- 98.76:ToString("F4");
```

###  3.2. <a name='Usingsharedmethod'></a>Using shared method

Using the .NET object its shared method or the method in a vb.net API module is invoke by ``::`` operator:

```R
IO::SaveTo(list, "D:/test.csv");
```

###  3.3. <a name='Usingextensionmethod'></a>Using extension method

all of the shared method and if it has at least on parameter can be using in pipeline mode:

```R
{"abc", "123"} | SaveTo("D:/test.txt");
```