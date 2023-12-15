export enum tokenType {
    number,
    character,
    logical,
    factor,
    keyword,
    symbol,
    operator,
    comment,
    newLine,
    whitespace
}

/**
 * pattern for match the number token
*/
export const number_regexp = /[-]?\d+(\.\d+)?([eE][-]?\d+)?/ig;

export interface token {
    text: string;
    type: tokenType;
}

export function renderTextSet(chars: string[]): object {
    var set = {};

    for (let char of chars) {
        set[char] = 1;
    }

    return set;
}

export const logical: {} = renderTextSet(["true", "false", "TRUE", "FALSE", "True", "False"]);
export const keywords: {} = renderTextSet(["imports", "from", "require", "if", "else", "for", "function", "let", "const", "return", "", "", "", "", "", "", ""]);
export const operators: {} = renderTextSet(["+", "-", "*", "/", "\\", "!", "$", "%", "^", "&", "=", "<", ">", ":", "|", ";", ""]);

