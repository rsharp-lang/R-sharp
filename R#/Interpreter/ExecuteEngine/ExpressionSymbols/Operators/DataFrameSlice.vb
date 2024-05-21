#Region "Microsoft.VisualBasic::7fcc9fd24e5bcc2f0e3cb4147ab4a682, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\DataFrameSlice.vb"

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

    '   Total Lines: 51
    '    Code Lines: 35 (68.63%)
    ' Comment Lines: 5 (9.80%)
    '    - Xml Docs: 60.00%
    ' 
    '   Blank Lines: 11 (21.57%)
    '     File Size: 1.76 KB


    '     Class DataFrameSlice
    ' 
    '         Properties: expressionName, type
    ' 
    '         Function: Evaluate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Development.Package.File

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    ''' <summary>
    ''' get value of ``data[selector, ]``
    ''' </summary>
    Public Class DataFrameSlice : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.SymbolIndex
            End Get
        End Property

        Dim dataframe As Expression
        Dim selector As Expression

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim dataframe As Object = Me.dataframe.Evaluate(envir)
            Dim selectorVal As Object = Me.selector.Evaluate(envir)

            If Program.isException(dataframe) Then
                Return dataframe
            ElseIf dataframe Is Nothing Then
                Return Internal.debug.stop(New NullReferenceException, envir)
            End If

            If Program.isException(selectorVal) Then
                Return selectorVal
            ElseIf selectorVal Is Nothing Then
                Return Internal.debug.stop(New NullReferenceException, envir)
            End If

            Dim selector As Array = asVector(Of Object)(selectorVal)
            Dim selectorType As Type = MeasureRealElementType(selector)

            If selectorType Is GetType(String) Then
                ' select by row names
            Else
                ' select by row index
            End If

            Throw New NotImplementedException
        End Function
    End Class
End Namespace
