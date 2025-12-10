#Region "Microsoft.VisualBasic::e98640ed4054a0628709c03947f44a4c, R#\Runtime\Internal\internalInvokes\string\RFormat.vb"

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

    '   Total Lines: 331
    '    Code Lines: 207 (62.54%)
    ' Comment Lines: 81 (24.47%)
    '    - Xml Docs: 65.43%
    ' 
    '   Blank Lines: 43 (12.99%)
    '     File Size: 15.43 KB


    '     Class RFormat
    ' 
    ' 
    '         Enum Justification
    ' 
    '             centre, left, none, right
    ' 
    ' 
    ' 
    '  
    ' 
    '     Function: ApplyJustificationAndWidth, ApplySmallMark, ConvertRDateFormatToDotNet, FormatDate, FormatNumber
    '               FormatString, FormatToSignificantDigits, FormatValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.Text
Imports std = System.Math

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
        Optional format As String = Nothing,
        Optional trim As Boolean = False,
        Optional digits As Integer? = Nothing,
        Optional nsmall As Integer = 0,
        Optional justify As Justification = Justification.left,
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
                    Return FormatDate(CDate(x), If(format, "G"), trim, justify, width)
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
            Dim factor As Double = 10 ^ (digits - scale)
            Dim scaledNum As Double = std.Round(num * factor, MidpointRounding.AwayFromZero)

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
        Private Shared Function FormatDate(dt As Date, format As String, trim As Boolean, justify As Justification, width As Integer?) As String
            ' R 默认的日期格式通常是 YYYY-MM-DD 或类似的，这里我们使用 VB.NET 的通用格式 "G"
            ' 如果需要更复杂的 R 风格格式，可以添加一个 dateFormat 参数
            Dim clr_format As String = ConvertRDateFormatToDotNet(format)
            Dim result As String = dt.ToString(clr_format, CultureInfo.InvariantCulture)
            Return ApplyJustificationAndWidth(result, justify, width)
        End Function

        ''' <summary>
        ''' 将 R 语言的日期时间格式字符串转换为等价的 .NET 自定义日期时间格式字符串。
        ''' </summary>
        ''' <param name="rFormat">一个有效的 R 日期时间格式字符串，例如 "%Y-%m-%d %H:%M:%S"。</param>
        ''' <returns>一个等价的 .NET 自定义日期时间格式字符串，例如 "yyyy-MM-dd HH:mm:ss"。</returns>
        ''' <remarks>
        ''' 此函数支持大多数常用的 R 格式化占位符。
        ''' 对于一些在 .NET 中没有直接等价物的复杂占位符（如一年中的第几天 %j，或周数 %U/%W），
        ''' 函数会保留原始占位符，并可能需要在转换后进行手动处理。
        ''' </remarks>
        Public Shared Function ConvertRDateFormatToDotNet(rFormat As String) As String
            If String.IsNullOrWhiteSpace(rFormat) Then
                Return String.Empty
            End If

            ' 创建 R 到 .NET 格式占位符的映射字典
            ' Key: R 格式化字符 (去掉 %)
            ' Value: .NET 对应的格式化字符串
            Dim formatMap As New Dictionary(Of Char, String) From {
        {"Y"c, "yyyy"}, ' 4位年份
        {"y"c, "yy"},   ' 2位年份
        {"m"c, "MM"},   ' 月份 (01-12)
        {"B"c, "MMMM"}, ' 月份全名
        {"b"c, "MMM"},  ' 月份缩写
        {"d"c, "dd"},   ' 月中的天数 (01-31)
        {"H"c, "HH"},   ' 24小时制小时 (00-23)
        {"I"c, "hh"},   ' 12小时制小时 (01-12)
        {"M"c, "mm"},   ' 分钟 (00-59)
        {"S"c, "ss"},   ' 秒 (00-59)
        {"p"c, "tt"},   ' AM/PM 标记
        {"A"c, "dddd"}, ' 星期全名
        {"a"c, "ddd"},  ' 星期缩写
        {"j"c, "DDD"},  ' 一年中的天数 (001-366) - 注意：.NET 的 "D" 是自定义格式，需要数字，这里用 DDD 提示
        {"U"c, "ww"},   ' 一年中的周数 (周日为第一天) - 注意：.NET 的 "ww" 与 R 的计算方式可能不同
        {"W"c, "ww"},   ' 一年中的周数 (周一为第一天) - 注意：.NET 的 "ww" 与 R 的计算方式可能不同
        {"w"c, "dddd"}, ' 星期中的天数 (0-6, 0=周日) - .NET 没有直接对应的数字格式，这里转为星期名
        {"x"c, "d"},    ' 地区性的日期表示
        {"X"c, "t"},    ' 地区性的时间表示
        {"c"c, "F"},    ' 地区性的日期和时间表示
        {"Z"c, "zzz"},  ' 时区名称
        {"%"c, "\%"}    ' 字面量百分号
    }

            Dim dotNetFormat As New StringBuilder()
            Dim i As Integer = 0

            While i < rFormat.Length
                Dim currentChar As Char = rFormat(i)

                If currentChar = "%"c Then
                    ' 检查是否为字符串末尾，防止越界
                    If i + 1 >= rFormat.Length Then
                        dotNetFormat.Append("%"c)
                        Exit While
                    End If

                    Dim nextChar As Char = rFormat(i + 1)

                    ' 在映射表中查找
                    If formatMap.ContainsKey(nextChar) Then
                        dotNetFormat.Append(formatMap(nextChar))
                        i += 2 ' 跳过 % 和下一个字符
                    Else
                        ' 如果是未知的占位符，保留原始字符，避免信息丢失
                        dotNetFormat.Append("%"c).Append(nextChar)
                        i += 2
                    End If
                Else
                    ' 普通字符，直接追加
                    dotNetFormat.Append(currentChar)
                    i += 1
                End If
            End While

            Return dotNetFormat.ToString()
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
