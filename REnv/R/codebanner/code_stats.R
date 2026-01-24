#' export the result as csv file
#' 
const code_stats = function(stats, proj_folder, save) {
    let v = [
        proj_folder, 
        "<root>", 
        sum(stats$totalLines), 
        sum(stats$commentLines), 
        sum(stats$blankLines), 
        sum(stats$lineOfCodes), 
        sum(stats$classes), 
        sum(stats$property), 
        sum(stats$method), 
        sum(stats$functions), 
        sum(stats$operator), 
        sum(stats$size)
    ];
    let stat = data.frame(
        proj          = stats$projList, 
        files         = stats$files, 
        totalLines    = stats$totalLines, 
        commentLines  = stats$commentLines, 
        blankLines    = stats$blankLines, 
        lineOfCodes   = stats$lineOfCodes, 
        "class"       = stats$classes, 
        property      = stats$property, 
        method        = stats$method, 
        functions     = stats$functions, 
        operator      = stats$operator, 
        "size(bytes)" = stats$size
    );
    
    stat = rbind(stat, v);
    stat[, "size"] = byte_size(stat[, "size(bytes)"]);
    
    print(stat, max.print = 13);

    write.csv(stat, file = save, row.names = FALSE);
}