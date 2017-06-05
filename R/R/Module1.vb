Module Module1

    Sub Main()
        Call TokenIcer.Parse("
var x <- ""33333333"" & 33:ToString(""F2"");

if (x:Length <= 10) {
    println(x);
}
").ToArray _
.GetSourceTree _
.SaveTo("x:\test.xml")
    End Sub
End Module
