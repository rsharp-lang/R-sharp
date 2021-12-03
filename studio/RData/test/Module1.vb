#Region "Microsoft.VisualBasic::416f9da3ff88858a7205a994b1cb8443, studio\RData\test\Module1.vb"

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

' Module Module1
' 
'     Sub: Main, testLogical
' 
' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.RData

Module Module1

    Sub Main()

        ' Call testLogical()

        ' Dim output2 = New MemoryStream()
        ' Dim decompressor = New BZip2InputStream("F:\report.rda.tar".Open, True)
        ' decompressor.CopyTo(output2)

        ' Call output2.FlushStream("F:\report.rda\report2.rda")
        Using file = "E:\GCModeller\src\R-sharp\studio\RData\test\test_list.rda".Open
            Dim obj As RData = Reader.ParseData(file)
            Dim r = ConvertToR.ToRObject(obj.object)

            Pause()
        End Using
    End Sub

    Sub testLogical()
        Using file = "D:\GCModeller\src\R-sharp\studio\test\data\test_logical.rda".Open
            Dim obj = Reader.ParseData(file)

            Pause()
        End Using
    End Sub

End Module
