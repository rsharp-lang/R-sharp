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

export function parseText(str: string) {

}

export class TokenParser {

    escaped: boolean = false;
    escape_char: string | null = null;

    /**
     * for get char at index
    */
    i: number = 0;
    str_len: number = -1;

    public constructor(public source: string) {
        if (source) {
            this.str_len = source.length;
        }
    }

    public getTokens(): token[] {
        const tokens: token[] = [];

        while (this.i < this.str_len) {

        }

        return tokens;
    }


}