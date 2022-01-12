Imports SMRUCC.Python
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components

Module test

    Sub Main()
        Call testAcceptor()
        Call testFor()
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

    Sub inspectSyntax(python As XElement)
        Call inspectSyntax(python.Value)
    End Sub

    Sub inspectSyntax(python As String)
        Dim text As Rscript = Rscript.AutoHandleScript(python)
        Dim py As Program = text.ParsePyScript

        For Each line As Expression In py
            Call Console.WriteLine(line)
        Next
    End Sub
End Module
