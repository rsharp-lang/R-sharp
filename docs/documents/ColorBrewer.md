# ColorBrewer

Color schema name terms that comes from the ColorBrewer system:

 All of the color schema in ColorBrewer system have several levels, which could be use expression 
 pattern such as ``schema_name:c[level]`` for get colors from the color designer, examples as 
 ``YlGnBu:c6`` will generate a color sequence which have 6 colors that comes from the ``YlGnBu`` 
 pattern.

 1. Sequential schemes are suited to ordered data that progress from low to high. Lightness steps 
 dominate the look of these schemes, with light colors for low data values to dark colors for 
 high data values. 

 All of the colors terms in Sequential schemes have levels from 3 to 9, schema name terms 
 includes:

 ```
 OrRd:c[3,9], PuBu:c[3,9], BuPu:c[3,9], Oranges:c[3,9], BuGn:c[3,9], YlOrBr:c[3,9]
 YlGn:c[3,9], Reds:c[3,9], RdPu:c[3,9], Greens:c[3,9], YlGnBu:c[3,9], Purples:c[3,9]
 GnBu:c[3,9], Greys:c[3,9], YlOrRd:c[3,9], PuRd:c[3,9], Blues:c[3,9], PuBuGn:c[3,9]
 ```

 2. Qualitative schemes do not imply magnitude differences between legend classes, and hues are used 
 to create the primary visual differences between classes. Qualitative schemes are best suited to 
 representing nominal or categorical data. 

 The color levels in this schema range from 3 to 12, schema name terms includes:

 ```
 Set2:c[3,8], Accent:c[3,8], Set1:c[3,9], Set3:c[3,12], Dark2:c[3,8], Paired:c[3,12]
 Pastel2:c[3,8], Pastel1:c[3,9]
 ```
 
 3. Diverging schemes put equal emphasis on mid-range critical values and extremes at both ends of 
 the data range. The critical class or break in the middle of the legend is emphasized with light 
 colors and low and high extremes are emphasized with dark colors that have contrasting hues.

 All of the colors terms in Sequential schemes have levels from 3 to 11, schema name terms 
 includes:

 ```
 Spectral:c[3,11], RdYlGn:c[3,11], RdBu:c[3,11], PiYG:c[3,11], PRGn:c[3,11], RdYlBu:c[3,11]
 BrBG:c[3,11], RdGy:c[3,11], PuOr:c[3,11]
 ```

+ [TrIQ](ColorBrewer/TrIQ.1) get cutoff threshold value via TrIQ algorithm
