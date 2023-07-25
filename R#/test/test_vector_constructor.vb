#Region "Microsoft.VisualBasic::6db864942ff9c1b2fd2b3c89dc38be86, D:/GCModeller/src/R-sharp/R#/Test//test_vector_constructor.vb"

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

    '   Total Lines: 14
    '    Code Lines: 12
    ' Comment Lines: 0
    '   Blank Lines: 2
    '     File Size: 496 B


    ' Module test_vector_constructor
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Interop

Module test_vector_constructor

    Sub Main()
        Dim env As GlobalEnvironment = GlobalEnvironment.defaultEmpty
        Dim a As Object() = {"65", 144, 1567643245564664456&, 1.231, 5.456, 4.5, 4, 5.64, 56.4, 5.6, 45.6, 4.56, 4, 56}
        Dim numeric As Type = GetType(Double)
        Dim v As New vector(numeric, a, env)
        Pause()
    End Sub
End Module
