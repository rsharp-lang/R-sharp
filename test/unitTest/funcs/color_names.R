require(ColorBrewer);
require(JSON);

ColorBrewer::scaler_palette()
|> JSON::json_encode()
|> writeLines(
    con = `${@dir}/colors.json`
);
