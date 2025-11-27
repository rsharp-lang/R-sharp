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