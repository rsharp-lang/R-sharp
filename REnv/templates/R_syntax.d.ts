declare namespace Token {
    type tokenType = "number" | "character" | "logical" | "factor" | "keyword" | "symbol" | "operator" | "comment" | "newLine" | "whitespace" | "bracket" | "terminator";
    interface token {
        text: string;
        type: tokenType;
    }
    const logical: {};
    const operators: {};
    const stacks: {};
    const keywords: {};
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
declare function highlights(str: string, verbose?: boolean): string;
declare function escape_op(str: string): string;
