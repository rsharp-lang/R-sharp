x = c(0.99868887,0.99868887,0.99868887,1,1,1,0.5,0.5,0.5,0.77548173,0.77548173,0.77548173,1,0.3,1,1,1,1,1,1);

print(t.test(x,rep(0.0,length(x)) , var.equal = TRUE, alternative = 0));