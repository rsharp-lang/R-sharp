#Region "Microsoft.VisualBasic::ce8f1e71ef68375de4304b5e94416d2f, G:/GCModeller/src/R-sharp/Library/graphics//grSVG.vb"

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

    '   Total Lines: 47
    '    Code Lines: 33
    ' Comment Lines: 7
    '   Blank Lines: 7
    '     File Size: 1.83 KB


    ' Module grSVG
    ' 
    '     Function: styleByclass
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("grDevices.SVG")>
Module grSVG

    ''' <summary>
    ''' Add css style by css selector
    ''' </summary>
    ''' <param name="svg"></param>
    ''' <param name="selector$"></param>
    ''' <param name="styles"></param>
    ''' <returns></returns>
    <ExportAPI("styles")>
    Public Function styleByclass(svg As SVGData, selector$, <RListObjectArgument> styles As Object, Optional env As Environment = Nothing) As SVGData
        Dim args = DirectCast(styles, InvokeParameter()).Skip(1).ToArray
        Dim stylesVal As list

        If args.Length = 1 AndAlso Not args(Scan0).haveSymbolName(hasObjectList:=False) AndAlso TypeOf args(Scan0).value Is FunctionInvoke Then
            Dim list As FunctionInvoke = args(Scan0).value

            If TypeOf list.funcName Is Literal AndAlso DirectCast(list.funcName, Literal).Evaluate(env) = "list" Then
                stylesVal = DirectCast(list.Evaluate(env), list)
            Else
                Throw New NotImplementedException
            End If
        Else
            Throw New NotImplementedException
        End If

        Dim cssVal$ = $"{selector} {{
{stylesVal.slots.Select(Function(t) $"{t.Key}: {t.Value};").JoinBy(vbCrLf)}
}}"

        svg.SVG.styles.Add(cssVal)

        Return svg
    End Function
End Module
