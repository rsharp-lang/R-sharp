from Rstudio import gtk

# banner data for update onto the source files
banner_xml = gtk::selectFiles(title = "Load code banner data", filter = "Data XML(*.xml)|*.xml", multiple = FALSE)
# folder that contains the source projects
proj_folder = gtk::selectFolder(default = getwd(), desc = "Open the folder which contains the project sources to write banner data.")


