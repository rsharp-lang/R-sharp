#Region "Microsoft.VisualBasic::36c2477738d7fcbf890b8656fd75d971, R-sharp\R#\Runtime\Interop\RInteropAttributes\RPackageModuleAttribute.vb"

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


     Code Statistics:

        Total Lines:   9
        Code Lines:    5
        Comment Lines: 3
        Blank Lines:   1
        File Size:     310.00 B


    '     Class RPackageModuleAttribute
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime.Interop

    ''' <summary>
    ''' 标注这个程序集是一个``R#``程序包
    ''' </summary>
    <AttributeUsage(AttributeTargets.Assembly, AllowMultiple:=False, Inherited:=True)>
    Public Class RPackageModuleAttribute : Inherits RInteropAttribute
    End Class
End Namespace
