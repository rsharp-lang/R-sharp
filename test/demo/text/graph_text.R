imports ["Html", "graphquery"] from "webKit";

const query = graphquery::parseQuery("
	graphquery css('script', *) [
        text()
    ] 
");

const document = readText(`${@dir}/err_text2.html`);

print("parse of a simple content vector");
cat("\n");
str(graphquery::query(Html::parse(document), query));

