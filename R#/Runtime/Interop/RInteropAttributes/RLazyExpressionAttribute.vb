#Region "Microsoft.VisualBasic::af42cacaece6d8b7830fea9d0a14906f, R#\Runtime\Interop\RInteropAttributes\RLazyExpressionAttribute.vb"

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

    '   Total Lines: 10
    '    Code Lines: 5 (50.00%)
    ' Comment Lines: 3 (30.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (20.00%)
    '     File Size: 405 B


    '     Class RLazyExpressionAttribute
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime.Interop

    ''' <summary>
    ''' Tells the script host: dot not evaluate this parameter its source expression, it should pass the raw expression object to this parameter
    ''' </summary>
    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class RLazyExpressionAttribute : Inherits RInteropAttribute

    End Class
End Namespace
