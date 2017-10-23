Imports SMRUCC.Rsharp.Runtime.PrimitiveTypes

Module Module2

    Sub BenchmarkTest()
        Dim n = 5000000000
        Dim t As Type

        Call New Action(Sub()
                            For i& = 0 To n
                                t = GetType(Integer)
                            Next
                        End Sub).BENCHMARK

        Call New Action(Sub()
                            For i& = 0 To n
                                t = Core.TypeDefine(Of Integer).BaseType
                            Next
                        End Sub).BENCHMARK

        Pause()
    End Sub
End Module
