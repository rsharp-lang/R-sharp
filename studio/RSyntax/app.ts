import { TokenParser } from "./parser";
import { token } from "./token";

function parseText(str: string): token[] {
    var parser = new TokenParser(str);
    var tokens = parser.getTokens();

    return tokens;
}

function highlights(str: string) {

}

