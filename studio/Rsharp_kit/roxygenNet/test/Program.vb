#Region "Microsoft.VisualBasic::6284253f6c5c4af0df61dabc2ad819be, G:/GCModeller/src/R-sharp/studio/Rsharp_kit/roxygenNet/test//Program.vb"

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
    '     File Size: 368 B


    ' Module Program
    ' 
    '     Sub: Main, type_test
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports roxygenNet

Module Program
    Sub Main(args As String())
        Console.WriteLine("Hello World!")
        type_test()
    End Sub

    Sub type_test()
        Call Console.WriteLine(clr_xml.typeLink(GetType(List(Of Double))))
        Call Console.WriteLine(clr_xml.typeLink(GetType(IEnumerable(Of Double))))
    End Sub
End Module
