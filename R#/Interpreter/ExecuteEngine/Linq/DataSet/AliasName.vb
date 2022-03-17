#Region "Microsoft.VisualBasic::d18e68916b5eca108947fc162adcf79c, R-sharp\R#\Interpreter\ExecuteEngine\Linq\DataSet\AliasName.vb"

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

    '   Total Lines: 29
    '    Code Lines: 23
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 877.00 B


    '     Class AliasName
    ' 
    '         Properties: name, NewAlias, OldName
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Exec, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class AliasName : Inherits Expression

        Public Property OldName As String
        Public Property NewAlias As String

        Public Overrides ReadOnly Property name As String
            Get
                Return ToString()
            End Get
        End Property

        Sub New(old As String, [alias] As String)
            Me.OldName = old
            Me.NewAlias = [alias]
        End Sub

        Public Overrides Function ToString() As String
            Return $"{OldName} As {NewAlias}"
        End Function

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Dim value As Object = context.FindSymbol(OldName)?.value
            context.SetSymbol(NewAlias, value)
            Return value
        End Function
    End Class
End Namespace
