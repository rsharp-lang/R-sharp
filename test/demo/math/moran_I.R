require(REnv);

data(ozone);

print(ozone);

print("get moran I test result:");
str(moran.test(ozone$Av8top, ozone$Lon, ozone$Lat));