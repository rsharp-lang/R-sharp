data = data.frame(id = 1:5, name = ['Bob','Tom','alice','Jerry','Wendy'], age = runif(5, min = 32, max = 60))
rownames(data) = `${data[, "name"]} Smith`

print(data)