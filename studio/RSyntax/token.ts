namespace Token {

    export type tokenType = "undefined" |
        "number" | "character" | "logical" | "factor" |
        "keyword" | "symbol" | "operator" | "comment" |
        "newLine" | "whitespace" | "bracket" | "terminator" |
        "color" | "delimiter";

    export const html_color = /"[#][a-zA-Z0-9]{6}"/ig;
    /**
     * pattern for match the number token
    */
    export const number_regexp = /[-]?\d+(\.\d+)?([eE][-]?\d+)?/ig;
    export const symbol_name = /[a-zA-Z_\.]/ig;


    export interface token {
        text: string;
        type: tokenType;
    }

    function renderTextSet(chars: string[]): object {
        var set = {};

        for (let char of chars) {
            set[char] = 1;
        }

        return set;
    }

    export const logical: {} = renderTextSet(["true", "false", "TRUE", "FALSE", "True", "False"]);
    export const operators: {} = renderTextSet(["+", "-", "*", "/", "\\", "!", "$", "%", "^", "&", "=", "<", ">", ":", "|", ",", "~", "?"]);
    export const stacks: {} = renderTextSet(["[", "]", "(", ")", "{", "}"]);
    export const keywords: {} = renderTextSet([
        "imports", "from", "require",
        "if", "else", "for", "break", "while",
        "function", "return",
        "let", "const",
        "stop", "invisible",
        "export", "namespace", "class",
        "string", "double", "integer", "list"
    ]);
}