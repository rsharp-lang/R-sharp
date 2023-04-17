Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports REnv = SMRUCC.Rsharp.Runtime

Public Module convertTest

    Dim ints As Integer() = Enumerable.Range(0, 10000).ToArray
    Dim intList As list = New list With {.slots = ints.ToDictionary(Function(i) i.ToString, Function(i) CObj(i))}
    Dim intStr As String() = ints.Select(Function(a) a.ToString).ToArray

    Sub Main()
        Dim env As New RInterpreter

        Call env.Print(ints)
        Call env.Inspect(intList)
        Call env.Print(intStr)

        Call convertInts()
        Call convertFloat()

        Pause()
    End Sub

    Dim t As Integer = 100

    Private Sub convertFloat()
        Call Console.WriteLine("via as vector general method:")
        Call VBDebugger.BENCHMARK(
            Sub()
                For i As Integer = 0 To t
                    Call REnv.asVector(Of Double)(ints)
                    Try
                        Call REnv.asVector(Of Double)(intList)
                    Catch ex As Exception

                    End Try
                    Call REnv.asVector(Of Double)(intStr)
                Next
            End Sub)

        Call Console.WriteLine("via asinteger generic")
        Call VBDebugger.BENCHMARK(
            Sub()
                For i As Integer = 0 To t
                    Call CLRVector.asNumeric(ints)
                    Try
                        Call CLRVector.asNumeric(intList)
                    Catch ex As Exception

                    End Try
                    Call CLRVector.asNumeric(intStr)
                Next
            End Sub)
    End Sub

    Private Sub convertInts()
        Call Console.WriteLine("via as vector general method:")
        Call VBDebugger.BENCHMARK(
            Sub()
                For i As Integer = 0 To t
                    Call REnv.asVector(Of Integer)(ints)
                    Try
                        Call REnv.asVector(Of Integer)(intList)
                    Catch ex As Exception

                    End Try
                    Call REnv.asVector(Of Integer)(intStr)
                Next
            End Sub)

        Call Console.WriteLine("via asinteger generic")
        Call VBDebugger.BENCHMARK(
            Sub()
                For i As Integer = 0 To t
                    Call CLRVector.asInteger(ints)
                    Try
                        Call CLRVector.asInteger(intList)
                    Catch ex As Exception

                    End Try
                    Call CLRVector.asInteger(intStr)
                Next
            End Sub)
    End Sub
End Module
