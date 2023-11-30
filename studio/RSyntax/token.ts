export enum tokenType {
    number,
    character,
    logical,
    factor,
    symbol,
    operator,
    comment,
    newLine
}

export interface token {
    text: string;
    type: tokenType;
}