Imports SMRUCC.Rsharp.Interpreter

Module scriptTest

    Const script$ = "D:\GCModeller\src\GCModeller\engine\Rscript\run.R"

    Dim R As RInterpreter = New RInterpreter() _
 _
        .LoadLibrary("D:\GCModeller\GCModeller\bin\R.base.dll")

    Sub Main()
        Call R.Source(script)
    End Sub
End Module
