#Region "Microsoft.VisualBasic::3dbbcc5a9aea08066bef082679a78c9e, G:/GCModeller/src/R-sharp/studio/Rsharp_kit/roxygenNet//rdocumentation/package_template.vb"

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

    '   Total Lines: 123
    '    Code Lines: 101
    ' Comment Lines: 8
    '   Blank Lines: 14
    '     File Size: 5.16 KB


    ' Module package_template
    ' 
    '     Function: getDefaultTemplate
    ' 
    ' /********************************************************************************/

#End Region

Module package_template

    ''' <summary>
    ''' default template for a clr package module in a dll assembly file:
    ''' 
    ''' ```
    ''' imports name from dll
    ''' ```
    ''' </summary>
    ''' <returns></returns>
    Friend Function getDefaultTemplate() As XElement
        Return <html lang="zh-CN">
                   <head>
                       <meta http-equiv="content-type" content="text/html; charset=UTF-8"/>
                       <meta http-equiv="X-UA-Compatible" content="IE=Edge"/>
                       <meta charset="utf-8"/>
                       <meta name="viewport" content="width=device-width, minimum-scale=1.0, maximum-scale=1.0"/>

                       <title>{$packageName}</title>

                       <meta name="author" content="xie.guigang@gcmodeller.org"/>
                       <meta name="copyright" content="SMRUCC genomics Copyright (c) 2022"/>
                       <meta name="keywords" content="R#; {$packageName}; {$base_dll}"/>
                       <meta name="generator" content="https://github.com/rsharp-lang"/>
                       <meta name="theme-color" content="#333"/>
                       <meta name="description" content="{$shortDescription}"/>

                       <meta class="foundation-data-attribute-namespace"/>
                       <meta class="foundation-mq-xxlarge"/>
                       <meta class="foundation-mq-xlarge"/>
                       <meta class="foundation-mq-large"/>
                       <meta class="foundation-mq-medium"/>
                       <meta class="foundation-mq-small"/>
                       <meta class="foundation-mq-topbar"/>

                       <style>

.table-three-line {
border-collapse:collapse; /* 关键属性：合并表格内外边框(其实表格边框有2px，外面1px，里面还有1px哦) */
border:solid #000000; /* 设置边框属性；样式(solid=实线)、颜色(#999=灰) */
border-width:2px 0 2px 0px; /* 设置边框状粗细：上 右 下 左 = 对应：1px 0 0 1px */
}
.left-1{
    border:solid #000000;border-width:1px 1px 2px 0px;padding:2px;
    font-weight:bolder;
}
.right-1{
    border:solid #000000;border-width:1px 0px 2px 1px;padding:2px;
    font-weight:bolder;
}
.mid-1{
    border:solid #000000;border-width:1px 1px 2px 1px;padding:2px;
    font-weight:bolder;
}
.left{
    border:solid #000000;border-width:1px 1px 1px 0px;padding:2px;
}
.right{
    border:solid #000000;border-width:1px 0px 1px 1px;padding:2px;
}
.mid{
    border:solid #000000;border-width:1px 1px 1px 1px;padding:2px;
}
table caption {font-size:14px;font-weight:bolder;}
</style>
                   </head>
                   <body>
                       <table width="100%" summary=<%= "page for {{$packageName}}" %>>
                           <tbody>
                               <tr>
                                   <td>{{$packageName}}</td><td style="text-align: right;">R# Documentation</td>
                               </tr>
                           </tbody>
                       </table>

                       <h1>{$packageName}</h1>
                       <hr/>
                       <p style="
    font-size: 1.125em;
    line-height: .8em;
    margin-left: 0.5%;
    background-color: #fbfbfb;
    padding: 24px;
">
                           <code>
                               <span style="color: blue;">require</span>(<span style="color: black; font-weight: bold;">{$package}</span>);
                               <br/>
                               <br/>
                               <span style="color: green;">{$desc_comments}</span><br/>
                               <span style="color: blue;">imports</span><span style="color: brown"> "{$packageName}"</span><span style="color: blue;"> from</span><span style="color: brown"> "{$base_dll}"</span>;
                           </code>
                       </p>

                       <p>{$packageDescription}</p>

                       <blockquote>
                           <p style="font-style: italic; font-size: 0.9em;">
                           {$packageRemarks}
                           </p>
                       </blockquote>

                       <div id="main-wrapper">
                           <table class="table-three-line" style="display: {$clr_type_display}">
                               <caption>.NET clr type export</caption>
                               <tbody>{$typeList}</tbody>
                           </table>

                           <br/>
                           <br/>

                           <table class="table-three-line">
                               <caption>.NET clr function exports</caption>
                               <tbody>{$apiList}</tbody>
                           </table>
                       </div>

                       <hr/>
                       <div style="text-align: center;">[<a href="../index.html">Document Index</a>]</div>
                   </body>
               </html>
    End Function

End Module
