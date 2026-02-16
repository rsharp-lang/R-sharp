Namespace Interpreter

    ''' <summary>
    ''' 调试动作类型
    ''' </summary>
    Public Enum DebugAction
        [Continue] ' 继续运行直到下一个断点
        [StepOver] ' 单步跳过（执行下一行）
        [StepInto] ' 单步进入（进入函数内部，如果支持）
        [Stop]     ' 停止执行
    End Enum

End Namespace