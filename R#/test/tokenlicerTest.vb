Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Language

Module tokenlicerTest

    Sub Main()
        Call declareTest()

        Pause()
    End Sub

    Sub declareTest()
        Dim script$ = "
let x as integer;
let y  as  double = 9999 / 8 + 5.0;
let z as integer = [1,2,3,4,5];
let flags = [yes, yes, yes, false, false, false];

const abc = ""abc: 'aaa', \""yes\n\n"";

"
        Dim tokens = New Scanner(script).GetTokens.ToArray

        Call New XmlList(Of Token) With {.items = tokens}.GetXml.SaveTo("./declares.Xml")

        Pause()
    End Sub

End Module
