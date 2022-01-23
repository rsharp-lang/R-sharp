setwd(@dir)

open("myfile.txt", "w") do io
    write(io, "Hello world!")
end

str(open(f->read(f, "String"), "myfile.txt"))
