Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Language.TokenIcer

Module tokenlicerTest

    Sub Main()
        Call stringParser()
        Call declareTest()

        Pause()
    End Sub

    Sub stringParser()
        Dim stringExpression = New StringInterpolation().GetTokens("Hello ${""world"" & '!'}!! Test expression: 1+1= ${1+1} ${1+1 < x} < x;").ToArray

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
