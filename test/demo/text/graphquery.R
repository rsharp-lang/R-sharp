imports ["Html", "graphquery"] from "webKit";

const query = graphquery::parseQuery("
	graphquery { 
    
        anchor css('a') [ 
            text() 
        ] 
    }
");

const document = '
    <html>

        <body>
            <a href="01.html">Page 1</a>
            <a href="02.html">Page 2</a>
            <a href="03.html">Page 3</a>
        </body>

    </html>
';

print("parse of a simple content vector");
cat("\n");
str(graphquery::query(Html::parse(document), query));

cat("\n\n");

print("a complex parser example:");
cat("\n");
str(graphquery::query(Html::parse(document), graphquery::parseQuery(
"            
	a css('a') [{
		title  text() | trim() 
		url    attr('href') 
	}]

")));