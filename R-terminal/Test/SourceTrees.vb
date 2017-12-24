#Region "Microsoft.VisualBasic::22d1f04228b30697f755ca03bff7e74f, ..\R-sharp\R-terminal\Test\SourceTrees.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports SMRUCC.Rsharp.Interpreter.Language

Module SourceTrees

    Sub Main()
        Console.WriteLine(TokenIcer.Parse("
# Generic type variable
var x <- 123;").GetSourceTree)

        Console.WriteLine(TokenIcer.Parse("
# Type constraint variable
var x as integer <- {1, 2, 3};").GetSourceTree)

        Console.WriteLine(TokenIcer.Parse("
if(TRUE) {
    # blablabla
    println(""%s is TRUE"", TRUE);
}").GetSourceTree)

        Console.WriteLine(TokenIcer.Parse("
if(a == b) {
    println(""%s is equals to %s"", a, b);
} else if(a <= b) {
    println(""%s is less than or equals to %s"", a, b);
} else {
    println(""Not sure about this."");
}
").GetSourceTree)

        Pause()
    End Sub
End Module
