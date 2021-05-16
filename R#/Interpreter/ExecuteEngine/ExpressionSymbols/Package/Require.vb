#Region "Microsoft.VisualBasic::cb54ff6f1f571eb1a1c23eb3d740b87f, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Package\Require.vb"

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
'         Properties: expressionName, packages, type
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: Evaluate, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Development.Package.File

Namespace Interpreter.ExecuteEngine.ExpressionSymbols

    ''' <summary>
    ''' Loading/Attaching and Listing of Packages
    ''' </summary>
    Public Class Require : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.string
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Require
            End Get
        End Property

        Public Property packages As Expression()
        Public Property options As ValueAssign()

        Sub New(names As IEnumerable(Of Expression))
            packages = names.ToArray
            options = packages _
                .Where(Function(exp) TypeOf exp Is ValueAssign) _
                .Select(Function(exp) DirectCast(exp, ValueAssign)) _
                .ToArray
            packages = packages _
                .Where(Function(exp) Not TypeOf exp Is ValueAssign) _
                .ToArray
        End Sub

        Sub New(names As IEnumerable(Of String))
            packages = names.Select(Function(name) New Literal(name)).ToArray
        End Sub

        Private Function getOptions(env As Environment) As Dictionary(Of String, Object)
            Dim opts As New Dictionary(Of String, Object)

            For Each opt As ValueAssign In options
                Dim name As String = ValueAssign.GetSymbol(opt.targetSymbols(Scan0))
                Dim value As Object = opt.value.Evaluate(env)

                opts(name) = value
            Next

            Return opts
        End Function

        ''' <summary>
        ''' require returns (invisibly) a logical indicating whether the 
        ''' required package is available.
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim names As New List(Of String)
            Dim [global] As GlobalEnvironment = envir.globalEnvironment
            Dim options = getOptions(envir)
            Dim pkgName As String
            Dim message As Message
            Dim quietly As Boolean = options.TryGetValue("quietly")

            For Each name As Expression In packages
                pkgName = ValueAssign.GetSymbol(name)
                message = [global].LoadLibrary(pkgName, silent:=quietly)

                If Not message Is Nothing AndAlso Not quietly Then
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
