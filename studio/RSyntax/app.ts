///<reference path="token.ts" />
///<reference path="parser.ts" />

type token = Token.token;

function parseText(str: string): token[] {
    var parser = new TokenParser(str);
    var tokens = parser.getTokens();

    return tokens;
}

/**
 * parse the script text to syntax highlight html content
*/
function highlights(str: string): string {
    var html: string = "";

    for (let t of parseText(str)) {
        switch (t.type) {
            case "newLine":
                html = html + "\n";
                break;
            case "whitespace":
            case "operator":
            case "symbol":
                html = html + t.text;
                break;

            default:
                html = html + `<span class="${t.type}">${t.text}</span>`;
        }
    }

    return html;
}

