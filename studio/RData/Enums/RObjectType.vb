#Region "Microsoft.VisualBasic::ba5a00817ae4cf54bbb3ead0bf87ac86, G:/GCModeller/src/R-sharp/studio/RData//Enums/RObjectType.vb"

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

    '   Total Lines: 134
    '    Code Lines: 34
    ' Comment Lines: 96
    '   Blank Lines: 4
    '     File Size: 3.36 KB


    '     Enum RObjectType
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Flags

    ''' <summary>
    ''' Type of a R object.
    ''' </summary>
    Public Enum RObjectType As Byte
        ''' <summary>
        ''' NULL, nil = NULL
        ''' </summary>
        NIL = 0
        ''' <summary>
        ''' symbols
        ''' </summary>
        SYM = 1
        ''' <summary>
        ''' pairlists, lists of dotted pairs
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
        ''' promises, promises: [un]evaluated closure arguments
        ''' </summary>
        PROM = 5
        ''' <summary>
        ''' language objects, language constructs (special lists)
        ''' </summary>
        LANG = 6
        ''' <summary>
        ''' special functions
        ''' </summary>
        SPECIAL = 7
        ''' <summary>
        ''' builtin functions, builtin non-special forms 
        ''' </summary>
        BUILTIN = 8
        ''' <summary>
        ''' internal character strings, "scalar" string type (internal only, CHARSXP)
        ''' </summary>
        [CHAR] = 9
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
        ''' character vectors(STRSXP)
        ''' </summary>
        STR = 16
        ''' <summary>
        ''' dot-dot-dot Object
        ''' </summary>
        DOT = 17
        ''' <summary>
        ''' make “any” args work
        ''' 
        ''' Used in specifying types for symbol
        ''' registration To mean anything Is okay
        ''' </summary>
        ANY = 18
        ''' <summary>
        ''' VECSXP, list (generic vector)
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
        ''' raw bytes vector
        ''' </summary>
        RAW = 24
        ''' <summary>
        ''' S4 classes Not Of simple type
        ''' </summary>
        S4 = 25

        ''' <summary>
        ''' Closure or Builtin or Special
        ''' </summary>
        FUNSXP = 99

        ''' <summary>
        ''' Alternative representations
        ''' </summary>
        ALTREP = 238

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
End Namespace
