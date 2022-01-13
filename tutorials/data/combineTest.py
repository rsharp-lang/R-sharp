

x = 1:6

print(combn(x, 3))

print("by index")

i = combn(length(x), 3)

for i in as.list(i, byrow = True):

    print(i)    
    print(x[i])