﻿#Region "Microsoft.VisualBasic::7cdfca4ea30bdfdefe7cf3c3121f5c1f, D:/GCModeller/src/R-sharp/R#//Runtime/Internal/internalInvokes/Math/time.vb"

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

    '   Total Lines: 17
    '    Code Lines: 13
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 450 B


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
    End Module
End Namespace
