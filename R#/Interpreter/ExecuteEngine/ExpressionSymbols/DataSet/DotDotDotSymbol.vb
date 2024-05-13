#Region "Microsoft.VisualBasic::5b3237c647bf3381d69812fc80167db3, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\DotDotDotSymbol.vb"

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

    '   Total Lines: 41
    '    Code Lines: 30
    ' Comment Lines: 3
    '   Blank Lines: 8
    '     File Size: 1.25 KB


    '     Class DotDotDotSymbol
    ' 
    '         Properties: expressionName, type
    ' 
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' ... symbol for the R function
    ''' </summary>
    Public Class DotDotDotSymbol : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.list
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.DotDotDot
            End Get
        End Property

        Public Const dddSymbolName As String = "!...{dot-dot-dot}"

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim ddd As Symbol = envir.FindSymbol(dddSymbolName, [inherits]:=True)

            If ddd Is Nothing Then
                Return list.empty
            Else
                Return ddd.value
            End If
        End Function

        Public Overrides Function ToString() As String
            Return "..."
        End Function
    End Class
End Namespace
