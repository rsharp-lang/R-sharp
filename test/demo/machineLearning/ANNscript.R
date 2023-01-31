require();

data = read.csv("");

tensor(model = ANN)
|> input(data, )
|> hidden()
|> output(x -> log(x ^ 2))
|> build()
|> snapshot(file = "./model.hds")
;

validates = read.csv("");

tensor(model = "./model.hds")
|> fit(validates)
|> print()
;

