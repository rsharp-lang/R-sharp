Imports Parallel
Imports snowFall.Protocol

Module Program

    Sub Main()
        Dim list As Integer() = New SlaveTask(Host.CreateProcessor, AddressOf Host.SlaveTask).RunTask(New Func(Of Integer, Integer, Integer())(AddressOf demoTask), {5, 2})

        Pause()
    End Sub

    Public Function demoTask(a As Integer, b As Integer) As Integer()
        Return {a + b, b ^ 5}
    End Function
End Module
