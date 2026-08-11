Imports System.IO
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.RDataSet
Imports SMRUCC.Rsharp.RDataSet.Convertor
Imports SMRUCC.Rsharp.RDataSet.Struct
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Module Module1

    ReadOnly R As New RInterpreter

    ReadOnly dataDir As String = "g:\GCModeller\src\R-sharp\studio\RData\test\data\"

    Sub Main()
        App.CurrentDirectory = dataDir

        Call loadAllSamples()

        Pause()
    End Sub

    ''' <summary>
    ''' Load every generated rda/rds sample and print a short summary so the
    ''' read values can be compared against what GNU R produced.
    ''' </summary>
    Sub loadAllSamples()
        Dim files() As String = {
            "samples.rda",
            "int_vec.rds", "str_vec.rds", "cplx_vec.rds", "raw_vec.rds",
            "named_vec.rds", "factor_vec.rds", "mat.rds", "df.rds",
            "df_with_factor.rds", "ts_obj.rds", "nested.rds",
            "altrep_intseq.rds", "altrep_realseq.rds", "deferred_str.rds",
            "ref_list.rds"
        }

        For Each file As String In files
            Call Console.WriteLine(New String("-"c, 70))
            Call Console.WriteLine($"FILE: {file}")

            Try
                Using stream = file.Open
                    Dim obj = Reader.ParseData(stream, debug:=False)
                    Dim value = ConvertToR.ToRObject(obj.object)

                    Call summarize(value, file)
                End Using
            Catch ex As Exception
                Call Console.WriteLine($"!!! FAILED to read {file}: {ex.GetType.Name}: {ex.Message}")
                Call Console.WriteLine(ex.StackTrace)
            End Try
        Next
    End Sub

    ''' <summary>
    ''' Print a short, greppable summary of the read value.
    ''' </summary>
    Sub summarize(value As Object, file As String)
        If TypeOf value Is list Then
            Dim l As list = value

            Call Console.WriteLine($"  type=list, length={l.length}")

            For Each name In l.getNames
                Dim v = l(name)

                Call Console.WriteLine($"    ${name} -> {describe(v)}")
            End If
        ElseIf TypeOf value Is dataframe Then
            Dim df As dataframe = value

            Call Console.WriteLine($"  type=dataframe, rows={df.nrows}, cols={df.ncols}")
            Call Console.WriteLine($"    columns: {df.getColumns(True).JoinBy(", ")}")
        Else
            Call Console.WriteLine($"  {describe(value)}")
        End If
    End Sub

    Function describe(v As Object) As String
        If v Is Nothing Then
            Return "NULL"
        End If

        If TypeOf v Is Array Then
            Dim a As Array = v
            Dim n As Integer = a.Length

            If n = 0 Then
                Return $"vector[0]"
            End If

            Dim head As String = $"[{a.GetValue(0)}"
            If n > 1 Then head &= $", {a.GetValue(1)}"
            If n > 2 Then head &= $", {a.GetValue(2)}"
            head &= "...]"

            Return $"vector[{n}] = {head}"
        End If

        Return v.GetType.Name & " = " & v.ToString
    End Function
End Module
