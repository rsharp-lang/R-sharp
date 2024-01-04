declare namespace Token {
    type tokenType = "undefined" | "number" | "character" | "logical" | "factor" | "keyword" | "symbol" | "operator" | "comment" | "newLine" | "whitespace" | "bracket" | "terminator" | "color" | "delimiter";
    /**
     * regexp for test html colors
    */
    const html_color: RegExp;
    /**
     * pattern for match the number token
    */
    const number_regexp: RegExp;
    const symbol_name: RegExp;
    interface token {
        text: string;
        type: tokenType;
    }
    const logical: {};
    const operators: {};
    const stacks: {};
    const keywords: {};
    function isColorLiteral(pull_str: string): boolean;
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
type token = Token.token;
declare function parseText(str: string): token[];
/**
 * parse the script text to syntax highlight html content
*/
declare function highlights(str: string, verbose?: boolean): string;
declare function escape_op(str: string): string;
