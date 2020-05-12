#Region "Microsoft.VisualBasic::10fe9dbb37ad592a776acd8a293bbe61, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Package\Require.vb"

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

    '     Class Require
    ' 
    '         Properties: packages, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.ExpressionSymbols

    ''' <summary>
    ''' Loading/Attaching and Listing of Packages
    ''' </summary>
    Public Class Require : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Public Property packages As Expression()

        Sub New(names As Expression())
            packages = names
        End Sub

        ''' <summary>
        ''' require returns (invisibly) a logical indicating whether the 
        ''' required package is available.
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim names As New List(Of String)
            Dim [global] As GlobalEnvironment = envir.globalEnvironment
            Dim pkgName As String
            Dim message As Message

            For Each name As Expression In packages
                pkgName = ValueAssign.GetSymbol(name)
                message = [global].LoadLibrary(pkgName)

                If Not message Is Nothing Then
                    Call Internal.debug.PrintMessageInternal(message, envir.globalEnvironment)
                End If
            Next

            Return names.ToArray
        End Function

        Public Overrides Function ToString() As String
            Return $"require({packages.JoinBy(", ")})"
        End Function
    End Class
End Namespace
