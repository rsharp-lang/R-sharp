#Region "Microsoft.VisualBasic::893042118bf6d4cdb9069093f936990f, studio\Rserver\Rweb\AccessController.vb"

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

    '   Total Lines: 42
    '    Code Lines: 29
    ' Comment Lines: 4
    '   Blank Lines: 9
    '     File Size: 1.21 KB


    ' Class AccessController
    ' 
    '     Properties: ignores, redirect, status_key
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: CheckAccess
    ' 
    ' /********************************************************************************/

#End Region

Imports Flute.Http.Configurations
Imports Flute.SessionManager
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Net.HTTP

Public Class AccessController

    ''' <summary>
    ''' key for check of the session file
    ''' </summary>
    ''' <returns></returns>
    Public Property status_key As String
    Public Property redirect As String

    Public Property ignores As String()
        Get
            Return m_ignoreIndex.Objects
        End Get
        Set(value As String())
            m_ignoreIndex = value.Indexing
        End Set
    End Property

    Dim m_ignoreIndex As Index(Of String)

    Sub New()
    End Sub

    Public Function CheckAccess(url As URL, ssid As String, config As Configuration) As Boolean
        If url.path Like m_ignoreIndex Then
            Return True
        ElseIf Not url.path.ExtensionSuffix("html", "htm") Then
            Return True
        End If

        Dim session As SessionFile = Flute.SessionManager.Open(ssid, config)
        Dim check As String = session.OpenKeyString(status_key)

        Return Not check.StringEmpty(testEmptyFactor:=True)
    End Function

End Class

