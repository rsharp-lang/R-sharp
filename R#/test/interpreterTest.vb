#Region "Microsoft.VisualBasic::32ef0474b8c69cc9ccc136b8d916972b, R#\test\interpreterTest.vb"

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

' Module interpreterTest
' 
'     Sub: boolTest, branchTest, cliTest, dataframeTest, declareFunctionTest
'          declareTest, elementIndexerTest, exceptionHandler, forLoopTest, genericTest
'          iifTest, ImportsDll, inTest, invokeTest, lambdaTest
'          linqTest, listTest, logicalTest, Main, nameAccessorTest
'          namespaceTest, namesTest, optionsTest, parameterTest, pipelineTest
'          StackTest, stringInterpolateTest, symbolNotFoundTest, testScript, tupleTest
'          whichTest
' 
' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal

Module interpreterTest

    Dim R As New RInterpreter

    Sub Main()
        Call printClassTest()

        Call forLoop2()
        Call sequenceGeneratorTest()
        Call iifTest()
        Call closureTest()
        Call tupleTest()

        Call lambdaTest()
        Call suppressTest()

        Call closureEnvironmentTest()

        Call lastSymbolTest()


        ' Call whichTest()

        '    Call elementIndexerTest()
        Call nameAccessorTest()
        Call cliTest()

        Call inTest()

        Call namespaceTest()
        Call namesTest()

        Call optionsTest()

        Call ImportsDll()
        Call dataframeTest()
        Call parameterTest()

        Call linqTest()
        Call pipelineTest()

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

        Call declareTest()
        Call stringInterpolateTest()


        Pause()
    End Sub

    Sub printClassTest()
        Dim [class] As Object = New SMRUCC.Rsharp.Runtime.Environment

        Call Console.WriteLine(printer.ValueToString([class]))

        Pause()
    End Sub

    Sub sequenceGeneratorTest()
        Call R.Evaluate("let x <- 1:100 step 0.5")
        Call R.Evaluate("print(x)")
        Call R.Evaluate("
# The numeric sequence generator demo

print('An integer sequence');
print(1:10);

print('An integer sequence with offset 5');
print(1:100 step 5);

print('A numeric sequence with step 0.5');
print(1:10 step 0.5);

print('A numeric sequence with step 1.0');
print(1.0:10.0);

")

        Pause()
    End Sub

    Sub suppressTest()
        Call R.Evaluate("let ex as function() {
    stop('just create a new exception!');
}")
        Call R.Evaluate("let none = suppress ex();")
        Call R.Evaluate("print(none);")
        Call R.Evaluate("none = ex();")

        Pause()
    End Sub

    Sub closureEnvironmentTest()
        Call R.Evaluate("let x as function(y) {
    let a <- y-1;

    function() {
        a <- a+1;
        a ^ 2
    }
}")
        Call R.Evaluate("let inner = x(3)")
        Call R.Evaluate("print(inner())")
        Call R.Evaluate("print(inner())")
        Call R.Evaluate("print(inner())")

        Pause()
    End Sub

    Sub lastSymbolTest()
        Call R.Evaluate("function(x) {
    x ^ 3
}")
        Call R.Evaluate("print($([3,2,6,5]))")

        Pause()
    End Sub

    Sub closureTest()

        Call R.Evaluate("
let closure as function() {

    let x as integer = 999;

    let setX as function(value) {
        x = value;
    }

    let getX as function() {
        x;
    }

    list(getX = getX, setX = setX);
}
")
        Call R.Evaluate("let holder = closure();")
        Call R.Evaluate("print(holder$getX());")
        Call R.Evaluate("holder$setX([123,233,333])")
        Call R.Evaluate("print(holder$getX());")
        Call R.Evaluate("print(holder$setX('123 AAAAA'));")
        Call R.Evaluate("print(holder$getX());")

        Pause()
    End Sub

    Sub moduleTest()
        Call R.Evaluate("
# The entire script file is a module
module test1;

let println as function(s) {
    print(s);
}
")
        Call R.Evaluate("'This is the string print content!' :> test1::println")

        Call R.Evaluate("# a module block
module test2

let println as function(s) {
    print(`print from test2 module: ${s}`);
}

end module

test2::println('123');
test1::println('123');

")
    End Sub

    Sub genericTest()

    End Sub

    Sub whichTest()
        Call R.Evaluate("let x = [1,3,5,7,9,10,16,32,64]")
        Call R.Evaluate("5+ which x % 5 == 0")
    End Sub

    Sub elementIndexerTest()
        Call R.Evaluate("let a = [1,2,3,4,5]")
        Call R.Evaluate("let B = 3")
        Call R.Evaluate("let c = a[B + 2]")
        Call R.Evaluate("a[8] <- 99999")
        Call R.PrintMemory()

        Pause()
    End Sub

    Sub nameAccessorTest()
        Call R.Evaluate("let l <- list(a = 1, b = FALSE)")
        Call R.Evaluate("print(l$a+2)")
        Call R.Evaluate("print(l$b)")
        Call R.Evaluate("print(l[['b']])")
        Call R.Evaluate("l$b <- [99,88,77,66,55,44]+99")
        Call R.Evaluate("print(l$b)")

        Pause()
    End Sub

    Sub cliTest()
        Call R.Evaluate("let app as string = 'eggHTS'")
        Call R.Evaluate("let std_out <- @`E:/GCModeller/GCModeller/bin/${app}.exe`")
        Call R.Evaluate("print(std_out)")

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

    Sub inTest()
        Call R.Evaluate("print( 55 in [1,55,7])")
        Call R.Evaluate("print(99 in 3)")

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
        Call R.Evaluate("print(1 > 0 ? 999 : 777)")

        Pause()
    End Sub

    Sub lambdaTest()
        Call R.Evaluate("let pop5 = [] -> 5")
        Call R.Evaluate("print(pop5)")
        Call R.Evaluate("print(pop5()^2)")
        Call R.Evaluate("let add <- [x,y] => x+y;")
        Call R.Evaluate("print(x -> x +333)")
        Call R.Evaluate("print(add([5,5]))")
        Call R.Evaluate("print(add)")

        Pause()
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

    Sub forLoop2()
        Call R.Evaluate("

for(x in 1:5 step 0.5) {
	print(`x -> ${x}`);
}
")

        Pause()
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

        Call R.Evaluate("print(x); print(a)")

        Call R.Evaluate("[x , a] <- list(55555, FALSE)")
        Call R.Evaluate("print(x); print(a)")

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

