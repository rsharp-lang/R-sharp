#Region "Microsoft.VisualBasic::e29cf5fa8b4a97449c119811d62020b3, E:/GCModeller/src/R-sharp/R#//Runtime/Internal/internalInvokes/Math/time.vb"

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

    '   Total Lines: 19
    '    Code Lines: 15
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 523 B


    '     Module time
    ' 
    '         Function: hours, minutes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Runtime.Internal.Invokes

    <Package("time")>
    Module time

        <ExportAPI("hours")>
        Public Function hours(h As Double) As TimeSpan
            Return TimeSpan.FromHours(h)
        End Function

        <ExportAPI("minutes")>
        Public Function minutes(m As Double) As TimeSpan
            Return TimeSpan.FromMinutes(m)
        End Function

        <ExportAPI("seconds")>
        Public Function seconds(s As Double) As TimeSpan
            Return TimeSpan.FromSeconds(s)
        End Function

        ''' <summary>
        ''' create time span from given days value.
        ''' </summary>
        ''' <param name="d"></param>
        ''' <returns></returns>
        <ExportAPI("days")>
        Public Function days(d As Double) As TimeSpan
            Return TimeSpan.FromDays(d)
        End Function

        ''' <summary>
        ''' create timespan value
        ''' </summary>
        ''' <param name="seconds"></param>
        ''' <param name="minutes"></param>
        ''' <param name="hours"></param>
        ''' <param name="days"></param>
        ''' <returns></returns>
        <ExportAPI("time_spanval")>
        Public Function time_span(Optional seconds As Double = 0,
                                  Optional minutes As Double = 0,
                                  Optional hours As Double = 0,
                                  Optional days As Double = 0) As TimeSpan

            Return New TimeSpan(days, hours, minutes, seconds)
        End Function
    End Module
End Namespace
