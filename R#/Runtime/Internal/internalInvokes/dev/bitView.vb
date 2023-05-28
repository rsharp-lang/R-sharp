#Region "Microsoft.VisualBasic::426824fd4204e414064bec235c35207d, F:/GCModeller/src/R-sharp/R#//Runtime/Internal/internalInvokes/dev/bitView.vb"

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

    '   Total Lines: 46
    '    Code Lines: 37
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.70 KB


    '     Module bitView
    ' 
    '         Function: bitConvert, doubles, floats, int64, integers
    '                   shorts
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Runtime.Internal.Invokes

    <Package("bitView")>
    Module bitView

        <ExportAPI("doubles")>
        Public Function doubles(buffer As MemoryStream) As Double()
            Return buffer.bitConvert(sizeof:=8, AddressOf BitConverter.ToDouble)
        End Function

        <ExportAPI("integers")>
        Public Function integers(buffer As MemoryStream) As Integer()
            Return buffer.bitConvert(sizeof:=4, AddressOf BitConverter.ToInt32)
        End Function

        <ExportAPI("floats")>
        Public Function floats(buffer As MemoryStream) As Single()
            Return buffer.bitConvert(sizeof:=4, AddressOf BitConverter.ToSingle)
        End Function

        <ExportAPI("int16s")>
        Public Function shorts(buffer As MemoryStream) As Short()
            Return buffer.bitConvert(sizeof:=2, AddressOf BitConverter.ToInt16)
        End Function

        <ExportAPI("int64s")>
        Public Function int64(buffer As MemoryStream) As Long()
            Return buffer.bitConvert(sizeof:=8, AddressOf BitConverter.ToInt64)
        End Function

        <Extension>
        Private Function bitConvert(Of T)(buffer As MemoryStream, sizeof As Integer, load As Func(Of Byte(), Integer, T)) As T()
            Return buffer.ToArray _
                .Split(sizeof) _
                .Where(Function(b) b.Length = sizeof) _
                .Select(Function(byts) load(byts, Scan0)) _
                .ToArray
        End Function

    End Module
End Namespace
