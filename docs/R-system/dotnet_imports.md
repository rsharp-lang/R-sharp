# Using .NET Library 

<!-- vscode-markdown-toc -->
* 1. [Add .NET Assembly Reference](#Add.NETAssemblyReference)
* 2. [Construct .NET object instance](#Construct.NETobjectinstance)
	* 2.1. [Difference between the .NET type and R# ``list``](#Differencebetweenthe.NETtypeandRlist)
		* 2.1.1. [Construct using difference words](#Constructusingdifferencewords)
		* 2.1.2. [Dynamics binding operator](#Dynamicsbindingoperator)
* 3. [Using methods in R#](#UsingmethodsinR)
	* 3.1. [Using instant method](#Usinginstantmethod)
	* 3.2. [Using shared method](#Usingsharedmethod)
	* 3.3. [Using extension method](#Usingextensionmethod)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

The ``R#`` language is original writen in VB.NET language, and it have the ability of using .NET library natively.

##  1. <a name='Add.NETAssemblyReference'></a>Add .NET Assembly Reference

You can dynamics add the ``.NET`` assembly reference by using the ``imports`` operator

```R
# imports a dll file means dynamics load the this .NET assembly 
# into the R# library manager
# But the assembly will not registered into the library
imports "Microsoft.VisualBasic.Data.visualize.Network.Visualizer.dll";
```

You can using a string variable for dynamics load library

```R
# Example that dll file a.dll and b.dll both contains an API function 
# in the same namespace location:
#
# a.dll!namespace::func1
# b.dll!namespace::func1
#
# so that, you can

let [a.dll, b.dll] as string <- ["a.dll", "b.dll"];

`%||%` <- function(ver1, ver2) if (opt["version"] == "a") ver1 else ver2;
	
let library.dll as string <- a.ll %||% b.dll;

# Dynamics load library based on the configuration
# without change your R# script code.
imports library.dll;
```

For imports selected component, you can using selector. If the library.dll contains namespace1 and namespace2, then you can just imports the components from namesapce1 by using:

```R
# imports one namespace
imports "namespace1" from library.dll;

# imports multiple namespace
imports ["namespace1", "namespace2"] from library.dll
```

By default ``R#`` is imports all of the components from the ``library.dll``, So that the statement ``imports library.dll`` is equals to:

```R
# imports all components from dll library
imports "*" from library.dll;
``` 

Probably sometimes, the .NET namespace that too long for write in your ``R#`` script, so that you can using ``imports...as`` for the root namespace alias:

```R
let a.dll as string <- "./a.dll";
let b.dll as string <- "./b.dll";

imports * as nlp from a.ll else b.dll when opt["version"] == "b";

let tokens as string <- nlp::tokenlizer(input.txt);
``` 

and then you can using the .NET API by using ``library`` API imports directly:

```R
library("Microsoft.VisualBasic.Data.visualize.Network.Visualizer");
library("Microsoft.VisualBasic.Imaging");
library("System.Drawing");

# pipeline for R# language read a network graph model and 
# then visualize as png image
Network::Load("./test.network/")
|GraphModel()
|doForceLayout()
|visualize(bg="lightblue")
|SaveTo("./test.png")
;
```

NOTE: 

1. If the .NET object that load by ``library`` function, is a namespace, that means just only imports .NET types in that namespace. So that if you want to using the API in a type, then you can using ``::`` delimiter for the shared method. Object instance method is not avaliable for access directly!
2. If the .NET object that load by ``library`` function, is a .NET type, then all of the shared method in the target will be imports as R function, so that you can directly using the function in R. Object instance method is not avaliable for access directly!
3. For using the instance method in a .NET type, you should access it by an object instance.

##  2. <a name='Construct.NETobjectinstance'></a>Construct .NET object instance

Construct a .NET object instance almost keeps the same as the VB.NET it does:

```R
let x as new type(args) with {
   $property1 = ...;
   $property2 = ...;
}
``` 

```vbnet
Dim x As New type(args) With {
    .property1 = ...,
	.property2 = ...
}
```

But they are have slightly different:

1. In VB.NET you can reference the type by namespace path, like ``namespace1.namespace2.type``, but the R# can not. so that which it means R# should imports the namespace at first by using ``library`` function, and then you are able to using target type.
2. The .NET type in R# by ``library`` imports is not a package, so that it will not be registered into the R# library. If you want to using the .NET type in next time, you should imports the dll again. but the R# package will not as it have registered in the package manager, so that will not required install again.

###  2.1. <a name='Differencebetweenthe.NETtypeandRlist'></a>Difference between the .NET type and R# ``list``

####  2.1.1. <a name='Constructusingdifferencewords'></a>Construct using difference words

```R
# For creates a R# list, using list() function
# In fact, all of the list type in R# language is the Dictionary(Of String, Object) collection type.
# The R# list property is the Key in the dictionary.
let x = list(a =1, b=2, c=3) {
    $property <- "dddddd";
}

# For creates a .NET object, using new keyword
let x as new NamedValue[string]() with {
    $name <- "888888";
    $value <- "abcd";
}
```

####  2.1.2. <a name='Dynamicsbindingoperator'></a>Dynamics binding operator

Only R# list can bind operator dynamics, this operator binding is not allowed on the .NET object, we just imports the .NET object its operators

```R
let x <- list(name = "123") with {
    %&% <- function($, last.name as string) {
        $name <- $name & last.name
    }
}
```

##  3. <a name='UsingmethodsinR'></a>Using methods in R#

###  3.1. <a name='Usinginstantmethod'></a>Using instant method

Using the .NET object its instant method in ``R#`` language is invoke by ``$`` operator:

```R
# All of the .NET object should be convert to vbObject 
# via as.object method at first
# then you can invoke the method in .NET runtime
let display as string <- (98.76 :> as.object)$ToString("F4");
```

###  3.2. <a name='Usingsharedmethod'></a>Using shared method

Using the .NET object its shared method or the method in a vb.net API module is invoke by ``::`` operator:

```R
IO::SaveTo(list, "D:/test.csv");
```

###  3.3. <a name='Usingextensionmethod'></a>Using extension method

All of the imported .NET shared method and R# method, and if it has at least one parameter, then it can be using in pipeline mode:

```R
["abc", "123"] :> SaveTo("D:/test.txt");

# something like in VB.NET
# Call {"abc", "123"}.SaveTo("D:/test.txt")
```
