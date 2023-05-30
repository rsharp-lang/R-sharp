#Region "Microsoft.VisualBasic::0bc616b35d2a11b1353e488fde9ed8e4, F:/GCModeller/src/R-sharp/R#/Test//convertTest.vb"

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

    '   Total Lines: 86
    '    Code Lines: 71
    ' Comment Lines: 0
    '   Blank Lines: 15
    '     File Size: 2.73 KB


    ' Module convertTest
    ' 
    '     Sub: convertFloat, convertInts, Main
    ' 
    ' /********************************************************************************/

#End Region

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
#Disable Warning
                For i As Integer = 0 To t
                    Call REnv.asVector(Of Integer)(ints)
                    Try
                        Call REnv.asVector(Of Integer)(intList)
                    Catch ex As Exception

                    End Try
                    Call REnv.asVector(Of Integer)(intStr)
                Next
#Enable Warning
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
