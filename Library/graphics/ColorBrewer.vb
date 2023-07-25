#Region "Microsoft.VisualBasic::24572bb54eb99e4396a13cbebd43735e, D:/GCModeller/src/R-sharp/Library/graphics//ColorBrewer.vb"

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

    '   Total Lines: 221
    '    Code Lines: 129
    ' Comment Lines: 51
    '   Blank Lines: 41
    '     File Size: 7.44 KB


    ' Module ColorBrewerSystem
    ' 
    '     Function: Accent, Blues, BrBG, BuGn, BuPu
    '               Dark2, GnBu, Greens, Greys, Oranges
    '               OrRd, Paired, Pastel1, Pastel2, PiYG
    '               PRGn, PuBu, PuBuGn, PuOr, PuRd
    '               Purples, RdBu, RdGy, RdPu, RdYlBu
    '               RdYlGn, Reds, Set1, Set2, Set3
    '               Spectral, TrIQ, YlGn, YlGnBu, YlOrBr
    '               YlOrRd
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Scaler
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' Color schema name terms that comes from the ColorBrewer system:
'''
''' All of the color schema in ColorBrewer system have several levels, which could be use expression 
''' pattern such as ``schema_name:c[level]`` for get colors from the color designer, examples as 
''' ``YlGnBu:c6`` will generate a color sequence which have 6 colors that comes from the ``YlGnBu`` 
''' pattern.
'''
''' 1. Sequential schemes are suited to ordered data that progress from low to high. Lightness steps 
''' dominate the look of these schemes, with light colors for low data values to dark colors for 
''' high data values. 
'''
''' All of the colors terms in Sequential schemes have levels from 3 to 9, schema name terms 
''' includes:
'''
''' ```
''' OrRd:c[3,9], PuBu:c[3,9], BuPu:c[3,9], Oranges:c[3,9], BuGn:c[3,9], YlOrBr:c[3,9]
''' YlGn:c[3,9], Reds:c[3,9], RdPu:c[3,9], Greens:c[3,9], YlGnBu:c[3,9], Purples:c[3,9]
''' GnBu:c[3,9], Greys:c[3,9], YlOrRd:c[3,9], PuRd:c[3,9], Blues:c[3,9], PuBuGn:c[3,9]
''' ```
'''
''' 2. Qualitative schemes do not imply magnitude differences between legend classes, and hues are used 
''' to create the primary visual differences between classes. Qualitative schemes are best suited to 
''' representing nominal or categorical data. 
'''
''' The color levels in this schema range from 3 to 12, schema name terms includes:
'''
''' ```
''' Set2:c[3,8], Accent:c[3,8], Set1:c[3,9], Set3:c[3,12], Dark2:c[3,8], Paired:c[3,12]
''' Pastel2:c[3,8], Pastel1:c[3,9]
''' ```
''' 
''' 3. Diverging schemes put equal emphasis on mid-range critical values and extremes at both ends of 
''' the data range. The critical class or break in the middle of the legend is emphasized with light 
''' colors and low and high extremes are emphasized with dark colors that have contrasting hues.
'''
''' All of the colors terms in Sequential schemes have levels from 3 to 11, schema name terms 
''' includes:
'''
''' ```
''' Spectral:c[3,11], RdYlGn:c[3,11], RdBu:c[3,11], PiYG:c[3,11], PRGn:c[3,11], RdYlBu:c[3,11]
''' BrBG:c[3,11], RdGy:c[3,11], PuOr:c[3,11]
''' ```
''' </summary>
<Package("ColorBrewer")>
Public Module ColorBrewerSystem

    ''' <summary>
    ''' get cutoff threshold value via TrIQ algorithm
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="q"></param>
    ''' <param name="levels"></param>
    ''' <returns></returns>
    <ExportAPI("TrIQ")>
    Public Function TrIQ(<RRawVectorArgument>
                         data As Object,
                         Optional q As Double = 0.65,
                         Optional levels As Integer = 30) As Double

        Return CLRVector.asNumeric(data).FindThreshold(q, levels)
    End Function

#Region "Sequential"

    <ExportAPI> Public Function OrRd() As Color()
        Return Designer.GetColors("OrRd:c9")
    End Function

    <ExportAPI> Public Function PuBu() As Color()
        Return Designer.GetColors("PuBu:c9")
    End Function

    <ExportAPI> Public Function BuPu() As Color()
        Return Designer.GetColors("BuPu:c9")
    End Function

    <ExportAPI> Public Function Oranges() As Color()
        Return Designer.GetColors("Oranges:c9")
    End Function

    <ExportAPI> Public Function BuGn() As Color()
        Return Designer.GetColors("BuGn:c9")
    End Function

    <ExportAPI> Public Function YlOrBr() As Color()
        Return Designer.GetColors("YlOrBr:c9")
    End Function

    <ExportAPI> Public Function YlGn() As Color()
        Return Designer.GetColors("YlGn:c9")
    End Function

    <ExportAPI> Public Function Reds() As Color()
        Return Designer.GetColors("Reds:c9")
    End Function

    <ExportAPI> Public Function RdPu() As Color()
        Return Designer.GetColors("RdPu:c9")
    End Function

    <ExportAPI> Public Function Greens() As Color()
        Return Designer.GetColors("Greens:c9")
    End Function

    <ExportAPI> Public Function YlGnBu() As Color()
        Return Designer.GetColors("YlGnBu:c9")
    End Function

    <ExportAPI> Public Function Purples() As Color()
        Return Designer.GetColors("Purples:c9")
    End Function

    <ExportAPI> Public Function GnBu() As Color()
        Return Designer.GetColors("GnBu:c9")
    End Function

    <ExportAPI> Public Function Greys() As Color()
        Return Designer.GetColors("Greys:c9")
    End Function

    <ExportAPI> Public Function YlOrRd() As Color()
        Return Designer.GetColors("YlOrRd:c9")
    End Function

    <ExportAPI> Public Function PuRd() As Color()
        Return Designer.GetColors("PuRd:c9")
    End Function

    <ExportAPI> Public Function Blues() As Color()
        Return Designer.GetColors("Blues:c9")
    End Function

    <ExportAPI> Public Function PuBuGn() As Color()
        Return Designer.GetColors("PuBuGn:c9")
    End Function
#End Region

#Region "Qualitative"

    <ExportAPI> Public Function Set2() As Color()
        Return Designer.GetColors("Set2:c8")
    End Function

    <ExportAPI> Public Function Accent() As Color()
        Return Designer.GetColors("Accent:c8")
    End Function

    <ExportAPI> Public Function Set1() As Color()
        Return Designer.GetColors("Set1:c9")
    End Function

    <ExportAPI> Public Function Set3() As Color()
        Return Designer.GetColors("Set3:c12")
    End Function

    <ExportAPI> Public Function Dark2() As Color()
        Return Designer.GetColors("Dark2:c8")
    End Function

    <ExportAPI> Public Function Paired() As Color()
        Return Designer.GetColors("Paired:c12")
    End Function

    <ExportAPI> Public Function Pastel2() As Color()
        Return Designer.GetColors("Pastel2:c8")
    End Function

    <ExportAPI> Public Function Pastel1() As Color()
        Return Designer.GetColors("Pastel1:c9")
    End Function
#End Region

#Region "Diverging"
    <ExportAPI> Public Function Spectral() As Color()
        Return Designer.GetColors("Spectral:c11")
    End Function

    <ExportAPI> Public Function RdYlGn() As Color()
        Return Designer.GetColors("RdYlGn:c11")
    End Function

    <ExportAPI> Public Function RdBu() As Color()
        Return Designer.GetColors("RdBu:c11")
    End Function

    <ExportAPI> Public Function PiYG() As Color()
        Return Designer.GetColors("PiYG:c11")
    End Function

    <ExportAPI> Public Function PRGn() As Color()
        Return Designer.GetColors("PRGn:c11")
    End Function

    <ExportAPI> Public Function RdYlBu() As Color()
        Return Designer.GetColors("RdYlBu:c11")
    End Function

    <ExportAPI> Public Function BrBG() As Color()
        Return Designer.GetColors("BrBG:c11")
    End Function

    <ExportAPI> Public Function RdGy() As Color()
        Return Designer.GetColors("RdGy:c11")
    End Function

    <ExportAPI> Public Function PuOr() As Color()
        Return Designer.GetColors("PuOr:c11")
    End Function
#End Region

End Module
