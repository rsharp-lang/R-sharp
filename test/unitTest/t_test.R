m = data.frame(a = [1,1,1,1], b = [0.24,0.2785,0.783,0.41234], c = [0.345,0.673,0.8957,0.9323]);

x = [0.89,0.6964,0.997079,0.7979];

p = t.test(m, [0,0,0,0], t ~ a + b + c + x);

print([p]::Pvalue);

print([p]::TestValue);

print("mean of x:");
print([p]::MeanX);


