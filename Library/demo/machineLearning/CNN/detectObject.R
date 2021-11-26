imports "CNN" from "MLkit";

"P:\imagenet-matconvnet-vgg-f.cenin"
|> CeNiN
|> detectObject(target = readImage(file = `${@dir}/SampleImage_NASAAtlantisShuttle.jpg`))
|> head(n = 10)
|> print()
;