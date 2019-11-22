#Region "Microsoft.VisualBasic::3d67f81efc5e778880dbe746e6719568, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Imports.vb"

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

    '     Class [Imports]
    ' 
    '         Properties: library, packages, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Package

Namespace Interpreter.ExecuteEngine

    Public Class [Imports] : Inherits Expression

        Public ReadOnly Property packages As Expression
        Public ReadOnly Property library As Expression
        Public Overrides ReadOnly Property type As TypeCodes

        Sub New(code As IEnumerable(Of Token()))
            With code.ToArray
                If Not .ElementAt(Scan0).isKeyword("imports") OrElse Not .ElementAt(2).isKeyword("from") Then
                    Throw New SyntaxErrorException
                Else
                    packages = .ElementAt(1).DoCall(AddressOf Expression.CreateExpression)
                    library = .ElementAt(3).DoCall(AddressOf Expression.CreateExpression)
                End If
            End With
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim names As Index(Of String) = Runtime.asVector(Of String)(Me.packages.Evaluate(envir)) _
                .AsObjectEnumerator _
                .Select(Function(o)
                            Return Scripting.ToString(o, Nothing)
                        End Function) _
                .ToArray
            Dim libDll = Scripting.ToString(Runtime.getFirst(library.Evaluate(envir)))

            If libDll.StringEmpty Then
                Return Internal.stop("No package module provided!", envir)
            End If

            Dim packages = PackageLoader.ParsePackages($"{App.HOME}/{libDll}") _
                .Where(Function(pkg) pkg.info.Namespace Like names) _
                .GroupBy(Function(pkg) pkg.namespace) _
                .ToDictionary(Function(pkg) pkg.Key,
                              Function(group)
                                  Return group.First
                              End Function)

            For Each required In names.Objects
                If packages.ContainsKey(required) Then
                    Call ImportsPackage.ImportsStatic(envir.GlobalEnvironment, packages(required).package)
                Else
                    Return Internal.stop({
                        $"There is no package named '{required}' in given module!",
                        $"namespace: {required}",
                        $"library module: {libDll}"}, envir
                    )
                End If
            Next

            Return Nothing
        End Function
    End Class
End Namespace
