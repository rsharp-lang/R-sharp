Imports njl.Language
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components

Module jlTest

    Sub Main()
        Call simpleFunctionTest()
        Call closureTest()
        Call functionTest()
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
