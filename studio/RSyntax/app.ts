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
function highlights(str: string, verbose: boolean = true): string {
    let html: string = "";
    let syntax = parseText(str);

    if (verbose) {
        console.log("view of the syntax tokens:");
        console.table(syntax);
    }

    for (let t of syntax) {
        switch (t.type) {
            case "newLine":
                html = html + "\n";
                break;
            case "whitespace":
            case "symbol":
                html = html + escape_op(t.text);
                break;

            case "color":
                html = html + `<span class="color" style="font-style: italic; background-color: ${t.text.replace(/'/ig, "").replace(/"/ig, "")}; color: white; font-weight: bold;">${t.text}</span>`;
                break;

            default:
                html = html + `<span class="${t.type}">${t.text}</span>`;
        }
    }

    return html;
}

function escape_op(str: string): string {
    if (!str) {
        return "";
    } else {
        return str
            .replace("&", "&amp;")
            .replace(">", "&gt;")
            .replace("<", "&lt;");
    }
}