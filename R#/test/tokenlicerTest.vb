Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Language.TokenIcer

Module tokenlicerTest

    Sub Main()
        Call lambdaTest()

        Call sequnceTest()
        Call operatorTest()
        Call declareFunctionTest()
        Call stringParser()
        Call declareTest()

        Pause()
    End Sub

    Sub lambdaTest()
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
        Dim stringExpression = StringInterpolation.ParseTokens("Hello ${""world"" & '!'}!! Test expression: 1+1= ${1+1} ${1+1 < x} < x;")
        Dim str2 = StringInterpolation.ParseTokens("Another string ${ `expression is ${1+1} ? ` } + escape test \${this is not a ${'expression'}}")

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
