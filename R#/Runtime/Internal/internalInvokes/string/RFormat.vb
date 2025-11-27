Imports System.Globalization
Imports System.Text

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' 提供与 R 语言 base::format 函数功能相似的格式化方法。
    ''' </summary>
    Public Class RFormat

        ''' <summary>
        ''' 指定字符串的对齐方式，模仿 R 的 justify 参数。
        ''' </summary>
        Public Enum Justification
            none
            left
            right
            centre
        End Enum

        ''' <summary>
        ''' 格式化一个对象（数字、日期或字符串），模仿 R 的 format 函数。
        ''' </summary>
        ''' <param name="x">要格式化的对象 (数字, 日期, 字符串)。</param>
        ''' <param name="trim">对于字符串，是否移除前导和尾随空白。默认为 False。</param>
        ''' <param name="digits">数字的有效数字位数。如果为 Nothing，则不指定。默认为 Nothing。</param>
        ''' <param name="nsmall">数字小数点后的最小位数。默认为 0。</param>
        ''' <param name="justify">字符串的对齐方式。默认为 Left。</param>
        ''' <param name="width">格式化后字符串的目标宽度。如果为 Nothing，则不填充。默认为 Nothing。</param>
        ''' <param name="na_encode">如果 x 为 Nothing 或 DBNull，是否将其编码为 "NA"。默认为 True。</param>
        ''' <param name="scientific">是否强制使用科学计数法。Nothing 表示自动决定。默认为 Nothing。</param>
        ''' <param name="big_mark">用于分隔大数整数部分的千位分隔符。默认为 ""。</param>
        ''' <param name="big_interval">千位分隔符的间隔位数。默认为 3。</param>
        ''' <param name="small_mark">用于分隔小数部分的标记。默认为 ""。</param>
        ''' <param name="small_interval">小数部分分隔标记的间隔位数。默认为 5。</param>
        ''' <param name="decimal_mark">用作小数点的字符。默认为当前系统设置。</param>
        ''' <param name="zero_print">用于表示零值的字符串。如果为 Nothing，则正常显示。默认为 Nothing。</param>
        ''' <param name="drop0trailing">是否移除小数点后的尾随零。默认为 False。</param>
        ''' <returns>格式化后的字符串。</returns>
        Public Shared Function FormatValue(
        x As Object,
        Optional trim As Boolean = False,
        Optional digits As Integer? = Nothing,
        Optional nsmall As Integer = 0,
        Optional justify As Justification = Justification.Left,
        Optional width As Integer? = Nothing,
        Optional na_encode As Boolean = True,
        Optional scientific As Boolean? = Nothing,
        Optional big_mark As String = "",
        Optional big_interval As Integer = 3,
        Optional small_mark As String = "",
        Optional small_interval As Integer = 5,
        Optional decimal_mark As String = Nothing,
        Optional zero_print As String = Nothing,
        Optional drop0trailing As Boolean = False
    ) As String

            ' 1. 处理 NA/Nothing 值
            If x Is Nothing OrElse IsDBNull(x) Then
                Return If(na_encode, "NA", "")
            End If

            ' 2. 根据类型分发到具体的格式化函数
            Select Case True
                Case TypeOf x Is Double OrElse TypeOf x Is Single OrElse TypeOf x Is Decimal OrElse TypeOf x Is Integer OrElse TypeOf x Is Long
                    Return FormatNumber(CDbl(x), trim, digits, nsmall, justify, width, scientific, big_mark, big_interval, small_mark, small_interval, decimal_mark, zero_print, drop0trailing)
                Case TypeOf x Is Date
                    Return FormatDate(CDate(x), trim, justify, width)
                Case TypeOf x Is String
                    Return FormatString(CStr(x), trim, justify, width)
                Case Else
                    ' 对于其他未知类型，回退到其默认的 ToString() 表示
                    Return x.ToString()
            End Select
        End Function

#Region "Private Helper Methods"

        ''' <summary>
        ''' 格式化数字。
        ''' </summary>
        Private Shared Function FormatNumber(
        num As Double,
        trim As Boolean,
        digits As Integer?,
        nsmall As Integer,
        justify As Justification,
        width As Integer?,
        scientific As Boolean?,
        big_mark As String,
        big_interval As Integer,
        small_mark As String,
        small_interval As Integer,
        decimal_mark As String,
        zero_print As String,
        drop0trailing As Boolean
    ) As String
            Dim result As String = ""

            ' 处理 zero.print
            If num = 0 AndAlso Not String.IsNullOrEmpty(zero_print) Then
                result = zero_print
            Else
                ' 创建一个自定义的 NumberFormatInfo 来控制符号
                Dim nfi As NumberFormatInfo = CType(CultureInfo.InvariantCulture.NumberFormat.Clone(), NumberFormatInfo)

                ' 设置小数点分隔符
                If String.IsNullOrEmpty(decimal_mark) Then
                    decimal_mark = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator
                End If
                nfi.NumberDecimalSeparator = decimal_mark

                ' 设置千位分隔符
                nfi.NumberGroupSeparator = big_mark
                nfi.NumberGroupSizes = New Integer() {big_interval}

                ' 决定使用哪种格式
                Dim formatSpecifier As String = ""
                If scientific.HasValue Then
                    formatSpecifier = If(scientific.Value, "E", "F") ' E for scientific, F for fixed-point
                Else
                    ' R's default is often fixed-point unless the number is too large/small
                    formatSpecifier = "F"
                End If

                ' 处理有效数字 vs. 小数位数
                If digits.HasValue Then
                    ' R 的 digits 是指有效数字，需要特殊处理
                    result = FormatToSignificantDigits(num, digits.Value, nfi)
                Else
                    ' R 的 nsmall 是指小数位数
                    formatSpecifier &= nsmall.ToString()
                    result = num.ToString(formatSpecifier, nfi)
                End If

                ' 应用 small.mark (在小数部分添加标记)
                If Not String.IsNullOrEmpty(small_mark) AndAlso small_interval > 0 Then
                    result = ApplySmallMark(result, decimal_mark, small_mark, small_interval)
                End If

                ' 应用 drop0trailing
                If drop0trailing Then
                    result = result.TrimEnd("0"c)
                    If result.EndsWith(decimal_mark) Then
                        result = result.Substring(0, result.Length - 1)
                    End If
                End If
            End If

            ' 最后，应用填充和对齐
            Return ApplyJustificationAndWidth(result, justify, width)
        End Function

        ''' <summary>
        ''' 将数字格式化为指定的有效数字位数。
        ''' </summary>
        Private Shared Function FormatToSignificantDigits(num As Double, digits As Integer, nfi As NumberFormatInfo) As String
            If num = 0 Then Return "0"

            Dim scale As Integer = Math.Ceiling(Math.Log10(Math.Abs(num)))
            Dim factor As Double = Math.Pow(10, digits - scale)
            Dim scaledNum As Double = Math.Round(num * factor, MidpointRounding.AwayFromZero)

            ' 使用足够大的精度来格式化，然后截断
            ' 使用 "G" 通用格式，它会自动处理科学计数法
            Dim result As String = scaledNum.ToString("G" & digits.ToString(), nfi)

            ' R 的 format(digits=...) 会去掉可能多余的 .0
            If result.Contains(nfi.NumberDecimalSeparator) Then
                result = result.TrimEnd("0"c).TrimEnd(nfi.NumberDecimalSeparator.ToCharArray())
            End If

            Return result
        End Function

        ''' <summary>
        ''' 在小数部分应用 small.mark 标记。
        ''' </summary>
        Private Shared Function ApplySmallMark(s As String, decimalMark As String, smallMark As String, interval As Integer) As String
            If Not s.Contains(decimalMark) Then Return s

            Dim parts As String() = s.Split(New String() {decimalMark}, StringSplitOptions.None)
            Dim intPart As String = parts(0)
            Dim fracPart As String = parts(1)

            If String.IsNullOrEmpty(fracPart) Then Return s

            Dim sb As New StringBuilder()
            For i As Integer = 0 To fracPart.Length - 1
                sb.Append(fracPart(i))
                ' 在指定间隔处插入标记，但不是在末尾
                If (i + 1) Mod interval = 0 AndAlso (i + 1) < fracPart.Length Then
                    sb.Append(smallMark)
                End If
            Next

            Return $"{intPart}{decimalMark}{sb.ToString()}"
        End Function

        ''' <summary>
        ''' 格式化日期。
        ''' </summary>
        Private Shared Function FormatDate(dt As Date, trim As Boolean, justify As Justification, width As Integer?) As String
            ' R 默认的日期格式通常是 YYYY-MM-DD 或类似的，这里我们使用 VB.NET 的通用格式 "G"
            ' 如果需要更复杂的 R 风格格式，可以添加一个 dateFormat 参数
            Dim result As String = dt.ToString("G", CultureInfo.InvariantCulture)
            Return ApplyJustificationAndWidth(result, justify, width)
        End Function

        ''' <summary>
        ''' 格式化字符串。
        ''' </summary>
        Private Shared Function FormatString(str As String, trim As Boolean, justify As Justification, width As Integer?) As String
            Dim result As String = str

            If trim Then
                result = result.Trim()
            End If

            Return ApplyJustificationAndWidth(result, justify, width)
        End Function

        ''' <summary>
        ''' 对字符串应用对齐和宽度填充。
        ''' </summary>
        Private Shared Function ApplyJustificationAndWidth(s As String, justify As Justification, width As Integer?) As String
            If Not width.HasValue OrElse width.Value <= s.Length Then
                Return s
            End If

            Dim padLength As Integer = width.Value - s.Length

            Select Case justify
                Case Justification.Left
                    Return s.PadRight(width.Value)
                Case Justification.Right
                    Return s.PadLeft(width.Value)
                Case Justification.Centre
                    Dim leftPad As Integer = padLength \ 2
                    Return s.PadLeft(s.Length + leftPad).PadRight(width.Value)
                Case Justification.None
                    Return s
                Case Else
                    Return s
            End Select
        End Function

#End Region

    End Class
End Namespace