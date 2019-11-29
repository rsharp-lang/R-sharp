Imports SMRUCC.Rsharp.Interpreter

Module scriptTest

    Const script$ = "D:\GCModeller\src\GCModeller\engine\Rscript\foldchange.R"

    Dim R As RInterpreter = New RInterpreter() With {
        .debug = True
    } _
        .LoadLibrary("D:\GCModeller\GCModeller\bin\R.base.dll")

    Sub Main()
        Call R.Source(script)
        Call Pause()
    End Sub
End Module
