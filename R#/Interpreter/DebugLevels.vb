Namespace Interpreter

    Public Enum DebugLevels As Byte
        None
        Memory
        Stack
        All = Memory Or Stack
    End Enum
End Namespace