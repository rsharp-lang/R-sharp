Imports SMRUCC.Rsharp.Interpreter

Module interpreterTest

    Dim R As New RInterpreter

    Sub Main()
        Call tupleTest()
        Call declareTest()
        Call branchTest()

        Pause()
    End Sub

    Sub tupleTest()
        Call R.Evaluate("let [a,b,c] = [12,3,6];")
        Call R.PrintMemory()

        Pause()
    End Sub

    Sub branchTest()
        Call R.Evaluate("let x = 99;")
        Call R.Evaluate("x <- if (x > 10) {
TRUE;
} else {
FALSE;
}")

        Pause()
    End Sub

    Sub declareTest()
        Call R.Evaluate("let a = 1+2*3+5^6; # code comments")
        Call R.Evaluate("let x as integer = [999, 888, 777, 666]  ;")
        Call R.Evaluate("let y as integer = $;")
        Call R.Evaluate("let flags  as boolean = [true, true, true, false];")
        Call R.Evaluate("let str as  string =[`hello world!`, 'This program is running on R# scripting engine!', ""And, this is a string value.""]; # declares a string vector")
        Call R.Evaluate("let z as double;")
        Call R.Evaluate("z <-   1+  length(x):(1+99),  2.5   ;")

        Call R.PrintMemory()

        Pause()
    End Sub
End Module
