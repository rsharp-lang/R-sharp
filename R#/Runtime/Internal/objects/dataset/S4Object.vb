#Region "Microsoft.VisualBasic::4d8605c5f3202401242b1702b350f99f, R#\Runtime\Internal\objects\dataset\S4Object.vb"

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

    '   Total Lines: 9
    '    Code Lines: 5 (55.56%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (44.44%)
    '     File Size: 168 B


    '     Class S4Object
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Object

    Public Class S4Object : Inherits RsharpDataObject
        Implements IRType

        ''' <summary>
        ''' The names and classes for the slots in the new class. This argument must be supplied by name, slots=, 
        ''' in the call, for back compatibility with other arguments no longer recommended.
        ''' 
        ''' The argument must be vector With a names attribute, the names being those Of the slots In the New Class. 
        ''' Each element Of the vector specifies an existing Class; the corresponding slot must be from this Class 
        ''' Or a subclass Of it. Usually, this Is a character vector naming the classes. It's also legal for the 
        ''' elements of the vector to be class representation objects, as returned by getClass.
        ''' 
        ''' As a limiting case, the argument may be an unnamed character vector; the elements are taken as slot 
        ''' names And all slots have the unrestricted class "ANY".
        ''' </summary>
        ''' <returns></returns>
        Public Property slots As Dictionary(Of String, String)
        ''' <summary>
        ''' character string name for the class.
        ''' </summary>
        ''' <returns></returns>
        Public Property class_name As String Implements IRType.className
        ''' <summary>
        ''' supplies an object with the default data for the slots in this class. A more flexible approach is to write a method for initialize().
        ''' </summary>
        ''' <returns></returns>
        Public Property prototype As Dictionary(Of String, Object)
        ''' <summary>
        ''' A vector specifying existing classes from which this class should inherit. The new class will 
        ''' have all the slots of the superclasses, with the same requirements on the classes of these slots. 
        ''' This argument must be supplied by name, contains=, in the call, for back compatibility with 
        ''' other arguments no longer recommended.
        ''' 
        ''' See the section 'Virtual Classes’ for the special superclass "VIRTUAL".
        ''' </summary>
        ''' <returns></returns>
        Public Property contains As String

        Public ReadOnly Property mode As TypeCodes Implements IRType.mode
            Get
                Return TypeCodes.generic
            End Get
        End Property

        Public ReadOnly Property raw As Type Implements IRType.raw

        Public Function eval() As S4Object

        End Function

        Public Overrides Function ToString() As String
            If contains.StringEmpty Then
                Return class_name
            Else
                Return $"{class_name} inherits {contains}"
            End If
        End Function

        Public Function getNames() As String() Implements IReflector.getNames
            Return slots.Keys.ToArray
        End Function
    End Class
End Namespace
