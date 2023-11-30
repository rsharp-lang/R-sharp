import { TokenParser } from "./parser";
import { token, tokenType } from "./token";

export function parseText(str: string): token[] {
    var parser = new TokenParser(str);
    var tokens = parser.getTokens();

    return tokens;
}

/**
 * parse the script text to syntax highlight html content
*/
export function highlights(str: string): string {
    var tokens = parseText(str);
    var html: string = "";

    for (let t of tokens) {
        switch (t.type) {
            case tokenType.newLine:
                html = html + "\n";
                break;
            case tokenType.whitespace,
                tokenType.operator,
                tokenType.symbol:
                html = html + t.text;
                break;

            default:
                html = html + `<span class="${t.type}">${t.text}</span>`;
        }
    }

    return html;
}

