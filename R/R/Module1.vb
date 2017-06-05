Module Module1

    Sub Main()
        Call TokenIcer.Parse("var x <- ""33333333"" & 33:ToString(""F2"");").First.GetSourceTree.SaveTo("x:\test.xml")
    End Sub
End Module
