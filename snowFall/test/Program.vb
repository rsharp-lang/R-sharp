#Region "Microsoft.VisualBasic::021a8a46ab096044efab303cf60fb77b, snowFall\test\Program.vb"

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

    ' Module Program
    ' 
    '     Function: demoTask, populate
    ' 
    '     Sub: Main
    ' 
    ' Class integerValue
    ' 
    '     Properties: vector
    '     Operators: ^, +
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Parallel
Imports snowFall.Protocol

Module Program

    Sub Main()
        Dim taskApi As New Func(Of integerValue, integerValue, integerValue())(AddressOf demoTask)
        Dim list As integerValue() = New SlaveTask(Host.CreateProcessor, AddressOf Host.SlaveTask, 1802).RunTask(taskApi, CType(5, integerValue), CType(2, integerValue))

        ' 7 
        ' 32
        For Each item In list
            Call Console.WriteLine(item.GetJson)
        Next


        Dim api2 As New Func(Of Dictionary(Of String, integerValue))(AddressOf populate)
        Dim result2 As Dictionary(Of String, integerValue) = New SlaveTask(Host.CreateProcessor, AddressOf Host.SlaveTask).RunTask(api2)

        Call Console.WriteLine(result2.GetJson)

        Pause()
    End Sub

    Public Function populate() As Dictionary(Of String, integerValue)
        Return New Dictionary(Of String, integerValue) From {{"a", New integerValue With {.vector = {1, 2, 3, 4, 5}}}}
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

