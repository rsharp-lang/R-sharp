from Rstudio import gtk
from devkit import VisualStudio

# banner data for update onto the source files
info        = "Load code banner data"
banner_xml  = gtk::selectFiles(title = info, filter = "Data XML(*.xml)|*.xml", multiple = FALSE)
banner      = read.banner(banner_xml)
# folder that contains the source projects
info        = "Open the folder which contains the project sources to write banner data."
proj_folder = gtk::selectFolder(default = getwd(), desc = info)

print(`banner data from '${banner_xml}'!`)
print(banner)

# only apply of the banner to the project source file?
projects = list.files(proj_folder, pattern = "*.vbproj", recursive = True)

totalLines   = []
commentLines = []
blankLines   = []
size         = []
lineOfCodes  = []
files        = []
projList     = []

print(`get ${length(projects)} target source projects!`)
print(projects)

def walkFiles(vbproj, refer):
    v = list()
    root = dirname(refer)

    for file in sourceFiles(vbproj):
        print(file)

        stat = write.code_banner(file, banner, rootDir = proj_folder)

        v$totalLines   = append(v$totalLines, [stat]::totalLines)
        v$commentLines = append(v$commentLines, [stat]::commentLines)
        v$blankLines   = append(v$blankLines, [stat]::blankLines)
        v$size         = append(v$size, [stat]::size)
        v$lineOfCodes  = append(v$lineOfCodes, [stat]::lineOfCodes)
        v$files        = append(v$files, getRelativePath(file, root))
        v$projList     = append(v$projList, getRelativePath(refer, proj_folder))

    print("-----------------------------")

    return v

def process_project(vbproj, refer):
    print(vbproj)

    v = walkFiles(vbproj, refer) 

    # append project details
    totalLines   = append(totalLines, v$totalLines)
    commentLines = append(commentLines, v$commentLines)
    blankLines   = append(blankLines, v$blankLines)
    size         = append(size, v$size)
    lineOfCodes  = append(lineOfCodes, v$lineOfCodes)
    files        = append(files, v$files)
    projList     = append(projList, v$projList)

    return v

for refer in projects:
    vbproj = read.vbproj(file = refer)
    
    if is.null(vbproj):
        print(`skip of the .NET Sdk project: ${refer}...`)
    else        
        print(`processing vbproj: ${refer}`)

        # run project processing...
        v = process_project(vbproj, refer)

        # append project summary
        totalLines   = append(totalLines, sum(v$totalLines))
        commentLines = append(commentLines, sum(v$commentLines))
        blankLines   = append(blankLines, sum(v$blankLines))
        size         = append(size, sum(v$size))
        lineOfCodes  = append(lineOfCodes, sum(v$lineOfCodes))
        files        = append(files, "<project>")
        projList     = append(projList, getRelativePath(refer, proj_folder))
    
    print("~done!")

stat = data.frame(proj = projList, files, totalLines, commentLines, blankLines, size, lineOfCodes)
save = `${proj_folder}/proj_stats.csv`

print(stat, max.print = 13)

write.csv(stat, file = save, row.names = False)