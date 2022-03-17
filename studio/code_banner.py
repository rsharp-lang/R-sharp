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

print(`get ${length(projects)} target source projects!`)
print(projects)

def process_project(vbproj):
    print(vbproj)


for refer in projects:
    vbproj = read.vbproj(file = refer)
    
    if is.null(vbproj):
        print(`skip of the .NET Sdk project: ${refer}...`)
    else        
        process_project(vbproj)
    
