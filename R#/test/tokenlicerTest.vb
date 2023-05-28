#Region "Microsoft.VisualBasic::e04a7a38dd9370f7aba3e6e3d3369d11, F:/GCModeller/src/R-sharp/R#/Test//tokenlicerTest.vb"

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

    '   Total Lines: 195
    '    Code Lines: 131
    ' Comment Lines: 4
    '   Blank Lines: 60
    '     File Size: 4.83 KB


    ' Module tokenlicerTest
    ' 
    '     Sub: cliInvoke, customOperatorTest, declareFunctionTest, declareTest, elementIndexer
    '          lambdaTest, linqQueryTest, Main, numberUnittest, operatorTest
    '          pipelineTest, regexpLiteral, sequnceTest, sourceScriptTest, specialNameTest
    '          stringParser, stringValueAssign, syntaxBugTest
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Module tokenlicerTest

    Sub Main()
        Call syntaxBugTest()
        Call lambdaTest()
        Call specialNameTest()

        Call regexpLiteral()

        Call customOperatorTest()


        Call numberUnittest()
        Call sourceScriptTest()


        Call elementIndexer()
        Call cliInvoke()

        Call stringValueAssign()
        Call linqQueryTest()
        Call pipelineTest()

        Call sequnceTest()
        Call operatorTest()
        Call declareFunctionTest()
        Call stringParser()
        Call declareTest()

        Pause()
    End Sub

    Sub syntaxBugTest()
        Program.BuildProgram("x$a")
        Program.BuildProgram("x$'12334'")

        Program.BuildProgram("let scores = sapply(a, x -> x$""Score(bits)"");")

        Pause()
    End Sub

    Sub customOperatorTest()
        ' Dim tokens = Rscript.FromFile("D:\GCModeller\src\R-sharp\tutorials\parallel.R").GetTokens
        Dim program As Program = Program.BuildProgram("D:\GCModeller\src\R-sharp\tutorials\parallel.R")

        Pause()
    End Sub

    Sub regexpLiteral()
        ' Dim cli = New Scanner("@'hello'").GetTokens.ToArray
        Dim tokens = New Scanner("$'\d{,5}';").GetTokens.ToArray

        Pause()
    End Sub

    Sub numberUnittest()
        Dim tokens = Rscript.FromText("5TB+888MB").GetTokens
        Dim tokens2 = Rscript.FromText("33.569 [km/s]")


        Pause()
    End Sub

    Sub sourceScriptTest()
        Dim tokens As Token() = Rscript.FromFile("S:\2019\CD_plants\mzCloud\runExport.R").GetTokens

        Pause()
    End Sub

    Sub specialNameTest()
        Dim names = Rscript.FromText(".GlobalEnv + _ae86").GetTokens
        Dim tokens As Token() = Rscript.FromText("let x as string = (!script)$dir;").GetTokens

        Pause()
    End Sub

    Sub elementIndexer()

        Dim tokens = New Scanner("A[b+1]").GetTokens.ToArray

        Pause()
    End Sub


    Sub cliInvoke()
        Dim tokens = New Scanner("@'E'").GetTokens.ToArray

        Pause()
    End Sub

    Sub stringValueAssign()
        Dim tokens = New Scanner("x='A'").GetTokens.ToArray

        Pause()
    End Sub

    Sub linqQueryTest()
        Dim tokens = New Scanner("a <- from x as double in [1,2,3,4] where x < 2 order by x distinct select x^2;").GetTokens.ToArray

        Pause()
    End Sub

    Sub pipelineTest()
        Dim tokens = New Scanner("a :> b").GetTokens.ToArray


        Pause()
    End Sub

    Sub lambdaTest()
        Dim t = New Scanner("([]->x)()").GetTokens.ToArray

        Dim tokens = New Scanner("lapply(l, x -> x * 100)").GetTokens.ToArray


        Pause()
    End Sub

    Sub sequnceTest()
        Dim tokens = New Scanner("1 && 2").GetTokens.ToArray
        Dim tokens3 = New Scanner("1:22").GetTokens.ToArray
        Dim tokens2 = New Scanner("99*(1+2/3^8) % 5:6").GetTokens.ToArray


        Pause()
    End Sub

    Sub operatorTest()
        Dim script = "g <-true == ! ((a + b %6^2) >= 33 ) && (FASLE || true) % ++i;"
        ' script = "9 >= 33;"
        ' script = "g<-true;"
        Dim tokens = New Scanner(script).GetTokens.ToArray

        Pause()
    End Sub

    Sub declareFunctionTest()
        Dim script = "
let user.echo as function(text as string) {
    print(`Hello ${text}!`);
}
"
        Dim tokens = New Scanner(script).GetTokens.ToArray

        Pause()
    End Sub

    Sub stringParser()
        Dim opts As SyntaxBuilderOptions = Nothing
        Dim stringExpression = StringInterpolation.ParseTokens("Hello ${""world"" & '!'}!! Test expression: 1+1= ${1+1} ${1+1 < x} < x;", opts)
        Dim str2 = StringInterpolation.ParseTokens("Another string ${ `expression is ${1+1} ? ` } + escape test \${this is not a ${'expression'}}", opts)

        Pause()
    End Sub

    Sub declareTest()
        Dim script$ = "
let x as integer;
let y  as  double = 9999 / 8 + 5.0;
let z as integer = [1,2,3,4,5];
let flags = [yes, yes, yes, false, false, false];

const abc = ""abc: 'aaa', \""yes\n\n"";
const values = imports ""./vector.R"";

declare function addWith (x as double, y  as double) {
    return x + y;
}

let abc.size = if (len(abc) > x) {
    999;
} else {
    888;
}

x <- addWith(y, abc.size) :> addWith(z);

"
        Dim tokens = New Scanner(script).GetTokens.ToArray

        Call New XmlList(Of Token) With {
            .items = tokens
        }.GetXml _
         .SaveTo("./declares.Xml")

        Pause()
    End Sub

End Module
