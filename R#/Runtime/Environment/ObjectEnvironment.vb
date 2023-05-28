#Region "Microsoft.VisualBasic::84189afb8f053cf892c0a007a9c5ab8f, F:/GCModeller/src/R-sharp/R#//Runtime/Environment/ObjectEnvironment.vb"

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

    '   Total Lines: 52
    '    Code Lines: 22
    ' Comment Lines: 23
    '   Blank Lines: 7
    '     File Size: 1.59 KB


    '     Class ObjectEnvironment
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: FindSymbol
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime

    ''' <summary>
    ''' helper for object with syntax implements
    ''' </summary>
    Public Class ObjectEnvironment : Inherits Environment

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 优先应用对象的成员
        ''' </remarks>
        Default Public Overrides Property value(name As String) As Symbol
            Get
                Return MyBase.value(name)
            End Get
            Set(value As Symbol)
                MyBase.value(name) = value
            End Set
        End Property

        ''' <summary>
        ''' target object
        ''' </summary>
        ReadOnly target As Object

        Public Sub New(target As Object, parent As Environment, stackFrame As StackFrame)
            MyBase.New(parent, stackFrame, isInherits:=False)

            Me.target = target
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="[inherits]"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 优先应用对象的成员
        ''' </remarks>
        Public Overrides Function FindSymbol(name As String, Optional [inherits] As Boolean = True) As Symbol
            Return MyBase.FindSymbol(name, [inherits])
        End Function
    End Class
End Namespace
