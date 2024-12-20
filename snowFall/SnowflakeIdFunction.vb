﻿#Region "Microsoft.VisualBasic::208356d14e5a1a5aae1b90813282df41, snowFall\SnowflakeIdFunction.vb"

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
    '    Code Lines: 12 (63.16%)
    ' Comment Lines: 3 (15.79%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (21.05%)
    '     File Size: 528 B


    ' Class SnowflakeIdFunction
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: nextId
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Repository
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' a R# function wrapper for <see cref="SnowflakeIdGenerator"/>
''' </summary>
Public Class SnowflakeIdFunction : Inherits RDefaultFunction

    ReadOnly snowflakeId As SnowflakeIdGenerator

    Sub New(config As SnowflakeIdGenerator)
        snowflakeId = config
    End Sub

    <RDefaultFunctionAttribute>
    Public Function nextId() As Long
        Return snowflakeId.GenerateId
    End Function
End Class
