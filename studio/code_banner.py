from Rstudio import gtk
from devkit import VisualStudio

# banner data for update onto the source files
banner_xml = gtk::selectFiles(title = "Load code banner data", filter = "Data XML(*.xml)|*.xml", multiple = FALSE)
banner = read.banner(banner_xml)

# folder that contains the source projects
info = "Open the folder which contains the project sources to write banner data."
proj_folder = gtk::selectFolder(default = getwd(), desc = info)

print(`banner data from '${banner_xml}'!`)
print(banner)

# only apply of the banner to the project source file?
projects = list.files(proj_folder, pattern = "*.vbproj", recursive = True)

totalLines = []
commentLines = []
blankLines = []
size = []
lineOfCodes = []
files = []
projList = []

print(`get ${length(projects)} target source projects!`)
print(projects)

def process_project(vbproj, refer):
    print(vbproj)

    for file in sourceFiles(vbproj):
        print(file)

        stat = write.code_banner(file, banner, rootDir = proj_folder)

        totalLines = append(totalLines, [stat]::totalLines)
        commentLines =append(commentLines, [stat]::commentLines)
        blankLines = append(blankLines, [stat]::blankLines)
        size = append(size, [stat]::size)
        lineOfCodes = append(lineOfCodes, [stat]::lineOfCodes)
        files = append(files, file)
        projList = append(projList, refer)

for refer in projects:
    vbproj = read.vbproj(file = refer)
    
    if is.null(vbproj):
        print(`skip of the .NET Sdk project: ${refer}...`)
    else        
        print(`processing vbproj: ${refer}`)

        # run project processing...
        process_project(vbproj, refer)
    
stat = data.frame(projList, files, totalLines, commentLines, blankLines, size, lineOfCodes)

print(stat, max.print = 13)

write.csv(stat, file = `${proj_folder}/projects.csv`, row.names = False)