#Region "Microsoft.VisualBasic::24d99a47d38e81373aa513b7a21bba98, F:/GCModeller/src/R-sharp/studio/Rsharp_IL/test//test.vb"

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

    '   Total Lines: 213
    '    Code Lines: 125
    ' Comment Lines: 2
    '   Blank Lines: 86
    '     File Size: 4.56 KB


    ' Module test
    ' 
    '     Sub: blanktest, extensionTest, indentTest, (+2 Overloads) inspectSyntax, Main
    '          namespaceFunc, objectFunc, testAcceptor, testError, testFor
    '          testFunc
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Python
Imports SMRUCC.Python.Language
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language
Imports Rscript = SMRUCC.Rsharp.Runtime.Components.Rscript

Module test

    Sub Main()
        Call namespaceFunc()

        Call objectFunc()
        Call extensionTest()
        ' Call indentTest()
        Call testFunc()

        Call blanktest()

        Call testError()
        Call testAcceptor()
        Call testFor()
    End Sub

    Sub inspectSyntax(python As XElement)
        Call inspectSyntax(python.Value)
    End Sub

    Sub inspectSyntax(python As String)
        Dim text As Rscript = Rscript.AutoHandleScript(python)
        Dim py As Program = text.ParsePyScript

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

    Sub namespaceFunc()
        Call inspectSyntax("objects = `object_${graphics2D::pointVector(x, y).dbscan_objects()}`")
        Call inspectSyntax("as.data.frame(list)")
        Call inspectSyntax("write.csv(x, file = './save.csv', row.names = True, file.name.2 = 'abc')")
        Call inspectSyntax("graphics2D::pointVector(x, y).dbscan_objects()")
    End Sub

    Sub objectFunc()
        ' Call inspectSyntax("2+x(5)")
        Call inspectSyntax("obj.run()")
    End Sub

    Sub extensionTest()
        Call inspectSyntax("test = kinetics('(Vmax * S) / (Km + S)', Vmax = 10, S = 's', Km = 2).kinetics_lambda().eval_lambda(s = 5)")
    End Sub

    Sub indentTest()
        Dim python = ("
      i = 5
 
  
   
    
     
      
       
        
      j = 6
")

        Dim text As Rscript = Rscript.AutoHandleScript(python)
        Dim scanner As New PyScanner(text.script)
        Dim tokens = scanner.GetTokens.ToArray
        Dim lines = tokens.Split(Function(t) t.name = TokenType.newLine).Where(Function(l) l.Length > 0).ToArray

        Call inspectSyntax(python)
    End Sub

    Sub blanktest()

        Call inspectSyntax(<python>

                               def a():

                                    i = 1

                                    for x in 1:100:
                                         i = i + 1


                                         if 8 > None :
                                              raise "!"


                                         raise bbb()







                                    return 5 + i





                               raise "stop()"



                               b() + a()

                           </python>)

        Call inspectSyntax("
                               def b():


                                 return 999

                               def a():

                                    i = 1

                                    for x in 1:100:
                                         i = i + 1


                                    return 5 + i


                               b() + a()

                           ")
    End Sub

    Sub testFunc()
        inspectSyntax(
            <python>

                def run(a, b, c):
                 
                   func1()
                   func2()
             

                   bitmap(file = xxx):
                      plot(func3())


                   t = func()
                   return t


            # invoke
            run(1,1,1)

            </python>)
    End Sub

    Sub testError()
        Call inspectSyntax(<python>

                               raise "unexpected error!"
                           </python>)
    End Sub

    Sub testAcceptor()
        Call inspectSyntax(
            <python>

                bitmap(file = "123.png", size = [500,2000]):
                     
                    plot(x, y)

            </python>)
    End Sub

    Sub testFor()
        Call inspectSyntax("
for x in [""apple"", ""banana"", ""cherry""]:
  print(x)
  print(""next"")

  if x == ""none"":
     print(""yes!"")

  print(123)

print(123)

")

        Pause()

        Call inspectSyntax("
fruits = [""apple"", ""banana"", ""cherry""]

for x in fruits:
  print(x)

")

    End Sub
End Module
