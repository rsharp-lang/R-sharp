#Region "Microsoft.VisualBasic::a56f59b32a1566352fd0d860a6568da8, studio\Rsharp_kit\MLkit\MachineLearning\randomForest.vb"

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

    '   Total Lines: 29
    '    Code Lines: 23 (79.31%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (20.69%)
    '     File Size: 941 B


    ' Module randomForest
    ' 
    '     Function: importance, randomForest, varImpPlot
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.MachineLearning.RandomForests
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("randomForest")>
Public Module randomForest

    <ExportAPI("randomForest")>
    <RApiReturn(GetType(Result))>
    Public Function randomForest(x As MLDataFrame, Optional env As Environment = Nothing) As Object
        Dim data As New Data(x)
        Dim tree As New RanFog With {.max_branch = 3, .max_tree = 1000}
        Dim result As Result = tree.Run(data)
        Return result
    End Function

    <ExportAPI("importance")>
    Public Function importance(tree As Result) As Object
        Return tree.Model.VI
    End Function

    Public Function varImpPlot()

    End Function

End Module
