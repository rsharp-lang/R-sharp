#Region "Microsoft.VisualBasic::1d69d624d6421dc8c542fd2d2a8d0cd9, R-sharp\Library\base\base.vb"

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

    '   Total Lines: 31
    '    Code Lines: 12
    ' Comment Lines: 17
    '   Blank Lines: 2
    '     File Size: 1.23 KB


    ' Module base
    ' 
    '     Function: impute
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.Math.DataFrame.Impute
Imports Microsoft.VisualBasic.Scripting.MetaData

''' <summary>
''' #### The R Base Package
''' 
''' This package contains the basic functions which let R function as a language: 
''' arithmetic, input/output, basic programming support, etc. Its contents are 
''' available through inheritance from any environment.
'''
''' For a complete list of functions, use ``ls("base")``.
''' </summary>
<Package("base", Category:=APICategories.UtilityTools, Publisher:="xie.guigang@gcmodeller.org")>
Public Module base

    ''' <summary>
    ''' impute for missing values
    ''' </summary>
    ''' <param name="rawMatrix"></param>
    ''' <param name="byRow"></param>
    ''' <param name="infer"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("impute")>
    Public Function impute(rawMatrix As DataSet(), Optional byRow As Boolean = True, Optional infer As InferMethods = InferMethods.Average) As DataSet()
        Return rawMatrix.SimulateMissingValues(byRow, infer).ToArray
    End Function
End Module
