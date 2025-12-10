#Region "Microsoft.VisualBasic::e1928c5c3a4787e5b3a959191adbdd7f, R#\Runtime\Internal\internalInvokes\file\NullStream.vb"

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

    '   Total Lines: 62
    '    Code Lines: 34 (54.84%)
    ' Comment Lines: 15 (24.19%)
    '    - Xml Docs: 26.67%
    ' 
    '   Blank Lines: 13 (20.97%)
    '     File Size: 2.30 KB


    '     Class NullStream
    ' 
    '         Properties: CanRead, CanSeek, CanWrite, Length, Position
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Read, Seek
    ' 
    '         Sub: Dispose, Flush, SetLength, Write
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' 提供一个不存储任何数据的 Stream。所有写入的数据都被丢弃，所有读取操作都返回0。
    ''' 这是对类 Unix 系统上的 /dev/null 或 Windows 上的 NUL 设备的 .NET 模拟。
    ''' </summary>
    Friend NotInheritable Class NullStream
        Inherits Stream

        ' 单例模式，因为 NullStream 不需要任何状态，一个实例就足够了
        Public Shared ReadOnly Instance As New NullStream()

        Private Sub New()
            ' 私有构造函数以强制使用单例
        End Sub

        ' 重写核心属性
        Public Overrides ReadOnly Property CanRead As Boolean = True
        Public Overrides ReadOnly Property CanSeek As Boolean = False
        Public Overrides ReadOnly Property CanWrite As Boolean = True
        Public Overrides ReadOnly Property Length As Long = 0

        Public Overrides Property Position As Long
            Get
                Return 0
            End Get
            Set(value As Long)

            End Set
        End Property

        ' 重写核心方法
        Public Overrides Sub Flush()
            ' 什么都不做，因为没有数据需要刷新
        End Sub

        Public Overrides Function Read(buffer() As Byte, offset As Integer, count As Integer) As Integer
            ' 立即返回0，表示已到达流的末尾
            Return 0
        End Function

        Public Overrides Function Seek(offset As Long, origin As SeekOrigin) As Long
            Return 0
        End Function

        Public Overrides Sub SetLength(value As Long)
        End Sub

        Public Overrides Sub Write(buffer() As Byte, offset As Integer, count As Integer)
            ' 什么都不做，数据被丢弃
            ' 为了符合 Stream 的约定，我们不需要抛出异常
            ' 如果需要，可以在这里记录被丢弃的字节数，但通常没有必要
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            ' 没有非托管资源需要释放，所以什么都不做
            ' 但仍然需要提供这个方法以符合基类契约
        End Sub
    End Class
End Namespace
