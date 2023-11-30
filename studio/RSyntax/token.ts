export enum tokenType {
    number,
    character,
    logical,
    factor,
    symbol,
    operator,
    comment,
    newLine,
    whitespace
}

export interface token {
    text: string;
    type: tokenType;
}

export const logical: {} = {
    "TRUE": 1,
    "true": 1,
    "FALSE": 1,
    "false": 1
};

export const operators: {} = {
    "+": 1, "-": 1, "*": 1, "\\": 1, "/": 1,
    "^": 1, "%": 1,
    "$": 1, "&": 1, "|": 1, "!": 1, ":": 1,
    ";": 1
}