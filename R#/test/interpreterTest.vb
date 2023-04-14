#Region "Microsoft.VisualBasic::0c506c97f1fe131a22bd4cd59fc83d74, D:/GCModeller/src/R-sharp/R#/Test//interpreterTest.vb"

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

    '   Total Lines: 1305
    '    Code Lines: 917
    ' Comment Lines: 21
    '   Blank Lines: 367
    '     File Size: 29.38 KB


    ' Module interpreterTest
    ' 
    '     Sub: acceptDoTest, acceptorTest, annotationTest, anonymous, appendTest
    '          booleanCLIArgumentTest, closureEnvironmentTest, closureTest, cVectorTest, debugTest
    '          doCalltest, expressionTest2, formulaTest, functiondeclaretest2, headTest
    '          ifTest, index2222, inlineFunctiontest, isEmptyTest, lastSymbolTest
    '          Main, markdownTest, missingSymbolInStringInterpolate, moduleTest, multipleIfElse
    '          negativeValTest, numberLiteralsTest, objClasstest, orDefaultTest, pipelineBuilder2
    '          pipelineBulder, printClassTest, regexpTest, sequenceGeneratorTest, simpleSymbolIndexTest
    '          strictTest, suppressTest, symbolIndextest, syntaxErrorTest, syntaxTest
    '          syntaxTest2, unaryNegTest, usingTest, whileTest, withTest
    ' module test1
    ' 
    '     Sub: boolLiteralTest, branchTest, cliTest, commandLineArgumentTest, constantTest
    '          dataframeIndexTest, dataframeTest, declareFunctionTest, declareTest, elementIndexerTest
    '          exceptionHandler, forLoop2, forLoopTest, genericTest, iifTest
    '          ImportsDll, inTest, invokeTest, joinParserTest, lambdaTest
    '          lambdaTest2, lambdaTest3, linqPipelineTest, linqTest, listoperationtest
    '          listTest, logicalTest, nameAccessorTest, namespaceTest, namesTest
    '          optionsTest, packageTest, parameterTest, pipelineParameterBugTest, pipelineTest
    '          sourceFunctionTest, sourceScripttest, StackTest, stringInterpolateTest, symbolNotFoundTest
    '          testScript, tupleTest, tupleValueAssignTest, whichTest
    ' module test2
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Module interpreterTest

    Dim R As New RInterpreter With {.debug = True}

    Sub doCalltest()
        Call R.Evaluate("do.call('ls', args = list(name = 'package:base'))")

        Pause()
    End Sub

    Sub multipleIfElse()
        Call R.Evaluate("

options(strict = FALSE);    

for(header in ['123', '97.5%-tile:', '2.5%-tile:', 'xxx']) {

	print(header);
	
	if (header == '2.5%-tile:') {
		print(4);
	} else if (header == '97.5%-tile:') {
		print(400);
	} else if (header == $""\d+"") {
		print('is a number');
	} else {
		stop('test');
	}
}


")

        Pause()
    End Sub

    Sub syntaxTest()
        Call R.Parse("  names = 1;   names$""Removed Synonyms"" = NULL;")

        Pause()

        Call R.Parse("a = data$""Names and Identifiers"";")

        ' Call R.Parse("if (aaa) {11/9} else {88^77}")

        Call R.Parse("if(aaa) 11/9 else 88 ^77")
        Call R.Parse("return(1)")

        Call R.Parse("require(base)")

        Call R.Parse("	
    const sampleRows = table 
		:> rows 
		:> which(r -> sum("""" != (r :> cells)) >= 4)
		:> skip(1)
		;")

        Pause()
    End Sub

    Sub symbolIndextest()
        Call R.Parse("	{
		if (file.exists(url)) {
			readText(url)
		} else {
			getHtml(url)
		}
	}
	:> Html::parse
	:> graphquery::query(graphquery, raw = raw)
	;")

        Call R.Parse("Html::parse(keyValues$""Other DBs"")")
    End Sub

    Sub ifTest()
        Call R.Parse("
const url = (
    if (Tcode == ""map"") {
        ""https://www.kegg.jp/entry/ko%s"";
    } else {
        gsub(""https://www.genome.jp/entry/pathway+%c%s"", ""%c"", Tcode);
    }
);

")

        Pause()
    End Sub

    Sub syntaxTest2()
        Call Console.WriteLine(R.Parse("readText(http_get(
    url = url, 
    streamTo = function(url, cache_file) {
      writeLines(con = cache_file) {
        # request from remote server
        # if the cache is not hit,
        # and then write it to the cache repository
        content(requests.get(url));
      }
    }, 
    interval = 3, 
    filetype = 'html')
    )
    ;"))

        Pause()
    End Sub

    Sub acceptDoTest()
        Call R.Parse("
bitmap(file = `./network.png`) %do% {
    print('XXXXX');
}
")
    End Sub

    Sub pipelineBulder()
        Dim a = R.Parse("(function(x) {
	print(x);
})(list(a =1, b = 2));")

        Call R.Evaluate("'AAAAAAAA'
|> (function(str) {
	print(str);
});")

        Pause()
    End Sub

    Sub pipelineBuilder2()
        Dim script As String = "filter_pipeline(log() -> TrIQ() -> soft())"
        Dim a = R.Parse(script)

        Pause()
    End Sub

    Sub expressionTest2()
        Dim p = R.Parse("sum += 1.0/(k*k)")
        Dim p2 = R.Parse("x$sum += 1.0/(k*k)")

        Pause()
    End Sub

    Sub simpleSymbolIndexTest()
        Dim exp2 = R.Parse("a@b")
        Dim exp1 = R.Parse("a$b")

        Pause()
    End Sub

    Sub index2222()
        Dim exp = R.Parse("allrows@{name};")
        Dim exp2 = R.Parse("if (flag) {aaa;  } else { bbb;}")

        Pause()
    End Sub

    Sub Main()
        Call convertTest.Main()


        Call pipelineBuilder2()
        Call index2222()
        Call simpleSymbolIndexTest()

        Call multipleIfElse()
        Call expressionTest2()

        Call pipelineBulder()
        Call negativeValTest()

        Call syntaxTest()
        Call acceptDoTest()
        Call syntaxTest2()

        Call ifTest()
        Call symbolIndextest()

        Call joinParserTest()
        Call syntaxTest()

        Call linqTest()
        Call annotationTest()

        Call doCalltest()

        Call acceptorTest()
        Call forLoop2()

        Call cVectorTest()

        Call inlineFunctiontest()
        Call debugTest()

        Call formulaTest()


        Call strictTest()

        Call whileTest()
        Call functiondeclaretest2()

        Call tupleValueAssignTest()
        Call withTest()
        Call headTest()

        Call lambdaTest3()
        Call constantTest()
        Call regexpTest()

        Call negativeValTest()
        Call numberLiteralsTest()

        Call anonymous()
        Call exceptionHandler()
        Call StackTest()

        Call listoperationtest()
        Call syntaxErrorTest()

        Call usingTest()
        Call dataframeIndexTest()

        Call dataframeTest()
        Call lambdaTest2()


        Call packageTest()

        Call listTest()
        Call booleanCLIArgumentTest()

        Call unaryNegTest()

        Call sourceFunctionTest()
        Call markdownTest()

        Call declareTest()

        ' Call R.globalEnvir.packages.InstallLocals("D:\GCModeller\GCModeller\bin\R.base.dll")
        Call orDefaultTest()
        Call linqPipelineTest()
        Call pipelineParameterBugTest()

        Call namespaceTest()
        Call appendTest()
        '  Call iifTest()
        ' Call isEmptyTest()
        Call sourceScripttest()

        Call missingSymbolInStringInterpolate()

        Call commandLineArgumentTest()
        Call objClasstest()

        Call printClassTest()


        Call sequenceGeneratorTest()

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


        Call namesTest()

        Call optionsTest()

        Call ImportsDll()

        Call parameterTest()


        Call pipelineTest()

        Call testScript()

        Call symbolNotFoundTest()

        Call branchTest()
        Call forLoopTest()

        Call logicalTest()
        Call boolLiteralTest()

        Call declareFunctionTest()

        Call stringInterpolateTest()


        Pause()
    End Sub

    Sub annotationTest()
        Call R.Evaluate("


[@unit ""KM""]
[@span 5555555555555555]
let dist = 100;

[@unit ""sec""]
let time = 1;

print(dist / time);



")

        Pause()
    End Sub

    Sub acceptorTest()

        ' acceptor syntax
        Call R.Evaluate("print() {

        [1,(2),(3),4,5,6]
}")
        ' equals to 
        Call R.Evaluate("
let a = NULL;

print({

    a = TRUE;
    
    [1,2,3,4,5,6]
});

print(a);

")

        Pause()
    End Sub

    Sub cVectorTest()
        Call R.Evaluate("print(c(1:5, bb = 10.5, 'next'))")

        Pause()
    End Sub

    Sub inlineFunctiontest()
        ' Call R.Evaluate("let x2 = function(x) { 333 / x  }")
        ' Call R.Evaluate("let x = function(y) y+55555;  ")
        Call R.Evaluate("let score = function(ppm) if(ppm > cutoff) {0;} else { 1-(ppm/(cutoff * 1.1125)) }")

        Pause()
    End Sub

    Sub debugTest()
        Call R.Print("debug(~1+1);")

        Pause()
    End Sub

    Sub formulaTest()
        Call R.Print("y ~ x + b")

        Pause()
    End Sub

    Sub strictTest()
        R.strict = False
        Call R.Evaluate("a =1:3")
        Call R.Print("a")
        Pause()
    End Sub

    Sub whileTest()
        Call R.Evaluate("
let i as integer = 1;

let gg <- while (TRUE) {
sleep(2);
print(i);

if (i < 10) {
    i <- i + 1;
} else {
    break;
}
}

print(gg);

")

        Pause()
    End Sub

    Sub functiondeclaretest2()
        R.debug = False

        Call R.Evaluate("let plot_ionRaws as function(MRM, mzML, tolerance, export) {
	for(ion in getIonsSampleRaw(MRM, mzML. tolerance)) {
		ion$chromatograms 
		:> MRM.chromatogramPeaks.plot(
			fill              = FALSE, 
			gridFill          = 'white', 
			lineStyle         = 'stroke: black; stroke-width: 5px; stroke-dash: solid;',
			size              = [1600, 900],
			relativeTimeScale = NULL,
			parallel          = TRUE
		)
		:> save.graphics(file = `${export}/${ion$id}.png`)
		;
	}
}
")

        Pause()
    End Sub

    Sub withTest()
        Call R.Evaluate("let x <- list() with {
   a <- 999;
   b <- [FALSE, TRUE];
    c <- function() {
   a + 1;
}
}")
        Call R.Print("x$a")
        Call R.Print("x$b")
        Call R.Print("x$c")
        Call R.Print("x$c()")

        Pause()
    End Sub

    Sub headTest()
        Call R.Evaluate("print(head(1:100))")
        Pause()
    End Sub

    Sub regexpTest()

        Call R.Evaluate("$'(\d{3})|[a-z]'(['123', '54668t665a888','888','000']) :> str ;")
        ' stacktrace test
        Call R.Evaluate("$'\d'(stop(123))")

        Call R.Evaluate("print(['123', 'abc'] like $'\d+');")
        Call R.Evaluate("print(['abc', 'ABC'] like $'[a-z]');")
        Call R.Evaluate("print(['123', '5466','888','000'] == $'\d{3}');")

        Pause()
    End Sub

    Sub negativeValTest()

        Dim x# = 8
        Dim y = -x * -2.0E+133

        Call R.Add("x", 88)

        Call R.Print("-x")

        Call R.Add("R", 555)
        Call R.Evaluate("let a <- -R * 2E3;")
        Call R.Evaluate("(-999.54e3)+a")
        Call R.PrintMemory()

        Pause()
    End Sub

    Sub numberLiteralsTest()
        '        Call R.Evaluate("print(1);
        'print(1.2);
        '")

        Call R.Evaluate("-3.9E-99")

        Call R.PrintMemory()

        Call R.Evaluate("3.3e-5")

        Call R.Evaluate("print(3e-2);
print(8.8e-6);
print(4e8);")

        Pause()
    End Sub

    Sub anonymous()

        Call R.Evaluate("
# a <- function(x) {x}
# a();
(function(x) {

print(x);
print(traceback())
;
})('XXXXXXXXX');")

        Pause()

        Call R.Evaluate("let call = FUNC -> FUNC();

# call anonymous function
call(function() {
	print('call a anonymous function');
});
")

        Pause()
    End Sub

    Sub syntaxErrorTest()
        Call R.Evaluate("let a as string = '123'

print(a);")
    End Sub

    Sub usingTest()
        Call R.Evaluate("using a as file('./test.txt') {
	# call System.IDispose
	# operations with variable a
	print((a :> as.object)$Name);
}

# call a$Dispose()
# and then delete a from the current environment
let a as string = 'abc: ';

print(a);")

        Pause()
    End Sub

    Sub booleanCLIArgumentTest()
        Call R.Evaluate("print(?'--flag')")
        Call R.Evaluate("let b as boolean = ?'--flag'")
        Call R.Evaluate("print(b)")

        Pause()
    End Sub

    Sub unaryNegTest()

        Call R.Evaluate("options(f64.format = 'G', digits = 3)")

        Call R.Print("-99 + 99999")
        Call R.Print("+99 + 99999")
        Call R.Print("-99 + 99999 - (-10000)")

        Pause()
    End Sub

    Sub markdownTest()
        Call R.Evaluate("print(sum)")
        Call R.Evaluate("print(names)")

        Pause()
    End Sub

    Sub orDefaultTest()
        Call R.Evaluate("print(?'--save' || '12345')")
        Call R.Evaluate("print(5+ (?'--save2' || 12345))")

        Pause()
    End Sub

    Sub appendTest()
        Call R.Evaluate("print( 1:5 << [88,99,100] )")

        Pause()
    End Sub

    Sub isEmptyTest()
        Call R.Evaluate("print(is.empty(?'--ABCDEFG'))")
        Call R.Evaluate("let tag.name as string= ?'--ABCDEFG'")
        Call R.Evaluate("print(is.empty(tag.name) ? 'replicate=' : tag.name)")

        Pause()
    End Sub

    Sub missingSymbolInStringInterpolate()
        Call R.Evaluate("`test ${xxx} symbol not found `")

        Pause()
    End Sub

    Sub objClasstest()
        Call R.Evaluate("let x <- as.object(globalenv())")
        Call R.Evaluate("print(names(x))")
        Call R.Evaluate("let intptr <- x$GetHashCode()")
        Call R.Evaluate("print(intptr)")
        Call R.Evaluate("print(x$isGlobal)")
        Call R.Evaluate("print(x$ToString())")
        Call R.Evaluate("print(x$Evaluate)")
        Call R.Evaluate("print(x$FindSymbol('x'))")

        Call R.Evaluate("let f64 = as.object(199965584522335547855.44455555555)")
        Call R.Evaluate("print(names(f64))")
        Call R.Evaluate("print(f64$ToString)")
        Call R.Evaluate("print(f64$ToString('G4', NULL))")
        Call R.Evaluate("print(f64$ToString('F2', NULL))")

        Pause()
    End Sub

    Sub printClassTest()
        Dim [class] As Object = New SMRUCC.Rsharp.Runtime.Environment

        Call Console.WriteLine(printer.ValueToString([class], R.globalEnvir))

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
        Call R.Evaluate("print(inner())") ' 3; 3 ^ 2
        Call R.Evaluate("print(inner())") ' 4; 4 ^ 2
        Call R.Evaluate("print(inner())") ' 5; 5 ^ 2

        Call R.PrintMemory()

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

    Sub sourceFunctionTest()
        Call R.Evaluate("source(path = 'abc.R')")
        Call R.Evaluate("source('a.R')")

        Pause()
    End Sub

    Sub sourceScripttest()
        ' Call R.Source("D:\GCModeller\src\R-sharp\tutorials\lambda.R")
        Call R.LoadLibrary("D:\GCModeller\GCModeller\bin\R.base.dll")
        Call R.Evaluate("let [a,b,c,d] = x :> [add1, add2, add3, addWith(9)];")
        ' Call R.Source("D:\GCModeller\src\GCModeller\engine\Rscript\dataset.R")
        Call R.Source("D:\GCModeller\src\R-sharp\tutorials\invokeTuple.R")

        Pause()
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

    Sub commandLineArgumentTest()
        Call R.Evaluate("(5 > 9) ? 'A' : (?'--in') ")
        Call R.Evaluate("print($)")
        Call R.Evaluate("print(?'--in')")

        Pause()
    End Sub

    Sub cliTest()
        Call R.Evaluate("let app as string = 'eggHTS'")
        Call R.Evaluate("let std_out <- @`E:/GCModeller/GCModeller/bin/${app}.exe`")
        Call R.Evaluate("print(std_out)")

        Pause()
    End Sub

    Sub namespaceTest()
        Call R.Evaluate("console::progressbar.pin.top();")
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

    Sub packageTest()
        R = RInterpreter.FromEnvironmentConfiguration("C:\Users\lipidsearch\AppData\Local\GCModeller\R#\R#.configs.xml")
        R.debug = True

        Call R.Evaluate("print(installed.packages())")

        Pause()
    End Sub

    Sub ImportsDll()
        Call R.Evaluate("imports 'VBMath' from 'Microsoft.VisualBasic.Framework_v47_dotnet_8da45dcd8060cc9a.dll'")
        Call R.Evaluate(" print( RMS([1,2,3,4,5,6,7,8,9,10], (1:10) * 5))")
        Call R.Evaluate("print(RMS)")

        Pause()
    End Sub

    Sub dataframeIndexTest()
        R.Add("x", New dataframe With {.columns = New Dictionary(Of String, Array) From {
              {"A", {1, 2, 3, 4, 5}},
              {"BB", {False}}
        }})

        Call R.Evaluate("x[, 'BB']")

        Pause()
    End Sub

    Sub dataframeTest()
        Call R.Evaluate("print(data.frame(a = 1, b = ['g','h','eee'], c = T, [TRUE,FALSE,FALSE]))")

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

    Sub joinParserTest()
        Call R.Parse("
FROM [ID, Name, AddressId] IN employee
JOIN [ID, AddressLine] IN address
ON employee$AddressId == address$ID
WHERE employee$ID > 5
")
    End Sub

    Sub linqTest()
        Call R.Evaluate("
const table = data.frame(
    X = [1,2,3,4,5],
    Y = [5,4,3,2,1],
    Z = runif(5, min = 600, max = 80000)
);

print(table);

let runQuery = {
    FROM [X, Y, Z] 
    IN table
    WHERE Z > 700 && Z <= 79000 
    SELECT X, Y, zz = log(Z) * X, Z 
    ORDER BY zz DESCENDING
    TAKE 10
    SKIP 1
}

print(runQuery);
")

        Call Pause()
    End Sub

    Sub linqPipelineTest()
        Dim seq = Iterator Function() As IEnumerable(Of NamedValue(Of String))
                      For i As Integer = 0 To 10
                          For j As Integer = 0 To 3
                              Yield New NamedValue(Of String)(i.ToHexString, RandomASCIIString(16))
                          Next
                      Next
                  End Function().ToArray

        Call R.Add("seq", seq)
        Call R.Evaluate("let x = seq :> as.object :> groupBy(x -> x$Name)")

        Call Pause()
    End Sub

    Sub pipelineParameterBugTest()
        Call R.Evaluate("
let add1 as function(xx) {
    xx + 1;
}
let div as function(x ,y) {
    x / (y+1);
}

")
        Call R.Evaluate("[xx = [99,23,44,55,66]] :> add1;")
        Call R.Evaluate("print($)")

        Call R.Evaluate("print([x = 5:10] :> div(55));")
        Call R.Evaluate("print(  5:10 :> div(55));")
        Call R.Evaluate("print([y = 5:10] :> div(x = 55));")
        Call R.Evaluate("print(div(55, 5:10))")

        Pause()
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
        Call R.Add("deletions", "S:\synthetic_biology\test_model\wildtype\model\1025_EG.txt", TypeCodes.string)

        Call R.Evaluate("print(file.exists(deletions) ? readLines(deletions) : NULL);")
        Call R.Evaluate("print(1 > 0 ? 999 : 777)")

        Pause()
    End Sub

    Sub lambdaTest3()
        Call R.Evaluate("([]->traceback())()")
        Call R.Print("$")
        Call R.PrintMemory()

        Pause()
    End Sub

    Sub lambdaTest2()
        Call R.Evaluate("let pop5 = function() 5;")
        Call R.Evaluate("let add233 = function(x) x+233;")

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

for(let x in 1:5 step 0.5) {
	print(`x -> ${x}`);
}
")
        Call forLoopTest()
        '  Pause()
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

    Sub boolLiteralTest()
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

   innerPrivate();
}

internal();

innerPrivate();

")



        Pause()
    End Sub

    Sub exceptionHandler()
        R.debug = False

        Call R.Evaluate("   #1
let err as function(message) {#2
#3
    let internal_createHelper as function() {#4
 stop('demo to create stackframe data.' << message); #5
   }#6
  #7
internal_createHelper(); #8
}

let tryStop as function(message = 'default exception message') {

    print('start exception stack trace test');

    let internalCalls as function() {
        let anotherInternalCalls as function() {

            for(i in [110,20,50,11,9,6]) {

                if (i <= 10) {
                    err(message);
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
        Call R.Evaluate("print(traceback())")

        Pause()
    End Sub

    Sub listoperationtest()
        Call R.Source("D:\GCModeller\src\R-sharp\tutorials\data\listValue.R")

        Pause()
    End Sub

    Sub listTest()
        Call R.Evaluate("list([1,2,3,4,5],'999',FALSE)")

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

    Sub tupleValueAssignTest()
        Call R.Evaluate("let a")
        Call R.Evaluate("let b")
        Call R.Add("val", New Func(Of String, Double)(AddressOf Val))
        Call R.Add("int", New Func(Of String, Integer)(AddressOf Integer.Parse))
        Call R.Evaluate("[a, b] = '1234' :> [val, int]")

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

    Sub constantTest()
        Call R.Evaluate("let a = 999")
        Call R.Evaluate("print(a)")
        Call R.Evaluate("a = $'[a-z]+'")
        Call R.Evaluate("print(a)")

        Call R.Evaluate("const b = [999, 888.98]")
        Call R.Evaluate("print(b)")
        Call R.Evaluate("b = $'[a-z]+'")
        Call R.Evaluate("print(b)")

        Pause()
    End Sub

    Sub declareTest()
        Call R.Evaluate("let a = 1+2*3+5^6; # code comments")
        Call R.Evaluate("let x as double = [999, 888, 777, 666] / 5.3 ;")
        Call R.Evaluate("let y = round($, 0) ;")

        Call R.Evaluate("print(x)")
        Call R.Evaluate("print(`length of x is ${length(x)}`)")

        Call R.Evaluate("let flags  as boolean = [true, true, true, false];")
        Call R.Evaluate("let str as  string =[`hello world!`, 'This program is running on R# scripting engine!', ""And, this is a string value.""]; # declares a string vector")
        Call R.Evaluate("let z as double;")
        Call R.Evaluate("z <-   1+  length(x):(1+99):  2.5   ;")

        Call R.PrintMemory()

        Pause()
    End Sub
End Module
