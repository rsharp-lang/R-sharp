#Region "Microsoft.VisualBasic::db9c94633486f10dc98fa925ba2170dc, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\SymbolIndexer\SymbolIndexers.vb"

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

    '   Total Lines: 33
    '    Code Lines: 10 (30.30%)
    ' Comment Lines: 22 (66.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (3.03%)
    '     File Size: 839 B


    '     Enum SymbolIndexers
    ' 
    '         dataframeColumns, dataframeRanges, dataframeRows, listIndex, nameIndex
    '         vectorIndex
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' enumerate all kinds of the symbol indexer syntax inside current R-sharp language
    ''' </summary>
    ''' <remarks><see cref="Byte"/></remarks>
    Public Enum SymbolIndexers As Byte
        ''' <summary>
        ''' a[x]
        ''' </summary>
        vectorIndex
        ''' <summary>
        ''' a$x
        ''' </summary>
        nameIndex
        ''' <summary>
        ''' a[[x]]
        ''' </summary>
        listIndex
        ''' <summary>
        ''' a[, x]
        ''' </summary>
        dataframeColumns
        ''' <summary>
        ''' a[x, ]
        ''' </summary>
        dataframeRows
        ''' <summary>
        ''' a[x,y]
        ''' </summary>
        dataframeRanges
    End Enum
End Namespace
