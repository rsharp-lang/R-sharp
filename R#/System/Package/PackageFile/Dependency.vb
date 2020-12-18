#Region "Microsoft.VisualBasic::935ffb6e13a7d5a2803152cffd3cdfda, R#\System\Package\PackageFile\Dependency.vb"

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

    '     Class Dependency
    ' 
    '         Properties: library, packages
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetDependency
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace System.Package.File

    Public Class Dependency

        Public Property packages As String()
        Public Property library As String

        Sub New()
        End Sub

        Sub New([imports] As [Imports])
            library = ValueAssign.GetSymbol([imports].library)

            If TypeOf [imports].packages Is Literal Then
                packages = {DirectCast([imports].packages, Literal).value.ToString}
            Else
                packages = DirectCast([imports].packages, VectorLiteral).values _
                    .Select(AddressOf ValueAssign.GetSymbol) _
                    .ToArray
            End If
        End Sub

        Sub New(require As Require)
            packages = require.packages.Select(AddressOf ValueAssign.GetSymbol).ToArray
        End Sub

        Public Shared Iterator Function GetDependency(loading As IEnumerable(Of Expression)) As IEnumerable(Of Dependency)
            For Each line As Expression In loading
                Select Case line.GetType
                    Case GetType([Imports]) : Yield New Dependency(DirectCast(line, [Imports]))
                    Case GetType(Require) : Yield New Dependency(DirectCast(line, Require))
                    Case Else
                        Throw New InvalidProgramException($"'{line.GetType.FullName}' is not a dependency expression!")
                End Select
            Next
        End Function

    End Class
End Namespace
