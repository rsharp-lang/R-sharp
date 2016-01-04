
Public Class BooleanVector : Inherits GenericVector(Of Boolean)

    Public Shared ReadOnly Property [True] As BooleanVector
        Get
            Return New BooleanVector({True})
        End Get
    End Property

    Sub New(Elements As Generic.IEnumerable(Of Boolean))
        Me.Elements = Elements.ToArray
    End Sub

    Public Shared Operator &(x As Boolean, y As BooleanVector) As BooleanVector
        Return New BooleanVector((From b As Boolean In y Select b AndAlso x).ToArray)
    End Operator

    Public Shared Operator &(x As BooleanVector, y As BooleanVector) As BooleanVector
        Return New BooleanVector((From i As Integer In x.Sequence Select x.Elements(i) AndAlso y.Elements(i)).ToArray)
    End Operator

    Public Shared Operator Not(x As BooleanVector) As BooleanVector
        Return New BooleanVector((From b As Boolean In x Select Not b).ToArray)
    End Operator

    Public Shared Narrowing Operator CType(x As BooleanVector) As Boolean
        Return x.Elements(0)
    End Operator

    Public Shared Widening Operator CType(b As Boolean()) As BooleanVector
        Return New BooleanVector(b)
    End Operator

    Public Shared Operator Or(x As BooleanVector, y As Boolean()) As BooleanVector
        Return New BooleanVector((From i As Integer In x.Elements.Sequence Select x.Elements(i) Or y(i)).ToArray)
    End Operator

    Public Shared Operator Or(x As BooleanVector, y As BooleanVector) As BooleanVector
        Return x Or y.Elements
    End Operator

    Public Shared Narrowing Operator CType(x As BooleanVector) As Boolean()
        Return x.Elements
    End Operator

End Class

