import { TokenParser } from "./parser";
import { token } from "./token";

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

    }

    return html;
}

