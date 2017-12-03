#Region "Microsoft.VisualBasic::4503654216a356223a8e8c5b0de1edce, ..\R-sharp\R-terminal\Test\Module2.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

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

