from Rstudio import gtk
from devkit import VisualStudio

import "./banner.py"

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

print(`get ${length(projects)} target source projects!`)
print(projects)

for refer in projects:
    processSingle(refer)

code_stats(save = `${proj_folder}/proj_stats.csv`)