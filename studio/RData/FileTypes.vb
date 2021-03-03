Imports System.ComponentModel

''' <summary>
''' Type of file containing a R file.
''' </summary>
Public Enum FileTypes
    Unknown = 0

    <Description("bz2")> bzip2
    <Description("gzip")> gzip
    <Description("xz")> xz
    <Description("rdata version 2 (binary)")> rdata_binary_v2
    <Description("rdata version 3 (binary)")> rdata_binary_v3
End Enum

''' <summary>
''' Format of a R file.
''' </summary>
Public Enum RdataFormats
    Unknown = 0

    <Description("XDR")> XDR
    <Description("ASCII")> ASCII
    <Description("binary")> binary
End Enum

''' <summary>
''' Type of a R object.
''' </summary>
Public Enum RObjectType
    ''' <summary>
    ''' NULL
    ''' </summary>
    NIL = 0
    ''' <summary>
    ''' symbols
    ''' </summary>
    SYM = 1
    ''' <summary>
    ''' pairlists
    ''' </summary>
    LIST = 2
    ''' <summary>
    ''' closures
    ''' </summary>
    CLO = 3
    ''' <summary>
    ''' environments
    ''' </summary>
    ENV = 4
    ''' <summary>
    ''' promises
    ''' </summary>
    PROM = 5
    ''' <summary>
    ''' language objects
    ''' </summary>
    LANG = 6
    ''' <summary>
    ''' special functions
    ''' </summary>
    SPECIAL = 7
    ''' <summary>
    ''' builtin functions
    ''' </summary>
    BUILTIN = 8
    ''' <summary>
    ''' internal character strings
    ''' </summary>
    [Char] = 9
    ''' <summary>
    ''' logical vectors
    ''' </summary>
    LGL = 10
    ''' <summary>
    ''' Integer vectors
    ''' </summary>
    INT = 13
    ''' <summary>
    ''' numeric vectors
    ''' </summary>
    REAL = 14
    ''' <summary>
    ''' complex vectors
    ''' </summary>
    CPLX = 15
    ''' <summary>
    ''' character vectors
    ''' </summary>
    STR = 16
    ''' <summary>
    ''' dot-dot-dot Object
    ''' </summary>
    DOT = 17
    ''' <summary>
    ''' make “any” args work
    ''' </summary>
    ANY = 18
    ''' <summary>
    ''' list (generic vector)
    ''' </summary>
    VEC = 19
    ''' <summary>
    ''' expression vector
    ''' </summary>
    EXPR = 20
    ''' <summary>
    ''' Byte code
    ''' </summary>
    BCODE = 21
    ''' <summary>
    ''' external pointer
    ''' </summary>
    EXTPTR = 22
    ''' <summary>
    ''' weak reference
    ''' </summary>
    WEAKREF = 23
    ''' <summary>
    ''' raw vector
    ''' </summary>
    RAW = 24
    ''' <summary>
    ''' S4 classes Not Of simple type
    ''' </summary>
    S4 = 25
    ''' <summary>
    ''' Empty environment
    ''' </summary>
    EMPTYENV = 242
    ''' <summary>
    ''' Global environment
    ''' </summary>
    GLOBALENV = 253
    ''' <summary>
    ''' NIL value
    ''' </summary>
    NILVALUE = 254
    ''' <summary>
    ''' Reference
    ''' </summary>
    REF = 255
End Enum

Public Enum CharFlags
    HAS_HASH = 1
    BYTES = 1 << 1
    LATIN1 = 1 << 2
    UTF8 = 1 << 3
    CACHED = 1 << 5
    ASCII = 1 << 6
End Enum