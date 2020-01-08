require(VisualStudio.devkit);

setwd(!script$dir);
setwd("../../../App");

AssemblyInfo("devkit.dll") 
:> as.object 
:> as.list 
:> print;