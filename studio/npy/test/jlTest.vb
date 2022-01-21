Imports njl.Language
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components

Module jlTest

    Sub Main()
        Call functionTest()
    End Sub

    Sub functionTest()
        Call inspectSyntax("
using Compat,ggplot

import kegg_kit.repository

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
