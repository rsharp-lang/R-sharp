#Region "Microsoft.VisualBasic::33082f99f19cad53458446ea716553c8, R#\Runtime\Interop\RInteropAttributes\VectorMask\DefaultVectorParser.vb"

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

'     Structure DefaultVectorParser
' 
'         Function: ParseVector
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Net.Http.Base64Codec

Namespace Runtime.Interop

    ''' <summary>
    ''' 1. 字符串类型默认使用``|``作为分隔符
    ''' 2. 数值类型默认使用``,``作为分隔符
    ''' </summary>
    Public Structure DefaultVectorParser : Implements IVectorExpressionLiteral

        Public Function ParseVector(default$, schema As Type) As Array Implements IVectorExpressionLiteral.ParseVector
            Select Case schema
                Case GetType(String)
                    Return [default]?.Split("|"c)
                Case GetType(Double), GetType(Single)
                    Return [default]?.Split(","c) _
                        .Select(AddressOf Trim) _
                        .Select(AddressOf Val) _
                        .ToArray
                Case GetType(Integer), GetType(Long), GetType(Short)
                    Return [default]?.Split(","c) _
                        .Select(AddressOf Trim) _
                        .Select(AddressOf Long.Parse) _
                        .ToArray
                Case GetType(Boolean)
                    Return [default]?.Split(","c) _
                        .Select(AddressOf Trim) _
                        .Select(AddressOf ParseBoolean) _
                        .ToArray
                Case GetType(Byte)
                    ' byte data is parse from base64 string
                    Return [default]?.Base64RawBytes
                Case Else
                    Throw New NotImplementedException(schema.FullName)
            End Select
        End Function
    End Structure
End Namespace
