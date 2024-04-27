#Region "Microsoft.VisualBasic::2338fac927620995260c0dd45ba40368, G:/GCModeller/src/R-sharp/studio/Rsharp_IL/test//jlTest.vb"

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

    '   Total Lines: 132
    '    Code Lines: 98
    ' Comment Lines: 1
    '   Blank Lines: 33
    '     File Size: 2.26 KB


    ' Module jlTest
    ' 
    '     Sub: closureTest, demoTest2, fileIoTest, functionTest, ifTest
    '          (+2 Overloads) inspectSyntax, Main, simpleFunctionTest
    ' 
    ' /********************************************************************************/

#End Region

Imports njl.Language
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components

Module jlTest

    Sub Main()
        Call fileIoTest()
        ' Call ifTest()
        Call demoTest2()
        Call simpleFunctionTest()
        Call closureTest()
        Call functionTest()
    End Sub

    Sub fileIoTest()
        Call inspectSyntax(<Julia>

                               setwd(@dir)

open("myfile.txt", "w") do io
    write(io, "Hello world!")
end

str(open(f->read(f, String), "myfile.txt"))


                           </Julia>)
    End Sub


    Sub ifTest()
        Call inspectSyntax("

if y == 0
      return zero(x)
    end
")
    End Sub

    Sub demoTest2()
        Call inspectSyntax("

function hypot(x,y)
    x = abs(x)
    y = abs(y)
    if x > y
      r = y/x
      return x*sqrt(1+r*r)
    end
    if y == 0
      return zero(x)
    end
    r = x/y
    return y*sqrt(1+r*r)
  end


  print(hypot(2,3))

")
    End Sub

    Sub simpleFunctionTest()
        Call inspectSyntax("

f(x, y) = x + y

print(f(2,3))
")
    End Sub

    Sub closureTest()
        Call inspectSyntax("
z = begin
        x = 1
        y = 2
        x + y
    end

print(z)
")
    End Sub

    Sub functionTest()
        Call inspectSyntax("
using Compat,ggplot

import kegg_kit.repository

include('../perfutil.jl')

## slow pi series ##

function pisum()
    sum = 0.0
    for j = 1:500
        sum = 0.0
        for k = 1:10000
            sum += 1.0/(k*k)
        end
    end
    sum
end

print(pisum())

")
    End Sub

    Sub inspectSyntax(julia As XElement)
        Call inspectSyntax(julia.Value)
    End Sub

    Sub inspectSyntax(julia As String)
        Dim text As Rscript = Rscript.AutoHandleScript(julia)
        Dim py As Program = text.ParseJlScript

        For Each line As Expression In py
            Call Console.WriteLine(line)
        Next

        Call Console.WriteLine()
        Call Console.WriteLine()
        Call Console.WriteLine()
        Call Console.WriteLine()
        Call Console.WriteLine()

        Pause()
    End Sub
End Module
