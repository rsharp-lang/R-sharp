g = bitmap(width = 256, height = 256, color = "white");
dev.set(g);

text(50,50, "hello world", col = "green");

graphics(g, file = `${@dir}/test_.wmf`);
   
   