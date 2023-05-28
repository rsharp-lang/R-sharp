#Region "Microsoft.VisualBasic::907740faefec26f1b8915197b303da2e, F:/GCModeller/src/R-sharp/snowFall//Lambda.vb"

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

    '   Total Lines: 30
    '    Code Lines: 13
    ' Comment Lines: 14
    '   Blank Lines: 3
    '     File Size: 952 B


    ' Module Lambda
    ' 
    '     Function: runLambda, start
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' the ``R#`` cloud lambda feature.(RPC)
''' </summary>
<Package("lambda")>
Module Lambda

    ''' <summary>
    ''' start the lambda cloud services engine.
    ''' </summary>
    ''' <param name="port"></param>
    ''' <returns></returns>
    Public Function start(port As Integer) As Object
        Throw New NotImplementedException
    End Function

    ''' <summary>
    ''' run ``R#`` expression on remote lambda services engine
    ''' </summary>
    ''' <param name="lambda"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    Public Function runLambda(<RLazyExpression> lambda As Expression, Optional env As Environment = Nothing) As Object
        Throw New NotImplementedException
    End Function
End Module
