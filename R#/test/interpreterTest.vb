Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Module interpreterTest

    Dim R As New RInterpreter

    Sub Main()
        Call namespaceTest()
        Call namesTest()

        Call optionsTest()

        Call ImportsDll()
        Call dataframeTest()
        Call parameterTest()

        Call linqTest()
        Call pipelineTest()
        Call iifTest()
        Call lambdaTest()
        Call testScript()

        Call symbolNotFoundTest()
        Call StackTest()

        Call exceptionHandler()

        Call branchTest()
        Call forLoopTest()

        Call logicalTest()
        Call boolTest()

        Call listTest()

        Call declareFunctionTest()
        Call tupleTest()

        Call declareTest()
        Call stringInterpolateTest()


        Pause()
    End Sub

    Sub namespaceTest()
        Call R.Evaluate("base::load('D:\GCModeller\src\R-sharp\tutorials\io\R#save.rda') +8")

        Pause()
    End Sub

    Sub namesTest()
        Call R.Evaluate("let l <- list(a=1,b='FFFF', c=F,d=[T,T])")
        Call R.Evaluate("print(l)")
        Call R.Evaluate("print(names(l))")
        Call R.Evaluate("names(l) <- ['Q','A','W','S']")
        Call R.Evaluate("print(l)")
        Call R.Evaluate("names(l) <- NULL")
        Call R.Evaluate("print(l)")

        Pause()
    End Sub

    Sub optionsTest()
        Call R.Evaluate("options(a=1,bb=2,cc=3, q=TRUE);")

        Pause()
    End Sub

    Sub ImportsDll()
        Call R.Evaluate("imports 'VBMath' from 'Microsoft.VisualBasic.Framework_v47_dotnet_8da45dcd8060cc9a.dll'")
        Call R.Evaluate(" print( RMS([1,2,3,4,5,6,7,8,9,10], (1:10) * 5))")
        Call R.Evaluate("print(RMS)")

        Pause()
    End Sub

    Sub dataframeTest()
        Call R.Evaluate("let d <- data.frame(a = [1,2,3,4,5], b= F, xx='ABCDEFG')")
        Call R.Evaluate("print(d)")

        Pause()
    End Sub

    Sub parameterTest()
        Call R.Evaluate("let div as function(a,b) {
    a / b
}")
        Call R.Evaluate("print(div(b = 99, a = 1))")
        Call R.Evaluate("print(div(a = 1, b = 99))")
        Call R.Evaluate("print(div(1,99))")
        Call R.Evaluate("print(div(99,1))")

        Pause()
    End Sub

    Sub linqTest()
        Call R.Evaluate("
let x = 8;
let zzz <- from x as double in list(skip = x, A =5,B =1, C=2,D =3,E =4) 
           let y as double = x+6
           where x <= 5 
           let z = x + 5
           select [AA = z,BB = y, x^2];
		   
print(zzz);
")

        Call Pause()
    End Sub

    Sub pipelineTest()
        Call R.Add("add2", Function(x, y, z)
                               Return x + z + y + 2
                           End Function)

        Call R.Evaluate(" print( ((1+2)^4 * 88) :> add2(500,1) + 9^2)")
        Call R.Evaluate("print('# Compares with normal function invoke style:')")
        Call R.Evaluate("print( add2((1+2)^4 * 88 ,500,1) + 9^2)")

        Pause()
    End Sub

    Sub iifTest()
        Call R.Evaluate("x > 0 ? 999 : 777")

        Pause()
    End Sub

    Sub lambdaTest()
        Call R.Evaluate("let add <- [x,y] => x+y;")
        Call R.Evaluate("x -> x +333")
    End Sub

    Sub testScript()
        Call R.Evaluate("1:12")
        Call R.PrintMemory()
        Call R.Evaluate("E:\GCModeller\src\R-sharp\tutorials\declare_variable.R".ReadAllText)
        Call R.PrintMemory()

        Pause()
    End Sub

    Sub invokeTest()
        'Call R.Add("x", {"hello", "world"}, TypeCodes.string)
        'Call R.Add("debug", closure:=Function(o) Internal.print(o))

        'Call R.Invoke("debug", R!x)

        'Pause()
    End Sub

    Sub forLoopTest()
        Call R.Evaluate("

let seq as integer = [1,2,3,4,5,6,7,8,9];

seq <- (seq + 1) - 1;

let vec as string = for(x in seq) {
    print(`${x} => ${ x ^ 2}`);
}

print(`Math result: ${vec}`);

")

        Call R.PrintMemory()

        Pause()
    End Sub

    Sub symbolNotFoundTest()
        R.Evaluate("

# x is not declared
x <- 999;

")
    End Sub

    Sub logicalTest()
        Call R.Evaluate("print('a' & 'bc')")
        Call R.Evaluate("print(FALSE && [FALSE, TRUE, TRUE, FALSE])")
        Call R.Evaluate("print(FALSE || [FALSE, TRUE, TRUE, FALSE])")
        Call R.PrintMemory()

        Pause()
    End Sub

    Sub boolTest()
        Call R.Evaluate("
let b as boolean = [✔, false, false, ✔];

print(b);
print(✔);
print(true);
")
        Call R.PrintMemory()

        Pause()
    End Sub

    Sub StackTest()
        Call R.Evaluate("

let internal as function() {

    let innerPrivate as function() {
        print('This function could not be invoked by code outside the [internal] closure stack!');
    }

    print('declare a new function inside the [internal] closure stack.');
    print(innerPrivate);
}

internal();

innerPrivate();

")

        Pause()
    End Sub

    Sub exceptionHandler()
        Call R.Evaluate("

let tryStop as function(message = 'default exception message') {

    print('start exception stack trace test');

    let internalCalls as function() {
        let anotherInternalCalls as function() {

            for(i in [110,20,50,11,9,6]) {

                if (i <= 10) {
                    stop(message);
                } else {
                       print(`value of i=${  i}...`);
                }
            }

        }

        anotherInternalCalls();
    }
    
    internalCalls();

    print('this message will never print on screen');
} 

# tryStop();
tryStop(['This','is','an','exception', 'test']);
")

        Pause()
    End Sub

    Sub listTest()
        Call R.Evaluate("let l = list([FALSE, TRUE, FALSE],  a = 123, b = 999, c = TRUE, d = list(aaa = FALSE, ccc = ['a','b','c'])  );")
        Call R.Evaluate("print(l);")

        Pause()
    End Sub

    Sub declareFunctionTest()
        Dim script = "
let user.echo as function(text as string = ['world', 'R# programmer'], callerName = NULL) {
    print(`Hello ${text}!`);
}
"
        Call R.Evaluate("let x = [1,2,3];")
        Call R.Evaluate(script)
        Call R.Evaluate("user.echo();")
        Call R.Evaluate("user.echo(`NO. ${x}`);")

        Call R.Evaluate("
let addWith as function(x, y = 1) {
return x + y;
return 999;
}

")

        Call R.Evaluate("let z = addWith(1 , [99,999,9999,99999]);")
        Call R.Evaluate("print(z / 11);")

        Call R.PrintMemory()

        Pause()
    End Sub

    Sub stringInterpolateTest()
        Call R.Evaluate("print( ((1 + 3):30:5 ) * 5 );")

        Call R.Evaluate("let word = ['world', ""R# user"", ""tester""];")
        Call R.Evaluate("let s = `Hello ${word}!` & "" ok"";")

        Call R.Evaluate("print(s);")
        Call R.Evaluate("print([1,2,3,4,5] + 4);")

        Call R.PrintMemory()

        Pause()
    End Sub

    Sub tupleTest()
        Call R.Evaluate("let [x, y] = [[99, 66], 88];")
        Call R.Evaluate("let [a,b,c, d] = [12,3,6, x / 3.3];")
        Call R.Evaluate("let [e,f,g,h,i,j,k] = FALSE;")

        Call R.PrintMemory()

        Pause()
    End Sub

    Sub branchTest()
        Call R.Evaluate("

let x = 99;

print(`value of the x='${x}'`);

x <- if (x > 100) {


[TRUE, FALSE, TRUE];

} else {
print( 'This is false result');
}

print(`value of the x='${x}'`);
")
        Pause()
    End Sub

    Sub declareTest()
        Call R.Evaluate("let a = 1+2*3+5^6; # code comments")
        Call R.Evaluate("let x as double = [999, 888, 777, 666] / 5.3 ;")
        Call R.Evaluate("let y = round($, 0) ;")
        Call R.Evaluate("let flags  as boolean = [true, true, true, false];")
        Call R.Evaluate("let str as  string =[`hello world!`, 'This program is running on R# scripting engine!', ""And, this is a string value.""]; # declares a string vector")
        Call R.Evaluate("let z as double;")
        Call R.Evaluate("z <-   1+  length(x):(1+99):  2.5   ;")

        Call R.PrintMemory()

        Pause()
    End Sub
End Module
