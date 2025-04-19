data(wine);

print(wine);

writeBin(wine, con = file.path(@dir,"wine.dat"));

wine_read = readBin(file.path(@dir,"wine.dat"), what = "dataframe");

print(wine_read );