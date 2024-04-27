#Region "Microsoft.VisualBasic::aabd6fd402bc5ea24325b518cf282978, G:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/Linq/DataSet/RuntimeVectorDataSet.vb"

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

    '   Total Lines: 17
    '    Code Lines: 13
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 467 B


    '     Class RuntimeVectorDataSet
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: PopulatesData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class RuntimeVectorDataSet : Inherits DataSet

        ReadOnly vector As Array

        Sub New(any As Array)
            vector = any
        End Sub

        Friend Overrides Iterator Function PopulatesData() As IEnumerable(Of Object)
            For i As Integer = 0 To vector.Length - 1
                Yield vector.GetValue(i)
            Next
        End Function
    End Class
End Namespace
