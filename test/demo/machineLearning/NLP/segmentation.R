imports "NLP" from "MLkit";

options(strict = FALSE);

data = NLP::segmentation(readText("E:\biodeep\biodeepdb_v3\Flavor\wikipedia\raw\honey\terraria.fandom.com\wiki\Honey.txt"));

# print(data);

for(p in data) {
	print(sapply(as.object(p)$sentences, toString));
}