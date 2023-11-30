import { TokenParser } from "./parser";
import { token } from "./token";

export function parseText(str: string): token[] {
    var parser = new TokenParser(str);
    var tokens = parser.getTokens();

    return tokens;
}

export function highlights(str: string) {

}

