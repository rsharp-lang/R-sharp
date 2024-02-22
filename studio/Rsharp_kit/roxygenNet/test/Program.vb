Imports System
Imports roxygenNet

Module Program
    Sub Main(args As String())
        Console.WriteLine("Hello World!")
        type_test()
    End Sub

    Sub type_test()
        Call Console.WriteLine(clr_xml.typeLink(GetType(List(Of Double))))
        Call Console.WriteLine(clr_xml.typeLink(GetType(IEnumerable(Of Double))))
    End Sub
End Module
