#Region "Microsoft.VisualBasic::5f09bbe54fcd8a2b1e3f25f58e0ac2f9, R#\Interpreter\ExecuteEngine\Linq\Expression\Expressions\RunTimeValueExpression.vb"

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

'     Class RunTimeValueExpression
' 
'         Properties: name
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: Exec, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports REnv = SMRUCC.Rsharp.Runtime
Imports RExpression = SMRUCC.Rsharp.Interpreter.ExecuteEngine.Expression

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' 这个是为了解决R#脚本中的表达式对象与Linq脚本中的表达式对象的不兼容问题创建的
    ''' </summary>
    Public Class RunTimeValueExpression : Inherits Expression

        Friend ReadOnly R As RExpression

        Public Overrides ReadOnly Property name As String
            Get
                Return $"R#: {R}"
            End Get
        End Property

        Sub New(value As RExpression)
            R = value
        End Sub

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Return REnv.single(R.Evaluate(context))
        End Function

        Public Overrides Function ToString() As String
            Return R.ToString
        End Function
    End Class
End Namespace
