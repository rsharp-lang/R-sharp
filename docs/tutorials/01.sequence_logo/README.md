# SequenceLogo Drawer

The R# program did not provides the ability to do a sequence logo drawing, but R# could accomplish such job by using some toolkit api that provides by GCModeller system. In this first demo tutorials, we will learn how to imports the GCModeller library for implemenets such bioinformatics data analysis:

```R
# Demo script for create sequence logo based on the MSA alignment analysis
# nt base frequency is created based on the MSA alignment operation.

imports "bioseq.sequenceLogo" from "seqtoolkit.dll";
imports "bioseq.fasta" from "seqtoolkit.dll";

# script cli usage
#
# R# sequenceLogo.R --seq input.fasta [--title <logo.title> --save output.png] 
#

# get input data from commandline arguments and
# fix for the optional arguments default value
# by apply or default syntax for non-logical values
let seq.fasta as string = ?"--seq";
let logo.png as string  = ?"--save"  || `${seq.fasta}.logo.png`;
let title as string     = ?"--title" || basename(seq.fasta);

# read sequence and then do MSA alignment
# finally count the nucleotide base frequency
# and then draw the sequence logo
# by invoke sequence logo drawer api
seq.fasta
:> read.fasta
:> MSA.of
:> plot.seqLogo(title)
:> save.graphics( file = logo.png );
```

For using the function that comes from the GCModeller library, that we must provides the entry information for load the package module. We must provides the package module name and library module assembly name to the R# interpreter through ``imports`` command. Here is the details information about the ``imports`` command:

```R
# The syntax of the imports command is
# imports [package.names] from "assemblyFile";

# imports a single package module
imports "bioseq.fasta" from "seqtoolkit.dll";

# imports multiple package module
imports ["bioseq.fasta", "bioseq.sequenceLogo"] from "seqtoolkit.dll";

# imports all package module
imports "*" from "seqtoolkit.dll";
# or make it short
imports "seqtoolkit.dll";
```

Once the package module is loaded, then the api function that comes from the GCModeller library will added into the R# runtime environment dynamically. For passing the input data file for the data api, we could write the file path in our R# script directly. But such way is not flexible, it is recommended that we should get the file path from the commandline input. In R# script, we could get commandline argument value via ``?"<argumentName>"``. For an instance example, we have such commandline calls:

```batch
@echo off

REM R# ./sequenceLogo.R --seq <input.fasta> [--save <save.png> --title "drawing title"]
REM Argument wrapped by [] means it is an optional argument
REm it may be missing from the input. 
R# ./sequenceLogo.R --seq LexA.fasta --save LexA.png --title "LexA"
```

Then we could use ``?"--seq"`` for get the argument value in the commandline parameter ``--seq``, and ``?"--save"`` for get value from ``--save`` argument, etc:

```R
# A full example for read commandline argument input value:

let seq.fasta as string = ?"--seq";
let logo.png as string  = ?"--save"  || `${seq.fasta}.logo.png`;
let title as string     = ?"--title" || basename(seq.fasta);
```

As we notice about that there is a strange expression in get argument value of ``--save`` and ``--title``. The syntax of ``value1 || value2`` is called ``or default`` syntax in R# language. How it works and why we needs this syntax? For an instance example, that:

```batch
@echo off

REM The user may omit the title input, so the ?"--title" in our demo R# script
REM may be empty
R# ./sequenceLogo.R --seq LexA.fasta --save LexA.png
```

But empty may caused the program code executation failure, then we could do something for ensure that the commandline argument input value is not empty, like

```R
let title as string = ?"--title";

# We must ensure that the title variable 
# is always have a string text value
if (is.empty(title)) {
    title <- basename(seq.fasta);
}
```

The demo code that show above is works fine, but maybe too much verbose. So we could simplify the code by applying the ``or default`` syntax:

```R
let logo.png as string  = ?"--save"  || `${seq.fasta}.logo.png`;
let title as string     = ?"--title" || basename(seq.fasta);
```

Then we have all of the data input for doing a sequence logo drawing job. In the R code, we call a function should be in way like:

```R
# Code in R or R#

fasta <- read.fasta(seq.fasta);
msa   <- MSA.of(fasta);
plot  <- plot.seqLogo(msa, title);
save.graphics(plot, file = logo.png );
```

Probably you have found that the function invoke in R language maybe too verbose. Inspired by the ``extension method`` in VisualBasic.NET language, we've implements a pipeline calls operator in R# for doing such extension pipeline code. So you could do such pipeline by:

```R
# Code in R#

# All of the function that have at least one parameter 
# can be piped natively in R# language
seq.fasta
:> read.fasta
:> MSA.of
:> plot.seqLogo(title)
:> save.graphics( file = logo.png );
```

The input data is a fasta sequence file in format like:

```R
>SA0684:-124
CACAGAACGTTTGTTCGGTA

>SA1749:-133
AACAGAACATATGTTCGTAT

>SA1975:-66
ACCCGAAAATATGTTCGTGT

>SA1180:-31
AAGCGAACAAATGTTCTATA

>SA0713:-107
TTACGAACAAACGTTTGCTT

# ...
```

The demo code that show above will generates such sequence logo drawing, like:

![](LexA.png)