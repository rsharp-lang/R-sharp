ozone <- read.csv("https://stats.idre.ucla.edu/stat/r/faq/ozone.csv");

print(ozone);

print("get moran I test result:");
str(moran.test(ozone$Av8top, ozone$Lon, ozone$Lat));