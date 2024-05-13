#Region "Microsoft.VisualBasic::e3bb777d742edf089ea983da02d169c2, R#\Runtime\Interop\RInteropAttributes\RPlotStyleCSSAttribute.vb"

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

    '   Total Lines: 27
    '    Code Lines: 12
    ' Comment Lines: 12
    '   Blank Lines: 3
    '     File Size: 997 B


    '     Class RPlotStyleCSSAttribute
    ' 
    '         Properties: bg, legendTitle, legendTitleFont, main, mainTitleFont
    '                     padding, size
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime.Interop

    ''' <summary>
    ''' 这个主要是针对plot泛型函数，因为泛型函数没有办法正常展示与绘图样式相关的函数参数，所以会需要使用这个参数来进行展示
    ''' </summary>
    ''' 
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class RPlotStyleCSSAttribute : Inherits RInteropAttribute

        ''' <summary>
        ''' the background color or texture image path
        ''' </summary>
        ''' <returns></returns>
        Public Property bg As String
        ''' <summary>
        ''' the main title
        ''' </summary>
        ''' <returns></returns>
        Public Property main As String
        Public Property size As String
        Public Property padding As String
        Public Property mainTitleFont As String
        Public Property legendTitle As String
        Public Property legendTitleFont As String

    End Class
End Namespace
