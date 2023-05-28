#Region "Microsoft.VisualBasic::eb2d6c94ec8c68793fecd323c4a8cef4, F:/GCModeller/src/R-sharp/snowFall/test//Program.vb"

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

    '   Total Lines: 127
    '    Code Lines: 96
    ' Comment Lines: 4
    '   Blank Lines: 27
    '     File Size: 4.42 KB


    ' Module Program
    ' 
    '     Function: createErrorMessage, demoTask, populate
    ' 
    '     Sub: ErrorTest, Main, serializeTest
    ' 
    ' Class integerValue
    ' 
    '     Properties: vector
    '     Operators: ^, +
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Data
Imports System.Threading
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Parallel
Imports Parallel.IpcStream
Imports snowFall.Protocol

Module Program

    ReadOnly longStrings = {
        New String("c"c, 10240),
        RandomASCIIString(409600),
        New String("+"c, 8888888)
    }

    Sub ErrorTest()
        Dim host2 As New SlaveTask(Host.CreateProcessor, AddressOf Host.SlaveTask, 6588, ignoreError:=True)
        Dim taskApi As New Func(Of String, String(), Integer())(AddressOf createErrorMessage)

        Dim result = host2.RunTask(Of Integer())(taskApi, "hello world!", longStrings)

        Pause()
    End Sub

    Public Function createErrorMessage(message As String, longStringCollection As String()) As Integer()
        For Each line As String In longStringCollection
            Call Console.WriteLine(line)
        Next

        Throw New InvalidProgramException($"test error message: '{message}'")
    End Function

    Sub serializeTest()
        Try
            Try
                Try
                    Call createErrorMessage("12345", longStrings)
                Catch ex As Exception
                    Throw New SyntaxErrorException("bbbbb", ex)
                End Try
            Catch ex As Exception
                Throw New NotImplementedException("aaaaaa", ex)
            End Try
        Catch ex As Exception
            Dim err As New IPCError(ex)

            For Each line As String In err.GetAllErrorMessages
                Call Console.WriteLine(line)
            Next

            Pause()
        End Try
    End Sub

    Sub Main()
        ' Call serializeTest()
        ' Call ErrorTest()

        Dim taskApi As New Func(Of integerValue, integerValue, integerValue())(AddressOf demoTask)
        Dim host2 As New SlaveTask(Host.CreateProcessor, AddressOf Host.SlaveTask)
        Dim list As integerValue() = host2.RunTask(Of integerValue())(taskApi, CType(5, integerValue), CType(2, integerValue))

        Call Console.WriteLine("function:")

        ' 7 
        ' 32
        For Each item In list
            Call Console.WriteLine(item.GetJson)
        Next

        host2 = New SlaveTask(Host.CreateProcessor, AddressOf Host.SlaveTask, 1569)

        Dim lis2 As integerValue() = host2.RunTask(Of integerValue())(Function(a As integerValue, b As integerValue)
                                                                          Return demoTask(a, b)
                                                                      End Function, CType(5, integerValue), CType(2, integerValue))
        Call Console.WriteLine("lambda:")

        For Each item In lis2
            Call Console.WriteLine(item.GetJson)
        Next

        Dim api2 As New Func(Of Dictionary(Of String, integerValue))(AddressOf populate)
        Dim result2 As Dictionary(Of String, integerValue) = New SlaveTask(Host.CreateProcessor, AddressOf Host.SlaveTask).RunTask(Of Dictionary(Of String, integerValue))(api2)

        Call Console.WriteLine(result2.GetJson)

        Pause()
    End Sub

    Public Function populate() As Dictionary(Of String, integerValue)
        Return New Dictionary(Of String, integerValue) From {
            {"a", New integerValue With {.vector = {1, 2, 3, 4, 5}}}
        }
    End Function

    Public Function demoTask(a As integerValue, b As integerValue) As integerValue()
        Call Console.WriteLine(a.GetJson)
        Call Console.WriteLine(b.GetJson)
        Call Thread.Sleep(1000)
        Return {a + b, b ^ 5}
    End Function
End Module

Public Class integerValue

    Public Property vector As Integer()

    Public Shared Widening Operator CType(i As Integer) As integerValue
        Return New integerValue() With {.vector = {i}}
    End Operator

    Public Shared Operator +(a As integerValue, b As integerValue) As integerValue
        Return New integerValue With {
            .vector = a.vector _
                .Select(Function(x, i) x + b.vector(i)) _
                .ToArray
        }
    End Operator

    Public Shared Operator ^(x As integerValue, p As Double) As integerValue
        Return New integerValue With {
            .vector = x.vector _
                .Select(Function(xi) CInt(xi ^ p)) _
                .ToArray
        }
    End Operator
End Class
