#Region "Microsoft.VisualBasic::8bc7f3dcc08b40daefa622c6a4e8b881, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\SymbolReference.vb"

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

'     Class SymbolReference
' 
'         Properties: symbol, type
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: Evaluate, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' 符号引用
    ''' </summary>
    Public Class SymbolReference : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        ''' <summary>
        ''' 目标变量名
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property symbol As String

        Sub New(symbol As Token)
            Me.symbol = symbol.text
        End Sub

        Sub New(symbol As String)
            Me.symbol = symbol
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim symbol As [Variant](Of Variable, RInternalFuncInvoke) = envir.FindSymbol(Me.symbol)

            If symbol Is Nothing Then
                symbol = Runtime.Internal.invoke.getFunction(Me.symbol)
            End If
            If symbol Is Nothing Then
                Return Message.SymbolNotFound(envir, Me.symbol, TypeCodes.generic)
            Else
                Return symbol
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"&{symbol}"
        End Function
    End Class
End Namespace
