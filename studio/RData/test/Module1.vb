#Region "Microsoft.VisualBasic::bd14f76ac6004fda94edfc2976fa964e, studio\RData\test\Module1.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 129
    '    Code Lines: 97 (75.19%)
    ' Comment Lines: 8 (6.20%)
    '    - Xml Docs: 87.50%
    ' 
    '   Blank Lines: 24 (18.60%)
    '     File Size: 4.75 KB


    ' Module Module1
    ' 
    '     Function: describe
    ' 
    '     Sub: loadAllSamples, Main, summarize, verifyVector
    ' 
    ' /********************************************************************************/

#End Region

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
                    Dim obj = Reader.ParseData(stream)
                    Dim value = ConvertToR.ToRObject(obj.object)

                    Call summarize(value, file)
                    Call verifyVector(value, file)
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
            Next
        ElseIf TypeOf value Is dataframe Then
            Dim df As dataframe = value

            Call Console.WriteLine($"  type=dataframe, rows={df.nrows}, cols={df.ncols}")
            Call Console.WriteLine($"    columns: {df.colnames.JoinBy(", ")}")
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

    ' Spot-check parsed vector values against the R 4.5.0 source values.
    Sub verifyVector(value As Object, file As String)
        If Not (TypeOf value Is vector) Then
            Return
        End If

        Dim data As Array = DirectCast(value, vector).data
        Dim n As Integer = data.Length

        Select Case file
            Case "altrep_intseq.rds"
                Dim a = data.OfType(Of Object)().Select(Function(x) CInt(x)).ToArray()
                Dim okHead = (a(0) = 1) AndAlso (a(1) = 2)
                Dim okTail = (a(n - 1) = n)
                Call Console.WriteLine($"    [verify] 1..{n}: head=({a(0)},{a(1)}) tail={a(n - 1)} -> {(If(okHead AndAlso okTail, "OK", "MISMATCH"))}")
            Case "altrep_realseq.rds"
                Dim a = data.OfType(Of Object)().Select(Function(x) CDbl(x)).ToArray()
                Dim okHead = Math.Abs(a(0) - 0.0) < 1E-9
                Dim okTail = Math.Abs(a(n - 1) - 1.0) < 1E-9
                Call Console.WriteLine($"    [verify] seq(0,1,by=0.01): first={a(0)} last={a(n - 1)} -> {(If(okHead AndAlso okTail, "OK", "MISMATCH"))}")
            Case "str_vec.rds"
                Call Console.WriteLine($"    [verify] ('{data.GetValue(0)}','{data.GetValue(1)}',...,'{data.GetValue(n - 1)}')")
            Case "deferred_str.rds"
                Call Console.WriteLine($"    [verify] deferred string vec length={n}, first='{data.GetValue(0)}'")
        End Select
    End Sub
End Module
