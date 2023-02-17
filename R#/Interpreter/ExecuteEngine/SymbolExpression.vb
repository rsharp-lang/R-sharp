#Region "Microsoft.VisualBasic::5ad2da8c8ec4404b5b948642cc32a384, D:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/SymbolExpression.vb"

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
    '    Code Lines: 18
    ' Comment Lines: 10
    '   Blank Lines: 5
    '     File Size: 1.15 KB


    '     Class SymbolExpression
    ' 
    '         Sub: AddCustomAttributes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' An expression that will create a new symbol in R# environment
    ''' </summary>
    Public MustInherit Class SymbolExpression : Inherits Expression

        ''' <summary>
        ''' the annotation data from the attribute annotation:
        ''' 
        ''' ```R
        ''' 
        ''' ```
        ''' </summary>
        Protected Friend ReadOnly attributes As New Dictionary(Of String, String())

        Protected Friend Overridable Sub AddCustomAttributes(attrs As IEnumerable(Of NamedValue(Of String())))
            For Each attr As NamedValue(Of String()) In attrs
                If attributes.ContainsKey(attr.Name) Then
                    attributes(attr.Name) = attributes(attr.Name) _
                        .JoinIterates(attr.Value) _
                        .ToArray
                Else
                    Call attributes.Add(attr.Name, attr.Value)
                End If
            Next
        End Sub

    End Class
End Namespace
