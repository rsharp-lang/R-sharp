#Region "Microsoft.VisualBasic::a9ad87a90a2ff15bda0dcfd12080497b, R#\Interpreter\ExecuteEngine\Linq\Expression\Expressions\SymbolReference.vb"

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

    '   Total Lines: 35
    '    Code Lines: 27
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 1.03 KB


    '     Class SymbolReference
    ' 
    '         Properties: name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Exec, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class SymbolReference : Inherits Expression

        Friend ReadOnly symbolName As String

        Public Overrides ReadOnly Property name As String
            Get
                Return "&x"
            End Get
        End Property

        Sub New(name As String)
            Me.symbolName = name
        End Sub

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Dim symbol As Symbol = context.FindSymbol(symbolName)

            If symbol Is Nothing Then
                Return Message.SymbolNotFound(context, symbolName, TypeCodes.generic)
            Else
                Return symbol.value
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"&{symbolName}"
        End Function
    End Class
End Namespace
