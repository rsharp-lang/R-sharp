#Region "Microsoft.VisualBasic::c609db3767b2ce89dc0aa5900c13bea5, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Annotation\ScriptSymbol.vb"

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

    '     Class ScriptSymbol
    ' 
    '         Properties: expressionName, type
    ' 
    '         Function: Evaluate, ToString, TryGetScriptFileName
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Annotation

    ''' <summary>
    ''' ``@script``
    ''' </summary>
    Public Class ScriptSymbol : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.string
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Annotation
            End Get
        End Property

        Public Overrides Function Evaluate(envir As Environment) As Object
            Return TryGetScriptFileName(envir)
        End Function

        Public Shared Function TryGetScriptFileName(env As Environment) As String
            Dim script As Symbol = env.FindSymbol("!script", [inherits]:=True)

            If script Is Nothing Then
                Return Nothing
            Else
                Dim magic As MagicScriptSymbol = DirectCast(script.value, vbObject).TryCast(Of MagicScriptSymbol)
                Dim fullName As String = magic.fullName

                Return fullName
            End If
        End Function

        Public Overrides Function ToString() As String
            Return "@script"
        End Function
    End Class
End Namespace
