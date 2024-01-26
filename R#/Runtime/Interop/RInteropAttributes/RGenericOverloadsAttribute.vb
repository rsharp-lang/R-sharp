#Region "Microsoft.VisualBasic::e0847f77c79d541e2cb82a78a495c411, D:/GCModeller/src/R-sharp/R#//Runtime/Interop/RInteropAttributes/RGenericOverloadsAttribute.vb"

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

    '   Total Lines: 22
    '    Code Lines: 12
    ' Comment Lines: 6
    '   Blank Lines: 4
    '     File Size: 658 B


    '     Class RGenericOverloadsAttribute
    ' 
    '         Properties: FunctionName
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime.Interop

    ''' <summary>
    ''' construct a flag that indicates that target clr function will 
    ''' be overloads in R# environment, the function overloads is 
    ''' determined based on the data type of the first parameter. 
    ''' 
    ''' the function overloads in R# usually be ``plot(...)``, ``as.data.frame(...)``,
    ''' typically.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=True, Inherited:=True)>
    Public Class RGenericOverloadsAttribute : Inherits RInteropAttribute

        Public ReadOnly Property FunctionName As String

        ''' <summary>
        ''' construct a flag that indicates that target clr function will 
        ''' be overloads in R# environment, the function overloads is 
        ''' determined based on the data type of the first parameter. 
        ''' 
        ''' the function overloads in R# usually be ``plot(...)``, ``as.data.frame(...)``,
        ''' typically.
        ''' </summary>
        ''' <param name="func">
        ''' The name of the target function for overloads
        ''' </param>
        Sub New(func As String)
            FunctionName = func
        End Sub

        Public Overrides Function ToString() As String
            Return $"{FunctionName}(...)"
        End Function
    End Class
End Namespace
