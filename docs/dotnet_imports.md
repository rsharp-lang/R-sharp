## Add Assembly Reference

You can dynamics add the ``.NET`` assembly reference by using the ``imports`` operator

```R
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

### Using instant method

Using the .NET object its instant method in R language is invoke by ``:`` operator:

```R
var display as string <- 98.76:ToString("F4");
```

### Using shared method

Using the .NET object its shared method or the method in a vb.net API module is invoke by ``::`` operator:

```R
IO::SaveTo(list, "D:/test.csv");
```

### Using extension method

all of the shared method and if it has at least on parameter can be using in pipeline mode:

```R
{"abc", "123"} | SaveTo("D:/test.txt");
```