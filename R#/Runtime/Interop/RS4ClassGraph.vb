﻿#Region "Microsoft.VisualBasic::08257a6e689dc7615e2c3d85177c34e5, R#\Runtime\Interop\RS4ClassGraph.vb"

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

    '   Total Lines: 86
    '    Code Lines: 59 (68.60%)
    ' Comment Lines: 10 (11.63%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 17 (19.77%)
    '     File Size: 2.73 KB


    '     Class RUnionClass
    ' 
    '         Properties: className, mode, raw, unionTypes
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: getNames
    ' 
    '     Class RS4ClassGraph
    ' 
    '         Properties: className, members, mode, raw
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: getNames, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Interop

    Public Class RUnionClass : Implements IRType

        Public ReadOnly Property className As String Implements IRType.className

        Public ReadOnly Property mode As TypeCodes Implements IRType.mode
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return TypeCodes.list
            End Get
        End Property

        Public ReadOnly Property raw As Type Implements IRType.raw
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return GetType(list)
            End Get
        End Property

        Public ReadOnly Property unionTypes As Type()

        Sub New(name As String, ParamArray unions As Type())
            className = name
            unionTypes = unions
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getNames() As String() Implements IReflector.getNames
            Return unionTypes.Select(Function(t) t.Name).ToArray
        End Function
    End Class

    ''' <summary>
    ''' An clr interface liked type define in R#
    ''' </summary>
    ''' <remarks>
    ''' this is a model of the R# runtime generated type
    ''' </remarks>
    Public Class RS4ClassGraph : Implements IReflector, IRType

        ''' <summary>
        ''' the type name
        ''' </summary>
        ''' <returns></returns>
        Public Property className As String Implements IRType.className
        Public Property members As String()

        Private ReadOnly Property mode As TypeCodes Implements IRType.mode
            Get
                Return TypeCodes.list
            End Get
        End Property

        Public ReadOnly Property raw As Type Implements IRType.raw
            Get
                Return GetType(list)
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(name As String, ParamArray [interface] As String())
            Me.className = name
            Me.members = [interface]
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getNames() As String() Implements IReflector.getNames
            Return members.ToArray
        End Function

        Public Overrides Function ToString() As String
            Return $"{className}: {members.GetJson}"
        End Function

    End Class
End Namespace