Public Class GenericVector(Of T) : Implements Generic.IEnumerable(Of T)

    Implements System.IDisposable

    ''' <summary>
    ''' 向量维数
    ''' </summary>
    ''' <remarks></remarks>
    Public ReadOnly Property [Dim] As Integer
        Get
            Return Elements.Count
        End Get
    End Property

    Public Property Elements As T()

    Default Public Overloads Property ElementValues(Conditions As BooleanVector) As T()
        Get
            Dim LQuery = (From i As Integer In Conditions.Sequence Where Conditions._Elements(i) Select _Elements(i)).ToArray
            Return LQuery
        End Get
        Set(value As T())
            For i As Integer = 0 To Conditions.Count - 1
                If Conditions._Elements(i) Then
                    _Elements(i) = value(i)
                End If
            Next
        End Set
    End Property

    Public Sub Factor(value As Integer)
        ReDim Preserve _Elements(value - 1)
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="a">只有一个元素的</param>
    ''' <param name="b">只有一个元素的</param>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Default Public Overloads Property ElementValues(a As Vector, b As Vector) As T()
        Get
            Dim x As Integer = a.Elements(0), y As Integer = b.Elements(0)
            Dim ChunkBuffer As T() = New T(y - x - 1) {}
            Call Array.ConstrainedCopy(Elements, x, ChunkBuffer, 0, ChunkBuffer.Length)
            Return ChunkBuffer
        End Get
        Set(value As T())
            Dim x As Integer = a.Elements(0), y As Integer = b.Elements(0)
            Dim idx As Integer = 0
            For i As Integer = x To y
                _Elements(i) = value(idx)
                idx += 1
            Next
        End Set
    End Property

    Default Public Overloads Property ElementValues(a As Integer, b As Vector) As T()
        Get
            Return ElementValues(New Vector({a}), b)
        End Get
        Set(value As T())
            ElementValues(New Vector({a}), b) = value
        End Set
    End Property

#Region "Implements Generic.IEnumerable(Of T)"

    Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
        For Each Element As T In Elements
            Yield Element
        Next
    End Function

    Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
        Yield GetEnumerator()
    End Function
#End Region

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        Me.disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class

Public Class Vector : Inherits GenericVector(Of Double)
    Implements Generic.IEnumerable(Of Double)

    Default Public Overloads Property ElementValues(Conditions As BooleanVector) As Vector
        Get
            Return New Vector(Me.ElementValues(Conditions))
        End Get
        Set(value As Vector)
            Me.ElementValues(Conditions) = value.Elements
        End Set
    End Property

    Public Shared ReadOnly Property NAN As Vector
        Get
            Return New Vector({Double.NaN})
        End Get
    End Property

    Public Shared ReadOnly Property Inf As Vector
        Get
            Return New Vector({Double.PositiveInfinity})
        End Get
    End Property

    Public Shared ReadOnly Property Zero As Vector
        Get
            Return New Vector({0})
        End Get
    End Property

    Public Sub New(m As Integer)
        Elements = New Double(m - 1) {}
    End Sub

    Protected Sub New()
    End Sub

    Public Sub New(data As Generic.IEnumerable(Of Double))
        Elements = data.ToArray
    End Sub

    Public Shared Widening Operator CType(data As Double()) As Vector
        Return New Vector With {.Elements = data}
    End Operator

    Public Shared Narrowing Operator CType(vector As Vector) As Integer
        Return vector.Elements(0)
    End Operator

    ''' <summary>
    ''' 两个向量加法算符重载，分量分别相加
    ''' </summary>
    ''' <param name="v1"></param>
    ''' <param name="v2"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Operator +(v1 As Vector, v2 As Vector) As Vector
        Dim N0 As Integer

        '获取变量维数
        N0 = v1.[Dim]

        Dim v3 As New Vector(N0)

        Dim j As Integer
        For j = 0 To N0 - 1
            v3.Elements(j) = v1.Elements(j) + v2.Elements(j)
        Next
        Return v3
    End Operator

    Public Shared Operator >(x As Vector, n As Double) As BooleanVector
        Return New BooleanVector((From d As Double In x.Elements Select d > n).ToArray)
    End Operator

    Public Shared Operator <(x As Vector, n As Double) As BooleanVector
        Return New BooleanVector((From d As Double In x.Elements Select d < n).ToArray)
    End Operator

    Public Shared Operator >=(x As Vector, n As Double) As BooleanVector
        Return New BooleanVector((From d As Double In x Select d >= n).ToArray)
    End Operator

    Public Shared Operator <=(x As Vector, n As Double) As BooleanVector
        Return New BooleanVector((From d As Double In x Select d <= n).ToArray)
    End Operator

    ''' <summary>
    ''' 向量减法算符重载，分量分别想减
    ''' </summary>
    ''' <param name="v1"></param>
    ''' <param name="v2"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Operator -(v1 As Vector, v2 As Vector) As Vector
        Dim N0 As Integer
        '获取变量维数
        N0 = v1.[Dim]

        Dim v3 As New Vector(N0)

        Dim j As Integer
        For j = 0 To N0 - 1
            v3.Elements(j) = v1.Elements(j) - v2.Elements(j)
        Next
        Return v3
    End Operator

    ''' <summary>
    ''' 向量乘法算符重载，分量分别相乘，相当于MATLAB中的  .*算符
    ''' </summary>
    ''' <param name="v1"></param>
    ''' <param name="v2"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Operator *(v1 As Vector, v2 As Vector) As Vector

        Dim N0 As Integer

        '获取变量维数
        N0 = v1.[Dim]

        Dim v3 As New Vector(N0)

        Dim j As Integer
        For j = 0 To N0 - 1
            v3.Elements(j) = v1.Elements(j) * v2.Elements(j)
        Next
        Return v3
    End Operator

    ''' <summary>
    ''' 向量除法算符重载，分量分别相除，相当于MATLAB中的   ./算符
    ''' </summary>
    ''' <param name="v1"></param>
    ''' <param name="v2"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Operator /(v1 As Vector, v2 As Vector) As Vector
        Dim N0 As Integer

        '获取变量维数
        N0 = v1.[Dim]

        Dim v3 As New Vector(N0)

        Dim j As Integer
        For j = 0 To N0 - 1
            v3.Elements(j) = v1.Elements(j) / v2.Elements(j)
        Next
        Return v3
    End Operator

    ''' <summary>
    ''' 向量减加实数，各分量分别加实数
    ''' </summary>
    ''' <param name="v1"></param>
    ''' <param name="a"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Operator +(v1 As Vector, a As Double) As Vector
        '向量数加算符重载
        Dim N0 As Integer

        '获取变量维数
        N0 = v1.[Dim]

        Dim v2 As New Vector(N0)

        Dim j As Integer
        For j = 0 To N0 - 1
            v2.Elements(j) = v1.Elements(j) + a
        Next
        Return v2
    End Operator

    ''' <summary>
    ''' 向量减实数，各分量分别减实数
    ''' </summary>
    ''' <param name="v1"></param>
    ''' <param name="a"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Operator -(v1 As Vector, a As Double) As Vector
        '向量数加算符重载
        Dim N0 As Integer

        '获取变量维数
        N0 = v1.[Dim]

        Dim v2 As New Vector(N0)

        Dim j As Integer
        For j = 0 To N0 - 1
            v2.Elements(j) = v1.Elements(j) - a
        Next
        Return v2
    End Operator

    ''' <summary>
    ''' 向量 数乘，各分量分别乘以实数
    ''' </summary>
    ''' <param name="v1"></param>
    ''' <param name="a"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Operator *(v1 As Vector, a As Double) As Vector
        Dim N0 As Integer

        '获取变量维数
        N0 = v1.[Dim]

        Dim v2 As New Vector(N0)

        Dim j As Integer
        For j = 0 To N0 - 1
            v2.Elements(j) = v1.Elements(j) * a
        Next
        Return v2
    End Operator

    ''' <summary>
    ''' 向量 数除，各分量分别除以实数
    ''' </summary>
    ''' <param name="v1"></param>
    ''' <param name="a"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Operator /(v1 As Vector, a As Double) As Vector
        Dim N0 As Integer

        '获取变量维数
        N0 = v1.[Dim]

        Dim v2 As New Vector(N0)

        Dim j As Integer
        For j = 0 To N0 - 1
            v2.Elements(j) = v1.Elements(j) / a
        Next
        Return v2
    End Operator

    Public Shared Operator /(n As Double, x As Vector) As Vector
        Return New Vector((From d As Double In x.Elements Select n / d).ToArray)
    End Operator

    ''' <summary>
    ''' 实数加向量
    ''' </summary>
    ''' <param name="a"></param>
    ''' <param name="v1"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Operator +(a As Double, v1 As Vector) As Vector
        '向量数加算符重载
        Dim N0 As Integer

        '获取变量维数
        N0 = v1.[Dim]

        Dim v2 As New Vector(N0)

        Dim j As Integer
        For j = 0 To N0 - 1
            v2.Elements(j) = v1.Elements(j) + a
        Next
        Return v2
    End Operator

    ''' <summary>
    ''' 实数减向量
    ''' </summary>
    ''' <param name="a"></param>
    ''' <param name="v1"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Operator -(a As Double, v1 As Vector) As Vector
        '向量数加算符重载
        Dim N0 As Integer

        '获取变量维数
        N0 = v1.[Dim]

        Dim v2 As New Vector(N0)

        Dim j As Integer
        For j = 0 To N0 - 1
            v2.Elements(j) = v1.Elements(j) - a
        Next
        Return v2
    End Operator

    ''' <summary>
    ''' 向量 数乘
    ''' </summary>
    ''' <param name="a"></param>
    ''' <param name="v1"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Operator *(a As Double, v1 As Vector) As Vector
        Dim N0 As Integer

        '获取变量维数
        N0 = v1.[Dim]

        Dim v2 As New Vector(N0)

        Dim j As Integer
        For j = 0 To N0 - 1
            v2.Elements(j) = v1.Elements(j) * a
        Next
        Return v2
    End Operator

    Public Shared Operator Mod(x As Vector, y As Vector) As Vector
        Return New Vector((From i As Integer In x Select x.Elements(i) Mod y.Elements(i)).ToArray)
    End Operator

    Public Shared Operator Mod(n As Double, x As Vector) As Vector
        Return New Vector((From d As Double In x.Elements Select n Mod d).ToArray)
    End Operator

    ''' <summary>
    ''' 向量内积
    ''' </summary>
    ''' <param name="v1"></param>
    ''' <param name="v2"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Operator Or(v1 As Vector, v2 As Vector) As Double
        Dim N0 As Integer, M0 As Integer

        '获取变量维数
        N0 = v1.[Dim]

        M0 = v2.[Dim]

        If N0 <> M0 Then
            System.Console.WriteLine("Inner vector dimensions must agree！")
        End If
        '如果向量维数不匹配，给出告警信息

        Dim sum As Double
        sum = 0.0

        Dim j As Integer
        For j = 0 To N0 - 1
            sum = sum + v1.Elements(j) * v2.Elements(j)
        Next
        Return sum
    End Operator

    ' ''' <summary>
    ' ''' 向量外积（相当于列向量，乘以横向量）
    ' ''' </summary>
    ' ''' <param name="v1"></param>
    ' ''' <param name="v2"></param>
    ' ''' <returns></returns>
    ' ''' <remarks></remarks>
    'Public Shared Operator Xor(v1 As Vector, v2 As Vector) As MATRIX
    '    Dim N0 As Integer, M0 As Integer

    '    '获取变量维数
    '    N0 = v1.[dim]

    '    M0 = v2.[dim]

    '    If N0 <> M0 Then
    '        System.Console.WriteLine("Inner vector dimensions must agree！")
    '    End If
    '    '如果向量维数不匹配，给出告警信息

    '    Dim vvmat As New MATRIX(N0, N0)

    '    For i As Integer = 0 To N0 - 1
    '        For j As Integer = 0 To N0 - 1
    '            vvmat(i, j) = v1(i) * v2(j)
    '        Next
    '    Next

    '    '返回外积矩阵

    '    Return vvmat
    'End Operator

    ''' <summary>
    ''' 向量模的平方
    ''' </summary>
    ''' <param name="v1"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Operator Not(v1 As Vector) As Double
        Dim N0 As Integer

        '获取变量维数
        N0 = v1.[Dim]

        Dim sum As Double
        sum = 0.0

        Dim j As Integer
        For j = 0 To N0 - 1
            sum = sum + v1.Elements(j) * v1.Elements(j)
        Next
        Return sum
    End Operator

    ''' <summary>
    ''' 负向量 
    ''' </summary>
    ''' <param name="v1"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Operator -(v1 As Vector) As Vector
        Dim N0 As Integer = v1.[Dim]

        Dim v2 As New Vector(N0)

        For i As Integer = 0 To N0 - 1
            v2.Elements(i) = -v1.Elements(i)
        Next

        Return v2
    End Operator

    Public Shared Operator <>(x As Vector, y As Vector) As BooleanVector
        Dim LQuery = (From i As Integer In x.Elements.Sequence Select x.Elements(i) <> y.Elements(i)).ToArray
        Return New BooleanVector(LQuery)
    End Operator

    Public Shared Operator =(x As Vector, y As Vector) As BooleanVector
        Dim LQuery = (From i As Integer In x.Elements.Sequence Select x.Elements(i) = y.Elements(i)).ToArray
        Return New BooleanVector(LQuery)
    End Operator

    Public Shared Operator =(x As Vector, n As Integer) As BooleanVector
        Return New BooleanVector((From d As Double In x.Elements Select d = n).ToArray)
    End Operator

    Public Shared Operator <>(x As Vector, n As Integer) As BooleanVector
        Return New BooleanVector((From d As Double In x.Elements Select d <> n).ToArray)
    End Operator

    Public Shared Operator ^(v As Vector, n As Integer) As Vector
        Return New Vector With {.Elements = (From d As Double In v.Elements Select d ^ n).ToArray}
    End Operator
End Class
