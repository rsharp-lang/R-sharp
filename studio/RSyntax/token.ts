namespace Token {

    export type tokenType = "number" | "character" | "logical" | "factor" | "keyword" | "symbol" | "operator" | "comment" | "newLine" | "whitespace" | "bracket" | "terminator";

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
    export const operators: {} = renderTextSet(["+", "-", "*", "/", "\\", "!", "$", "%", "^", "&", "=", "<", ">", ":", "|"]);
    export const stacks: {} = renderTextSet(["[", "]", "(", ")", "{", "}"]);
    export const keywords: {} = renderTextSet([
        "imports", "from", "require",
        "if", "else", "for", "break", "while",
        "function", "return",
        "let", "const",
        "stop", "invisible", "", "", ""
    ]);
}