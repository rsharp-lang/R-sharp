export = "X:/"
rid = "wwwwwwwwwwww"
target_mz = 895.2556666666666666
precursor = "[M+HCOO]-"

png = `${export}/${rid}/xic_${toString(target_mz, format = "F4")}_${toString(precursor)}.png`

print(png)