declare namespace Token {
    enum tokenType {
        number = 0,
        character = 1,
        logical = 2,
        factor = 3,
        keyword = 4,
        symbol = 5,
        operator = 6,
        comment = 7,
        newLine = 8,
        whitespace = 9
    }
    interface token {
        text: string;
        type: tokenType;
    }
    function renderTextSet(chars: string[]): object;
    const logical: {};
    const keywords: {};
    const operators: {};
}
declare class TokenParser {
    source: string;
    escaped: boolean;
    escape_char: string | null;
    escape_comment: boolean;
    /**
     * for get char at index
    */
    i: number;
    str_len: number;
    /**
     * the token text buffer
    */
    buf: string[];
    constructor(source: string);
    getTokens(): token[];
    private walkChar;
    private measureToken;
}
declare type token = Token.token;
declare function parseText(str: string): token[];
/**
 * parse the script text to syntax highlight html content
*/
declare function highlights(str: string): string;
