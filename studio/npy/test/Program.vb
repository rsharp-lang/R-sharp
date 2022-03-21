#Region "Microsoft.VisualBasic::f5993b7945db82fa1027071023d16398, R-sharp\studio\npy\test\Program.vb"

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

    '   Total Lines: 91
    '    Code Lines: 62
    ' Comment Lines: 1
    '   Blank Lines: 28
    '     File Size: 1.84 KB


    ' Module Programa
    ' 
    '     Sub: forLoop, Main, parseFile, parseFunction, parseHelloWorld
    '          tupleTest
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Python
Imports SMRUCC.Python.Language
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components

Module Programa
    Sub Main(args As String())

        Call tupleTest()
        Call forLoop()
        Call parseFile()

        Call parseFunction()
        Call parseHelloWorld()

    End Sub

    Sub tupleTest()
        Dim tuple = "
a = 1
b = 2

print([a, b])

b, a = a, b

print([a, b])

str((a, b))
"

        Dim text As Rscript = Rscript.AutoHandleScript(tuple)
        Dim py As Program = text.ParsePyScript

        Pause()
    End Sub

    Sub forLoop()
        Dim forL = "
fruits = [""apple"", ""banana"", ""cherry""]

for x in fruits:
  print(x)

"

        Dim text As Rscript = Rscript.AutoHandleScript(forL)
        Dim py As Program = text.ParsePyScript

        Pause()
    End Sub

    Sub parseFile()
        ' Dim text As Rscript = Rscript.FromFile("E:\GCModeller\src\R-sharp\studio\test\hybridTest\base.py")
        Dim text As Rscript = Rscript.FromFile("D:\GCModeller\src\R-sharp\studio\test\hybridTest\ifTest.py")
        Dim py As Program = text.ParsePyScript

        Pause()
    End Sub

    Sub parseFunction()
        Dim func = "
def hello(x = [1,2,3], zzz = TRUE): 
   
   print(x)
   print(""hello world!"")
   x = zzz
   
   return x
    
f = x -> print(x)
"
        Dim scanner As New PyScanner(func)
        Dim tokens = scanner.GetTokens.ToArray

        Dim text As Rscript = Rscript.FromText(func)
        Dim py As Program = text.ParsePyScript

        Pause()
    End Sub

    Sub parseHelloWorld()
        Dim hello = "print(""Hello World!"")"
        Dim scanner As New PyScanner(hello)
        Dim tokens = scanner.GetTokens.ToArray


        Pause()
    End Sub

End Module
