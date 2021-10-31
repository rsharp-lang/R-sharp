#Region "Microsoft.VisualBasic::37d9df43295e44ab01cbb61ce984997c, Library\R.base\base\IO.vb"

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

' Module RawIO
' 
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' R# raw I/O api module
''' </summary>
<Package("IO", Category:=APICategories.UtilityTools, Publisher:="xie.guigang@gmail.com")>
Module RawIO

    Public Function dumpDf(<RRawVectorArgument>
                           source As Object,
                           Optional type As sourceTypes = sourceTypes.JSON,
                           Optional projection As String() = Nothing,
                           Optional trim As String() = Nothing,
                           Optional env As Environment = Nothing) As pipeline

    End Function
End Module

Public Enum sourceTypes
    XML
    JSON
End Enum