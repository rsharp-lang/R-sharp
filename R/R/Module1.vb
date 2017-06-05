Module Module1

    Sub Main()
        Call TokenIcer.Parse("var x <- ""33333333"";").First.GetSourceTree.SaveTo("x:\test.xml")
    End Sub
End Module
