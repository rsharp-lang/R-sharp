#' process a single vb.net source file
#' 
const walkFiles = function(vbproj, refer, banner, proj_folder) {
    let v = list();
    let root = dirname(refer);

    for(file in tqdm(sourceFiles(vbproj))) {
        let stat = write.code_banner(file, banner, rootDir = proj_folder);

        v$totalLines   = append(v$totalLines, [stat]::totalLines);
        v$commentLines = append(v$commentLines, [stat]::commentLines);
        v$blankLines   = append(v$blankLines, [stat]::blankLines);
        v$size         = append(v$size, [stat]::size);
        v$lineOfCodes  = append(v$lineOfCodes, [stat]::lineOfCodes);

        v$Class    = append(v$Class, [stat]::classes);
        v$method   = append(v$method, [stat]::method);
        v$operator = append(v$operator, [stat]::operator);
        v$function = append(v$function, [stat]::function);
        v$property = append(v$property, [stat]::properties);

        v$files    = append(v$files, relative_path(file, root));
        v$projList = append(v$projList, relative_path(refer, proj_folder));
    }

    print("-----------------------------");

    return(v);
}