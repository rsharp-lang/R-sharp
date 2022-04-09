#Region "Microsoft.VisualBasic::d171dd01fee38f38c3122958aca67fff, R-sharp\R#\Runtime\Environment\NamespaceEnvironment.vb"

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
    '    Code Lines: 25
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 1.12 KB


    '     Class NamespaceEnvironment
    ' 
    '         Properties: [namespace], libpath, symbols
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: AddSymbols
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Runtime

    Public Class NamespaceEnvironment

        Public ReadOnly Property [namespace] As String
        Public ReadOnly Property symbols As New Dictionary(Of String, RFunction)
        Public ReadOnly Property libpath As String

        Sub New(namespace$, libpath$)
            Me.namespace = [namespace]
            Me.libpath = libpath
        End Sub

        Public Sub AddSymbols(symbols As IEnumerable(Of RFunction))
            For Each symbol As RFunction In symbols
                Call Me.symbols.Add(symbol.name, symbol)

                If TypeOf symbol Is DeclareNewFunction Then
                    DirectCast(symbol, DeclareNewFunction).Namespace = [namespace]
                End If
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return $"{[namespace]}: {libpath}"
        End Function

    End Class
End Namespace
