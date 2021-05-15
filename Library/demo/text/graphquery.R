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

str(graphquery::query(Html::parse(document), query));