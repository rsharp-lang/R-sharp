imports ["Html", "http", "graphquery"] from "webKit";

const demo_url = "https://raw.githubusercontent.com/xieguigang/sciBASIC/master/Data/data/query.html";
const query = graphquery::parseQuery('

	graphquery
	{
		# parser function pipeline can be 
		# in different line,
		# this will let you write graphquery
		# code in a more graceful style when
		# you needs a lot of pipeline function
		# for parse value data.
		bookID    css("book") 
				| attr("id")

		title     css("title")
		isbn      xpath("//isbn")
		quote     css("quote")
		language  css("title") | attr("lang")

		# another sub query in current graph query
		author css("author") {
			name css("name")
			born css("born")
			dead css("dead")
		}

		# this is a array of type character
		character xpath("//character") [{
			name          css("name")
			born          css("born")
			qualification xpath("qualification")
		}]
	}
	
');

const document = demo_url
:> requests.get
:> content
;

print("the raw html document text that request from the remote web server:");
cat("\n");
print(document);

cat("\n\n");

print("data query result from the html document text:");
cat("\n");
str(graphquery::query(Html::parse(document), query));