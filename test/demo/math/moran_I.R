require(REnv);

data(ozone);

print(ozone, max.print  = 13);

print("get moran I test result:");
str(moran.test(ozone$Av8top, ozone$Lon, ozone$Lat));