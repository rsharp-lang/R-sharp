type token = Token.token;

export function parseText(str: string): token[] {
    var parser = new TokenParser(str);
    var tokens = parser.getTokens();

    return tokens;
}

/**
 * parse the script text to syntax highlight html content
*/
export function highlights(str: string): string {
    var html: string = "";

    for (let t of parseText(str)) {
        switch (t.type) {
            case Token.tokenType.newLine:
                html = html + "\n";
                break;
            case Token.tokenType.whitespace,
                Token.tokenType.operator,
                Token.tokenType.symbol:
                html = html + t.text;
                break;

            default:
                html = html + `<span class="${t.type}">${t.text}</span>`;
        }
    }

    return html;
}

